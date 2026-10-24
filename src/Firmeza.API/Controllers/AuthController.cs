// Controlador encargado del registro de usuarios y del inicio de sesión en la plataforma.
// Permite crear cuentas nuevas vinculadas a clientes, validar contraseñas, entregar llaves
// de acceso digitales y enviar correos de bienvenida.

using Firmeza.Application.DTOs;
using Firmeza.Application.Interfaces;
using Firmeza.Domain.Entities;
using Firmeza.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("AuthPolicy")]
public class AuthController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IEmailService _emailService;
    private readonly ApplicationDbContext? _context;

    // Inicializa las herramientas de gestión de usuarios, roles, generación de llaves de acceso y correo.
    public AuthController(
        UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IJwtTokenService jwtTokenService,
        IEmailService emailService,
        ApplicationDbContext? context = null)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _jwtTokenService = jwtTokenService;
        _emailService = emailService;
        _context = context;
    }

    // Registra un nuevo usuario en el sistema, crea su ficha de cliente si corresponde y le envía un correo de bienvenida.
    // Devuelve de inmediato su llave de acceso digital para que quede con la sesión iniciada.
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto dto)
    {
        // 1. Verificar que el nombre de usuario o el correo no estén ya registrados
        var existingUser = await _userManager.FindByNameAsync(dto.Username) ?? await _userManager.FindByEmailAsync(dto.Email);
        if (existingUser != null)
        {
            return BadRequest(new { message = "El usuario o correo electrónico ya se encuentra registrado." });
        }

        // 2. Crear el usuario con su contraseña protegida
        var user = new IdentityUser
        {
            UserName = dto.Username,
            Email = dto.Email,
            PhoneNumber = dto.Telefono
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
        {
            return BadRequest(new { message = "Error al crear usuario", errors = result.Errors.Select(e => e.Description) });
        }

        // 3. Asignar el rol de forma segura: los registros públicos siempre quedan como 'Cliente'
        var callerIsAdmin = User.Identity is { IsAuthenticated: true } && User.IsInRole("Admin");
        var roleName = callerIsAdmin && !string.IsNullOrWhiteSpace(dto.Role) ? dto.Role : "Cliente";

        if (!await _roleManager.RoleExistsAsync(roleName))
        {
            await _roleManager.CreateAsync(new IdentityRole(roleName));
        }

        await _userManager.AddToRoleAsync(user, roleName);

        // 3.1 Vincular o crear la ficha en la tabla de clientes si su rol es 'Cliente'
        if (_context != null && string.Equals(roleName, "Cliente", StringComparison.OrdinalIgnoreCase))
        {
            var existingClient = await _context.Clientes
                .FirstOrDefaultAsync(c => c.Email.ToLower() == dto.Email.ToLower());

            if (existingClient != null)
            {
                existingClient.UserId = user.Id;
                await _context.SaveChangesAsync();
            }
            else
            {
                var identificacion = !string.IsNullOrWhiteSpace(dto.Identificacion)
                    ? dto.Identificacion.Trim()
                    : $"CLI-{Guid.NewGuid().ToString("N")[..10].ToUpper()}";

                // Evitar duplicados si la identificación ya existe
                if (await _context.Clientes.AnyAsync(c => c.Identificacion == identificacion))
                {
                    identificacion = $"CLI-{Guid.NewGuid().ToString("N")[..10].ToUpper()}";
                }

                var nuevoCliente = new Cliente
                {
                    UserId = user.Id,
                    Identificacion = identificacion,
                    Nombres = !string.IsNullOrWhiteSpace(dto.Nombres) ? dto.Nombres.Trim() : dto.Username.Trim(),
                    Apellidos = !string.IsNullOrWhiteSpace(dto.Apellidos) ? dto.Apellidos.Trim() : "Cliente Registrado",
                    Email = dto.Email.Trim(),
                    Telefono = dto.Telefono?.Trim(),
                    Direccion = dto.Direccion?.Trim(),
                    Edad = dto.Edad is >= 0 ? dto.Edad : null,
                    Activo = true,
                    CreadoEn = DateTime.UtcNow
                };

                _context.Clientes.Add(nuevoCliente);
                await _context.SaveChangesAsync();
            }
        }

        // 4. Generar la llave de acceso para iniciar sesión de inmediato
        var roles = await _userManager.GetRolesAsync(user);
        var (token, expiresAt) = _jwtTokenService.GenerateToken(user.Id, user.UserName!, user.Email!, roles);

        // 5. Enviar correo de bienvenida sin detener el registro si el servidor de correo falla
        try
        {
            await _emailService.SendWelcomeEmailAsync(user.Email!, user.UserName!);
        }
        catch
        {
            // Si el envío de correo falla, el registro continúa normalmente
        }

        return Ok(new AuthResponseDto(token, user.UserName!, user.Email!, roles, expiresAt));
    }

    // Verifica el correo o nombre de usuario junto con la contraseña para iniciar sesión.
    // Si los datos son correctos, devuelve una llave de acceso digital con los permisos del usuario.
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
    {
        // 1. Buscar al usuario por su correo o nombre de usuario
        var user = await _userManager.FindByEmailAsync(dto.EmailOrUsername) ?? await _userManager.FindByNameAsync(dto.EmailOrUsername);
        if (user == null)
        {
            return Unauthorized(new { message = "Credenciales inválidas." });
        }

        // 1.1 Comprobar si la cuenta está bloqueada por demasiados intentos fallidos
        if (await _userManager.IsLockedOutAsync(user))
        {
            return Unauthorized(new { message = "La cuenta se encuentra bloqueada temporalmente por múltiples intentos fallidos." });
        }

        // 2. Comprobar que la contraseña ingresada coincida con la guardada
        var isPasswordValid = await _userManager.CheckPasswordAsync(user, dto.Password);
        if (!isPasswordValid)
        {
            await _userManager.AccessFailedAsync(user);
            return Unauthorized(new { message = "Credenciales inválidas." });
        }

        await _userManager.ResetAccessFailedCountAsync(user);

        // 3. Consultar los roles asignados al usuario
        var roles = await _userManager.GetRolesAsync(user);

        // 4. Generar la llave de acceso con su fecha de vencimiento
        var (token, expiresAt) = _jwtTokenService.GenerateToken(user.Id, user.UserName!, user.Email!, roles);

        return Ok(new AuthResponseDto(token, user.UserName!, user.Email!, roles, expiresAt));
    }
}
