// Clase que carga los datos básicos iniciales en la base de datos cuando arranca la aplicación.
// Trabaja con las tablas de roles ("AspNetRoles"), usuarios ("AspNetUsers") y formas de pago ("metodos_pago").
// Revisa si los datos ya existen antes de crearlos para no duplicar información en cada inicio.
using Firmeza.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.Infrastructure.Persistence.Seeders;

public class DbSeeder : IDbSeeder
{
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ApplicationDbContext _context;

    // Recibe los administradores de roles, de usuarios y el contexto de la base de datos.
    public DbSeeder(
        RoleManager<IdentityRole> roleManager,
        UserManager<IdentityUser> userManager,
        ApplicationDbContext context)
    {
        _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    // Actualiza las tablas de la base de datos y ejecuta la creación de roles, usuario administrador y métodos de pago.
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (_context.Database.IsRelational())
        {
            await _context.Database.MigrateAsync(cancellationToken);
        }

        await SeedRolesAsync();
        await SeedAdminUserAsync();
        await SeedPaymentMethodsAsync(cancellationToken);
    }

    // Crea los roles del sistema (Admin, Vendedor, Cliente, etc.) si todavía no están registrados.
    private async Task SeedRolesAsync()
    {
        string[] roles = {
            "Admin", "Vendedor", "Supervisor", "Cajero", "Gerente",
            "Almacenista", "Contador", "Auditor", "SoporteTecnico", "Cliente"
        };

        foreach (var role in roles)
        {
            if (!await _roleManager.RoleExistsAsync(role))
            {
                await _roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }

    // Crea el usuario administrador por defecto con su correo y contraseña si aún no existe.
    private async Task SeedAdminUserAsync()
    {
        const string adminEmail = "admin@firmeza.com";
        const string adminUsername = "admin";

        var adminUser = await _userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new IdentityUser
            {
                UserName = adminUsername,
                Email = adminEmail,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(adminUser, "Admin123*");
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }
    }

    // Agrega los métodos de pago iniciales en la tabla "metodos_pago" si la tabla está vacía.
    private async Task SeedPaymentMethodsAsync(CancellationToken cancellationToken)
    {
        if (!await _context.MetodosPago.AnyAsync(cancellationToken))
        {
            var metodos = new[]
            {
                new MetodoPago { Codigo = "EFECTIVO", Nombre = "Efectivo", Descripcion = "Pago contra entrega o en caja física", Activo = true },
                new MetodoPago { Codigo = "TRANSFERENCIA", Nombre = "Transferencia Bancaria", Descripcion = "Transferencia Bancolombia, Davivienda o PSE", Activo = true },
                new MetodoPago { Codigo = "TARJETA_DEBITO", Nombre = "Tarjeta Débito", Descripcion = "Datáfono en punto de venta", Activo = true },
                new MetodoPago { Codigo = "TARJETA_CREDITO", Nombre = "Tarjeta de Crédito", Descripcion = "Visa, MasterCard, American Express", Activo = true },
                new MetodoPago { Codigo = "CREDITO_COMERCIAL", Nombre = "Crédito Comercial a 30 Días", Descripcion = "Línea de crédito para constructores", Activo = true },
                new MetodoPago { Codigo = "CHEQUE_GERENCIA", Nombre = "Cheque de Gerencia", Descripcion = "Cheque bancario garantizado", Activo = true }
            };

            await _context.MetodosPago.AddRangeAsync(metodos, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
