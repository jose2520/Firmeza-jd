// =================================================================================================
// CONTROLADOR: UsersController.cs
// CAPA: Firmeza.API (Presentación / Controladores HTTP)
// RUTA BASE: /api/Users
// PROPÓSITO:
//   Permite a los administradores del sistema auditar y consultar las cuentas de usuario
//   creadas en ASP.NET Core Identity y el catálogo de roles disponibles.
//
// ORIGEN DE LOS DATOS:
//   - Usuarios: Base de datos PostgreSQL (tabla 'AspNetUsers').
//   - Roles de cada usuario: Base de datos PostgreSQL (tabla 'AspNetUserRoles' cruzada con 'AspNetRoles').
//   - Catálogo de Roles del Sistema: Base de datos PostgreSQL (tabla 'AspNetRoles').
// =================================================================================================

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")] // Restringido exclusivamente al Administrador
public class UsersController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public UsersController(
        UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    /// <summary>
    /// Lista todos los usuarios registrados en el sistema junto con la lista de roles asignados a cada uno.
    /// ACCESO: Protegido (Requiere rol 'Admin').
    /// ORIGEN DE DATOS: Base de datos PostgreSQL (tablas 'AspNetUsers' y 'AspNetUserRoles').
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var users = await _userManager.Users.ToListAsync();
        var userList = new List<object>();

        foreach (var u in users)
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

    /// <summary>
    /// Consulta el listado de todos los roles configurados en la plataforma (Admin, Vendedor, Supervisor, Cliente, etc.).
    /// ACCESO: Protegido (Requiere rol 'Admin').
    /// ORIGEN DE DATOS: Base de datos PostgreSQL (tabla 'AspNetRoles').
    /// </summary>
    [HttpGet("roles")]
    public async Task<IActionResult> GetRoles()
    {
        var roles = await _roleManager.Roles
            .Select(r => new { r.Id, r.Name })
            .ToListAsync();

        return Ok(roles);
    }
}
