using Firmeza.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.Infrastructure.Data;

public partial class ApplicationDbContext : IdentityDbContext<IdentityUser>
{
    public ApplicationDbContext()
    {
    }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Cliente> Clientes { get; set; } = null!;
    public virtual DbSet<DetalleVenta> DetallesVenta { get; set; } = null!;
    public virtual DbSet<Producto> Productos { get; set; } = null!;
    public virtual DbSet<Venta> Ventas { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("clientes_pkey");
            entity.ToTable("clientes");

            entity.HasIndex(e => e.Email, "clientes_email_key").IsUnique();
            entity.HasIndex(e => e.Identificacion, "clientes_identificacion_key").IsUnique();
            entity.HasIndex(e => e.Email, "idx_clientes_email");
            entity.HasIndex(e => e.Identificacion, "idx_clientes_identificacion");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Activo).HasDefaultValue(true).HasColumnName("activo");
            entity.Property(e => e.Apellidos).HasMaxLength(100).HasColumnName("apellidos");
            entity.Property(e => e.CreadoEn).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnName("creado_en");
            entity.Property(e => e.Direccion).HasMaxLength(200).HasColumnName("direccion");
            entity.Property(e => e.Email).HasMaxLength(150).HasColumnName("email");
            entity.Property(e => e.Identificacion).HasMaxLength(20).HasColumnName("identificacion");
            entity.Property(e => e.Nombres).HasMaxLength(100).HasColumnName("nombres");
            entity.Property(e => e.Telefono).HasMaxLength(20).HasColumnName("telefono");
        });

        modelBuilder.Entity<DetalleVenta>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("detalles_venta_pkey");
            entity.ToTable("detalles_venta");

            entity.HasIndex(e => e.ProductoId, "idx_detalles_venta_producto_id");
            entity.HasIndex(e => e.VentaId, "idx_detalles_venta_venta_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Cantidad).HasColumnName("cantidad");
            entity.Property(e => e.PrecioUnitario).HasPrecision(12, 2).HasColumnName("precio_unitario");
            entity.Property(e => e.ProductoId).HasColumnName("producto_id");
            entity.Property(e => e.Subtotal).HasPrecision(12, 2).HasColumnName("subtotal");
            entity.Property(e => e.VentaId).HasColumnName("venta_id");

            entity.HasOne(d => d.Producto).WithMany(p => p.DetallesVenta)
                .HasForeignKey(d => d.ProductoId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_detalles_producto");

            entity.HasOne(d => d.Venta).WithMany(p => p.DetallesVenta)
                .HasForeignKey(d => d.VentaId)
                .HasConstraintName("fk_detalles_venta");
        });

        modelBuilder.Entity<Producto>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("productos_pkey");
            entity.ToTable("productos");

            entity.HasIndex(e => e.Codigo, "idx_productos_codigo");
            entity.HasIndex(e => e.Nombre, "idx_productos_nombre");
            entity.HasIndex(e => e.Codigo, "productos_codigo_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Activo).HasDefaultValue(true).HasColumnName("activo");
            entity.Property(e => e.Codigo).HasMaxLength(50).HasColumnName("codigo");
            entity.Property(e => e.CreadoEn).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnName("creado_en");
            entity.Property(e => e.Descripcion).HasColumnName("descripcion");
            entity.Property(e => e.Nombre).HasMaxLength(150).HasColumnName("nombre");
            entity.Property(e => e.Precio).HasPrecision(12, 2).HasColumnName("precio");
            entity.Property(e => e.Stock).HasColumnName("stock");
        });

        modelBuilder.Entity<Venta>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("ventas_pkey");
            entity.ToTable("ventas");

            entity.HasIndex(e => e.ClienteId, "idx_ventas_cliente_id");
            entity.HasIndex(e => e.Fecha, "idx_ventas_fecha");
            entity.HasIndex(e => e.NumeroVenta, "idx_ventas_numero");
            entity.HasIndex(e => e.NumeroVenta, "ventas_numero_venta_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ClienteId).HasColumnName("cliente_id");
            entity.Property(e => e.Estado).HasMaxLength(20).HasDefaultValueSql("'Completada'::character varying").HasColumnName("estado");
            entity.Property(e => e.Fecha).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnName("fecha");
            entity.Property(e => e.Iva).HasPrecision(12, 2).HasColumnName("iva");
            entity.Property(e => e.NumeroVenta).HasMaxLength(50).HasColumnName("numero_venta");
            entity.Property(e => e.Subtotal).HasPrecision(12, 2).HasColumnName("subtotal");
            entity.Property(e => e.Total).HasPrecision(12, 2).HasColumnName("total");

            entity.HasOne(d => d.Cliente).WithMany(p => p.Ventas)
                .HasForeignKey(d => d.ClienteId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_ventas_cliente");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
