// Archivo: DashboardDto.cs
// Qué es: Estructuras de datos para el panel principal de estadísticas del negocio.
// Para qué sirve: Reúne los resúmenes de ventas, ingresos, alertas de pocas unidades y los productos más vendidos para mostrarlos al administrador.

namespace Firmeza.Application.DTOs;

// ReSharper disable NotAccessedPositionalProperty.Global

// Representa un producto que tiene pocas unidades disponibles y necesita comprarse pronto.
[System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "NotAccessedPositionalProperty.Global")]
public record ProductoBajoStockDto(
    int Id,
    string Codigo,
    string Nombre,
    int Stock,
    decimal Precio
);

// Representa un producto destacado dentro de la lista de los que más se han vendido, con sus unidades y dinero recaudado.
[System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "NotAccessedPositionalProperty.Global")]
public record TopProductoVendidoDto(
    int ProductoId,
    string Codigo,
    string Nombre,
    int TotalUnidadesVendidas,
    decimal TotalMontoGenerado
);

// Agrupa todos los números importantes del negocio en un solo paquete para alimentar la pantalla principal de control.
[System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "NotAccessedPositionalProperty.Global")]
public record DashboardSummaryDto(
    int TotalClientes,
    int TotalProductos,
    int TotalVentas,
    decimal TotalIngresos,
    int VentasHoy,
    decimal IngresosHoy,
    IEnumerable<ProductoBajoStockDto> ProductosBajoStock,
    IEnumerable<TopProductoVendidoDto> TopProductosVendidos
);

// ReSharper restore NotAccessedPositionalProperty.Global
