// Controlador que administra el catálogo de productos y materiales de ferretería.
// Permite a cualquier visitante consultar los productos, y a los administradores crear,
// editar, eliminar o cargar y descargar listados completos en archivos de Excel.

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

    // Recibe los servicios encargados de gestionar los productos y de procesar archivos de Excel.
    public ProductsController(
        IProductoService productoService,
        IExcelService excelService)
    {
        _productoService = productoService;
        _excelService = excelService;
    }

    // Devuelve la lista completa de todos los productos registrados en el catálogo.
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductoDto>>> GetAll()
    {
        var productos = await _productoService.GetAllAsync();
        return Ok(productos);
    }

    // Busca y devuelve el detalle de un producto específico por su número identificador.
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

    // Registra un nuevo producto en el catálogo validando que su código no esté repetido y sus valores sean válidos.
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
            // Devuelve un mensaje de error si el código ya existe o si el precio o la cantidad son negativos
            return BadRequest(new { message = ex.Message });
        }
    }

    // Modifica los datos, precio o cantidad disponible de un producto existente.
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

    // Elimina un producto o lo desactiva si ya fue incluido en alguna venta anterior.
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

    // Genera y descarga un archivo Excel con todo el listado actual de productos.
    [HttpGet("export-excel")]
    public async Task<IActionResult> ExportExcel()
    {
        var excelBytes = await _excelService.ExportProductsToExcelAsync();
        var fileName = $"Productos_Firmeza_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx";
        return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    // Descarga una plantilla de Excel de ejemplo para facilitar la carga masiva de productos.
    [HttpGet("template-excel")]
    [Authorize(Roles = "Admin")]
    public IActionResult DownloadTemplate()
    {
        var templateBytes = _excelService.GenerateProductsTemplate();
        return File(templateBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Plantilla_Carga_Productos.xlsx");
    }

    private const long MaxExcelFileSizeBytes = 10 * 1024 * 1024; // 10 MB

    // Recibe un archivo Excel subido por el administrador para crear o actualizar muchos productos de una sola vez.
    [HttpPost("import-excel")]
    [Authorize(Roles = "Admin")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(MaxExcelFileSizeBytes)]
    public async Task<ActionResult<ExcelImportResultDto>> ImportExcel(IFormFile? file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "Debe adjuntar un archivo Excel válido." });
        }

        if (file.Length > MaxExcelFileSizeBytes)
        {
            return BadRequest(new { message = "El archivo excede el tamaño máximo permitido de 10 MB." });
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (extension != ".xlsx" && extension != ".xls")
        {
            return BadRequest(new { message = "Formato de archivo no compatible. Debe ser un archivo Excel (.xlsx)." });
        }

        using var stream = file.OpenReadStream();
        var result = await _excelService.ImportProductsFromExcelAsync(stream);

        return Ok(result);
    }

    // Descarga una plantilla de Excel que combina datos de productos y clientes en una misma hoja.
    [HttpGet("template-desnormalized-excel")]
    [Authorize(Roles = "Admin")]
    public IActionResult DownloadDesnormalizedTemplate()
    {
        var templateBytes = _excelService.GenerateDesnormalizedTemplate();
        return File(templateBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Plantilla_Carga_Desnormalizada.xlsx");
    }

    // Procesa un archivo Excel con datos combinados de productos y clientes, organizándolos y guardándolos por separado.
    [HttpPost("import-desnormalized-excel")]
    [Authorize(Roles = "Admin")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(MaxExcelFileSizeBytes)]
    public async Task<ActionResult<DesnormalizedImportResultDto>> ImportDesnormalizedExcel(IFormFile? file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "Debe adjuntar un archivo Excel válido." });
        }

        if (file.Length > MaxExcelFileSizeBytes)
        {
            return BadRequest(new { message = "El archivo excede el tamaño máximo permitido de 10 MB." });
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (extension != ".xlsx" && extension != ".xls")
        {
            return BadRequest(new { message = "Formato no compatible. Debe ser un archivo Excel (.xlsx)." });
        }

        using var stream = file.OpenReadStream();
        var result = await _excelService.ImportDesnormalizedDataAsync(stream);

        return Ok(result);
    }
}
