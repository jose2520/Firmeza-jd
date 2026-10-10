using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Firmeza.Domain.Entities;

namespace Firmeza.Domain.Abstractions;

/// <summary>
/// Contrato de repositorio de dominio para las Categorías o familias de productos.
/// </summary>
public interface ICategoriaRepository : IRepository<Categoria>
{
    /// <summary>
    /// Busca una categoría por su nombre exacto.
    /// </summary>
    Task<Categoria?> GetByNombreAsync(string nombre, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene únicamente las categorías marcadas como activas.
    /// </summary>
    Task<IReadOnlyList<Categoria>> GetActivasAsync(CancellationToken cancellationToken = default);
}
