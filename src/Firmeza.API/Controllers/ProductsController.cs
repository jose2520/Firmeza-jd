// =================================================================================================
// CONTROLADOR: ProductsController.cs
// CAPA: Firmeza.API (Presentación / Controladores HTTP)
// RUTA BASE: /api/Products
// PROPÓSITO:
//   Gestiona el catálogo de productos y ferretería industrial de Firmeza.
//   Permite consultar el catálogo públicamente, realizar CRUD restringido a administradores,
//   y procesar importación/exportación masiva mediante hojas de cálculo Excel (.xlsx).
//
// ORIGEN DE LOS DATOS:
//   - Consultas y Modificaciones (CRUD): Base de datos PostgreSQL (tabla 'productos').
//   - Exportación Excel: Base de datos PostgreSQL (tabla 'productos') convertida a .xlsx en memoria con EPPlus.
//   - Plantilla Excel: Generada en memoria (sin persistencia previa) mediante EPPlus.
//   - Importación Excel: Archivo binario .xlsx enviado por el cliente vía HTTP multipart/form-data,
//     leído en memoria como Stream e insertado/actualizado en la base de datos PostgreSQL (tabla 'productos').
// =================================================================================================

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
    private readonly IExcelService _excelService;

    public ProductsController(
        IProductoService productoService,
        IExcelService excelService)
    {
        _productoService = productoService;
        _excelService = excelService;
    }

    /// <summary>
    /// Lista todos los productos registrados en el sistema.
    /// ACCESO: Público (no requiere token JWT).
    /// ORIGEN DE DATOS: Base de datos PostgreSQL (tabla 'productos').
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductoDto>>> GetAll()
    {
        var productos = await _productoService.GetAllAsync();
        return Ok(productos);
    }

    /// <summary>
    /// Consulta el detalle de un producto específico por su identificador primario (ID).
    /// ACCESO: Público.
    /// ORIGEN DE DATOS: Base de datos PostgreSQL (tabla 'productos' donde id = {id}).
    /// </summary>
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

    /// <summary>
    /// Crea un nuevo producto en el catálogo.
    /// ACCESO: Protegido (Requiere rol 'Admin' mediante token JWT).
    /// ORIGEN DE DATOS: Cuerpo JSON recibido en la solicitud HTTP -> Persistido en PostgreSQL (tabla 'productos').
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ProductoDto>> Create([FromBody] CreateProductoDto dto)
    {
        try
        {
            var nuevo = await _productoService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = nuevo.Id }, nuevo);
        }
        catch (InvalidOperationException ex)
        {
            // Responde 400 Bad Request si el código del producto ya existe o si precio/stock son negativos
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Actualiza los datos de un producto existente.
    /// ACCESO: Protegido (Requiere rol 'Admin').
    /// ORIGEN DE DATOS: Modifica el registro en PostgreSQL (tabla 'productos').
    /// </summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ProductoDto>> Update(int id, [FromBody] UpdateProductoDto dto)
    {
        try
        {
            var actualizado = await _productoService.UpdateAsync(id, dto);
            if (actualizado == null)
            {
                return NotFound(new { message = $"Producto con ID {id} no encontrado para actualizar." });
            }
            return Ok(actualizado);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Elimina un producto. Si el producto ya figura en ventas previas, aplica borrado lógico (Activo = false).
    /// ACCESO: Protegido (Requiere rol 'Admin').
    /// ORIGEN DE DATOS: Base de datos PostgreSQL (tabla 'productos' y verifica 'detalles_venta').
    /// </summary>
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

    /// <summary>
    /// Exporta el catálogo completo de productos a un archivo Excel (.xlsx).
    /// ACCESO: Público o autenticado.
    /// ORIGEN DE DATOS: Consulta la tabla 'productos' en PostgreSQL y genera el archivo binario en memoria con EPPlus.
    /// </summary>
    [HttpGet("export-excel")]
    public async Task<IActionResult> ExportExcel()
    {
        var excelBytes = await _excelService.ExportProductsToExcelAsync();
        var fileName = $"Productos_Firmeza_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx";
        return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    /// <summary>
    /// Descarga la plantilla oficial en blanco con ejemplos para realizar importaciones masivas.
    /// ACCESO: Protegido (Requiere rol 'Admin').
    /// ORIGEN DE DATOS: Archivo generado dinámicamente en memoria con encabezados requeridos.
    /// </summary>
    [HttpGet("template-excel")]
    [Authorize(Roles = "Admin")]
    public IActionResult DownloadTemplate()
    {
        var templateBytes = _excelService.GenerateProductsTemplate();
        return File(templateBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Plantilla_Carga_Productos.xlsx");
    }

    /// <summary>
    /// Importa masivamente productos a partir de un archivo Excel (.xlsx) subido por el cliente.
    /// ACCESO: Protegido (Requiere rol 'Admin').
    /// ORIGEN DE DATOS:
    ///   - Entrada: Archivo subido vía HTTP multipart/form-data.
    ///   - Destino: Se procesa en memoria con EPPlus y se guarda en PostgreSQL (tabla 'productos').
    /// </summary>
    [HttpPost("import-excel")]
    [Authorize(Roles = "Admin")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ExcelImportResultDto>> ImportExcel([FromForm] FileUploadRequest request)
    {
        if (request?.File == null || request.File.Length == 0)
        {
            return BadRequest(new { message = "Debe adjuntar un archivo Excel válido." });
        }

        var extension = Path.GetExtension(request.File.FileName).ToLowerInvariant();
        if (extension != ".xlsx" && extension != ".xls")
        {
            return BadRequest(new { message = "Formato de archivo no compatible. Debe ser un archivo Excel (.xlsx)." });
        }

        using var stream = request.File.OpenReadStream();
        var result = await _excelService.ImportProductsFromExcelAsync(stream);

        return Ok(result);
    }

    /// <summary>
    /// Descarga la plantilla oficial para carga masiva desnormalizada (productos y clientes combinados).
    /// ACCESO: Protegido (Requiere rol 'Admin').
    /// </summary>
    [HttpGet("template-desnormalized-excel")]
    [Authorize(Roles = "Admin")]
    public IActionResult DownloadDesnormalizedTemplate()
    {
        var templateBytes = _excelService.GenerateDesnormalizedTemplate();
        return File(templateBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Plantilla_Carga_Desnormalizada.xlsx");
    }

    /// <summary>
    /// Importa masivamente un archivo Excel desnormalizado (productos y clientes mezclados), normalizando en memoria.
    /// ACCESO: Protegido (Requiere rol 'Admin').
    /// </summary>
    [HttpPost("import-desnormalized-excel")]
    [Authorize(Roles = "Admin")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<DesnormalizedImportResultDto>> ImportDesnormalizedExcel([FromForm] FileUploadRequest request)
    {
        if (request?.File == null || request.File.Length == 0)
        {
            return BadRequest(new { message = "Debe adjuntar un archivo Excel válido." });
        }

        var extension = Path.GetExtension(request.File.FileName).ToLowerInvariant();
        if (extension != ".xlsx" && extension != ".xls")
        {
            return BadRequest(new { message = "Formato no compatible. Debe ser un archivo Excel (.xlsx)." });
        }

        using var stream = request.File.OpenReadStream();
        var result = await _excelService.ImportDesnormalizedDataAsync(stream);

        return Ok(result);
    }
}

/// <summary>
/// Modelo DTO para la carga de archivos multipart/form-data compatible con Swagger UI
/// </summary>
public class FileUploadRequest
{
    public IFormFile File { get; set; } = null!;
}
