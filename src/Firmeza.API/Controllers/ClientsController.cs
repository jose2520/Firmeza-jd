using Firmeza.Application.DTOs;
using Firmeza.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Firmeza.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class ClientsController : ControllerBase
{
    private readonly IClienteService _clienteService;

    public ClientsController(IClienteService clienteService)
    {
        _clienteService = clienteService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ClienteDto>>> GetAll()
    {
        var clientes = await _clienteService.GetAllAsync();
        return Ok(clientes);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ClienteDto>> GetById(int id)
    {
        var cliente = await _clienteService.GetByIdAsync(id);
        if (cliente == null)
        {
            return NotFound(new { message = $"Cliente con ID {id} no encontrado." });
        }
        return Ok(cliente);
    }

    [HttpPost]
    public async Task<ActionResult<ClienteDto>> Create([FromBody] CreateClienteDto dto)
    {
        var nuevo = await _clienteService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = nuevo.Id }, nuevo);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ClienteDto>> Update(int id, [FromBody] UpdateClienteDto dto)
    {
        var actualizado = await _clienteService.UpdateAsync(id, dto);
        if (actualizado == null)
        {
            return NotFound(new { message = $"Cliente con ID {id} no encontrado para actualizar." });
        }
        return Ok(actualizado);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var eliminado = await _clienteService.DeleteAsync(id);
        if (!eliminado)
        {
            return NotFound(new { message = $"Cliente con ID {id} no encontrado para eliminar." });
        }
        return NoContent();
    }
}
