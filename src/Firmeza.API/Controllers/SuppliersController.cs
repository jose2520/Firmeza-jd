using AutoMapper;
using Firmeza.Application.DTOs;
using Firmeza.Domain.Entities;
using Firmeza.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.API.Controllers;

/// <summary>
/// Gestiona los proveedores y fabricantes de materiales de construcción y ferretería.
/// RUTA BASE: /api/Suppliers
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class SuppliersController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public SuppliersController(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    /// <summary>
    /// Lista todos los proveedores registrados.
    /// ACCESO: Requiere rol 'Admin'.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProveedorDto>>> GetAll()
    {
        var proveedores = await _context.Proveedores
            .AsNoTracking()
            .OrderBy(p => p.RazonSocial)
            .ToListAsync();

        return Ok(_mapper.Map<IEnumerable<ProveedorDto>>(proveedores));
    }

    /// <summary>
    /// Consulta el detalle de un proveedor por su ID.
    /// ACCESO: Requiere rol 'Admin'.
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProveedorDto>> GetById(int id)
    {
        var proveedor = await _context.Proveedores
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);

        if (proveedor == null)
            return NotFound(new { message = $"Proveedor con ID {id} no encontrado." });

        return Ok(_mapper.Map<ProveedorDto>(proveedor));
    }

    /// <summary>
    /// Registra un nuevo proveedor aliado.
    /// ACCESO: Requiere rol 'Admin'.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ProveedorDto>> Create([FromBody] CreateProveedorDto dto)
    {
        if (await _context.Proveedores.AnyAsync(p => p.Nit == dto.Nit))
            return BadRequest(new { message = $"Ya existe un proveedor registrado con el NIT '{dto.Nit}'." });

        var proveedor = _mapper.Map<Proveedor>(dto);
        _context.Proveedores.Add(proveedor);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = proveedor.Id }, _mapper.Map<ProveedorDto>(proveedor));
    }

    /// <summary>
    /// Actualiza la información de contacto o fiscal de un proveedor.
    /// ACCESO: Requiere rol 'Admin'.
    /// </summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ProveedorDto>> Update(int id, [FromBody] UpdateProveedorDto dto)
    {
        var proveedor = await _context.Proveedores.FindAsync(id);
        if (proveedor == null)
            return NotFound(new { message = $"Proveedor con ID {id} no encontrado." });

        if (await _context.Proveedores.AnyAsync(p => p.Nit == dto.Nit && p.Id != id))
            return BadRequest(new { message = $"Ya existe otro proveedor registrado con el NIT '{dto.Nit}'." });

        proveedor.Nit = dto.Nit;
        proveedor.RazonSocial = dto.RazonSocial;
        proveedor.ContactoNombre = dto.ContactoNombre;
        proveedor.Telefono = dto.Telefono;
        proveedor.Email = dto.Email;
        proveedor.Direccion = dto.Direccion;
        proveedor.Ciudad = dto.Ciudad;
        proveedor.Activo = dto.Activo;

        await _context.SaveChangesAsync();
        return Ok(_mapper.Map<ProveedorDto>(proveedor));
    }
}
