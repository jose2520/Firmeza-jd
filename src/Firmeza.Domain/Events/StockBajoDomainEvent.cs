using System;
using Firmeza.Domain.Common;

namespace Firmeza.Domain.Events;

/// <summary>
/// Evento de Dominio emitido cuando el stock de un producto cae por debajo de su umbral mínimo de seguridad.
/// </summary>
public sealed record StockBajoDomainEvent(
    int ProductoId,
    string CodigoProducto,
    string NombreProducto,
    int StockRestante,
    DateTime OccurredOn
) : IDomainEvent;
