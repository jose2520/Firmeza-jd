using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Firmeza.Domain.Entities;

namespace Firmeza.Domain.Abstractions;

/// <summary>
/// Contrato de repositorio de dominio para el Kardex y la trazabilidad de Movimientos de Inventario.
/// </summary>
public interface IMovimientoInventarioRepository : IRepository<MovimientoInventario>
{
    /// <summary>
    /// Obtiene el historial completo de movimientos registrados para un producto determinado.
    /// </summary>
    Task<IReadOnlyList<MovimientoInventario>> GetByProductoIdAsync(int productoId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene los movimientos de inventario más recientes registrados en el sistema.
    /// </summary>
    Task<IReadOnlyList<MovimientoInventario>> GetUltimosMovimientosAsync(int cantidad = 50, CancellationToken cancellationToken = default);
}
