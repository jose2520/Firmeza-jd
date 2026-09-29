using Firmeza.Application.DTOs;
using Firmeza.Application.Interfaces;
using Firmeza.Domain.Entities;
using Firmeza.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.Infrastructure.Services;

public class VentaService : IVentaService
{
    private readonly ApplicationDbContext _context;

    public VentaService(ApplicationDbContext context)
    {
        _context = context;
    }

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

    public async Task<VentaDto> CreateAsync(CreateVentaDto dto)
    {
        if (dto.Detalles == null || dto.Detalles.Count == 0)
        {
            throw new InvalidOperationException("La venta debe contener al menos un producto.");
        }

        var cliente = await _context.Clientes.FindAsync(dto.ClienteId);
        if (cliente == null || !cliente.Activo)
        {
            throw new InvalidOperationException($"El cliente con ID {dto.ClienteId} no existe o se encuentra inactivo.");
        }

        var productIds = dto.Detalles.Select(d => d.ProductoId).Distinct().ToList();
        var productos = await _context.Productos
            .Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id);

        decimal subtotal = 0;
        var detallesVenta = new List<DetalleVenta>();

        foreach (var item in dto.Detalles)
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

            // Descontar inventario
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

        // Cálculo de IVA (19% en Colombia) y Total
        var iva = Math.Round(subtotal * 0.19m, 2);
        var total = subtotal + iva;

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

        // Reincorporar stock
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
