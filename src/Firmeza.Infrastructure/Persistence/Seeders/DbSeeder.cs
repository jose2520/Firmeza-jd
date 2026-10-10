using System;
using System.Threading;
using System.Threading.Tasks;
using Firmeza.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.Infrastructure.Persistence.Seeders;

/// <summary>
/// Implementación de siembra programática idempotente para inicializar datos esenciales de Firmeza.
/// Garantiza la existencia de roles, administrador y catálogos mínimos requeridos.
/// </summary>
public class DbSeeder : IDbSeeder
{
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ApplicationDbContext _context;

    public DbSeeder(
        RoleManager<IdentityRole> roleManager,
        UserManager<IdentityUser> userManager,
        ApplicationDbContext context)
    {
        _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await SeedRolesAsync();
        await SeedAdminUserAsync();
        await SeedPaymentMethodsAsync(cancellationToken);
    }

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
