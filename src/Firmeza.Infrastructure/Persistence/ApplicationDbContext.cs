// =================================================================================================
// CONTEXTO DE DATOS: ApplicationDbContext.cs
// CAPA: Firmeza.Infrastructure.Persistence (Persistencia y Acceso a Datos - Clean Architecture)
// PROPÓSITO:
//   Contexto principal de Entity Framework Core 10 para PostgreSQL (Npgsql).
//   Hereda de IdentityDbContext<IdentityUser> para unificar:
//   - Tablas de seguridad y roles (ASP.NET Core Identity: 'AspNetUsers', 'AspNetRoles', etc.).
//   - Tablas del modelo comercial y logístico ('categorias', 'proveedores', 'metodos_pago',
//     'clientes', 'productos', 'ventas', 'detalles_venta', 'despachos', 'movimientos_inventario',
//     'vehiculos', 'alquileres').
//
// ARQUITECTURA:
//   Desacopla los mapeos hacia clases independientes en Persistence/Configurations/
//   invocando modelBuilder.ApplyConfigurationsFromAssembly(...).
// =================================================================================================

using Firmeza.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.Infrastructure.Persistence;

public class ApplicationDbContext : IdentityDbContext<IdentityUser>
{
    public ApplicationDbContext()
    {
    }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected ApplicationDbContext(DbContextOptions options)
        : base(options)
    {
    }

    // ---------------------------------------------------------------------------------------------
    // CONJUNTOS DE ENTIDADES (DbSets) - Mapean directamente a tablas en PostgreSQL
    // ---------------------------------------------------------------------------------------------
    public virtual DbSet<Categoria> Categorias { get; set; } = null!;
    public virtual DbSet<Proveedor> Proveedores { get; set; } = null!;
    public virtual DbSet<MetodoPago> MetodosPago { get; set; } = null!;
    public virtual DbSet<Cliente> Clientes { get; set; } = null!;
    public virtual DbSet<Producto> Productos { get; set; } = null!;
    public virtual DbSet<Venta> Ventas { get; set; } = null!;
    public virtual DbSet<DetalleVenta> DetallesVenta { get; set; } = null!;
    public virtual DbSet<Despacho> Despachos { get; set; } = null!;
    public virtual DbSet<MovimientoInventario> MovimientosInventario { get; set; } = null!;
    public virtual DbSet<Vehiculo> Vehiculos { get; set; } = null!;
    public virtual DbSet<Alquiler> Alquileres { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // 1. Configuración obligatoria para las tablas internas de ASP.NET Core Identity
        base.OnModelCreating(modelBuilder);

        // 2. Aplicar automáticamente todas las configuraciones IEntityTypeConfiguration de este ensamblado
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
