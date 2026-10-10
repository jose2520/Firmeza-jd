using AutoMapper;
using Firmeza.Application.DTOs;
using Firmeza.Domain.Entities;
using Firmeza.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.API.Controllers;

/// <summary>
/// Gestiona las familias y categorías del catálogo ferretero, materiales y maquinaria de construcción.
/// RUTA BASE: /api/Categories
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public CategoriesController(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    /// <summary>
    /// Lista las categorías del catálogo con soporte para filtros por estado activo y búsqueda por texto.
    /// ACCESO: Público.
    /// </summary>
    /// <param name="soloActivas">Indica si solo se deben listar las categorías activas (por defecto true).</param>
    /// <param name="busqueda">Término de búsqueda opcional por nombre o descripción.</param>
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

    /// <summary>
    /// Consulta el detalle de una categoría por su ID incluyendo el conteo de artículos vinculados.
    /// ACCESO: Público.
    /// </summary>
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

    /// <summary>
    /// Consulta todos los productos y materiales pertenecientes a una categoría específica.
    /// ACCESO: Público.
    /// </summary>
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
            .Where(p => p.CategoriaId == id && p.Activo)
            .OrderBy(p => p.Nombre)
            .ToListAsync();

        return Ok(_mapper.Map<IEnumerable<ProductoDto>>(productos));
    }

    /// <summary>
    /// Crea una nueva categoría o línea comercial en el catálogo.
    /// ACCESO: Requiere rol 'Admin'.
    /// </summary>
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

    /// <summary>
    /// Actualiza una categoría existente.
    /// ACCESO: Requiere rol 'Admin'.
    /// </summary>
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

    /// <summary>
    /// Elimina o desactiva una categoría. Si posee productos asignados, aplica borrado lógico (Activo = false)
    /// para proteger la integridad referencial del catálogo.
    /// ACCESO: Requiere rol 'Admin'.
    /// </summary>
    /// <param name="id">ID de la categoría.</param>
    /// <param name="fisico">Si es true y no tiene productos, elimina la fila físicamente.</param>
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
            // Protección de integridad referencial: Borrado lógico
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

    /// <summary>
    /// Alterna rápidamente el estado activo/inactivo de una categoría.
    /// ACCESO: Requiere rol 'Admin'.
    /// </summary>
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
