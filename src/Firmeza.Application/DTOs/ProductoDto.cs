namespace Firmeza.Application.DTOs;

/// <summary>
/// DTO de respuesta para la representación de un producto en el catálogo.
/// 
/// PARA QUÉ SIRVE:
///   Expone los atributos comerciales y de inventario de un producto hacia el cliente HTTP.
/// 
/// ORIGEN DE LOS DATOS:
///   Proyectado desde la entidad Producto mapeada en la tabla 'productos' de PostgreSQL.
/// </summary>
public record ProductoDto(
    int Id,
    string Codigo,
    string Nombre,
    string? Descripcion,
    decimal Precio,
    int Stock,
    bool Activo,
    DateTime CreadoEn,
    int? CategoriaId = null,
    string? CategoriaNombre = null
);

/// <summary>
/// DTO de solicitud para el registro de un nuevo artículo en inventario.
/// 
/// PARA QUÉ SIRVE:
///   Define los datos requeridos para registrar un producto en el sistema (código SKU, nombre, precio base y existencias iniciales).
/// 
/// ORIGEN DE LOS DATOS:
///   Enviado en formato JSON en el cuerpo (Body) de peticiones POST /api/products.
/// </summary>
public record CreateProductoDto(
    string Codigo,
    string Nombre,
    string? Descripcion,
    decimal Precio,
    int Stock,
    int? CategoriaId = null
);

/// <summary>
/// DTO de solicitud para actualizar precios, descripción o stock de un producto.
/// 
/// PARA QUÉ SIRVE:
///   Permite modificar la ficha técnica, ajustar precios o cambiar el estado activo del producto.
/// 
/// ORIGEN DE LOS DATOS:
///   Enviado en formato JSON en el cuerpo (Body) de peticiones PUT /api/products/{id}.
/// </summary>
public record UpdateProductoDto(
    string Nombre,
    string? Descripcion,
    decimal Precio,
    int Stock,
    bool Activo,
    int? CategoriaId = null
);
