// Archivo: AuthDto.cs
// Qué es: Estructuras de datos para el inicio de sesión y registro de usuarios.
// Para qué sirve: Guarda temporalmente los datos que envía el usuario al entrar o crear su cuenta, y la respuesta con su llave de acceso.

namespace Firmeza.Application.DTOs;

// Datos necesarios para que un usuario inicie sesión con su correo o nombre de usuario y contraseña.
public record LoginRequestDto(
    string EmailOrUsername,
    string Password
);

// Datos requeridos para registrar una cuenta nueva en el sistema, incluyendo información personal opcional del cliente.
public record RegisterRequestDto(
    string Username,
    string Email,
    string Password,
    string? Role = null,
    string? Identificacion = null,
    string? Nombres = null,
    string? Apellidos = null,
    string? Telefono = null,
    string? Direccion = null,
    int? Edad = null
);

// Respuesta que se entrega al usuario cuando entra con éxito, con su código de acceso, permisos y fecha de vencimiento.
[System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "NotAccessedPositionalProperty.Global")]
public record AuthResponseDto(
    string Token,
    string Username,
    string Email,
    IEnumerable<string> Roles,
    DateTime ExpiresAt
);
