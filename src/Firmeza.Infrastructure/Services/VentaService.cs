// Servicio principal para registrar, consultar y anular ventas en el sistema.
// Trabaja con las tablas "ventas", "detalles_venta", "clientes", "productos", "metodos_pago" y "movimientos_inventario".
// Al crear o anular una venta, descuenta o devuelve automáticamente las unidades del inventario y guarda el movimiento.
using Firmeza.Application.DTOs;
using Firmeza.Application.Interfaces;
using Firmeza.Domain.Entities;
using Firmeza.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.Infrastructure.Services;

public class VentaService : IVentaService
{
    private readonly ApplicationDbContext _context;

    // Inicializa el servicio de ventas con la conexión a la base de datos.
    public VentaService(ApplicationDbContext context)
    {
        _context = context;
    }

    // Obtiene todas las ventas registradas incluyendo los datos del cliente y los productos comprados.
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

    // Busca y devuelve una venta específica por su ID junto con la información de su cliente y productos.
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

    // Registra una nueva venta validando el cliente, descontando el stock de cada producto, calculando el IVA y guardando el historial de salida.
    public async Task<VentaDto> CreateAsync(CreateVentaDto dto)
    {
        if (dto.Detalles == null || dto.Detalles.Count == 0)
        {
            throw new InvalidOperationException("La venta debe contener al menos un producto.");
        }

        // 1. Validar que el cliente exista y esté activo
        var cliente = await _context.Clientes.FindAsync(dto.ClienteId);
        if (cliente == null || !cliente.Activo)
        {
            throw new InvalidOperationException($"El cliente con ID {dto.ClienteId} no existe o se encuentra inactivo.");
        }

        // 1.1 Validar el método de pago si fue indicado
        if (dto.MetodoPagoId.HasValue)
        {
            var metodoPago = await _context.MetodosPago.FindAsync(dto.MetodoPagoId.Value);
            if (metodoPago == null || !metodoPago.Activo)
            {
                throw new InvalidOperationException($"El método de pago con ID {dto.MetodoPagoId.Value} no existe o se encuentra inactivo.");
            }
        }

        // 2. Agrupar productos repetidos en el pedido para revisar el stock total requerido
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
        var numeroVenta = $"VENTA-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}";
        var fechaActual = DateTime.UtcNow;

        // 3. Validar inventario, descontar stock y registrar salida en "movimientos_inventario"
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

            var stockPrevio = producto.Stock;
            producto.DescontarStock(item.Cantidad);
            var stockPosterior = producto.Stock;

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

            _context.MovimientosInventario.Add(new MovimientoInventario
            {
                ProductoId = producto.Id,
                TipoMovimiento = "Salida",
                Cantidad = item.Cantidad,
                StockPrevio = stockPrevio,
                StockPosterior = stockPosterior,
                Referencia = numeroVenta,
                Motivo = $"Salida automática por venta {numeroVenta}",
                Fecha = fechaActual
            });
        }

        // 4. Calcular el IVA del 19% y el valor total de la factura
        var iva = Math.Round(subtotal * 0.19m, 2);
        var total = subtotal + iva;

        var venta = new Venta
        {
            NumeroVenta = numeroVenta,
            ClienteId = cliente.Id,
            MetodoPagoId = dto.MetodoPagoId,
            Cliente = cliente,
            Fecha = fechaActual,
            Subtotal = subtotal,
            Iva = iva,
            Total = total,
            Estado = "Completada",
            DetallesVenta = detallesVenta
        };
        venta.RegistrarVenta();

        // 5. Guardar la venta, sus detalles y los cambios de stock en la base de datos
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

    // Anula una venta existente, devuelve las cantidades de cada producto al inventario y registra la entrada en "movimientos_inventario".
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

        var fechaActual = DateTime.UtcNow;

        // Devolver las unidades al stock del producto y guardar el movimiento de entrada
        foreach (var detalle in venta.DetallesVenta)
        {
            var stockPrevio = detalle.Producto.Stock;
            detalle.Producto.IncrementarStock(detalle.Cantidad);
            var stockPosterior = detalle.Producto.Stock;

            _context.MovimientosInventario.Add(new MovimientoInventario
            {
                ProductoId = detalle.Producto.Id,
                TipoMovimiento = "Entrada",
                Cantidad = detalle.Cantidad,
                StockPrevio = stockPrevio,
                StockPosterior = stockPosterior,
                Referencia = venta.NumeroVenta,
                Motivo = $"Reincorporación por anulación de venta {venta.NumeroVenta}",
                Fecha = fechaActual
            });
        }

        venta.CancelarVenta("Anulación solicitada en sistema");
        await _context.SaveChangesAsync();
        return true;
    }
}
