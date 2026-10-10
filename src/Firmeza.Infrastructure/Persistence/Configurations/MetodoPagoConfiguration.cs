using Firmeza.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Firmeza.Infrastructure.Persistence.Configurations;

public class MetodoPagoConfiguration : IEntityTypeConfiguration<MetodoPago>
{
    public void Configure(EntityTypeBuilder<MetodoPago> builder)
    {
        builder.HasKey(e => e.Id).HasName("metodos_pago_pkey");
        builder.ToTable("metodos_pago");

        builder.HasIndex(e => e.Codigo, "metodos_pago_codigo_key").IsUnique();
        builder.HasIndex(e => e.Codigo, "idx_metodos_pago_codigo");

        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.Codigo).HasMaxLength(20).HasColumnName("codigo");
        builder.Property(e => e.Nombre).HasMaxLength(100).HasColumnName("nombre");
        builder.Property(e => e.Descripcion).HasColumnName("descripcion");
        builder.Property(e => e.Activo).HasDefaultValue(true).HasColumnName("activo");
    }
}
