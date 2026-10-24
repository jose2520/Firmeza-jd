// Controlador encargado de gestionar la cartera de clientes de Firmeza.
// Permite a los administradores consultar, registrar, actualizar y dar de baja clientes
// verificando que no se repitan números de documento ni correos electrónicos.

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

    // Recibe el servicio que contiene la lógica de negocio para gestionar clientes.
    public ClientsController(IClienteService clienteService)
    {
        _clienteService = clienteService;
    }

    // Devuelve la lista completa de todos los clientes registrados en el sistema.
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ClienteDto>>> GetAll()
    {
        var clientes = await _clienteService.GetAllAsync();
        return Ok(clientes);
    }

    // Busca y devuelve los datos detallados de un cliente por su identificador.
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

    // Registra un nuevo cliente comprobando que su identificación y correo no estén repetidos.
    [HttpPost]
    public async Task<ActionResult<ClienteDto>> Create([FromBody] CreateClienteDto dto)
    {
        try
        {
            var nuevo = await _clienteService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = nuevo.Id }, nuevo);
        }
        catch (InvalidOperationException ex)
        {
            // Devuelve un mensaje de error si la identificación o el correo ya están en uso
            return BadRequest(new { message = ex.Message });
        }
    }

    // Actualiza los datos personales y de contacto de un cliente existente.
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ClienteDto>> Update(int id, [FromBody] UpdateClienteDto dto)
    {
        try
        {
            var actualizado = await _clienteService.UpdateAsync(id, dto);
            if (actualizado == null)
            {
                return NotFound(new { message = $"Cliente con ID {id} no encontrado para actualizar." });
            }
            return Ok(actualizado);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // Elimina un cliente o lo desactiva si ya tiene compras registradas para conservar el historial de ventas.
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
