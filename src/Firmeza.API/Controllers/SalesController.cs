// =================================================================================================
// CONTROLADOR: SalesController.cs
// CAPA: Firmeza.API (Presentación / Controladores HTTP)
// RUTA BASE: /api/Sales
// PROPÓSITO:
//   Controlador del núcleo transaccional del negocio. Gestiona el ciclo de vida de las ventas:
//   registro de compra, validación de stock, cálculo del IVA (19% en Colombia),
//   anulación con reposición de inventario, emisión de factura en PDF y reporte en Excel.
//
// ORIGEN DE LOS DATOS:
//   - Consultas y Ventas: Tablas relacionales 'ventas', 'detalles_venta', 'clientes' y 'productos' en PostgreSQL.
//   - Factura en PDF: Consulta la venta en PostgreSQL y la compila dinámicamente en memoria con iText 9.
//   - Reporte en Excel: Consulta el historial de ventas en PostgreSQL y lo serializa a .xlsx con EPPlus 8.
// =================================================================================================

using Firmeza.Application.DTOs;
using Firmeza.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Firmeza.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Requiere que el usuario esté autenticado con token JWT
public class SalesController : ControllerBase
{
    private readonly IVentaService _ventaService;
    private readonly IPdfService _pdfService;
    private readonly IExcelService _excelService;
    private readonly IEmailService _emailService;
    private readonly IClienteService _clienteService;

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

    /// <summary>
    /// Consulta el listado completo de ventas registradas con sus clientes y detalles.
    /// ACCESO: Autenticado (Cualquier usuario con token válido).
    /// ORIGEN DE DATOS: Base de datos PostgreSQL (tablas 'ventas' INNER JOIN 'clientes' LEFT JOIN 'detalles_venta').
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<VentaDto>>> GetAll()
    {
        var ventas = await _ventaService.GetAllAsync();
        return Ok(ventas);
    }

    /// <summary>
    /// Consulta el detalle completo de una venta específica mediante su identificador primario (ID).
    /// ACCESO: Autenticado.
    /// ORIGEN DE DATOS: Base de datos PostgreSQL (registro específico con cliente y renglones).
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<VentaDto>> GetById(int id)
    {
        var venta = await _ventaService.GetByIdAsync(id);
        if (venta == null)
        {
            return NotFound(new { message = $"Venta con ID {id} no encontrada." });
        }
        return Ok(venta);
    }

    /// <summary>
    /// Consulta exclusivamente los renglones (detalles/productos adquiridos) de una venta.
    /// ACCESO: Autenticado.
    /// ORIGEN DE DATOS: Base de datos PostgreSQL (tabla 'detalles_venta' asociada a la venta).
    /// </summary>
    [HttpGet("{id:int}/details")]
    public async Task<ActionResult<IEnumerable<DetalleVentaDto>>> GetDetails(int id)
    {
        var venta = await _ventaService.GetByIdAsync(id);
        if (venta == null)
        {
            return NotFound(new { message = $"Venta con ID {id} no encontrada." });
        }
        return Ok(venta.Detalles);
    }

    /// <summary>
    /// Registra una nueva venta de manera transaccional y atómica:
    ///   1. Valida que el cliente exista y esté activo en la BD.
    ///   2. Verifica disponibilidad de stock en la tabla 'productos'.
    ///   3. Descuenta el inventario automáticamente.
    ///   4. Calcula Subtotal, IVA del 19% y Total final.
    ///   5. Genera el código único 'VENTA-YYYYMMDD-XXXXXX' y guarda en 'ventas' y 'detalles_venta'.
    /// ACCESO: Autenticado.
    /// ORIGEN DE DATOS: Entrada JSON -> Persistido en PostgreSQL ('ventas' y 'detalles_venta').
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<VentaDto>> Create([FromBody] CreateVentaDto dto)
    {
        try
        {
            var venta = await _ventaService.CreateAsync(dto);

            // Generar la factura en PDF y enviarla por correo al cliente (en segundo plano / seguro)
            _ = Task.Run(async () =>
            {
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
                    // La falla en el envío de correo o generación en segundo plano no interrumpe la respuesta HTTP
                }
            });

            return CreatedAtAction(nameof(GetById), new { id = venta.Id }, venta);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error interno al procesar la venta", error = ex.Message });
        }
    }

    /// <summary>
    /// Anula una venta previamente completada y reincorpora el stock al inventario de forma automática.
    /// ACCESO: Protegido (Requiere rol 'Admin' o 'Supervisor').
    /// ORIGEN DE DATOS: Actualiza 'estado = Anulada' en tabla 'ventas' e incrementa 'stock' en tabla 'productos'.
    /// </summary>
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

    /// <summary>
    /// Genera y descarga el documento oficial de factura / comprobante de pago en formato PDF.
    /// ACCESO: Autenticado.
    /// ORIGEN DE DATOS:
    ///   - Consulta los datos de la venta y cliente en PostgreSQL.
    ///   - Se renderiza y compone directamente en memoria RAM usando la librería iText 9.
    ///   - No guarda archivos físicos temporales en disco (stream directo al cliente).
    /// </summary>
    [HttpGet("{id:int}/pdf")]
    public async Task<IActionResult> DownloadInvoicePdf(int id)
    {
        try
        {
            var pdfBytes = await _pdfService.GenerateSaleInvoicePdfAsync(id);
            var venta = await _ventaService.GetByIdAsync(id);
            var fileName = venta != null ? $"Factura_{venta.NumeroVenta}.pdf" : $"Factura_Venta_{id}.pdf";

            return File(pdfBytes, "application/pdf", fileName);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al generar la factura en PDF", error = ex.Message });
        }
    }

    /// <summary>
    /// Exporta el reporte consolidado de todo el histórico de ventas a formato Excel (.xlsx).
    /// ACCESO: Autenticado.
    /// ORIGEN DE DATOS: Lee las ventas en PostgreSQL y genera el libro binario en memoria usando EPPlus 8.
    /// </summary>
    [HttpGet("export-excel")]
    public async Task<IActionResult> ExportSalesExcel()
    {
        var excelBytes = await _excelService.ExportSalesToExcelAsync();
        var fileName = $"Ventas_Firmeza_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx";
        return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }
}
