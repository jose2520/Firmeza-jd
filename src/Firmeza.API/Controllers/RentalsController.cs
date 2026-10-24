// Controlador para gestionar los contratos de alquiler de vehículos y maquinaria pesada.
// Permite consultar los alquileres registrados, crear nuevos contratos cambiando el estado del
// vehículo a alquilado y liberar el vehículo cuando el contrato finaliza o se cancela.

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
public class RentalsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    private static readonly string[] ValidRentalStates = { "Activo", "Finalizado", "Cancelado" };

    // Inicializa la conexión a la base de datos y el convertidor de datos de alquileres.
    public RentalsController(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    // Verifica si el usuario conectado es únicamente un cliente para mostrarle solo sus propios contratos de alquiler.
    private bool IsClientOnlyUser()
    {
        return User.Identity is { IsAuthenticated: true } &&
               User.IsInRole("Cliente") &&
               !User.IsInRole("Admin") &&
               !User.IsInRole("Supervisor") &&
               !User.IsInRole("Vendedor") &&
               !User.IsInRole("Gerente");
    }

    // Devuelve la lista de contratos de alquiler junto con los datos del cliente y del vehículo.
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AlquilerDto>>> GetAll()
    {
        var query = _context.Alquileres
            .AsNoTracking()
            .Include(a => a.Cliente)
            .Include(a => a.Vehiculo)
            .AsQueryable();

        if (IsClientOnlyUser())
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            query = query.Where(a =>
                (userId != null && a.Cliente.UserId == userId) ||
                (userEmail != null && a.Cliente.Email.ToLower() == userEmail.ToLower()));
        }

        var alquileres = await query
            .OrderByDescending(a => a.FechaInicio)
            .ToListAsync();

        return Ok(_mapper.Map<IEnumerable<AlquilerDto>>(alquileres));
    }

    // Consulta el detalle de un contrato de alquiler por su identificador, validando que pertenezca al cliente si aplica.
    [HttpGet("{id:int}")]
    public async Task<ActionResult<AlquilerDto>> GetById(int id)
    {
        var alquiler = await _context.Alquileres
            .AsNoTracking()
            .Include(a => a.Cliente)
            .Include(a => a.Vehiculo)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (alquiler == null)
            return NotFound(new { message = $"Contrato de alquiler con ID {id} no encontrado." });

        if (IsClientOnlyUser())
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var isOwner = (!string.IsNullOrWhiteSpace(userId) && alquiler.Cliente.UserId == userId) ||
                          (!string.IsNullOrWhiteSpace(userEmail) && string.Equals(alquiler.Cliente.Email, userEmail, StringComparison.OrdinalIgnoreCase));
            if (!isOwner)
                return Forbid();
        }

        return Ok(_mapper.Map<AlquilerDto>(alquiler));
    }

    // Crea un nuevo contrato de alquiler verificando que el vehículo esté disponible y marca dicho vehículo como 'Alquilado'.
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<AlquilerDto>> Create([FromBody] CreateAlquilerDto dto)
    {
        if (dto.DiasAlquiler <= 0)
            return BadRequest(new { message = "La duración del alquiler debe ser de al menos 1 día." });

        if (dto.ValorDiario < 0)
            return BadRequest(new { message = "El valor diario del alquiler no puede ser negativo." });

        if (dto.FechaFin <= dto.FechaInicio)
            return BadRequest(new { message = "La fecha de finalización debe ser posterior a la fecha de inicio." });

        var cliente = await _context.Clientes.FindAsync(dto.ClienteId);
        if (cliente == null || !cliente.Activo)
            return BadRequest(new { message = $"El cliente con ID {dto.ClienteId} no existe o se encuentra inactivo." });

        var vehiculo = await _context.Vehiculos.FindAsync(dto.VehiculoId);
        if (vehiculo == null || !vehiculo.Activo)
            return BadRequest(new { message = $"El vehículo con ID {dto.VehiculoId} no existe o se encuentra inactivo." });

        if (vehiculo.Estado != "Disponible")
            return BadRequest(new { message = $"El vehículo con placa '{vehiculo.Placa}' no está disponible (Estado actual: '{vehiculo.Estado}')." });

        if (await _context.Alquileres.AnyAsync(a => a.NumeroContrato == dto.NumeroContrato))
            return BadRequest(new { message = $"Ya existe un contrato registrado con el número '{dto.NumeroContrato}'." });

        var alquiler = _mapper.Map<Alquiler>(dto);
        alquiler.Vehiculo = vehiculo;
        alquiler.Cliente = cliente;
        alquiler.CreadoEn = DateTime.UtcNow;
        alquiler.IniciarAlquiler();

        vehiculo.Estado = "Alquilado";

        _context.Alquileres.Add(alquiler);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = alquiler.Id }, _mapper.Map<AlquilerDto>(alquiler));
    }

    // Actualiza el estado del contrato ('Activo', 'Finalizado' o 'Cancelado') y vuelve a dejar disponible el vehículo al terminar.
    [HttpPatch("{id:int}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<AlquilerDto>> UpdateStatus(int id, [FromBody] string nuevoEstado)
    {
        if (string.IsNullOrWhiteSpace(nuevoEstado) || !ValidRentalStates.Contains(nuevoEstado))
            return BadRequest(new { message = "El estado debe ser 'Activo', 'Finalizado' o 'Cancelado'." });

        var alquiler = await _context.Alquileres
            .Include(a => a.Cliente)
            .Include(a => a.Vehiculo)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (alquiler == null)
            return NotFound(new { message = $"Contrato de alquiler con ID {id} no encontrado." });

        if (nuevoEstado == "Finalizado" && alquiler.Estado != "Finalizado")
        {
            alquiler.FinalizarAlquiler();
        }
        else
        {
            alquiler.Estado = nuevoEstado;
        }

        if (nuevoEstado == "Finalizado" || nuevoEstado == "Cancelado")
        {
            alquiler.Vehiculo.Estado = "Disponible";
        }

        await _context.SaveChangesAsync();
        return Ok(_mapper.Map<AlquilerDto>(alquiler));
    }
}
