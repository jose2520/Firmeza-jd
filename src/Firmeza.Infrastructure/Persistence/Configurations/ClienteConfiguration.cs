using Firmeza.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Firmeza.Infrastructure.Persistence.Configurations;

public class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
{
    public void Configure(EntityTypeBuilder<Cliente> builder)
    {
        builder.HasKey(e => e.Id).HasName("clientes_pkey");
        builder.ToTable("clientes");

        builder.HasIndex(e => e.Email, "clientes_email_key").IsUnique();
        builder.HasIndex(e => e.Identificacion, "clientes_identificacion_key").IsUnique();
        builder.HasIndex(e => e.Email, "idx_clientes_email");
        builder.HasIndex(e => e.Identificacion, "idx_clientes_identificacion");
        builder.HasIndex(e => e.UserId, "idx_clientes_user_id");

        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.UserId).HasColumnName("user_id");
        builder.Property(e => e.Edad).HasColumnName("edad");
        builder.Property(e => e.Activo).HasDefaultValue(true).HasColumnName("activo");
        builder.Property(e => e.Apellidos).HasMaxLength(100).HasColumnName("apellidos");
        builder.Property(e => e.CreadoEn).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnName("creado_en");
        builder.Property(e => e.Direccion).HasMaxLength(200).HasColumnName("direccion");
        builder.Property(e => e.Email).HasMaxLength(150).HasColumnName("email");
        builder.Property(e => e.Identificacion).HasMaxLength(20).HasColumnName("identificacion");
        builder.Property(e => e.Nombres).HasMaxLength(100).HasColumnName("nombres");
        builder.Property(e => e.Telefono).HasMaxLength(20).HasColumnName("telefono");
    }
}
