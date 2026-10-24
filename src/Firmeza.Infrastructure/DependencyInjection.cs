// Archivo de configuración para registrar los servicios de la capa de infraestructura.
// Conecta la base de datos PostgreSQL mediante ApplicationDbContext y AppDbContext.
// Registra los repositorios de todas las tablas y los servicios de correo, PDF, Excel y seguridad.
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

public static class DependencyInjection
{
    // Registra la conexión a la base de datos, los repositorios y los servicios del sistema para que estén disponibles en toda la aplicación.
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // 1. Obtener la cadena de conexión desde appsettings.json
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        // 2. Registrar Proveedor de Tiempo, Despachador de Eventos e Interceptores de base de datos
        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
        services.AddScoped<AuditableEntityInterceptor>();
        services.AddScoped<DispatchDomainEventsInterceptor>();

        // 3. Registrar contextos de base de datos (ApplicationDbContext y AppDbContext) con PostgreSQL
        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            options.UseNpgsql(connectionString);
            options.AddInterceptors(
                sp.GetRequiredService<AuditableEntityInterceptor>(),
                sp.GetRequiredService<DispatchDomainEventsInterceptor>());
        });

        services.AddScoped<AppDbContext>(sp =>
        {
            var options = sp.GetRequiredService<DbContextOptions<ApplicationDbContext>>();
            return new AppDbContext(options);
        });

        // 4. Configurar ASP.NET Core Identity con bloqueo ante intentos fallidos
        services.AddIdentityCore<IdentityUser>(options =>
        {
            options.Password.RequireDigit = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = false;
            options.Password.RequiredLength = 6;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;
            options.User.RequireUniqueEmail = true;
        })
        .AddRoles<IdentityRole>()
        .AddEntityFrameworkStores<ApplicationDbContext>();

        // 5. Registrar los repositorios de acceso a datos
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

        // 6. Registrar el cargador de datos iniciales
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
