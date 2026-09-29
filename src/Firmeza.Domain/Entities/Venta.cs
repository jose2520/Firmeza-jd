using System;
using System.Collections.Generic;

namespace Firmeza.Domain.Entities;

public partial class Venta
{
    public int Id { get; set; }

    public string NumeroVenta { get; set; } = null!;

    public int ClienteId { get; set; }

    public DateTime Fecha { get; set; }

    public decimal Subtotal { get; set; }

    public decimal Iva { get; set; }

    public decimal Total { get; set; }

    public string Estado { get; set; } = null!;

    public virtual Cliente Cliente { get; set; } = null!;

    public virtual ICollection<DetalleVenta> DetallesVenta { get; set; } = new List<DetalleVenta>();
}
