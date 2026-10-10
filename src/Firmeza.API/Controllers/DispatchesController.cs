using AutoMapper;
using Firmeza.Application.DTOs;
using Firmeza.Domain.Entities;
using Firmeza.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.API.Controllers;

/// <summary>
/// Gestiona la logística, fletes y despachos a pie de obra de los pedidos de materiales vendidos.
/// RUTA BASE: /api/Dispatches
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DispatchesController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public DispatchesController(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    /// <summary>
    /// Lista los despachos logísticos programados y en curso.
    /// ACCESO: Requiere autenticación JWT.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<DespachoDto>>> GetAll()
    {
        var despachos = await _context.Despachos
            .AsNoTracking()
            .OrderByDescending(d => d.FechaEstimada)
            .ToListAsync();

        return Ok(_mapper.Map<IEnumerable<DespachoDto>>(despachos));
    }

    /// <summary>
    /// Consulta el estado de un despacho por su ID.
    /// ACCESO: Requiere autenticación JWT.
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<DespachoDto>> GetById(int id)
    {
        var despacho = await _context.Despachos
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == id);

        if (despacho == null)
            return NotFound(new { message = $"Despacho con ID {id} no encontrado." });

        return Ok(_mapper.Map<DespachoDto>(despacho));
    }

    /// <summary>
    /// Consulta el despacho asociado a una orden de venta.
    /// ACCESO: Requiere autenticación JWT.
    /// </summary>
    [HttpGet("sale/{saleId:int}")]
    public async Task<ActionResult<DespachoDto>> GetBySaleId(int saleId)
    {
        var despacho = await _context.Despachos
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.VentaId == saleId);

        if (despacho == null)
            return NotFound(new { message = $"No se encontró orden de despacho para la venta con ID {saleId}." });

        return Ok(_mapper.Map<DespachoDto>(despacho));
    }

    /// <summary>
    /// Programa una entrega en obra para una orden de venta.
    /// ACCESO: Requiere rol 'Admin'.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<DespachoDto>> Create([FromBody] CreateDespachoDto dto)
    {
        var venta = await _context.Ventas.FindAsync(dto.VentaId);
        if (venta == null)
            return BadRequest(new { message = $"La venta con ID {dto.VentaId} no existe." });

        if (await _context.Despachos.AnyAsync(d => d.VentaId == dto.VentaId))
            return BadRequest(new { message = $"La venta con ID {dto.VentaId} ya tiene una orden de despacho programada." });

        var despacho = _mapper.Map<Despacho>(dto);
        despacho.Estado = "Pendiente";
        despacho.CreadoEn = DateTime.UtcNow;

        _context.Despachos.Add(despacho);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = despacho.Id }, _mapper.Map<DespachoDto>(despacho));
    }

    /// <summary>
    /// Actualiza el estado de entrega del despacho ('En Ruta', 'Entregado', 'Cancelado').
    /// ACCESO: Requiere rol 'Admin'.
    /// </summary>
    [HttpPatch("{id:int}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<DespachoDto>> UpdateStatus(int id, [FromBody] string nuevoEstado)
    {
        var despacho = await _context.Despachos.FindAsync(id);
        if (despacho == null)
            return NotFound(new { message = $"Despacho con ID {id} no encontrado." });

        despacho.Estado = nuevoEstado;
        if (nuevoEstado == "Entregado")
        {
            despacho.FechaEntrega = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return Ok(_mapper.Map<DespachoDto>(despacho));
    }
}
