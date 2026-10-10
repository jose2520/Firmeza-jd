using AutoMapper;
using Firmeza.Application.DTOs;
using Firmeza.Domain.Entities;
using Firmeza.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.API.Controllers;

/// <summary>
/// Gestiona los medios y métodos de pago aceptados en la plataforma.
/// RUTA BASE: /api/PaymentMethods
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PaymentMethodsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public PaymentMethodsController(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    /// <summary>
    /// Lista los métodos de pago habilitados para ventas y facturación.
    /// ACCESO: Público.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<MetodoPagoDto>>> GetAll()
    {
        var metodos = await _context.MetodosPago
            .AsNoTracking()
            .OrderBy(m => m.Id)
            .ToListAsync();

        return Ok(_mapper.Map<IEnumerable<MetodoPagoDto>>(metodos));
    }

    /// <summary>
    /// Consulta un método de pago por su ID.
    /// ACCESO: Público.
    /// </summary>
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<MetodoPagoDto>> GetById(int id)
    {
        var metodo = await _context.MetodosPago
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id);

        if (metodo == null)
            return NotFound(new { message = $"Método de pago con ID {id} no encontrado." });

        return Ok(_mapper.Map<MetodoPagoDto>(metodo));
    }

    /// <summary>
    /// Registra un nuevo método de pago.
    /// ACCESO: Requiere rol 'Admin'.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<MetodoPagoDto>> Create([FromBody] CreateMetodoPagoDto dto)
    {
        if (await _context.MetodosPago.AnyAsync(m => m.Codigo.ToUpper() == dto.Codigo.ToUpper()))
            return BadRequest(new { message = $"Ya existe un método de pago con el código '{dto.Codigo}'." });

        var metodo = _mapper.Map<MetodoPago>(dto);
        metodo.Codigo = dto.Codigo.ToUpper();
        _context.MetodosPago.Add(metodo);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = metodo.Id }, _mapper.Map<MetodoPagoDto>(metodo));
    }
}
