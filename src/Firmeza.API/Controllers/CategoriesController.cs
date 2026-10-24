// Controlador para administrar las categorías de productos y materiales de construcción.
// Permite listar y buscar categorías de forma pública, así como crear, editar y desactivar
// categorías cuando el usuario tiene permisos de administrador.

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
public class CategoriesController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    // Recibe la conexión a la base de datos y el convertidor de datos para las respuestas.
    public CategoriesController(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    // Obtiene la lista de categorías del catálogo, permitiendo filtrar solo las activas o buscar por texto.
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<CategoriaDto>>> GetAll(
        [FromQuery] bool? soloActivas = true,
        [FromQuery] string? busqueda = null)
    {
        var query = _context.Categorias
            .AsNoTracking()
            .Include(c => c.Productos)
            .AsQueryable();

        if (soloActivas.HasValue && soloActivas.Value)
        {
            query = query.Where(c => c.Activo);
        }

        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            var filtro = busqueda.Trim().ToLower();
            query = query.Where(c => c.Nombre.ToLower().Contains(filtro) ||
                                     (c.Descripcion != null && c.Descripcion.ToLower().Contains(filtro)));
        }

        var categorias = await query
            .OrderBy(c => c.Nombre)
            .ToListAsync();

        return Ok(_mapper.Map<IEnumerable<CategoriaDto>>(categorias));
    }

    // Busca y devuelve los datos de una categoría específica según su número identificador.
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<CategoriaDto>> GetById(int id)
    {
        var categoria = await _context.Categorias
            .AsNoTracking()
            .Include(c => c.Productos)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (categoria == null)
            return NotFound(new { message = $"Categoría con ID {id} no encontrada." });

        return Ok(_mapper.Map<CategoriaDto>(categoria));
    }

    // Devuelve todos los productos activos que pertenecen a una categoría determinada.
    [HttpGet("{id:int}/products")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<ProductoDto>>> GetProducts(int id)
    {
        var existeCategoria = await _context.Categorias.AnyAsync(c => c.Id == id);
        if (!existeCategoria)
            return NotFound(new { message = $"Categoría con ID {id} no encontrada." });

        var productos = await _context.Productos
            .AsNoTracking()
            .Include(p => p.Categoria)
            .Include(p => p.Proveedor)
            .Where(p => p.CategoriaId == id && p.Activo)
            .OrderBy(p => p.Nombre)
            .ToListAsync();

        return Ok(_mapper.Map<IEnumerable<ProductoDto>>(productos));
    }

    // Registra una nueva categoría verificando antes que no exista otra con el mismo nombre.
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<CategoriaDto>> Create([FromBody] CreateCategoriaDto dto)
    {
        if (await _context.Categorias.AnyAsync(c => c.Nombre.ToLower() == dto.Nombre.ToLower()))
            return BadRequest(new { message = $"Ya existe una categoría con el nombre '{dto.Nombre}'." });

        var categoria = _mapper.Map<Categoria>(dto);
        _context.Categorias.Add(categoria);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = categoria.Id }, _mapper.Map<CategoriaDto>(categoria));
    }

    // Modifica el nombre, descripción o estado de una categoría existente.
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<CategoriaDto>> Update(int id, [FromBody] UpdateCategoriaDto dto)
    {
        var categoria = await _context.Categorias
            .Include(c => c.Productos)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (categoria == null)
            return NotFound(new { message = $"Categoría con ID {id} no encontrada." });

        if (await _context.Categorias.AnyAsync(c => c.Nombre.ToLower() == dto.Nombre.ToLower() && c.Id != id))
            return BadRequest(new { message = $"Ya existe otra categoría con el nombre '{dto.Nombre}'." });

        categoria.Nombre = dto.Nombre;
        categoria.Descripcion = dto.Descripcion;
        categoria.Activo = dto.Activo;

        await _context.SaveChangesAsync();
        return Ok(_mapper.Map<CategoriaDto>(categoria));
    }

    // Elimina o desactiva una categoría; si ya tiene productos asociados, solo la marca como inactiva para no perder historial.
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id, [FromQuery] bool fisico = false)
    {
        var categoria = await _context.Categorias
            .Include(c => c.Productos)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (categoria == null)
            return NotFound(new { message = $"Categoría con ID {id} no encontrada para eliminar." });

        if (categoria.Productos.Any())
        {
            // Desactivación segura cuando existen productos vinculados
            categoria.Activo = false;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = $"La categoría '{categoria.Nombre}' tiene {categoria.Productos.Count} producto(s) asociado(s). Se aplicó borrado lógico (Activo = false).",
                categoria = _mapper.Map<CategoriaDto>(categoria)
            });
        }

        if (fisico)
        {
            _context.Categorias.Remove(categoria);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        categoria.Activo = false;
        await _context.SaveChangesAsync();
        return Ok(new
        {
            message = $"La categoría '{categoria.Nombre}' ha sido desactivada exitosamente.",
            categoria = _mapper.Map<CategoriaDto>(categoria)
        });
    }

    // Cambia rápidamente el estado de una categoría entre activa e inactiva.
    [HttpPatch("{id:int}/toggle-status")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<CategoriaDto>> ToggleStatus(int id)
    {
        var categoria = await _context.Categorias
            .Include(c => c.Productos)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (categoria == null)
            return NotFound(new { message = $"Categoría con ID {id} no encontrada." });

        categoria.Activo = !categoria.Activo;
        await _context.SaveChangesAsync();

        return Ok(_mapper.Map<CategoriaDto>(categoria));
    }
}
