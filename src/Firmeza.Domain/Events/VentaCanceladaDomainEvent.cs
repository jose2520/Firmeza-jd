// Archivo: VentaCanceladaDomainEvent.cs
// Representa el aviso automático cuando se anula una factura o compra ya registrada.
// Sirve para dejar constancia del motivo de cancelación y devolver los productos a la bodega.

using Firmeza.Domain.Common;

namespace Firmeza.Domain.Events;

// ReSharper disable NotAccessedPositionalProperty.Global

// Datos del aviso que informa la anulación de una venta y el motivo por el cual se canceló.
[System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "NotAccessedPositionalProperty.Global")]
public sealed record VentaCanceladaDomainEvent(
    int VentaId,
    string NumeroVenta,
    string Motivo,
    DateTime OccurredOn
) : IDomainEvent;

// ReSharper restore NotAccessedPositionalProperty.Global
