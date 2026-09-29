namespace Firmeza.Application.DTOs;

public record ClienteDto(
    int Id,
    string Identificacion,
    string Nombres,
    string Apellidos,
    string? Email,
    string? Telefono,
    string? Direccion,
    bool Activo,
    DateTime CreadoEn
);

public record CreateClienteDto(
    string Identificacion,
    string Nombres,
    string Apellidos,
    string? Email,
    string? Telefono,
    string? Direccion
);

public record UpdateClienteDto(
    string Nombres,
    string Apellidos,
    string? Email,
    string? Telefono,
    string? Direccion,
    bool Activo
);
