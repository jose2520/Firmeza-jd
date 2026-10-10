using System;
using System.Collections.Generic;

namespace Firmeza.Domain.Entities;

/// <summary>
/// Representa la categoría o familia de productos (ej. Cementos, Aceros, Tuberías, Acabados).
/// Mapea contra la tabla 'categorias' en PostgreSQL ('firmeza_db').
/// </summary>
public partial class Categoria
{
    public int Id { get; set; }
    public string Nombre { get; set; } = null!;
    public string? Descripcion { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime CreadoEn { get; set; }

    public virtual ICollection<Producto> Productos { get; set; } = new List<Producto>();
}
