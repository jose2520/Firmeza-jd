// =================================================================================================
// SERVICIO: DashboardService.cs
// CAPA: Firmeza.Infrastructure (Capa de Infraestructura y Persistencia)
// CONTRATO: IDashboardService (Firmeza.Application)
// PROPÓSITO:
//   Calcula las métricas de inteligencia comercial agregadas en tiempo real para el Panel de Control.
//
// ORIGEN Y DESTINO DE LOS DATOS:
//   - Todos los cálculos se realizan a partir de la base de datos PostgreSQL:
//     * Clientes activos: 'SELECT COUNT(*) FROM clientes WHERE activo = true'
//     * Productos activos: 'SELECT COUNT(*) FROM productos WHERE activo = true'
//     * Ventas totales e ingresos acumulados: Agregaciones sobre 'ventas WHERE estado = Completada'
//     * Ventas e ingresos de hoy: Filtro por rango de fecha actual UTC
//     * Alerta de stock crítico: 'productos WHERE activo = true AND stock <= 20'
//     * Top 5 más vendidos: 'detalles_venta' agrupados en memoria por producto
// =================================================================================================

using Firmeza.Application.DTOs;
using Firmeza.Application.Interfaces;
using Firmeza.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.Infrastructure.Services;

public class DashboardService : IDashboardService
{
    private readonly ApplicationDbContext _context;

    public DashboardService(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Ejecuta las consultas y agrupaciones para consolidar los KPIs comerciales.
    /// ORIGEN DE DATOS: PostgreSQL ('clientes', 'productos', 'ventas', 'detalles_venta').
    /// </summary>
    public async Task<DashboardSummaryDto> GetSummaryAsync()
    {
        // 1. Conteo de entidades activas
        var totalClientes = await _context.Clientes.CountAsync(c => c.Activo);
        var totalProductos = await _context.Productos.CountAsync(p => p.Activo);

        // 2. Acumulados históricos de ventas completadas
        var ventasCompletadas = _context.Ventas.Where(v => v.Estado == "Completada");
        var totalVentas = await ventasCompletadas.CountAsync();
        var totalIngresos = await ventasCompletadas.SumAsync(v => (decimal?)v.Total) ?? 0m;

        // 3. Ventas e ingresos facturados en el día actual (rango de 00:00 a 23:59 UTC)
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

        // 5. Top 5 productos con mayor rotación de ventas
        var detalles = await _context.DetallesVenta
            .Where(d => d.Venta.Estado == "Completada")
            .Select(d => new
            {
                d.ProductoId,
                d.Producto.Codigo,
                d.Producto.Nombre,
                d.Cantidad,
                d.Subtotal
            })
            .ToListAsync();

        var topProductos = detalles
            .GroupBy(d => new { d.ProductoId, d.Codigo, d.Nombre })
            .Select(g => new TopProductoVendidoDto(
                g.Key.ProductoId,
                g.Key.Codigo,
                g.Key.Nombre,
                g.Sum(x => x.Cantidad),
                g.Sum(x => x.Subtotal)))
            .OrderByDescending(x => x.TotalUnidadesVendidas)
            .Take(5)
            .ToList();

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
