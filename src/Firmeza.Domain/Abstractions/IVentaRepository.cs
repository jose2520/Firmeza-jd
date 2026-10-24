// Este archivo define las operaciones para consultar y guardar las ventas y facturas del negocio.
// En Firmeza, sirve para buscar recibos por su número consecutivo, ver el detalle completo de los
// productos comprados, revisar las compras de un cliente o sacar reportes por fechas.

using Firmeza.Domain.Entities;

namespace Firmeza.Domain.Abstractions;

// Define las funciones para consultar y registrar las órdenes de venta y facturación.
public interface IVentaRepository : IRepository<Venta>
{
    // Busca una venta específica usando su número consecutivo de factura (por ejemplo, VTA-2026-0001).
    Task<Venta?> GetByNumeroVentaAsync(string numeroVenta, CancellationToken cancellationToken = default);

    // Obtiene una venta completa incluyendo la lista de productos comprados y los datos de su envío.
    Task<Venta?> GetVentaConDetallesAsync(int ventaId, CancellationToken cancellationToken = default);

    // Devuelve todo el historial de compras realizadas por un cliente específico.
    Task<IReadOnlyList<Venta>> GetVentasByClienteAsync(int clienteId, CancellationToken cancellationToken = default);

    // Obtiene todas las ventas registradas entre una fecha de inicio y una fecha final.
    Task<IReadOnlyList<Venta>> GetVentasByRangoFechasAsync(DateTime fechaInicio, DateTime fechaFin, CancellationToken cancellationToken = default);
}
