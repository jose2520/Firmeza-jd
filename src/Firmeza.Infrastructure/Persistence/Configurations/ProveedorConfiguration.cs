// Define cómo se guarda la entidad Proveedor en la tabla "proveedores" de la base de datos.
// Configura sus columnas de NIT, razón social, contacto, teléfono, correo, dirección y ciudad.
// Garantiza que el NIT sea único para que no se registre dos veces la misma empresa proveedora.
using Firmeza.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Firmeza.Infrastructure.Persistence.Configurations;

public class ProveedorConfiguration : IEntityTypeConfiguration<Proveedor>
{
    // Configura la tabla "proveedores", el tamaño de sus columnas de texto y el índice de NIT único.
    public void Configure(EntityTypeBuilder<Proveedor> builder)
    {
        builder.HasKey(e => e.Id).HasName("proveedores_pkey");
        builder.ToTable("proveedores");

        builder.HasIndex(e => e.Nit, "proveedores_nit_key").IsUnique();
        builder.HasIndex(e => e.Nit, "idx_proveedores_nit");
        builder.HasIndex(e => e.RazonSocial, "idx_proveedores_razon");

        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.Nit).HasMaxLength(30).HasColumnName("nit");
        builder.Property(e => e.RazonSocial).HasMaxLength(150).HasColumnName("razon_social");
        builder.Property(e => e.ContactoNombre).HasMaxLength(100).HasColumnName("contacto_nombre");
        builder.Property(e => e.Telefono).HasMaxLength(30).HasColumnName("telefono");
        builder.Property(e => e.Email).HasMaxLength(150).HasColumnName("email");
        builder.Property(e => e.Direccion).HasMaxLength(200).HasColumnName("direccion");
        builder.Property(e => e.Ciudad).HasMaxLength(100).HasColumnName("ciudad");
        builder.Property(e => e.Activo).HasDefaultValue(true).HasColumnName("activo");
        builder.Property(e => e.CreadoEn).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnName("creado_en");
    }
}
