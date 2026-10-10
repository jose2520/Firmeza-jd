using Firmeza.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Firmeza.Infrastructure.Persistence.Configurations;

public class DetalleVentaConfiguration : IEntityTypeConfiguration<DetalleVenta>
{
    public void Configure(EntityTypeBuilder<DetalleVenta> builder)
    {
        builder.HasKey(e => e.Id).HasName("detalles_venta_pkey");
        builder.ToTable("detalles_venta");

        builder.HasIndex(e => e.ProductoId, "idx_detalles_venta_producto_id");
        builder.HasIndex(e => e.VentaId, "idx_detalles_venta_venta_id");

        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.Cantidad).HasColumnName("cantidad");
        builder.Property(e => e.PrecioUnitario).HasPrecision(12, 2).HasColumnName("precio_unitario");
        builder.Property(e => e.ProductoId).HasColumnName("producto_id");
        builder.Property(e => e.Subtotal).HasPrecision(12, 2).HasColumnName("subtotal");
        builder.Property(e => e.VentaId).HasColumnName("venta_id");

        builder.HasOne(d => d.Producto).WithMany(p => p.DetallesVenta)
            .HasForeignKey(d => d.ProductoId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_detalles_producto");

        builder.HasOne(d => d.Venta).WithMany(p => p.DetallesVenta)
            .HasForeignKey(d => d.VentaId)
            .HasConstraintName("fk_detalles_venta");
    }
}
