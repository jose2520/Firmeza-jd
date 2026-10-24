// Archivo: IExcelService.cs
// Qué es: Plantilla para el servicio que maneja archivos de hojas de cálculo Excel.
// Para qué sirve: Define las funciones para subir productos y clientes desde un archivo Excel, descargar reportes de inventario o ventas y bajar plantillas de ejemplo.

using Firmeza.Application.DTOs;

namespace Firmeza.Application.Interfaces;

// Define las operaciones para cargar y descargar información usando archivos Excel.
public interface IExcelService
{
    // Lee un archivo Excel subido por el usuario y guarda o actualiza los productos en el catálogo.
    Task<ExcelImportResultDto> ImportProductsFromExcelAsync(Stream fileStream);

    // Crea un archivo Excel con el listado completo de todos los productos registrados para poder descargarlo.
    Task<byte[]> ExportProductsToExcelAsync();

    // Crea un archivo Excel con el reporte de todas las ventas realizadas y sus totales para descargar.
    Task<byte[]> ExportSalesToExcelAsync();

    // Genera un archivo Excel de guía con las columnas listas y ejemplos para facilitar la carga de productos.
    byte[] GenerateProductsTemplate();

    // Lee un archivo Excel que trae clientes y productos mezclados en la misma hoja y los guarda en orden.
    Task<DesnormalizedImportResultDto> ImportDesnormalizedDataAsync(Stream fileStream);

    // Genera un archivo Excel de ejemplo para cargar clientes y productos combinados en una sola hoja.
    byte[] GenerateDesnormalizedTemplate();
}
