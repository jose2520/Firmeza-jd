// Este archivo define las operaciones para buscar, filtrar y validar los productos del catálogo.
// En el negocio de Firmeza, sirve para encontrar materiales por código, listar artículos por categoría,
// detectar cuáles tienen pocas unidades en bodega y evitar códigos repetidos.

using Firmeza.Domain.Entities;

namespace Firmeza.Domain.Abstractions;

// Define las funciones para consultar y administrar el catálogo de productos y materiales de construcción.
public interface IProductoRepository : IRepository<Producto>
{
    // Busca un producto específico usando su código de referencia o código de barras.
    Task<Producto?> GetByCodigoAsync(string codigo, CancellationToken cancellationToken = default);

    // Devuelve todos los productos que pertenecen a una categoría o familia determinada.
    Task<IReadOnlyList<Producto>> GetByCategoriaIdAsync(int categoriaId, CancellationToken cancellationToken = default);

    // Obtiene la lista de productos con pocas existencias en bodega para saber cuáles se deben surtir pronto.
    Task<IReadOnlyList<Producto>> GetProductosConStockBajoAsync(CancellationToken cancellationToken = default);

    // Comprueba si ya existe otro producto registrado con el mismo código de referencia.
    Task<bool> ExistsCodigoAsync(string codigo, int? excludeId = null, CancellationToken cancellationToken = default);
}
