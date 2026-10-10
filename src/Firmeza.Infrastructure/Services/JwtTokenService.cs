// =================================================================================================
// SERVICIO: JwtTokenService.cs
// CAPA: Firmeza.Infrastructure (Capa de Infraestructura y Seguridad)
// CONTRATO: IJwtTokenService (Firmeza.Application)
// PROPÓSITO:
//   Genera y firma digitalmente los tokens JWT (JSON Web Tokens) que permiten la autenticación
//   sin estado (stateless) entre el cliente (Frontend / Swagger) y la API REST.
//
// ORIGEN Y DESTINO DE LOS DATOS:
//   - Configuración de la llave criptográfica: Leída desde 'appsettings.json' (sección "Jwt:Key", "Jwt:Issuer", etc.).
//   - Identidad y Claims del Token: Proporcionados por el llamador a partir de los datos en PostgreSQL
//     (Id de usuario, nombre de usuario, correo electrónico y lista de roles asignados).
// =================================================================================================

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Firmeza.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Firmeza.Infrastructure.Services;

public class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _configuration;

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// Construye y firma un token JWT codificado en Base64 con un algoritmo HMAC-SHA256.
    /// ORIGEN DE PARÁMETROS:
    ///   - userId, userName, email: Datos extraídos de 'AspNetUsers'.
    ///   - roles: Roles extraídos de 'AspNetUserRoles' cruzado con 'AspNetRoles'.
    ///   - secretKey, issuer, audience, expiryMinutes: Parámetros del archivo 'appsettings.json'.
    /// </summary>
    public (string Token, DateTime ExpiresAt) GenerateToken(string userId, string userName, string email, IEnumerable<string> roles)
    {
        var jwtSettings = _configuration.GetSection("Jwt");
        var secretKey = jwtSettings["Key"] ?? "FirmezaSuperSecretKeyDefault2026!MustBeAtLeast32CharsLong";
        var issuer = jwtSettings["Issuer"] ?? "FirmezaAPI";
        var audience = jwtSettings["Audience"] ?? "FirmezaClient";
        var expiryMinutes = int.TryParse(jwtSettings["ExpiryMinutes"], out var minutes) ? minutes : 120;

        // Clave simétrica para firmar el token
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // Claims estándar de identidad incorporados en el Payload del token
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Name, userName),
            new(ClaimTypes.Email, email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        // Agregar cada rol como un Claim individual para que [Authorize(Roles = "...")] funcione en los controladores
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var expires = DateTime.UtcNow.AddMinutes(expiryMinutes);

        var tokenDescriptor = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expires,
            signingCredentials: credentials
        );

        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenString = tokenHandler.WriteToken(tokenDescriptor);

        return (tokenString, expires);
    }
}
