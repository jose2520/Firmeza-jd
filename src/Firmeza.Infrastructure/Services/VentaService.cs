// =================================================================================================
// SERVICIO: VentaService.cs
// CAPA: Firmeza.Infrastructure (Capa de Infraestructura y Persistencia)
// CONTRATO: IVentaService (Firmeza.Application)
// PROPÓSITO:
//   Implementa la lógica del núcleo del negocio para el procesamiento de transacciones comerciales:
//   - Consulta de ventas con relaciones completas (Cliente y Renglones de Productos).
//   - Creación transaccional de ventas con validación de cliente, control de stock y cálculo de IVA.
//   - Anulación de ventas y reincorporación de productos al inventario.
//
// ORIGEN Y DESTINO DE LOS DATOS:
//   - Base de Datos Relacional: PostgreSQL (vía ApplicationDbContext y Npgsql).
//   - Tablas Afectadas:
//     * 'clientes': Lectura y validación de estado activo.
//     * 'productos': Lectura, verificación y actualización del campo 'stock'.
//     * 'ventas': Inserción de la cabecera (subtotal, iva, total, estado, fecha).
//     * 'detalles_venta': Inserción de los renglones adquiridos (cantidad, precio, subtotal).
// =================================================================================================

using Firmeza.Application.DTOs;
using Firmeza.Application.Interfaces;
using Firmeza.Domain.Entities;
using Firmeza.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.Infrastructure.Services;

public class VentaService : IVentaService
{
    private readonly ApplicationDbContext _context;

    public VentaService(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Obtiene todas las ventas con su cliente y sus productos relacionados.
    /// ORIGEN DE DATOS: PostgreSQL -> JOIN entre tablas 'ventas', 'clientes', 'detalles_venta' y 'productos'.
    /// Optimizado con .AsNoTracking() para evitar sobrecarga de seguimiento de entidades en memoria.
    /// </summary>
    public async Task<IEnumerable<VentaDto>> GetAllAsync()
    {
        return await _context.Ventas
            .Include(v => v.Cliente)
            .Include(v => v.DetallesVenta)
                .ThenInclude(d => d.Producto)
            .AsNoTracking()
            .OrderByDescending(v => v.Fecha)
            .Select(v => new VentaDto(
                v.Id,
                v.NumeroVenta,
                v.ClienteId,
                $"{v.Cliente.Nombres} {v.Cliente.Apellidos}".Trim(),
                v.Cliente.Identificacion,
                v.Fecha,
                v.Subtotal,
                v.Iva,
                v.Total,
                v.Estado,
                v.DetallesVenta.Select(d => new DetalleVentaDto(
                    d.Id,
                    d.ProductoId,
                    d.Producto.Codigo,
                    d.Producto.Nombre,
                    d.Cantidad,
                    d.PrecioUnitario,
                    d.Subtotal
                ))
            ))
            .ToListAsync();
    }

    /// <summary>
    /// Consulta una venta específica por su identificador primario.
    /// ORIGEN DE DATOS: PostgreSQL -> SELECT en 'ventas' con sus relaciones por id = {id}.
    /// </summary>
    public async Task<VentaDto?> GetByIdAsync(int id)
    {
        var v = await _context.Ventas
            .Include(v => v.Cliente)
            .Include(v => v.DetallesVenta)
                .ThenInclude(d => d.Producto)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);

        if (v == null) return null;

        return new VentaDto(
            v.Id,
            v.NumeroVenta,
            v.ClienteId,
            $"{v.Cliente.Nombres} {v.Cliente.Apellidos}".Trim(),
            v.Cliente.Identificacion,
            v.Fecha,
            v.Subtotal,
            v.Iva,
            v.Total,
            v.Estado,
            v.DetallesVenta.Select(d => new DetalleVentaDto(
                d.Id,
                d.ProductoId,
                d.Producto.Codigo,
                d.Producto.Nombre,
                d.Cantidad,
                d.PrecioUnitario,
                d.Subtotal
            ))
        );
    }

    /// <summary>
    /// Procesa y registra una venta en una transacción atómica:
    ///   1. Verifica que el cliente exista y esté activo en la tabla 'clientes'.
    ///   2. Consolida ítems duplicados y valida que los productos existan y estén activos en 'productos'.
    ///   3. Comprueba que el stock sea suficiente y descuenta las unidades.
    ///   4. Calcula automáticamente el IVA (19%) sobre el subtotal acumulado.
    ///   5. Genera el código único 'VENTA-YYYYMMDD-XXXXXX'.
    ///   6. Guarda los cambios en PostgreSQL (inserta en 'ventas' y 'detalles_venta', actualiza 'productos').
    /// </summary>
    public async Task<VentaDto> CreateAsync(CreateVentaDto dto)
    {
        if (dto.Detalles == null || dto.Detalles.Count == 0)
        {
            throw new InvalidOperationException("La venta debe contener al menos un producto.");
        }

        // 1. Validar cliente en PostgreSQL
        var cliente = await _context.Clientes.FindAsync(dto.ClienteId);
        if (cliente == null || !cliente.Activo)
        {
            throw new InvalidOperationException($"El cliente con ID {dto.ClienteId} no existe o se encuentra inactivo.");
        }

        // 2. Consolidar productos duplicados para verificar stock total correctamente
        var consolidatedItems = dto.Detalles
            .GroupBy(d => d.ProductoId)
            .Select(g => new CreateDetalleVentaDto(g.Key, g.Sum(x => x.Cantidad)))
            .ToList();

        var productIds = consolidatedItems.Select(d => d.ProductoId).ToList();
        var productos = await _context.Productos
            .Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id);

        decimal subtotal = 0;
        var detallesVenta = new List<DetalleVenta>();

        // 3. Validar inventario y descontar stock
        foreach (var item in consolidatedItems)
        {
            if (item.Cantidad <= 0)
            {
                throw new InvalidOperationException("La cantidad de cada producto debe ser mayor a cero.");
            }

            if (!productos.TryGetValue(item.ProductoId, out var producto) || !producto.Activo)
            {
                throw new InvalidOperationException($"El producto con ID {item.ProductoId} no existe o no está activo.");
            }

            if (producto.Stock < item.Cantidad)
            {
                throw new InvalidOperationException($"Stock insuficiente para '{producto.Nombre}'. Disponible: {producto.Stock}, Solicitado: {item.Cantidad}.");
            }

            // Descontar inventario en la entidad de EF Core
            producto.Stock -= item.Cantidad;

            var precioUnitario = producto.Precio;
            var lineaSubtotal = precioUnitario * item.Cantidad;
            subtotal += lineaSubtotal;

            detallesVenta.Add(new DetalleVenta
            {
                ProductoId = producto.Id,
                Cantidad = item.Cantidad,
                PrecioUnitario = precioUnitario,
                Subtotal = lineaSubtotal,
                Producto = producto
            });
        }

        // 4. Cálculo fiscal: IVA del 19% (tarifa general en Colombia) y Total
        var iva = Math.Round(subtotal * 0.19m, 2);
        var total = subtotal + iva;

        // 5. Generar código de factura único
        var numeroVenta = $"VENTA-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}";

        var venta = new Venta
        {
            NumeroVenta = numeroVenta,
            ClienteId = cliente.Id,
            Cliente = cliente,
            Fecha = DateTime.UtcNow,
            Subtotal = subtotal,
            Iva = iva,
            Total = total,
            Estado = "Completada",
            DetallesVenta = detallesVenta
        };

        // 6. Confirmar la transacción en PostgreSQL
        _context.Ventas.Add(venta);
        await _context.SaveChangesAsync();

        return new VentaDto(
            venta.Id,
            venta.NumeroVenta,
            venta.ClienteId,
            $"{cliente.Nombres} {cliente.Apellidos}".Trim(),
            cliente.Identificacion,
            venta.Fecha,
            venta.Subtotal,
            venta.Iva,
            venta.Total,
            venta.Estado,
            detallesVenta.Select(d => new DetalleVentaDto(
                d.Id,
                d.ProductoId,
                d.Producto.Codigo,
                d.Producto.Nombre,
                d.Cantidad,
                d.PrecioUnitario,
                d.Subtotal
            ))
        );
    }

    /// <summary>
    /// Anula una venta completada:
    ///   1. Busca la venta y carga sus productos.
    ///   2. Reincorpora automáticamente las cantidades al 'stock' de cada producto en la tabla 'productos'.
    ///   3. Cambia el campo 'estado' de la venta a 'Anulada' en la tabla 'ventas'.
    /// </summary>
    public async Task<bool> CancelAsync(int id)
    {
        var venta = await _context.Ventas
            .Include(v => v.DetallesVenta)
                .ThenInclude(d => d.Producto)
            .FirstOrDefaultAsync(v => v.Id == id);

        if (venta == null || venta.Estado == "Anulada")
        {
            return false;
        }

        // Reincorporar unidades al stock en PostgreSQL
        foreach (var detalle in venta.DetallesVenta)
        {
            if (detalle.Producto != null)
            {
                detalle.Producto.Stock += detalle.Cantidad;
            }
        }

        venta.Estado = "Anulada";
        await _context.SaveChangesAsync();
        return true;
    }
}
