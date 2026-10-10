namespace Firmeza.Application.DTOs;

/// <summary>
/// DTO de resultado que consolida el informe de importación masiva de productos vía Excel (.xlsx).
/// 
/// PARA QUÉ SIRVE:
///   Notifica al usuario o administrador el balance de la operación: cuántas filas se leyeron,
///   cuántos productos nuevos o modificados se guardaron, cuántos fueron omitidos y el detalle de errores de validación.
/// 
/// ORIGEN DE LOS DATOS:
///   Generado en memoria por ExcelService tras parsear las celdas del archivo subido y persistir en la tabla 'productos' de PostgreSQL.
/// </summary>
public record ExcelImportResultDto(
    int TotalFilas,
    int TotalImportados,
    int TotalIgnorados,
    List<string> Errores
);

/// <summary>
/// DTO de resultado para la importación de datos desnormalizados (productos y clientes mezclados en la misma hoja).
/// </summary>
public record DesnormalizedImportResultDto(
    int TotalFilas,
    int ProductosCreados,
    int ProductosActualizados,
    int ClientesCreados,
    int ClientesActualizados,
    int TotalOmitidos,
    List<string> Errores
);
