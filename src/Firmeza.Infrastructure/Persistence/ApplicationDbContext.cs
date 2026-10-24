// Archivo principal que conecta la aplicación con la base de datos PostgreSQL.
// Maneja las tablas de usuarios y roles de seguridad junto con las tablas del negocio:
// categorias, proveedores, metodos_pago, clientes, productos, ventas, detalles_venta,
// despachos, movimientos_inventario, vehiculos y alquileres.
using Firmeza.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.Infrastructure.Persistence;

public class ApplicationDbContext : IdentityDbContext<IdentityUser>
{
    // Constructor vacío para inicializar el contexto sin opciones previas.
    public ApplicationDbContext()
    {
    }

    // Constructor principal que recibe la configuración de conexión a la base de datos.
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // Constructor protegido para permitir que otras clases como AppDbContext hereden de esta.
    protected ApplicationDbContext(DbContextOptions options)
        : base(options)
    {
    }

    // Tablas del negocio disponibles para consultas y guardados en PostgreSQL
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

    // Configura las tablas de usuarios y aplica las reglas de todas las tablas de la carpeta Configurations.
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // 1. Configuración obligatoria para las tablas internas de usuarios y roles
        base.OnModelCreating(modelBuilder);

        // 2. Aplicar automáticamente todas las configuraciones de tablas definidas en este proyecto
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
