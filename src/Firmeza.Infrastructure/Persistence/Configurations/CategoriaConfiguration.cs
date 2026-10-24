// Define cómo se guarda la entidad Categoria en la tabla "categorias" de la base de datos.
// Configura sus columnas como nombre, descripción, estado activo y fecha de creación.
// Garantiza que no existan dos categorías con el mismo nombre.
using Firmeza.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Firmeza.Infrastructure.Persistence.Configurations;

public class CategoriaConfiguration : IEntityTypeConfiguration<Categoria>
{
    // Configura la tabla "categorias", sus columnas, valores por defecto e índices de búsqueda por nombre.
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
