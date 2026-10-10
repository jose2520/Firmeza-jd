namespace Firmeza.Application.DTOs;

/// <summary>
/// DTO de solicitud para el inicio de sesión de un usuario en el sistema.
/// 
/// PARA QUÉ SIRVE:
///   Transfiere las credenciales ingresadas por el usuario para su validación contra ASP.NET Core Identity.
/// 
/// ORIGEN DE LOS DATOS:
///   Enviado en formato JSON en el cuerpo (Body) de peticiones POST /api/auth/login.
/// </summary>
public record LoginRequestDto(
    string EmailOrUsername,
    string Password
);

/// <summary>
/// DTO de solicitud para el registro de un nuevo usuario en el sistema.
/// 
/// PARA QUÉ SIRVE:
///   Permite crear una nueva cuenta en Identity asignando credenciales y opcionalmente un rol predeterminado.
/// 
/// ORIGEN DE LOS DATOS:
///   Enviado en formato JSON en el cuerpo (Body) de peticiones POST /api/auth/register.
/// </summary>
public record RegisterRequestDto(
    string Username,
    string Email,
    string Password,
    string? Role
);

/// <summary>
/// DTO de respuesta emitido tras una autenticación o registro exitoso.
/// 
/// PARA QUÉ SIRVE:
///   Devuelve al cliente el token JWT firmado, los roles autorizados y la fecha de expiración del token.
/// 
/// ORIGEN DE LOS DATOS:
///   Generado por JwtTokenService a partir de los datos verificados del usuario en Identity (PostgreSQL).
/// </summary>
public record AuthResponseDto(
    string Token,
    string Username,
    string Email,
    IEnumerable<string> Roles,
    DateTime ExpiresAt
);
