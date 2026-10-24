// Archivo: VentaRegistradaDomainEvent.cs
// Representa el aviso automático cuando se completa exitosamente una nueva venta.
// Sirve para notificar a otros procesos comerciales sobre la factura creada y su monto total.

using Firmeza.Domain.Common;

namespace Firmeza.Domain.Events;

// ReSharper disable NotAccessedPositionalProperty.Global

// Datos del aviso que se genera al confirmar y guardar una venta para un cliente.
[System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "NotAccessedPositionalProperty.Global")]
public sealed record VentaRegistradaDomainEvent(
    int VentaId,
    string NumeroVenta,
    int ClienteId,
    decimal Total,
    DateTime OccurredOn
) : IDomainEvent;

// ReSharper restore NotAccessedPositionalProperty.Global
