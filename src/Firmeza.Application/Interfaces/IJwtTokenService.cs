namespace Firmeza.Application.Interfaces;

/// <summary>
/// Contrato de servicio para la generación y firma de tokens de autenticación JWT (JSON Web Tokens).
/// 
/// PARA QUÉ SIRVE:
///   Construye tokens criptográficamente firmados con el algoritmo HmacSha256 para permitir
///   el acceso seguro a los endpoints protegidos por atributos [Authorize(Roles = "...")].
/// 
/// ORIGEN DE LOS DATOS:
///   - Claims del usuario: Parámetros del método (Id, UserName, Email, Roles de Identity en PostgreSQL).
///   - Secreto de firma y vigencia: Leídos desde 'appsettings.json' (sección Jwt:Key, Jwt:Issuer, Jwt:Audience).
/// </summary>
public interface IJwtTokenService
{
    /// <summary>
    /// Genera un token JWT firmado conteniendo los claims de identidad y roles del usuario autenticado.
    /// Origen de datos: Identidad de usuario en PostgreSQL + Clave simétrica de 'appsettings.json'.
    /// </summary>
    /// <param name="userId">Identificador GUID o texto del usuario en Identity.</param>
    /// <param name="userName">Nombre de usuario (login).</param>
    /// <param name="email">Correo electrónico del usuario.</param>
    /// <param name="roles">Lista de nombres de roles asignados (Admin, Vendedor, etc.).</param>
    /// <returns>Tupla con el token en base64 URL-safe y su fecha de expiración UTC.</returns>
    (string Token, DateTime ExpiresAt) GenerateToken(string userId, string userName, string email, IEnumerable<string> roles);
}
