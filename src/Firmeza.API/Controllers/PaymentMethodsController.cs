// Controlador para administrar las formas de pago aceptadas en Firmeza (efectivo, transferencia, tarjeta, etc.).
// Permite consultar los medios de pago disponibles al momento de facturar y autoriza a los
// administradores a crear, editar o desactivar métodos de pago.

using AutoMapper;
using Firmeza.Application.DTOs;
using Firmeza.Domain.Entities;
using Firmeza.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentMethodsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    // Recibe la conexión a la base de datos y el convertidor de datos de métodos de pago.
    public PaymentMethodsController(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    // Devuelve la lista de todos los métodos de pago registrados en la plataforma.
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

    // Busca y devuelve un método de pago específico por su número identificador.
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

    // Registra un nuevo método de pago comprobando antes que su código no esté repetido.
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

    // Actualiza el código, nombre, descripción o estado de un método de pago existente.
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<MetodoPagoDto>> Update(int id, [FromBody] UpdateMetodoPagoDto dto)
    {
        var metodo = await _context.MetodosPago.FindAsync(id);
        if (metodo == null)
            return NotFound(new { message = $"Método de pago con ID {id} no encontrado." });

        if (await _context.MetodosPago.AnyAsync(m => m.Codigo.ToUpper() == dto.Codigo.ToUpper() && m.Id != id))
            return BadRequest(new { message = $"Ya existe otro método de pago con el código '{dto.Codigo}'." });

        metodo.Codigo = dto.Codigo.ToUpper();
        metodo.Nombre = dto.Nombre;
        metodo.Descripcion = dto.Descripcion;
        metodo.Activo = dto.Activo;

        await _context.SaveChangesAsync();
        return Ok(_mapper.Map<MetodoPagoDto>(metodo));
    }

    // Elimina un método de pago o lo marca como inactivo si ya fue utilizado en alguna venta previa.
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var metodo = await _context.MetodosPago.FindAsync(id);
        if (metodo == null)
            return NotFound(new { message = $"Método de pago con ID {id} no encontrado." });

        var tieneVentas = await _context.Ventas.AnyAsync(v => v.MetodoPagoId == id);
        if (tieneVentas)
        {
            metodo.Activo = false;
        }
        else
        {
            _context.MetodosPago.Remove(metodo);
        }

        await _context.SaveChangesAsync();
        return NoContent();
    }
}
