// Controlador para administrar el directorio de proveedores y fabricantes aliados de Firmeza.
// Permite a los administradores consultar, registrar, actualizar y dar de baja proveedores
// validando que el número de identificación tributaria (NIT) sea único.

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
[Authorize(Roles = "Admin")]
public class SuppliersController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    // Recibe la conexión a la base de datos y el convertidor de datos de proveedores.
    public SuppliersController(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    // Devuelve la lista completa de todos los proveedores ordenados alfabéticamente por su razón social.
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProveedorDto>>> GetAll()
    {
        var proveedores = await _context.Proveedores
            .AsNoTracking()
            .OrderBy(p => p.RazonSocial)
            .ToListAsync();

        return Ok(_mapper.Map<IEnumerable<ProveedorDto>>(proveedores));
    }

    // Busca y devuelve la información detallada de un proveedor por su identificador.
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

    // Registra un nuevo proveedor verificando que no exista otro con el mismo número NIT.
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

    // Actualiza los datos comerciales y de contacto de un proveedor existente.
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

    // Elimina un proveedor o lo marca como inactivo si ya tiene productos asociados en el catálogo.
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var proveedor = await _context.Proveedores.FindAsync(id);
        if (proveedor == null)
            return NotFound(new { message = $"Proveedor con ID {id} no encontrado." });

        var tieneProductos = await _context.Productos.AnyAsync(p => p.ProveedorId == id);
        if (tieneProductos)
        {
            proveedor.Activo = false;
        }
        else
        {
            _context.Proveedores.Remove(proveedor);
        }

        await _context.SaveChangesAsync();
        return NoContent();
    }
}
