// Controlador para gestionar los envíos y entregas de materiales a las obras de construcción.
// Permite consultar el estado de los despachos, programar nuevas entregas asociadas a una venta
// y actualizar su avance hasta que el pedido es entregado.

using System.Security.Claims;
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
[Authorize]
public class DispatchesController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    private static readonly string[] ValidDispatchStates = { "Pendiente", "En Ruta", "Entregado", "Cancelado" };

    // Inicializa el acceso a la base de datos y el convertidor de datos para despachos.
    public DispatchesController(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    // Comprueba si el usuario conectado tiene únicamente el rol de cliente para mostrarle solo sus propios envíos.
    private bool IsClientOnlyUser()
    {
        return User.Identity is { IsAuthenticated: true } &&
               User.IsInRole("Cliente") &&
               !User.IsInRole("Admin") &&
               !User.IsInRole("Supervisor") &&
               !User.IsInRole("Vendedor") &&
               !User.IsInRole("Gerente") &&
               !User.IsInRole("Almacenista");
    }

    // Devuelve la lista de despachos programados; si quien consulta es un cliente, solo devuelve los suyos.
    [HttpGet]
    public async Task<ActionResult<IEnumerable<DespachoDto>>> GetAll()
    {
        var query = _context.Despachos
            .AsNoTracking()
            .AsQueryable();

        if (IsClientOnlyUser())
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            query = query
                .Include(d => d.Venta)
                    .ThenInclude(v => v.Cliente)
                .Where(d =>
                    (userId != null && d.Venta.Cliente.UserId == userId) ||
                    (userEmail != null && d.Venta.Cliente.Email.ToLower() == userEmail.ToLower()));
        }

        var despachos = await query
            .OrderByDescending(d => d.FechaEstimada)
            .ToListAsync();

        return Ok(_mapper.Map<IEnumerable<DespachoDto>>(despachos));
    }

    // Consulta la información de un despacho específico por su identificador, verificando que pertenezca al cliente si aplica.
    [HttpGet("{id:int}")]
    public async Task<ActionResult<DespachoDto>> GetById(int id)
    {
        var despacho = await _context.Despachos
            .AsNoTracking()
            .Include(d => d.Venta)
                .ThenInclude(v => v.Cliente)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (despacho == null)
            return NotFound(new { message = $"Despacho con ID {id} no encontrado." });

        if (IsClientOnlyUser())
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var cliente = despacho.Venta.Cliente;
            var isOwner = (!string.IsNullOrWhiteSpace(userId) && cliente.UserId == userId) ||
                          (!string.IsNullOrWhiteSpace(userEmail) && string.Equals(cliente.Email, userEmail, StringComparison.OrdinalIgnoreCase));
            if (!isOwner)
                return Forbid();
        }

        return Ok(_mapper.Map<DespachoDto>(despacho));
    }

    // Busca los datos de entrega asociados a una venta específica mediante el identificador de la venta.
    [HttpGet("sale/{saleId:int}")]
    public async Task<ActionResult<DespachoDto>> GetBySaleId(int saleId)
    {
        var despacho = await _context.Despachos
            .AsNoTracking()
            .Include(d => d.Venta)
                .ThenInclude(v => v.Cliente)
            .FirstOrDefaultAsync(d => d.VentaId == saleId);

        if (despacho == null)
            return NotFound(new { message = $"No se encontró orden de despacho para la venta con ID {saleId}." });

        if (IsClientOnlyUser())
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var cliente = despacho.Venta.Cliente;
            var isOwner = (!string.IsNullOrWhiteSpace(userId) && cliente.UserId == userId) ||
                          (!string.IsNullOrWhiteSpace(userEmail) && string.Equals(cliente.Email, userEmail, StringComparison.OrdinalIgnoreCase));
            if (!isOwner)
                return Forbid();
        }

        return Ok(_mapper.Map<DespachoDto>(despacho));
    }

    // Programa una nueva orden de entrega en obra para una venta registrada.
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<DespachoDto>> Create([FromBody] CreateDespachoDto dto)
    {
        if (dto.CostoFlete < 0)
            return BadRequest(new { message = "El costo del flete no puede ser negativo." });

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

    // Cambia el estado del envío ('Pendiente', 'En Ruta', 'Entregado' o 'Cancelado') y guarda la fecha real al entregarse.
    [HttpPatch("{id:int}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<DespachoDto>> UpdateStatus(int id, [FromBody] string nuevoEstado)
    {
        if (string.IsNullOrWhiteSpace(nuevoEstado) || !ValidDispatchStates.Contains(nuevoEstado))
            return BadRequest(new { message = "El estado del despacho debe ser 'Pendiente', 'En Ruta', 'Entregado' o 'Cancelado'." });

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
