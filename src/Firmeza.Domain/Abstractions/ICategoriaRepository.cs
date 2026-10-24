// Este archivo define las operaciones para consultar y administrar las categorías de productos.
// En el negocio de Firmeza, sirve para organizar el catálogo ferretero por familias (como cementos,
// aceros o herramientas) y facilitar la búsqueda de materiales activos.

using Firmeza.Domain.Entities;

namespace Firmeza.Domain.Abstractions;

// Define las funciones para buscar y guardar las categorías o familias de productos.
public interface ICategoriaRepository : IRepository<Categoria>
{
    // Busca una categoría específica por su nombre exacto.
    Task<Categoria?> GetByNombreAsync(string nombre, CancellationToken cancellationToken = default);

    // Obtiene únicamente la lista de categorías que están habilitadas para usarse en el catálogo.
    Task<IReadOnlyList<Categoria>> GetActivasAsync(CancellationToken cancellationToken = default);
}
