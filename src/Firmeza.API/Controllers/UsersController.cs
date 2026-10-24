// Controlador para que los administradores consulten las cuentas de usuario y los roles del sistema.
// Permite listar todos los usuarios registrados junto con sus permisos asignados y ver
// el catálogo completo de roles disponibles en la plataforma.

using Firmeza.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")] // Acceso exclusivo para administradores del sistema
public class UsersController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ApplicationDbContext? _context;

    // Recibe los administradores de usuarios y roles junto con la conexión a la base de datos.
    public UsersController(
        UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ApplicationDbContext? context = null)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _context = context;
    }

    // Devuelve la lista de todos los usuarios registrados junto con los roles que tiene asignados cada uno.
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        if (_context != null)
        {
            var users = await _context.Users.AsNoTracking().ToListAsync();
            var userRoles = await (from ur in _context.UserRoles.AsNoTracking()
                                   join r in _context.Roles.AsNoTracking() on ur.RoleId equals r.Id
                                   select new { ur.UserId, RoleName = r.Name ?? string.Empty })
                                  .ToListAsync();

            var rolesByUser = userRoles
                .GroupBy(x => x.UserId)
                .ToDictionary(g => g.Key, g => (IList<string>)g.Select(x => x.RoleName).ToList());

            var result = users.Select(u => new
            {
                u.Id,
                u.UserName,
                u.Email,
                u.PhoneNumber,
                Roles = rolesByUser.TryGetValue(u.Id, out var r) ? r : new List<string>()
            }).ToList();

            return Ok(result);
        }

        var fallbackUsers = await _userManager.Users.ToListAsync();
        var userList = new List<object>();

        foreach (var u in fallbackUsers)
        {
            var roles = await _userManager.GetRolesAsync(u);
            userList.Add(new
            {
                u.Id,
                u.UserName,
                u.Email,
                u.PhoneNumber,
                Roles = roles
            });
        }

        return Ok(userList);
    }

    // Devuelve la lista de todos los roles configurados en el sistema (Admin, Vendedor, Supervisor, Cliente, etc.).
    [HttpGet("roles")]
    public async Task<IActionResult> GetRoles()
    {
        var roles = await _roleManager.Roles
            .Select(r => new { r.Id, r.Name })
            .ToListAsync();

        return Ok(roles);
    }
}
