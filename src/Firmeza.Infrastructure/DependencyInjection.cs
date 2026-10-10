using Firmeza.Application.Interfaces;
using Firmeza.Domain.Abstractions;
using Firmeza.Infrastructure.Persistence;
using Firmeza.Infrastructure.Persistence.Interceptors;
using Firmeza.Infrastructure.Persistence.Repositories;
using Firmeza.Infrastructure.Persistence.Seeders;
using Firmeza.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Firmeza.Infrastructure;

/// <summary>
/// Módulo de configuración e inyección de dependencias de la capa de Infraestructura.
/// Registra Persistencia (DbContext, Interceptores, Repositorios, UnitOfWork, Seeders)
/// y Servicios de aplicación (JWT, Correo, PDF, Excel, CRUDs).
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registra los servicios de infraestructura en el contenedor IoC de la aplicación.
    /// </summary>
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // 1. Obtener la cadena de conexión desde appsettings.json
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        // 2. Registrar Interceptores de EF Core
        services.AddScoped<AuditableEntityInterceptor>();
        services.AddScoped<DispatchDomainEventsInterceptor>();

        // 3. Registrar DbContexts (ApplicationDbContext y AppDbContext) con PostgreSQL
        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            options.UseNpgsql(connectionString);
            options.AddInterceptors(sp.GetRequiredService<AuditableEntityInterceptor>());
        });

        services.AddScoped<AppDbContext>(sp =>
        {
            var options = sp.GetRequiredService<DbContextOptions<ApplicationDbContext>>();
            return new AppDbContext(options);
        });

        // 4. Configurar ASP.NET Core Identity
        services.AddIdentityCore<IdentityUser>(options =>
        {
            options.Password.RequireDigit = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = false;
            options.Password.RequiredLength = 6;
        })
        .AddRoles<IdentityRole>()
        .AddEntityFrameworkStores<ApplicationDbContext>();

        // 5. Registrar el Patrón Repositorio y Unit of Work
        services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IClienteRepository, ClienteRepository>();
        services.AddScoped<IProductoRepository, ProductoRepository>();
        services.AddScoped<IVentaRepository, VentaRepository>();
        services.AddScoped<IVehiculoRepository, VehiculoRepository>();
        services.AddScoped<IAlquilerRepository, AlquilerRepository>();
        services.AddScoped<ICategoriaRepository, CategoriaRepository>();
        services.AddScoped<IProveedorRepository, ProveedorRepository>();
        services.AddScoped<IDespachoRepository, DespachoRepository>();
        services.AddScoped<IMovimientoInventarioRepository, MovimientoInventarioRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // 6. Registrar Sembradores Programáticos
        services.AddScoped<IDbSeeder, DbSeeder>();

        // 7. Registrar Servicios de Aplicación e Infraestructura
        services.AddScoped<IClienteService, ClienteService>();
        services.AddScoped<IProductoService, ProductoService>();
        services.AddScoped<IVentaService, VentaService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IExcelService, ExcelService>();
        services.AddScoped<IPdfService, PdfService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IEmailService, EmailService>();

        return services;
    }
}
