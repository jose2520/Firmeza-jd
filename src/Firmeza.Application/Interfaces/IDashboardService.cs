using Firmeza.Application.DTOs;

namespace Firmeza.Application.Interfaces;

/// <summary>
/// Contrato de servicio para el cálculo de estadísticas ejecutivas e indicadores clave (KPIs) del negocio.
/// 
/// PARA QUÉ SIRVE:
///   Alimenta el panel principal (Dashboard) del frontend con métricas en tiempo real:
///   ingresos acumulados, ventas del día, ventas del mes, productos con inventario crítico y el top 5 más vendido.
/// 
/// ORIGEN DE LOS DATOS:
///   Ejecuta consultas de agregación SQL sobre las tablas 'ventas', 'detalles_venta' y 'productos'
///   en la base de datos PostgreSQL ('firmeza_db').
/// </summary>
public interface IDashboardService
{
    /// <summary>
    /// Calcula y consolida todas las métricas operativas del tablero gerencial.
    /// Origen de datos:
    ///   - Total recaudado: SUM(total) de la tabla 'ventas' (WHERE estado != 'Anulada').
    ///   - Ventas hoy/mes: COUNT(id) con filtro temporal sobre 'fecha' en 'ventas'.
    ///   - Top 5 más vendidos: GROUP BY 'detalles_venta.producto_id' ORDER BY SUM(cantidad) DESC LIMIT 5.
    ///   - Alertas bajo stock: SELECT FROM 'productos' WHERE stock menor o igual a 10.
    /// </summary>
    /// <returns>Objeto DashboardSummaryDto con todas las estadísticas consolidadas.</returns>
    Task<DashboardSummaryDto> GetSummaryAsync();
}
