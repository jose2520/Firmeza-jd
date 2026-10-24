// Este archivo representa una categoría o familia para agrupar los productos de la ferretería.
// En el negocio de Firmeza, sirve para clasificar el catálogo (por ejemplo: Cementos, Aceros, Tuberías
// o Acabados) para que los clientes y vendedores encuentren los materiales más rápido.

namespace Firmeza.Domain.Entities;

// Representa una familia o grupo de productos dentro de la tabla 'categorias' de la base de datos.
public class Categoria
{
    // Número identificador único de la categoría en la base de datos.
    public int Id { get; set; }

    // Nombre comercial de la categoría (por ejemplo, Cementos, Herramientas o Pinturas).
    public string Nombre { get; set; } = null!;

    // Explicación breve sobre qué tipo de materiales o artículos incluye esta categoría.
    public string? Descripcion { get; set; }

    // Indica si la categoría está habilitada (true) o desactivada (false) en el catálogo.
    public bool Activo { get; set; } = true;

    // Fecha y hora en que se registró la categoría en el sistema.
    public DateTime CreadoEn { get; set; }

    // Lista de todos los productos del catálogo que pertenecen a esta categoría.
    public virtual ICollection<Producto> Productos { get; set; } = new List<Producto>();
}
