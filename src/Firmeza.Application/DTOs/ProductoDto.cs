// Archivo: ProductoDto.cs
// Qué es: Estructuras de datos para el manejo de productos del catálogo.
// Para qué sirve: Transporta la información de precios, existencias, categoría y proveedor al consultar, crear o modificar un artículo.

namespace Firmeza.Application.DTOs;

// Muestra toda la información comercial y de inventario de un producto del catálogo.
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
    string? CategoriaNombre = null,
    int? ProveedorId = null,
    string? ProveedorNombre = null
);

// Datos necesarios para registrar un artículo nuevo en la tienda con su código, precio y cantidad inicial.
public record CreateProductoDto(
    string Codigo,
    string Nombre,
    string? Descripcion,
    decimal Precio,
    int Stock,
    int? CategoriaId = null,
    int? ProveedorId = null
);

// Datos que se reciben para cambiar el nombre, descripción, precio, cantidad disponible o estado de un producto.
public record UpdateProductoDto(
    string Nombre,
    string? Descripcion,
    decimal Precio,
    int Stock,
    bool Activo,
    int? CategoriaId = null,
    int? ProveedorId = null
);
