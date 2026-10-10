// =================================================================================================
// CONTROLADOR: AuthController.cs
// CAPA: Firmeza.API (Presentación / Controladores HTTP)
// RUTA BASE: /api/Auth
// PROPÓSITO:
//   Gestiona la autenticación de usuarios y el control de acceso:
//   - Registro de nuevas cuentas de usuario.
//   - Verificación de credenciales (login).
//   - Emisión de tokens de acceso firmados digitalmente (JSON Web Tokens - JWT).
//
// ORIGEN DE LOS DATOS:
//   - Usuarios y Roles: Base de datos PostgreSQL (tablas de Identity: 'AspNetUsers', 'AspNetRoles', 'AspNetUserRoles').
//   - Hash de Contraseñas: Algoritmo PBKDF2 administrado internamente por ASP.NET Core Identity.
//   - Firma JWT: Se genera con la clave secreta y parámetros configurados en 'appsettings.json' (sección "Jwt").
// =================================================================================================

using Firmeza.Application.DTOs;
using Firmeza.Application.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Firmeza.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IEmailService _emailService;

    public AuthController(
        UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IJwtTokenService jwtTokenService,
        IEmailService emailService)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _jwtTokenService = jwtTokenService;
        _emailService = emailService;
    }

    /// <summary>
    /// Registra un nuevo usuario en la plataforma.
    /// ACCESO: Público (para nuevos clientes).
    /// SEGURIDAD:
    ///   - Si quien hace la petición no es un administrador autenticado, se asigna obligatoriamente el rol 'Cliente'.
    ///   - Solo un 'Admin' autenticado puede registrar usuarios con roles privilegiados ('Vendedor', 'Supervisor', etc.).
    /// ORIGEN DE DATOS: Persiste en PostgreSQL ('AspNetUsers' y 'AspNetUserRoles').
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto dto)
    {
        // 1. Validar que el usuario o correo no existan previamente en la tabla 'AspNetUsers'
        var existingUser = await _userManager.FindByNameAsync(dto.Username) ?? await _userManager.FindByEmailAsync(dto.Email);
        if (existingUser != null)
        {
            return BadRequest(new { message = "El usuario o correo electrónico ya se encuentra registrado." });
        }

        // 2. Crear la entidad de IdentityUser y aplicar hashing a la contraseña
        var user = new IdentityUser
        {
            UserName = dto.Username,
            Email = dto.Email
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
        {
            return BadRequest(new { message = "Error al crear usuario", errors = result.Errors.Select(e => e.Description) });
        }

        // 3. Asignación segura de rol: usuarios públicos siempre reciben 'Cliente'
        var callerIsAdmin = User.Identity?.IsAuthenticated == true && User.IsInRole("Admin");
        var roleName = callerIsAdmin && !string.IsNullOrWhiteSpace(dto.Role) ? dto.Role : "Cliente";

        if (!await _roleManager.RoleExistsAsync(roleName))
        {
            await _roleManager.CreateAsync(new IdentityRole(roleName));
        }

        await _userManager.AddToRoleAsync(user, roleName);

        // 4. Emitir el token JWT para permitir el inicio de sesión inmediato
        var roles = await _userManager.GetRolesAsync(user);
        var (token, expiresAt) = _jwtTokenService.GenerateToken(user.Id, user.UserName!, user.Email!, roles);

        // 5. Enviar correo de bienvenida al nuevo cliente (ejecución asíncrona segura)
        _ = _emailService.SendWelcomeEmailAsync(user.Email!, user.UserName!);

        return Ok(new AuthResponseDto(token, user.UserName!, user.Email!, roles, expiresAt));
    }

    /// <summary>
    /// Inicia sesión verificando nombre de usuario / correo y contraseña.
    /// ACCESO: Público.
    /// ORIGEN DE DATOS:
    ///   - Valida contra PostgreSQL ('AspNetUsers' y 'AspNetUserRoles').
    ///   - Retorna el token JWT con los roles del usuario incrustados en sus Claims.
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
    {
        // 1. Buscar usuario por Email o por Username en 'AspNetUsers'
        var user = await _userManager.FindByEmailAsync(dto.EmailOrUsername) ?? await _userManager.FindByNameAsync(dto.EmailOrUsername);
        if (user == null)
        {
            return Unauthorized(new { message = "Credenciales inválidas." });
        }

        // 2. Verificar la contraseña con el hash PBKDF2 guardado en la base de datos
        var isPasswordValid = await _userManager.CheckPasswordAsync(user, dto.Password);
        if (!isPasswordValid)
        {
            return Unauthorized(new { message = "Credenciales inválidas." });
        }

        // 3. Obtener los roles del usuario desde la tabla 'AspNetUserRoles'
        var roles = await _userManager.GetRolesAsync(user);

        // 4. Generar el token JWT con expiración (usualmente 120 minutos)
        var (token, expiresAt) = _jwtTokenService.GenerateToken(user.Id, user.UserName!, user.Email!, roles);

        return Ok(new AuthResponseDto(token, user.UserName!, user.Email!, roles, expiresAt));
    }
}
