// Archivo: DespachoEntregadoDomainEvent.cs
// Representa el aviso automático cuando el pedido llega a la obra y es recibido por el cliente.
// Sirve para dejar constancia de la fecha y lugar exactos en que se completó la entrega.

using Firmeza.Domain.Common;

namespace Firmeza.Domain.Events;

// ReSharper disable NotAccessedPositionalProperty.Global

// Datos del aviso que confirma que el pedido fue descargado y entregado satisfactoriamente.
[System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "NotAccessedPositionalProperty.Global")]
public sealed record DespachoEntregadoDomainEvent(
    int DespachoId,
    int VentaId,
    string Ciudad,
    DateTime FechaEntrega,
    DateTime OccurredOn
) : IDomainEvent;

// ReSharper restore NotAccessedPositionalProperty.Global
