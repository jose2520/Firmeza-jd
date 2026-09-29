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

    public AuthController(
        UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IJwtTokenService jwtTokenService)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _jwtTokenService = jwtTokenService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto dto)
    {
        var existingUser = await _userManager.FindByNameAsync(dto.Username) ?? await _userManager.FindByEmailAsync(dto.Email);
        if (existingUser != null)
        {
            return BadRequest(new { message = "El usuario o correo electrónico ya se encuentra registrado." });
        }

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

        var roleName = string.IsNullOrWhiteSpace(dto.Role) ? "User" : dto.Role;
        if (!await _roleManager.RoleExistsAsync(roleName))
        {
            await _roleManager.CreateAsync(new IdentityRole(roleName));
        }

        await _userManager.AddToRoleAsync(user, roleName);

        var roles = await _userManager.GetRolesAsync(user);
        var (token, expiresAt) = _jwtTokenService.GenerateToken(user.Id, user.UserName!, user.Email!, roles);

        return Ok(new AuthResponseDto(token, user.UserName!, user.Email!, roles, expiresAt));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.EmailOrUsername) ?? await _userManager.FindByNameAsync(dto.EmailOrUsername);
        if (user == null)
        {
            return Unauthorized(new { message = "Credenciales inválidas." });
        }

        var isPasswordValid = await _userManager.CheckPasswordAsync(user, dto.Password);
        if (!isPasswordValid)
        {
            return Unauthorized(new { message = "Credenciales inválidas." });
        }

        var roles = await _userManager.GetRolesAsync(user);
        var (token, expiresAt) = _jwtTokenService.GenerateToken(user.Id, user.UserName!, user.Email!, roles);

        return Ok(new AuthResponseDto(token, user.UserName!, user.Email!, roles, expiresAt));
    }
}
