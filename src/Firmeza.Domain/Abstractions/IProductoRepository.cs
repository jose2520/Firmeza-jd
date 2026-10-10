using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Firmeza.Domain.Entities;

namespace Firmeza.Domain.Abstractions;

/// <summary>
/// Contrato de repositorio de dominio para el catálogo de Productos y materiales de construcción.
/// </summary>
public interface IProductoRepository : IRepository<Producto>
{
    /// <summary>
    /// Busca un producto por su código único de referencia o SKU.
    /// </summary>
    Task<Producto?> GetByCodigoAsync(string codigo, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene todos los productos pertenecientes a una categoría específica.
    /// </summary>
    Task<IReadOnlyList<Producto>> GetByCategoriaIdAsync(int categoriaId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene la lista de productos cuyo stock actual es menor o igual a su stock mínimo de seguridad.
    /// </summary>
    Task<IReadOnlyList<Producto>> GetProductosConStockBajoAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Comprueba si ya existe un producto con el código SKU dado, opcionalmente excluyendo un ID específico.
    /// </summary>
    Task<bool> ExistsCodigoAsync(string codigo, int? excludeId = null, CancellationToken cancellationToken = default);
}
