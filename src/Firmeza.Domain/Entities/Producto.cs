using System;
using System.Collections.Generic;

namespace Firmeza.Domain.Entities;

public partial class Producto
{
    public int Id { get; set; }

    public string Codigo { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public decimal Precio { get; set; }

    public int Stock { get; set; }

    public bool Activo { get; set; }

    public DateTime CreadoEn { get; set; }

    public virtual ICollection<DetalleVenta> DetallesVenta { get; set; } = new List<DetalleVenta>();
}
