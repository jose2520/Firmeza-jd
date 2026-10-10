namespace Firmeza.Application.DTOs;

/// <summary>
/// DTO que representa un producto con existencia crítica o inferior al umbral de seguridad.
/// 
/// PARA QUÉ SIRVE:
///   Alerta al administrador o encargado de compras sobre referencias que requieren reabastecimiento urgente.
/// 
/// ORIGEN DE LOS DATOS:
///   Filtrado desde la tabla 'productos' en PostgreSQL (WHERE activo = TRUE AND stock <= 10).
/// </summary>
public record ProductoBajoStockDto(
    int Id,
    string Codigo,
    string Nombre,
    int Stock,
    decimal Precio
);

/// <summary>
/// DTO que representa un producto del ranking de los más vendidos (Top Sellers).
/// 
/// PARA QUÉ SIRVE:
///   Permite conocer los productos de mayor rotación y volumen de facturación para análisis comercial.
/// 
/// ORIGEN DE LOS DATOS:
///   Agregación SQL sobre 'detalles_venta' agrupado por 'producto_id' en PostgreSQL.
/// </summary>
public record TopProductoVendidoDto(
    int ProductoId,
    string Codigo,
    string Nombre,
    int TotalUnidadesVendidas,
    decimal TotalMontoGenerado
);

/// <summary>
/// DTO consolidado con todos los indicadores gerenciales (KPIs) del Dashboard.
/// 
/// PARA QUÉ SIRVE:
///   Proporciona en una sola llamada HTTP todas las estadísticas necesarias para renderizar el tablero de control.
/// 
/// ORIGEN DE LOS DATOS:
///   Cálculo combinado sobre las tablas 'clientes', 'productos', 'ventas' y 'detalles_venta' en PostgreSQL.
/// </summary>
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
