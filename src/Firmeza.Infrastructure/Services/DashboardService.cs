// Servicio que calcula los resúmenes y estadísticas para el panel principal de administración.
// Consulta las tablas "clientes", "productos", "ventas" y "detalles_venta" en la base de datos.
// Obtiene el total de clientes, ingresos, ventas del día, productos con poco stock y los más vendidos.
using Firmeza.Application.DTOs;
using Firmeza.Application.Interfaces;
using Firmeza.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.Infrastructure.Services;

public class DashboardService : IDashboardService
{
    private readonly ApplicationDbContext _context;

    // Inicializa el servicio del panel de control con la conexión a la base de datos.
    public DashboardService(ApplicationDbContext context)
    {
        _context = context;
    }

    // Reúne todas las cifras de clientes, productos, ventas totales, ventas de hoy y alertas de inventario.
    public async Task<DashboardSummaryDto> GetSummaryAsync()
    {
        // 1. Conteo de clientes y productos activos
        var totalClientes = await _context.Clientes.CountAsync(c => c.Activo);
        var totalProductos = await _context.Productos.CountAsync(p => p.Activo);

        // 2. Totales históricos de ventas completadas
        var ventasCompletadas = _context.Ventas.Where(v => v.Estado == "Completada");
        var totalVentas = await ventasCompletadas.CountAsync();
        var totalIngresos = await ventasCompletadas.SumAsync(v => (decimal?)v.Total) ?? 0m;

        // 3. Ventas e ingresos facturados en el día actual
        var hoy = DateTime.UtcNow.Date;
        var manana = hoy.AddDays(1);

        var ventasHoyQuery = ventasCompletadas.Where(v => v.Fecha >= hoy && v.Fecha < manana);
        var ventasHoy = await ventasHoyQuery.CountAsync();
        var ingresosHoy = await ventasHoyQuery.SumAsync(v => (decimal?)v.Total) ?? 0m;

        // 4. Productos con bajo stock (menor o igual a 20 unidades)
        var bajoStock = await _context.Productos
            .Where(p => p.Activo && p.Stock <= 20)
            .OrderBy(p => p.Stock)
            .Take(10)
            .Select(p => new ProductoBajoStockDto(
                p.Id,
                p.Codigo,
                p.Nombre,
                p.Stock,
                p.Precio))
            .ToListAsync();

        // 5. Top 5 productos más vendidos
        var topProductos = await _context.DetallesVenta
            .AsNoTracking()
            .Where(d => d.Venta.Estado == "Completada")
            .GroupBy(d => new { d.ProductoId, d.Producto.Codigo, d.Producto.Nombre })
            .Select(g => new TopProductoVendidoDto(
                g.Key.ProductoId,
                g.Key.Codigo,
                g.Key.Nombre,
                g.Sum(x => x.Cantidad),
                g.Sum(x => x.Subtotal)))
            .OrderByDescending(x => x.TotalUnidadesVendidas)
            .Take(5)
            .ToListAsync();

        return new DashboardSummaryDto(
            totalClientes,
            totalProductos,
            totalVentas,
            totalIngresos,
            ventasHoy,
            ingresosHoy,
            bajoStock,
            topProductos);
    }
}
