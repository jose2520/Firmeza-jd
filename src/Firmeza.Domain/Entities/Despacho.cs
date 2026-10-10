using System;

namespace Firmeza.Domain.Entities;

/// <summary>
/// Representa el despacho, flete o entrega en obra de los materiales de una venta.
/// Mapea contra la tabla 'despachos' en PostgreSQL ('firmeza_db').
/// </summary>
public partial class Despacho
{
    public int Id { get; set; }
    public int VentaId { get; set; }
    public string DireccionEntrega { get; set; } = null!;
    public string Ciudad { get; set; } = null!;
    public string ContactoRecibe { get; set; } = null!;
    public string TelefonoContacto { get; set; } = null!;
    public DateTime FechaEstimada { get; set; }
    public DateTime? FechaEntrega { get; set; }
    public decimal CostoFlete { get; set; }
    public string Estado { get; set; } = "Pendiente";
    public string? Observaciones { get; set; }
    public DateTime CreadoEn { get; set; }

    public virtual Venta Venta { get; set; } = null!;
}
