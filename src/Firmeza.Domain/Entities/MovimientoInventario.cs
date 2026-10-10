using System;

namespace Firmeza.Domain.Entities;

/// <summary>
/// Representa una entrada, salida o ajuste en el inventario (Kardex).
/// Mapea contra la tabla 'movimientos_inventario' en PostgreSQL ('firmeza_db').
/// </summary>
public partial class MovimientoInventario
{
    public int Id { get; set; }
    public int ProductoId { get; set; }
    public string? UsuarioId { get; set; }
    public string TipoMovimiento { get; set; } = null!; // Entrada, Salida, Ajuste
    public int Cantidad { get; set; }
    public int StockPrevio { get; set; }
    public int StockPosterior { get; set; }
    public string? Referencia { get; set; }
    public string? Motivo { get; set; }
    public DateTime Fecha { get; set; }

    public virtual Producto Producto { get; set; } = null!;
}
