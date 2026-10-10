using Firmeza.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Firmeza.Infrastructure.Persistence.Configurations;

public class DespachoConfiguration : IEntityTypeConfiguration<Despacho>
{
    public void Configure(EntityTypeBuilder<Despacho> builder)
    {
        builder.HasKey(e => e.Id).HasName("despachos_pkey");
        builder.ToTable("despachos");

        builder.HasIndex(e => e.VentaId, "idx_despachos_venta_id").IsUnique();
        builder.HasIndex(e => e.Estado, "idx_despachos_estado");

        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.VentaId).HasColumnName("venta_id");
        builder.Property(e => e.DireccionEntrega).HasMaxLength(250).HasColumnName("direccion_entrega");
        builder.Property(e => e.Ciudad).HasMaxLength(100).HasColumnName("ciudad");
        builder.Property(e => e.ContactoRecibe).HasMaxLength(100).HasColumnName("contacto_recibe");
        builder.Property(e => e.TelefonoContacto).HasMaxLength(30).HasColumnName("telefono_contacto");
        builder.Property(e => e.FechaEstimada).HasColumnName("fecha_estimada");
        builder.Property(e => e.FechaEntrega).HasColumnName("fecha_entrega");
        builder.Property(e => e.CostoFlete).HasPrecision(12, 2).HasDefaultValue(0.00m).HasColumnName("costo_flete");
        builder.Property(e => e.Estado).HasMaxLength(30).HasDefaultValue("Pendiente").HasColumnName("estado");
        builder.Property(e => e.Observaciones).HasColumnName("observaciones");
        builder.Property(e => e.CreadoEn).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnName("creado_en");

        builder.HasOne(d => d.Venta).WithOne(v => v.Despacho)
            .HasForeignKey<Despacho>(d => d.VentaId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("fk_despachos_venta");
    }
}
