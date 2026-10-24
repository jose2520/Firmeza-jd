// Archivo: ClienteDto.cs
// Qué es: Estructuras de datos para manejar la información de los clientes.
// Para qué sirve: Transporta los datos personales y de contacto cuando se consultan, registran o actualizan clientes en el sistema.

namespace Firmeza.Application.DTOs;

// Contiene toda la información de un cliente registrado para mostrarla en pantalla.
public record ClienteDto(
    int Id,
    string Identificacion,
    string Nombres,
    string Apellidos,
    string? Email,
    string? Telefono,
    string? Direccion,
    bool Activo,
    DateTime CreadoEn,
    string? UserId = null,
    int? Edad = null
);

// Datos obligatorios y opcionales que se piden para registrar un cliente nuevo.
public record CreateClienteDto(
    string Identificacion,
    string Nombres,
    string Apellidos,
    string? Email,
    string? Telefono,
    string? Direccion,
    string? UserId = null,
    int? Edad = null
);

// Datos permitidos para actualizar la información de contacto o el estado de un cliente existente.
public record UpdateClienteDto(
    string Nombres,
    string Apellidos,
    string? Email,
    string? Telefono,
    string? Direccion,
    bool Activo,
    string? UserId = null,
    int? Edad = null
);
