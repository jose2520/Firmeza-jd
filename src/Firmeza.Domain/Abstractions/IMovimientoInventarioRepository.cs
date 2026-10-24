// Este archivo define las operaciones para consultar el historial de entradas y salidas de mercancía.
// En el negocio de Firmeza, sirve para auditar los movimientos de bodega de cada material y revisar
// rápidamente los últimos cambios de inventario realizados.

using Firmeza.Domain.Entities;

namespace Firmeza.Domain.Abstractions;

// Define las funciones para consultar y registrar el historial de movimientos de inventario en bodega.
public interface IMovimientoInventarioRepository : IRepository<MovimientoInventario>
{
    // Obtiene todo el historial de entradas, salidas y ajustes registrados para un producto específico.
    Task<IReadOnlyList<MovimientoInventario>> GetByProductoIdAsync(int productoId, CancellationToken cancellationToken = default);

    // Obtiene los movimientos de bodega más recientes registrados en el sistema hasta la cantidad indicada.
    Task<IReadOnlyList<MovimientoInventario>> GetUltimosMovimientosAsync(int cantidad = 50, CancellationToken cancellationToken = default);
}
