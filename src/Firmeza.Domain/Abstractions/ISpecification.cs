// Este archivo define una plantilla para crear reglas de búsqueda y filtros reutilizables.
// En el negocio de Firmeza, sirve para escribir condiciones claras (por ejemplo, qué productos tienen
// stock bajo o qué vehículos están disponibles) y aplicarlas tanto en base de datos como en memoria.

using System.Linq.Expressions;

namespace Firmeza.Domain.Abstractions;

// Define la estructura para armar reglas de filtrado y comprobar si un registro cumple con ellas.
public interface ISpecification<T>
{
    // Convierte la regla de negocio en una condición de búsqueda para filtrar datos en la base de datos.
    Expression<Func<T, bool>> ToExpression();

    // Comprueba directamente si un elemento en particular cumple o no con esta regla.
    bool IsSatisfiedBy(T entity);
}
