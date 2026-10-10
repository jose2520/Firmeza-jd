using Firmeza.Application.DTOs;

namespace Firmeza.Application.Interfaces;

/// <summary>
/// Contrato de servicio para operaciones de importación y exportación de archivos Excel (.xlsx).
/// 
/// PARA QUÉ SIRVE:
///   Permite la carga masiva de productos desde hojas de cálculo, la descarga de catálogos
///   en Excel, la exportación de reportes de ventas consolidados y la generación de plantillas vacías.
/// 
/// ORIGEN DE LOS DATOS:
///   - Importación: Lee el flujo binario (Stream) proveniente de un archivo cargado vía HTTP multipart/form-data.
///   - Exportación: Lee las tablas 'productos' y 'ventas' de PostgreSQL y genera un arreglo binario (byte[]) en memoria con EPPlus.
/// </summary>
public interface IExcelService
{
    /// <summary>
    /// Lee un archivo Excel subido por el usuario, valida las columnas y procesa la carga masiva de productos.
    /// Origen de datos: Flujo de entrada Stream (HTTP Form-Data) -> Escritura en tabla 'productos' de PostgreSQL.
    /// </summary>
    /// <param name="fileStream">Flujo binario del archivo .xlsx a procesar.</param>
    /// <returns>Resultado de la importación con conteo de procesados, creados, actualizados y errores.</returns>
    Task<ExcelImportResultDto> ImportProductsFromExcelAsync(Stream fileStream);

    /// <summary>
    /// Genera y descarga un libro de Excel con todos los productos registrados en el sistema.
    /// Origen de datos: Consulta SQL 'SELECT * FROM productos' en PostgreSQL -> Flujo de memoria con EPPlus.
    /// </summary>
    /// <returns>Arreglo de bytes del libro Excel generado (.xlsx).</returns>
    Task<byte[]> ExportProductsToExcelAsync();

    /// <summary>
    /// Genera y descarga un reporte consolidado en Excel con todas las ventas, clientes y totales facturados.
    /// Origen de datos: Consulta SQL sobre 'ventas' y 'clientes' en PostgreSQL -> Flujo de memoria con EPPlus.
    /// </summary>
    /// <returns>Arreglo de bytes del reporte en Excel (.xlsx).</returns>
    Task<byte[]> ExportSalesToExcelAsync();

    /// <summary>
    /// Genera una plantilla en blanco de Excel con las cabeceras predefinidas y filas de ejemplo para facilitar la carga.
    /// Origen de datos: Generado en memoria sin consultar base de datos.
    /// </summary>
    /// <returns>Arreglo de bytes de la plantilla Excel (.xlsx).</returns>
    byte[] GenerateProductsTemplate();

    /// <summary>
    /// Importa y normaliza en memoria un archivo Excel con datos desnormalizados (productos y clientes combinados).
    /// </summary>
    Task<DesnormalizedImportResultDto> ImportDesnormalizedDataAsync(Stream fileStream);

    /// <summary>
    /// Genera la plantilla de Excel para carga desnormalizada de clientes y productos.
    /// </summary>
    byte[] GenerateDesnormalizedTemplate();
}
