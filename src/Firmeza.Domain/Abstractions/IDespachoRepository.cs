using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Firmeza.Domain.Entities;

namespace Firmeza.Domain.Abstractions;

/// <summary>
/// Contrato de repositorio de dominio para la logística de Despachos y entregas de pedidos en obra.
/// </summary>
public interface IDespachoRepository : IRepository<Despacho>
{
    /// <summary>
    /// Obtiene el despacho asociado a una venta específica.
    /// </summary>
    Task<Despacho?> GetByVentaIdAsync(int ventaId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene todos los despachos que se encuentran pendientes de asignación o salida.
    /// </summary>
    Task<IReadOnlyList<Despacho>> GetDespachosPendientesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene los despachos que se encuentran actualmente en ruta hacia las obras de destino.
    /// </summary>
    Task<IReadOnlyList<Despacho>> GetDespachosEnRutaAsync(CancellationToken cancellationToken = default);
}
