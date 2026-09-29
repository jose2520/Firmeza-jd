using System;
using System.Collections.Generic;

namespace Firmeza.Domain.Entities;

public partial class Cliente
{
    public int Id { get; set; }

    public string Identificacion { get; set; } = null!;

    public string Nombres { get; set; } = null!;

    public string Apellidos { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? Telefono { get; set; }

    public string? Direccion { get; set; }

    public bool Activo { get; set; }

    public DateTime CreadoEn { get; set; }

    public virtual ICollection<Venta> Ventas { get; set; } = new List<Venta>();
}
