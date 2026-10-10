// =================================================================================================
// CONTROLADOR: ClientsController.cs
// CAPA: Firmeza.API (Presentación / Controladores HTTP)
// RUTA BASE: /api/Clients
// PROPÓSITO:
//   Gestiona la cartera de clientes de Firmeza (personas naturales y empresas).
//   Permite consultar, registrar, editar y desactivar clientes con control de duplicados y soft-delete.
//
// ORIGEN DE LOS DATOS:
//   - Todos los datos provienen y se persisten en la base de datos PostgreSQL (tabla 'clientes').
//   - Requiere rol 'Admin' mediante JWT para todas sus operaciones.
// =================================================================================================

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

    /// <summary>
    /// Lista todos los clientes registrados en el sistema.
    /// ACCESO: Protegido (Requiere rol 'Admin').
    /// ORIGEN DE DATOS: Base de datos PostgreSQL (tabla 'clientes' ordenados por ID).
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ClienteDto>>> GetAll()
    {
        var clientes = await _clienteService.GetAllAsync();
        return Ok(clientes);
    }

    /// <summary>
    /// Consulta la información detallada de un cliente por su ID.
    /// ACCESO: Protegido (Requiere rol 'Admin').
    /// ORIGEN DE DATOS: Base de datos PostgreSQL (tabla 'clientes' donde id = {id}).
    /// </summary>
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

    /// <summary>
    /// Registra un nuevo cliente validando que la identificación y el correo no existan previamente.
    /// ACCESO: Protegido (Requiere rol 'Admin').
    /// ORIGEN DE DATOS: Cuerpo JSON recibido en HTTP POST -> Insertado en PostgreSQL (tabla 'clientes').
    /// </summary>
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
            // Responde 400 Bad Request si la identificación o el correo ya se encuentran en uso
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Actualiza la información de un cliente existente.
    /// ACCESO: Protegido (Requiere rol 'Admin').
    /// ORIGEN DE DATOS: Actualiza la fila en PostgreSQL (tabla 'clientes').
    /// </summary>
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

    /// <summary>
    /// Elimina un cliente. Si tiene ventas registradas, aplica borrado lógico (Activo = false) para proteger la integridad financiera.
    /// ACCESO: Protegido (Requiere rol 'Admin').
    /// ORIGEN DE DATOS: Modifica o elimina en PostgreSQL (tabla 'clientes', evaluando si existen registros en 'ventas').
    /// </summary>
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
