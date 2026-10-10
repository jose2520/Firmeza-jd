using Firmeza.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Firmeza.Infrastructure.Persistence.Configurations;

public class CategoriaConfiguration : IEntityTypeConfiguration<Categoria>
{
    public void Configure(EntityTypeBuilder<Categoria> builder)
    {
        builder.HasKey(e => e.Id).HasName("categorias_pkey");
        builder.ToTable("categorias");

        builder.HasIndex(e => e.Nombre, "categorias_nombre_key").IsUnique();
        builder.HasIndex(e => e.Nombre, "idx_categorias_nombre");

        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.Nombre).HasMaxLength(100).HasColumnName("nombre");
        builder.Property(e => e.Descripcion).HasColumnName("descripcion");
        builder.Property(e => e.Activo).HasDefaultValue(true).HasColumnName("activo");
        builder.Property(e => e.CreadoEn).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnName("creado_en");
    }
}
