using AutoMapper;
using Firmeza.Application.DTOs;
using Firmeza.Domain.Entities;
using Firmeza.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.API.Controllers;

/// <summary>
/// Gestiona los contratos de alquiler de vehículos y maquinaria pesada a clientes (cumplimiento US 4).
/// RUTA BASE: /api/Rentals
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RentalsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public RentalsController(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    /// <summary>
    /// Lista los contratos de alquiler con información del cliente y del vehículo.
    /// ACCESO: Requiere autenticación JWT.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AlquilerDto>>> GetAll()
    {
        var alquileres = await _context.Alquileres
            .AsNoTracking()
            .Include(a => a.Cliente)
            .Include(a => a.Vehiculo)
            .OrderByDescending(a => a.FechaInicio)
            .ToListAsync();

        return Ok(_mapper.Map<IEnumerable<AlquilerDto>>(alquileres));
    }

    /// <summary>
    /// Consulta el detalle de un contrato de alquiler por su identificador primario.
    /// ACCESO: Requiere autenticación JWT.
    /// </summary>
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

        return Ok(_mapper.Map<AlquilerDto>(alquiler));
    }

    /// <summary>
    /// Registra un nuevo contrato de alquiler de maquinaria o vehículo.
    /// Actualiza automáticamente el estado del vehículo a 'Alquilado'.
    /// ACCESO: Requiere rol 'Admin'.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<AlquilerDto>> Create([FromBody] CreateAlquilerDto dto)
    {
        var cliente = await _context.Clientes.FindAsync(dto.ClienteId);
        if (cliente == null)
            return BadRequest(new { message = $"El cliente con ID {dto.ClienteId} no existe." });

        var vehiculo = await _context.Vehiculos.FindAsync(dto.VehiculoId);
        if (vehiculo == null)
            return BadRequest(new { message = $"El vehículo con ID {dto.VehiculoId} no existe." });

        if (vehiculo.Estado != "Disponible")
            return BadRequest(new { message = $"El vehículo con placa '{vehiculo.Placa}' no está disponible (Estado actual: '{vehiculo.Estado}')." });

        if (await _context.Alquileres.AnyAsync(a => a.NumeroContrato == dto.NumeroContrato))
            return BadRequest(new { message = $"Ya existe un contrato registrado con el número '{dto.NumeroContrato}'." });

        var alquiler = _mapper.Map<Alquiler>(dto);
        alquiler.Total = dto.DiasAlquiler * dto.ValorDiario;
        alquiler.Estado = "Activo";
        alquiler.CreadoEn = DateTime.UtcNow;

        vehiculo.Estado = "Alquilado";

        _context.Alquileres.Add(alquiler);
        await _context.SaveChangesAsync();

        // Cargar navegaciones para respuesta DTO completa
        await _context.Entry(alquiler).Reference(a => a.Cliente).LoadAsync();
        await _context.Entry(alquiler).Reference(a => a.Vehiculo).LoadAsync();

        return CreatedAtAction(nameof(GetById), new { id = alquiler.Id }, _mapper.Map<AlquilerDto>(alquiler));
    }

    /// <summary>
    /// Modifica el estado del contrato de alquiler ('Finalizado', 'Cancelado').
    /// Si finaliza, libera el vehículo volviéndolo a marcar como 'Disponible'.
    /// ACCESO: Requiere rol 'Admin'.
    /// </summary>
    [HttpPatch("{id:int}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<AlquilerDto>> UpdateStatus(int id, [FromBody] string nuevoEstado)
    {
        var alquiler = await _context.Alquileres
            .Include(a => a.Cliente)
            .Include(a => a.Vehiculo)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (alquiler == null)
            return NotFound(new { message = $"Contrato de alquiler con ID {id} no encontrado." });

        alquiler.Estado = nuevoEstado;

        if (nuevoEstado == "Finalizado" || nuevoEstado == "Cancelado")
        {
            if (alquiler.Vehiculo != null)
            {
                alquiler.Vehiculo.Estado = "Disponible";
            }
        }

        await _context.SaveChangesAsync();
        return Ok(_mapper.Map<AlquilerDto>(alquiler));
    }
}
