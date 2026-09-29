using Firmeza.Application.DTOs;
using Firmeza.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Firmeza.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SalesController : ControllerBase
{
    private readonly IVentaService _ventaService;

    public SalesController(IVentaService ventaService)
    {
        _ventaService = ventaService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<VentaDto>>> GetAll()
    {
        var ventas = await _ventaService.GetAllAsync();
        return Ok(ventas);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<VentaDto>> GetById(int id)
    {
        var venta = await _ventaService.GetByIdAsync(id);
        if (venta == null)
        {
            return NotFound(new { message = $"Venta con ID {id} no encontrada." });
        }
        return Ok(venta);
    }

    [HttpGet("{id:int}/details")]
    public async Task<ActionResult<IEnumerable<DetalleVentaDto>>> GetDetails(int id)
    {
        var venta = await _ventaService.GetByIdAsync(id);
        if (venta == null)
        {
            return NotFound(new { message = $"Venta con ID {id} no encontrada." });
        }
        return Ok(venta.Detalles);
    }

    [HttpPost]
    public async Task<ActionResult<VentaDto>> Create([FromBody] CreateVentaDto dto)
    {
        try
        {
            var venta = await _ventaService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = venta.Id }, venta);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error interno al procesar la venta", error = ex.Message });
        }
    }

    [HttpPut("{id:int}/cancel")]
    [Authorize(Roles = "Admin,Supervisor")]
    public async Task<IActionResult> Cancel(int id)
    {
        var result = await _ventaService.CancelAsync(id);
        if (!result)
        {
            return NotFound(new { message = $"Venta con ID {id} no encontrada o ya se encuentra anulada." });
        }
        return Ok(new { message = $"Venta {id} anulada exitosamente y stock reincorporado al inventario." });
    }
}
