using System;
using Firmeza.Domain.Common;

namespace Firmeza.Domain.Events;

/// <summary>
/// Evento de Dominio emitido cuando se completa satisfactoriamente una nueva venta comercial.
/// </summary>
public sealed record VentaRegistradaDomainEvent(
    int VentaId,
    string NumeroVenta,
    int ClienteId,
    decimal Total,
    DateTime OccurredOn
) : IDomainEvent;
