// Servicio encargado de crear los pases de acceso digitales (tokens JWT) para los usuarios que inician sesión.
// Lee la clave secreta y el tiempo de duración desde el archivo appsettings.json.
// Recibe los datos del usuario y sus roles desde las tablas "AspNetUsers" y "AspNetRoles".
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

    // Inicializa el servicio leyendo la configuración general de la aplicación.
    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    // Genera un código de acceso seguro con el ID, nombre, correo y roles del usuario, junto con su fecha de vencimiento.
    public (string Token, DateTime ExpiresAt) GenerateToken(string userId, string userName, string email, IEnumerable<string> roles)
    {
        var jwtSettings = _configuration.GetSection("Jwt");
        var secretKey = jwtSettings["Key"] ?? "FirmezaSuperSecretSecurityKey2026!*SecureKeyGreaterThan32Chars";
        var issuer = jwtSettings["Issuer"] ?? "FirmezaAPI";
        var audience = jwtSettings["Audience"] ?? "FirmezaClient";
        var expiryMinutes = int.TryParse(jwtSettings["ExpiryMinutes"], out var minutes) ? minutes : 120;

        // Clave secreta para firmar el token de seguridad
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // Datos básicos del usuario que viajan dentro del token
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Name, userName),
            new(ClaimTypes.Email, email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        // Agregar cada rol del usuario para que los permisos funcionen en los controladores
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
