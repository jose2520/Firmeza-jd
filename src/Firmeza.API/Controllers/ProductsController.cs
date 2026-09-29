using Firmeza.Application.DTOs;
using Firmeza.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Firmeza.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductoService _productoService;

    public ProductsController(IProductoService productoService)
    {
        _productoService = productoService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductoDto>>> GetAll()
    {
        var productos = await _productoService.GetAllAsync();
        return Ok(productos);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductoDto>> GetById(int id)
    {
        var producto = await _productoService.GetByIdAsync(id);
        if (producto == null)
        {
            return NotFound(new { message = $"Producto con ID {id} no encontrado." });
        }
        return Ok(producto);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ProductoDto>> Create([FromBody] CreateProductoDto dto)
    {
        var nuevo = await _productoService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = nuevo.Id }, nuevo);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ProductoDto>> Update(int id, [FromBody] UpdateProductoDto dto)
    {
        var actualizado = await _productoService.UpdateAsync(id, dto);
        if (actualizado == null)
        {
            return NotFound(new { message = $"Producto con ID {id} no encontrado para actualizar." });
        }
        return Ok(actualizado);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var eliminado = await _productoService.DeleteAsync(id);
        if (!eliminado)
        {
            return NotFound(new { message = $"Producto con ID {id} no encontrado para eliminar." });
        }
        return NoContent();
    }
}
