using System;
using Firmeza.Domain.Common;

namespace Firmeza.Domain.Events;

/// <summary>
/// Evento de Dominio emitido cuando el pedido de materiales es recibido y firmado a satisfacción en la obra.
/// </summary>
public sealed record DespachoEntregadoDomainEvent(
    int DespachoId,
    int VentaId,
    string Ciudad,
    DateTime FechaEntrega,
    DateTime OccurredOn
) : IDomainEvent;
