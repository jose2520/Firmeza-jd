using System;
using Firmeza.Domain.Common;

namespace Firmeza.Domain.Events;

/// <summary>
/// Evento de Dominio emitido cuando los materiales salen del almacén en ruta hacia la obra.
/// </summary>
public sealed record DespachoEnRutaDomainEvent(
    int DespachoId,
    int VentaId,
    string DireccionEntrega,
    string ContactoRecibe,
    DateTime OccurredOn
) : IDomainEvent;
