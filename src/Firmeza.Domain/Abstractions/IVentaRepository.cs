using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Firmeza.Domain.Entities;

namespace Firmeza.Domain.Abstractions;

/// <summary>
/// Contrato de repositorio de dominio para órdenes de Venta y Facturación.
/// </summary>
public interface IVentaRepository : IRepository<Venta>
{
    /// <summary>
    /// Busca una venta por su número consecutivo oficial de factura (ej. VTA-2026-0001).
    /// </summary>
    Task<Venta?> GetByNumeroVentaAsync(string numeroVenta, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene una venta incluyendo sus líneas de detalle de productos y despacho asociado.
    /// </summary>
    Task<Venta?> GetVentaConDetallesAsync(int ventaId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene el historial de ventas registradas para un cliente en particular.
    /// </summary>
    Task<IReadOnlyList<Venta>> GetVentasByClienteAsync(int clienteId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene las ventas registradas dentro de un rango de fechas.
    /// </summary>
    Task<IReadOnlyList<Venta>> GetVentasByRangoFechasAsync(DateTime fechaInicio, DateTime fechaFin, CancellationToken cancellationToken = default);
}
