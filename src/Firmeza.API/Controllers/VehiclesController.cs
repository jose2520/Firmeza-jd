using AutoMapper;
using Firmeza.Application.DTOs;
using Firmeza.Domain.Entities;
using Firmeza.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.API.Controllers;

/// <summary>
/// Gestiona la flota vehicular y maquinaria de construcción disponible para servicios y alquileres.
/// RUTA BASE: /api/Vehicles
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class VehiclesController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public VehiclesController(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    /// <summary>
    /// Lista los vehículos y maquinaria disponibles en el parque automotor.
    /// ACCESO: Público (para consulta de catálogo y tarifas de alquiler).
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<VehiculoDto>>> GetAll()
    {
        var vehiculos = await _context.Vehiculos
            .AsNoTracking()
            .OrderBy(v => v.Placa)
            .ToListAsync();

        return Ok(_mapper.Map<IEnumerable<VehiculoDto>>(vehiculos));
    }

    /// <summary>
    /// Consulta el detalle de un vehículo por su identificador primario.
    /// ACCESO: Público.
    /// </summary>
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<VehiculoDto>> GetById(int id)
    {
        var vehiculo = await _context.Vehiculos
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.Id == id);

        if (vehiculo == null)
            return NotFound(new { message = $"Vehículo con ID {id} no encontrado." });

        return Ok(_mapper.Map<VehiculoDto>(vehiculo));
    }

    /// <summary>
    /// Registra un nuevo vehículo o maquinaria pesada en el inventario de la empresa.
    /// ACCESO: Requiere rol 'Admin'.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<VehiculoDto>> Create([FromBody] CreateVehiculoDto dto)
    {
        if (await _context.Vehiculos.AnyAsync(v => v.Placa.ToUpper() == dto.Placa.ToUpper()))
            return BadRequest(new { message = $"Ya existe un vehículo registrado con la placa '{dto.Placa}'." });

        var vehiculo = _mapper.Map<Vehiculo>(dto);
        vehiculo.Placa = dto.Placa.ToUpper();
        _context.Vehiculos.Add(vehiculo);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = vehiculo.Id }, _mapper.Map<VehiculoDto>(vehiculo));
    }

    /// <summary>
    /// Actualiza especificaciones, tarifa diaria o disponibilidad del vehículo.
    /// ACCESO: Requiere rol 'Admin'.
    /// </summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<VehiculoDto>> Update(int id, [FromBody] UpdateVehiculoDto dto)
    {
        var vehiculo = await _context.Vehiculos.FindAsync(id);
        if (vehiculo == null)
            return NotFound(new { message = $"Vehículo con ID {id} no encontrado." });

        vehiculo.Marca = dto.Marca;
        vehiculo.Modelo = dto.Modelo;
        vehiculo.TipoVehiculo = dto.TipoVehiculo;
        vehiculo.CapacidadCargaTon = dto.CapacidadCargaTon;
        vehiculo.TarifaDiaria = dto.TarifaDiaria;
        vehiculo.Estado = dto.Estado;
        vehiculo.Activo = dto.Activo;

        await _context.SaveChangesAsync();
        return Ok(_mapper.Map<VehiculoDto>(vehiculo));
    }
}
