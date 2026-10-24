// Controlador para administrar la flota de vehículos de carga y maquinaria pesada de construcción.
// Permite consultar de forma pública los equipos y sus tarifas por día, así como registrar,
// actualizar o retirar vehículos cuando se cuenta con permisos de administrador.

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
public class VehiclesController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    // Recibe la conexión a la base de datos y el convertidor de datos de vehículos.
    public VehiclesController(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    // Devuelve la lista de todos los vehículos y maquinaria ordenados por número de placa.
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

    // Busca y devuelve los datos de un vehículo específico según su identificador.
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

    // Registra un nuevo vehículo o maquinaria comprobando que su placa no esté repetida y sus tarifas sean válidas.
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<VehiculoDto>> Create([FromBody] CreateVehiculoDto dto)
    {
        if (dto.TarifaDiaria < 0 || dto.CapacidadCargaTon < 0)
            return BadRequest(new { message = "La tarifa diaria y la capacidad de carga no pueden ser negativas." });

        if (await _context.Vehiculos.AnyAsync(v => v.Placa.ToUpper() == dto.Placa.ToUpper()))
            return BadRequest(new { message = $"Ya existe un vehículo registrado con la placa '{dto.Placa}'." });

        var vehiculo = _mapper.Map<Vehiculo>(dto);
        vehiculo.Placa = dto.Placa.ToUpper();
        _context.Vehiculos.Add(vehiculo);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = vehiculo.Id }, _mapper.Map<VehiculoDto>(vehiculo));
    }

    // Actualiza la marca, modelo, capacidad, tarifa diaria o disponibilidad de un vehículo existente.
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<VehiculoDto>> Update(int id, [FromBody] UpdateVehiculoDto dto)
    {
        if (dto.TarifaDiaria < 0 || dto.CapacidadCargaTon < 0)
            return BadRequest(new { message = "La tarifa diaria y la capacidad de carga no pueden ser negativas." });

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

    // Elimina un vehículo o lo marca como inactivo si ya tiene contratos de alquiler asociados en el historial.
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var vehiculo = await _context.Vehiculos.FindAsync(id);
        if (vehiculo == null)
            return NotFound(new { message = $"Vehículo con ID {id} no encontrado." });

        var tieneAlquileres = await _context.Alquileres.AnyAsync(a => a.VehiculoId == id);
        if (tieneAlquileres)
        {
            vehiculo.Activo = false;
        }
        else
        {
            _context.Vehiculos.Remove(vehiculo);
        }

        await _context.SaveChangesAsync();
        return NoContent();
    }
}
