// Archivo: CategoriaDto.cs
// Qué es: Estructuras de datos para las categorías de productos de la ferretería.
// Para qué sirve: Permite enviar y recibir la información para consultar, crear o editar las familias de productos del catálogo.

namespace Firmeza.Application.DTOs;

// ReSharper disable NotAccessedPositionalProperty.Global

// Muestra la información completa de una categoría de productos y cuántos artículos tiene asociados.
[System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "NotAccessedPositionalProperty.Global")]
public record CategoriaDto(
    int Id = 0,
    string Nombre = "",
    string? Descripcion = null,
    bool Activo = true,
    DateTime CreadoEn = default,
    int TotalProductos = 0
);

// Datos básicos que se piden para crear una nueva categoría en el catálogo.
[System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "NotAccessedPositionalProperty.Global")]
public record CreateCategoriaDto(
    string Nombre,
    string? Descripcion = null
);

// Datos que se reciben para cambiar el nombre, descripción o estado de una categoría existente.
[System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "NotAccessedPositionalProperty.Global")]
public record UpdateCategoriaDto(
    string Nombre,
    string? Descripcion = null,
    bool Activo = true
);

// ReSharper restore NotAccessedPositionalProperty.Global
