// Define cómo se guarda la entidad Producto en la tabla "productos" de la base de datos.
// Configura sus columnas de código, nombre, precio, stock y sus relaciones con "categorias" y "proveedores".
// Asegura que el código de cada producto sea único para evitar duplicados en el inventario.
using Firmeza.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Firmeza.Infrastructure.Persistence.Configurations;

public class ProductoConfiguration : IEntityTypeConfiguration<Producto>
{
    // Configura la tabla "productos", los tamaños de texto, el precio con decimales y las relaciones opcionales.
    public void Configure(EntityTypeBuilder<Producto> builder)
    {
        builder.HasKey(e => e.Id).HasName("productos_pkey");
        builder.ToTable("productos");

        builder.HasIndex(e => e.Codigo, "idx_productos_codigo");
        builder.HasIndex(e => e.Nombre, "idx_productos_nombre");
        builder.HasIndex(e => e.CategoriaId, "idx_productos_categoria_id");
        builder.HasIndex(e => e.ProveedorId, "idx_productos_proveedor_id");
        builder.HasIndex(e => e.Codigo, "productos_codigo_key").IsUnique();

        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.Activo).HasDefaultValue(true).HasColumnName("activo");
        builder.Property(e => e.Codigo).HasMaxLength(50).HasColumnName("codigo");
        builder.Property(e => e.CreadoEn).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnName("creado_en");
        builder.Property(e => e.Descripcion).HasColumnName("descripcion");
        builder.Property(e => e.Nombre).HasMaxLength(150).HasColumnName("nombre");
        builder.Property(e => e.Precio).HasPrecision(12, 2).HasColumnName("precio");
        builder.Property(e => e.Stock).HasColumnName("stock");
        builder.Property(e => e.CategoriaId).HasColumnName("categoria_id");
        builder.Property(e => e.ProveedorId).HasColumnName("proveedor_id");

        builder.HasOne(d => d.Categoria).WithMany(p => p.Productos)
            .HasForeignKey(d => d.CategoriaId)
            .OnDelete(DeleteBehavior.SetNull)
            .HasConstraintName("fk_productos_categoria");

        builder.HasOne(d => d.Proveedor).WithMany(p => p.Productos)
            .HasForeignKey(d => d.ProveedorId)
            .OnDelete(DeleteBehavior.SetNull)
            .HasConstraintName("fk_productos_proveedor");
    }
}
