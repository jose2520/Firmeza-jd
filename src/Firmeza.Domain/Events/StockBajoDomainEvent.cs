// Archivo: StockBajoDomainEvent.cs
// Representa el aviso automático cuando quedan pocas unidades de un material en la bodega.
// Sirve para alertar al equipo de compras que es necesario pedir más mercancía al proveedor.

using Firmeza.Domain.Common;

namespace Firmeza.Domain.Events;

// ReSharper disable NotAccessedPositionalProperty.Global

// Datos de la alerta que se genera cuando un producto llega o baja de su cantidad mínima permitida.
[System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "NotAccessedPositionalProperty.Global")]
public sealed record StockBajoDomainEvent(
    int ProductoId,
    string CodigoProducto,
    string NombreProducto,
    int StockRestante,
    DateTime OccurredOn
) : IDomainEvent;

// ReSharper restore NotAccessedPositionalProperty.Global
