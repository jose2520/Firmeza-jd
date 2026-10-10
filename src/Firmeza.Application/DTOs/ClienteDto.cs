namespace Firmeza.Application.DTOs;

/// <summary>
/// DTO de respuesta para la representación de un cliente hacia los consumidores del API.
/// 
/// PARA QUÉ SIRVE:
///   Transfiere los datos completos de un cliente hacia el frontend o clientes móviles,
///   ocultando detalles de persistencia interna y evitando referencias circulares.
/// 
/// ORIGEN DE LOS DATOS:
///   Proyectado desde la entidad Cliente mapeada en la tabla 'clientes' de PostgreSQL.
/// </summary>
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

/// <summary>
/// DTO de solicitud para el registro de un nuevo cliente.
/// 
/// PARA QUÉ SIRVE:
///   Encapsula los campos obligatorios y opcionales necesarios para crear un cliente.
/// 
/// ORIGEN DE LOS DATOS:
///   Enviado en formato JSON en el cuerpo (Body) de peticiones POST /api/clients.
/// </summary>
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

/// <summary>
/// DTO de solicitud para la actualización de un cliente existente.
/// 
/// PARA QUÉ SIRVE:
///   Permite modificar nombres, información de contacto o reactivar un cliente inactivo.
/// 
/// ORIGEN DE LOS DATOS:
///   Enviado en formato JSON en el cuerpo (Body) de peticiones PUT /api/clients/{id}.
/// </summary>
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
