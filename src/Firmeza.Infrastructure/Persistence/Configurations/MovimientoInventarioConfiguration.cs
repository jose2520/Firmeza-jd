using Firmeza.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Firmeza.Infrastructure.Persistence.Configurations;

public class MovimientoInventarioConfiguration : IEntityTypeConfiguration<MovimientoInventario>
{
    public void Configure(EntityTypeBuilder<MovimientoInventario> builder)
    {
        builder.HasKey(e => e.Id).HasName("movimientos_inventario_pkey");
        builder.ToTable("movimientos_inventario");

        builder.HasIndex(e => e.ProductoId, "idx_movimientos_producto_id");
        builder.HasIndex(e => e.Fecha, "idx_movimientos_fecha");

        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.ProductoId).HasColumnName("producto_id");
        builder.Property(e => e.UsuarioId).HasColumnName("usuario_id");
        builder.Property(e => e.TipoMovimiento).HasMaxLength(20).HasColumnName("tipo_movimiento");
        builder.Property(e => e.Cantidad).HasColumnName("cantidad");
        builder.Property(e => e.StockPrevio).HasColumnName("stock_previo");
        builder.Property(e => e.StockPosterior).HasColumnName("stock_posterior");
        builder.Property(e => e.Referencia).HasMaxLength(100).HasColumnName("referencia");
        builder.Property(e => e.Motivo).HasColumnName("motivo");
        builder.Property(e => e.Fecha).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnName("fecha");

        builder.HasOne(m => m.Producto).WithMany(p => p.MovimientosInventario)
            .HasForeignKey(m => m.ProductoId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_movimientos_producto");
    }
}
