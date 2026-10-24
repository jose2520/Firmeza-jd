// Archivo: ExcelDto.cs
// Qué es: Estructuras de datos para los resultados al subir archivos de Excel.
// Para qué sirve: Informa al usuario cuántos registros se guardaron bien, cuántos se saltaron y qué errores se encontraron en el archivo.

namespace Firmeza.Application.DTOs;

// ReSharper disable NotAccessedPositionalProperty.Global

// Resumen del resultado de subir un archivo Excel de productos, indicando filas leídas, guardadas, omitidas y la lista de errores.
[System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "NotAccessedPositionalProperty.Global")]
public record ExcelImportResultDto(
    int TotalFilas,
    int TotalImportados,
    int TotalIgnorados,
    List<string> Errores
);

// Resumen detallado cuando se sube un archivo Excel combinado que trae tanto productos como clientes en la misma hoja.
[System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "NotAccessedPositionalProperty.Global")]
public record DesnormalizedImportResultDto(
    int TotalFilas,
    int ProductosCreados,
    int ProductosActualizados,
    int ClientesCreados,
    int ClientesActualizados,
    int TotalOmitidos,
    List<string> Errores
);

// ReSharper restore NotAccessedPositionalProperty.Global
