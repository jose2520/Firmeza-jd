// Archivo: DespachoEnRutaDomainEvent.cs
// Representa el aviso automático cuando un camión sale de la bodega rumbo a la obra.
// Sirve para notificar a otros módulos que el pedido ya va en camino hacia el cliente.

using Firmeza.Domain.Common;

namespace Firmeza.Domain.Events;

// ReSharper disable NotAccessedPositionalProperty.Global

// Datos del aviso que se genera cuando los materiales salen en ruta hacia la dirección de entrega.
[System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "NotAccessedPositionalProperty.Global")]
public sealed record DespachoEnRutaDomainEvent(
    int DespachoId,
    int VentaId,
    string DireccionEntrega,
    string ContactoRecibe,
    DateTime OccurredOn
) : IDomainEvent;

// ReSharper restore NotAccessedPositionalProperty.Global
