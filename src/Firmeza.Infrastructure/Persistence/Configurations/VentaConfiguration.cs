using Firmeza.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Firmeza.Infrastructure.Persistence.Configurations;

public class VentaConfiguration : IEntityTypeConfiguration<Venta>
{
    public void Configure(EntityTypeBuilder<Venta> builder)
    {
        builder.HasKey(e => e.Id).HasName("ventas_pkey");
        builder.ToTable("ventas");

        builder.HasIndex(e => e.ClienteId, "idx_ventas_cliente_id");
        builder.HasIndex(e => e.MetodoPagoId, "idx_ventas_metodo_pago_id");
        builder.HasIndex(e => e.Fecha, "idx_ventas_fecha");
        builder.HasIndex(e => e.NumeroVenta, "idx_ventas_numero");
        builder.HasIndex(e => e.NumeroVenta, "ventas_numero_venta_key").IsUnique();

        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.ClienteId).HasColumnName("cliente_id");
        builder.Property(e => e.MetodoPagoId).HasColumnName("metodo_pago_id");
        builder.Property(e => e.Estado).HasMaxLength(20).HasDefaultValueSql("'Completada'::character varying").HasColumnName("estado");
        builder.Property(e => e.Fecha).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnName("fecha");
        builder.Property(e => e.Iva).HasPrecision(12, 2).HasColumnName("iva");
        builder.Property(e => e.NumeroVenta).HasMaxLength(50).HasColumnName("numero_venta");
        builder.Property(e => e.Subtotal).HasPrecision(12, 2).HasColumnName("subtotal");
        builder.Property(e => e.Total).HasPrecision(12, 2).HasColumnName("total");

        builder.HasOne(d => d.Cliente).WithMany(p => p.Ventas)
            .HasForeignKey(d => d.ClienteId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_ventas_cliente");

        builder.HasOne(d => d.MetodoPago).WithMany(p => p.Ventas)
            .HasForeignKey(d => d.MetodoPagoId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_ventas_metodo_pago");
    }
}
