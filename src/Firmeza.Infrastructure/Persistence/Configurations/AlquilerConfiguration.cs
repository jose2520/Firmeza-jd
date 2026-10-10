using Firmeza.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Firmeza.Infrastructure.Persistence.Configurations;

public class AlquilerConfiguration : IEntityTypeConfiguration<Alquiler>
{
    public void Configure(EntityTypeBuilder<Alquiler> builder)
    {
        builder.HasKey(e => e.Id).HasName("alquileres_pkey");
        builder.ToTable("alquileres");

        builder.HasIndex(e => e.NumeroContrato, "alquileres_numero_contrato_key").IsUnique();
        builder.HasIndex(e => e.NumeroContrato, "idx_alquileres_numero");
        builder.HasIndex(e => e.ClienteId, "idx_alquileres_cliente_id");
        builder.HasIndex(e => e.VehiculoId, "idx_alquileres_vehiculo_id");

        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.NumeroContrato).HasMaxLength(50).HasColumnName("numero_contrato");
        builder.Property(e => e.ClienteId).HasColumnName("cliente_id");
        builder.Property(e => e.VehiculoId).HasColumnName("vehiculo_id");
        builder.Property(e => e.FechaInicio).HasColumnName("fecha_inicio");
        builder.Property(e => e.FechaFin).HasColumnName("fecha_fin");
        builder.Property(e => e.DiasAlquiler).HasColumnName("dias_alquiler");
        builder.Property(e => e.ValorDiario).HasPrecision(12, 2).HasColumnName("valor_diario");
        builder.Property(e => e.Total).HasPrecision(12, 2).HasColumnName("total");
        builder.Property(e => e.Estado).HasMaxLength(30).HasDefaultValue("Activo").HasColumnName("estado");
        builder.Property(e => e.CreadoEn).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnName("creado_en");

        builder.HasOne(a => a.Cliente).WithMany(c => c.Alquileres)
            .HasForeignKey(a => a.ClienteId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_alquileres_cliente");

        builder.HasOne(a => a.Vehiculo).WithMany(v => v.Alquileres)
            .HasForeignKey(a => a.VehiculoId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_alquileres_vehiculo");
    }
}
