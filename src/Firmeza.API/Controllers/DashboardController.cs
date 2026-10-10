// =================================================================================================
// CONTROLADOR: DashboardController.cs
// CAPA: Firmeza.API (Presentación / Controladores HTTP)
// RUTA BASE: /api/Dashboard
// PROPÓSITO:
//   Suministra métricas agregadas y estadísticas de ventas en tiempo real para alimentar
//   las tarjetas y gráficas del Panel de Control (Dashboard) del Frontend.
//
// ORIGEN DE LOS DATOS:
//   - Consultas agregadas calculadas directamente sobre la base de datos PostgreSQL:
//     * Conteo de filas activas en 'clientes' y 'productos'.
//     * Suma y conteo de 'ventas' completadas en el histórico y en el día actual (UTC).
//     * Filtro de stock bajo (<= 20 unidades) en la tabla 'productos'.
//     * Agrupación (GroupBy) y ordenamiento sobre 'detalles_venta' para obtener el Top 5 más vendido.
// =================================================================================================

using Firmeza.Application.DTOs;
using Firmeza.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Firmeza.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Requiere autenticación con token JWT
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    /// <summary>
    /// Retorna el resumen consolidado de indicadores clave (KPIs) comerciales.
    /// ACCESO: Autenticado.
    /// ORIGEN DE DATOS: Base de datos PostgreSQL (cálculos en vivo sobre clientes, productos y ventas).
    /// </summary>
    [HttpGet("summary")]
    public async Task<ActionResult<DashboardSummaryDto>> GetSummary()
    {
        var summary = await _dashboardService.GetSummaryAsync();
        return Ok(summary);
    }
}
