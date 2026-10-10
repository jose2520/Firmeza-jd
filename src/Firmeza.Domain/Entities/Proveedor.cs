using System;
using System.Collections.Generic;

namespace Firmeza.Domain.Entities;

/// <summary>
/// Representa a un proveedor o fabricante aliado (ej. Cementos Argos, Gerdau Diaco, Pavco).
/// Mapea contra la tabla 'proveedores' en PostgreSQL ('firmeza_db').
/// </summary>
public partial class Proveedor
{
    public int Id { get; set; }
    public string Nit { get; set; } = null!;
    public string RazonSocial { get; set; } = null!;
    public string? ContactoNombre { get; set; }
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public string? Direccion { get; set; }
    public string? Ciudad { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime CreadoEn { get; set; }

    public virtual ICollection<Producto> Productos { get; set; } = new List<Producto>();
}
