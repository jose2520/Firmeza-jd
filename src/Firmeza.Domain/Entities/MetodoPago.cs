using System;
using System.Collections.Generic;

namespace Firmeza.Domain.Entities;

/// <summary>
/// Representa una forma de pago válida en las transacciones comerciales (Efectivo, Tarjeta, Transferencia, Crédito).
/// Mapea contra la tabla 'metodos_pago' en PostgreSQL ('firmeza_db').
/// </summary>
public partial class MetodoPago
{
    public int Id { get; set; }
    public string Codigo { get; set; } = null!;
    public string Nombre { get; set; } = null!;
    public string? Descripcion { get; set; }
    public bool Activo { get; set; } = true;

    public virtual ICollection<Venta> Ventas { get; set; } = new List<Venta>();
}
