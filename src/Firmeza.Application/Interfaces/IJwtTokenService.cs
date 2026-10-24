// Archivo: IJwtTokenService.cs
// Qué es: Plantilla para el servicio que crea las llaves digitales de acceso al sistema.
// Para qué sirve: Permite generar un código seguro cuando el usuario inicia sesión para comprobar su identidad y sus permisos en cada pantalla.

namespace Firmeza.Application.Interfaces;

// Define la operación encargada de crear el código de seguridad para los usuarios que inician sesión.
public interface IJwtTokenService
{
    // Genera un código de acceso seguro con los datos y permisos del usuario, indicando hasta qué hora es válido.
    (string Token, DateTime ExpiresAt) GenerateToken(string userId, string userName, string email, IEnumerable<string> roles);
}
