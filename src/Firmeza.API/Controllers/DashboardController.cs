// Controlador que entrega las estadísticas y resúmenes para el panel de control principal.
// Reúne datos de ventas, ingresos, clientes, productos con pocas existencias
// y los artículos más vendidos para mostrarlos en pantalla.

using Firmeza.Application.DTOs;
using Firmeza.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Firmeza.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Supervisor,Gerente,Vendedor,Contador,Auditor")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    // Recibe el servicio encargado de calcular las estadísticas comerciales.
    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    // Devuelve el resumen general con los totales de clientes, productos, ingresos, alertas de inventario y productos más vendidos.
    [HttpGet("summary")]
    public async Task<ActionResult<DashboardSummaryDto>> GetSummary()
    {
        var summary = await _dashboardService.GetSummaryAsync();
        return Ok(summary);
    }
}
