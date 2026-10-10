using System;
using Firmeza.Domain.Common;

namespace Firmeza.Domain.Events;

/// <summary>
/// Evento de Dominio emitido cuando se anula una factura y se reincorpora el stock al inventario.
/// </summary>
public sealed record VentaCanceladaDomainEvent(
    int VentaId,
    string NumeroVenta,
    string Motivo,
    DateTime OccurredOn
) : IDomainEvent;
