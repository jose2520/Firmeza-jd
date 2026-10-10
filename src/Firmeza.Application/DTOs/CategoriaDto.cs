using System;

namespace Firmeza.Application.DTOs;

/// <summary>
/// DTO de respuesta para la representación de una categoría o familia del catálogo ferretero y de maquinaria.
/// </summary>
public record CategoriaDto(
    int Id = 0,
    string Nombre = "",
    string? Descripcion = null,
    bool Activo = true,
    DateTime CreadoEn = default,
    int TotalProductos = 0
);

/// <summary>
/// DTO de solicitud para la creación de una nueva categoría o línea comercial en el catálogo.
/// </summary>
public record CreateCategoriaDto(
    string Nombre,
    string? Descripcion = null
);

/// <summary>
/// DTO de solicitud para la actualización de una categoría existente.
/// </summary>
public record UpdateCategoriaDto(
    string Nombre,
    string? Descripcion = null,
    bool Activo = true
);
