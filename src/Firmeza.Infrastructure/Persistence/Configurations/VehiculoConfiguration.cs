using Firmeza.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Firmeza.Infrastructure.Persistence.Configurations;

public class VehiculoConfiguration : IEntityTypeConfiguration<Vehiculo>
{
    public void Configure(EntityTypeBuilder<Vehiculo> builder)
    {
        builder.HasKey(e => e.Id).HasName("vehiculos_pkey");
        builder.ToTable("vehiculos");

        builder.HasIndex(e => e.Placa, "vehiculos_placa_key").IsUnique();
        builder.HasIndex(e => e.Placa, "idx_vehiculos_placa");
        builder.HasIndex(e => e.Estado, "idx_vehiculos_estado");

        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.Placa).HasMaxLength(20).HasColumnName("placa");
        builder.Property(e => e.Marca).HasMaxLength(50).HasColumnName("marca");
        builder.Property(e => e.Modelo).HasMaxLength(50).HasColumnName("modelo");
        builder.Property(e => e.TipoVehiculo).HasMaxLength(50).HasColumnName("tipo_vehiculo");
        builder.Property(e => e.CapacidadCargaTon).HasPrecision(6, 2).HasColumnName("capacidad_carga_ton");
        builder.Property(e => e.TarifaDiaria).HasPrecision(12, 2).HasColumnName("tarifa_diaria");
        builder.Property(e => e.Estado).HasMaxLength(30).HasDefaultValue("Disponible").HasColumnName("estado");
        builder.Property(e => e.Activo).HasDefaultValue(true).HasColumnName("activo");
        builder.Property(e => e.CreadoEn).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnName("creado_en");
    }
}
