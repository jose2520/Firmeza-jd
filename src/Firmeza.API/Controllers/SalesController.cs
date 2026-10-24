// Controlador principal para gestionar las ventas y la facturación del negocio.
// Permite registrar compras descontando existencias, calcular el impuesto del 19%, anular ventas
// devolviendo los productos a bodega, descargar facturas en PDF y exportar reportes en Excel.

using System.Security.Claims;
using Firmeza.Application.DTOs;
using Firmeza.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Firmeza.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Exige haber iniciado sesión para usar cualquiera de sus rutas
public class SalesController : ControllerBase
{
    private readonly IVentaService _ventaService;
    private readonly IPdfService _pdfService;
    private readonly IExcelService _excelService;
    private readonly IEmailService _emailService;
    private readonly IClienteService _clienteService;

    private static readonly string[] InternalRoles =
    {
        "Admin", "Supervisor", "Vendedor", "Gerente", "Cajero", "Contador", "Auditor", "Almacenista", "SoporteTecnico"
    };

    // Recibe los servicios de ventas, generación de PDF, reportes en Excel, envío de correos y consulta de clientes.
    public SalesController(
        IVentaService ventaService,
        IPdfService pdfService,
        IExcelService excelService,
        IEmailService emailService,
        IClienteService clienteService)
    {
        _ventaService = ventaService;
        _pdfService = pdfService;
        _excelService = excelService;
        _emailService = emailService;
        _clienteService = clienteService;
    }

    // Identifica si el usuario actual tiene únicamente el rol de cliente y ningún rol de empleado interno.
    private bool IsRestrictedClientUser()
    {
        if (User.Identity is not { IsAuthenticated: true })
            return false;

        foreach (var role in InternalRoles)
        {
            if (User.IsInRole(role))
                return false;
        }

        return User.IsInRole("Cliente");
    }

    // Verifica que un usuario con rol de cliente solo pueda consultar o facturar a nombre de su propia cuenta.
    private async Task<bool> IsClientOwnerAsync(int clienteId)
    {
        if (!IsRestrictedClientUser())
            return true;

        var cliente = await _clienteService.GetByIdAsync(clienteId);
        if (cliente == null)
            return false;

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userEmail = User.FindFirstValue(ClaimTypes.Email);

        if (!string.IsNullOrWhiteSpace(userId) && string.Equals(cliente.UserId, userId, StringComparison.OrdinalIgnoreCase))
            return true;

        if (!string.IsNullOrWhiteSpace(userEmail) && string.Equals(cliente.Email, userEmail, StringComparison.OrdinalIgnoreCase))
            return true;

        return false;
    }

    // Devuelve el listado de ventas; si quien consulta es un cliente, filtra la lista para mostrar solo sus compras.
    [HttpGet]
    public async Task<ActionResult<IEnumerable<VentaDto>>> GetAll()
    {
        var ventas = await _ventaService.GetAllAsync();

        if (IsRestrictedClientUser())
        {
            var clientes = await _clienteService.GetAllAsync();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userEmail = User.FindFirstValue(ClaimTypes.Email);

            var ownedClientIds = clientes
                .Where(c => (!string.IsNullOrWhiteSpace(userId) && string.Equals(c.UserId, userId, StringComparison.OrdinalIgnoreCase)) ||
                            (!string.IsNullOrWhiteSpace(userEmail) && string.Equals(c.Email, userEmail, StringComparison.OrdinalIgnoreCase)))
                .Select(c => c.Id)
                .ToHashSet();

            ventas = ventas.Where(v => ownedClientIds.Contains(v.ClienteId));
        }

        return Ok(ventas);
    }

    // Consulta toda la información de una venta por su identificador, asegurando que un cliente no vea compras ajenas.
    [HttpGet("{id:int}")]
    public async Task<ActionResult<VentaDto>> GetById(int id)
    {
        var venta = await _ventaService.GetByIdAsync(id);
        if (venta == null)
        {
            return NotFound(new { message = $"Venta con ID {id} no encontrada." });
        }

        if (!await IsClientOwnerAsync(venta.ClienteId))
        {
            return Forbid();
        }

        return Ok(venta);
    }

    // Devuelve únicamente la lista de productos y cantidades compradas dentro de una venta específica.
    [HttpGet("{id:int}/details")]
    public async Task<ActionResult<IEnumerable<DetalleVentaDto>>> GetDetails(int id)
    {
        var venta = await _ventaService.GetByIdAsync(id);
        if (venta == null)
        {
            return NotFound(new { message = $"Venta con ID {id} no encontrada." });
        }

        if (!await IsClientOwnerAsync(venta.ClienteId))
        {
            return Forbid();
        }

        return Ok(venta.Detalles);
    }

    // Registra una nueva venta, descuenta los productos del inventario y envía la factura en PDF al correo del cliente.
    [HttpPost]
    public async Task<ActionResult<VentaDto>> Create([FromBody] CreateVentaDto dto)
    {
        try
        {
            if (!await IsClientOwnerAsync(dto.ClienteId))
            {
                return Forbid();
            }

            var venta = await _ventaService.CreateAsync(dto);

            // Genera el PDF de la factura y lo envía por correo al comprador
            try
            {
                var pdfBytes = await _pdfService.GenerateSaleInvoicePdfAsync(venta.Id);
                var cliente = await _clienteService.GetByIdAsync(venta.ClienteId);
                if (cliente != null && !string.IsNullOrWhiteSpace(cliente.Email))
                {
                    var nombreCompleto = $"{cliente.Nombres} {cliente.Apellidos}".Trim();
                    await _emailService.SendInvoiceEmailAsync(cliente.Email, nombreCompleto, venta.NumeroVenta, venta.Total, pdfBytes);
                }
            }
            catch
            {
                // Si el envío de correo falla, la venta ya registrada se conserva sin inconvenientes
            }

            return CreatedAtAction(nameof(GetById), new { id = venta.Id }, venta);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "Error interno al procesar la venta." });
        }
    }

    // Anula una venta completada y devuelve automáticamente las unidades vendidas al inventario.
    [HttpPut("{id:int}/cancel")]
    [Authorize(Roles = "Admin,Supervisor")]
    public async Task<IActionResult> Cancel(int id)
    {
        var result = await _ventaService.CancelAsync(id);
        if (!result)
        {
            return NotFound(new { message = $"Venta con ID {id} no encontrada o ya se encuentra anulada." });
        }
        return Ok(new { message = $"Venta {id} anulada exitosamente y stock reincorporado al inventario." });
    }

    // Genera y descarga el comprobante oficial de la factura de venta en formato PDF.
    [HttpGet("{id:int}/pdf")]
    public async Task<IActionResult> DownloadInvoicePdf(int id)
    {
        try
        {
            var venta = await _ventaService.GetByIdAsync(id);
            if (venta != null && !await IsClientOwnerAsync(venta.ClienteId))
            {
                return Forbid();
            }

            var pdfBytes = await _pdfService.GenerateSaleInvoicePdfAsync(id);
            var fileName = venta != null ? $"Factura_{venta.NumeroVenta}.pdf" : $"Factura_Venta_{id}.pdf";

            return File(pdfBytes, "application/pdf", fileName);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "Error interno al generar la factura en PDF." });
        }
    }

    // Genera y descarga un archivo Excel con el historial completo de todas las ventas registradas.
    [HttpGet("export-excel")]
    [Authorize(Roles = "Admin,Supervisor,Gerente,Contador,Auditor,Vendedor")]
    public async Task<IActionResult> ExportSalesExcel()
    {
        var excelBytes = await _excelService.ExportSalesToExcelAsync();
        var fileName = $"Ventas_Firmeza_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx";
        return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }
}
