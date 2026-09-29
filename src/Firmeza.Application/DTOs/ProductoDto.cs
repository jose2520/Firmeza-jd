namespace Firmeza.Application.DTOs;

public record ProductoDto(
    int Id,
    string Codigo,
    string Nombre,
    string? Descripcion,
    decimal Precio,
    int Stock,
    bool Activo,
    DateTime CreadoEn
);

public record CreateProductoDto(
    string Codigo,
    string Nombre,
    string? Descripcion,
    decimal Precio,
    int Stock
);

public record UpdateProductoDto(
    string Nombre,
    string? Descripcion,
    decimal Precio,
    int Stock,
    bool Activo
);
