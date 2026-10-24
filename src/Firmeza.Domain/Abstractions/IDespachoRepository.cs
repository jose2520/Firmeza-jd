// Este archivo define las operaciones para consultar y gestionar los envíos de materiales a las obras.
// En el negocio de Firmeza, sirve para hacer seguimiento a los pedidos que están pendientes de salir
// de bodega o que ya van en camino hacia la dirección del cliente.

using Firmeza.Domain.Entities;

namespace Firmeza.Domain.Abstractions;

// Define las funciones para consultar y controlar la logística de entregas y despachos.
public interface IDespachoRepository : IRepository<Despacho>
{
    // Obtiene la información del envío asociado a una venta específica.
    Task<Despacho?> GetByVentaIdAsync(int ventaId, CancellationToken cancellationToken = default);

    // Devuelve la lista de todos los envíos que están esperando ser despachados desde la bodega.
    Task<IReadOnlyList<Despacho>> GetDespachosPendientesAsync(CancellationToken cancellationToken = default);

    // Devuelve la lista de los envíos que ya están en camino hacia las obras de destino.
    Task<IReadOnlyList<Despacho>> GetDespachosEnRutaAsync(CancellationToken cancellationToken = default);
}
