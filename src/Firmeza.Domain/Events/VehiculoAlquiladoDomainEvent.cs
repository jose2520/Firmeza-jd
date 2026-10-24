// Archivo: VehiculoAlquiladoDomainEvent.cs
// Representa el aviso automático cuando se firma y activa un alquiler de vehículo o maquinaria.
// Sirve para registrar el inicio del servicio y actualizar el estado del equipo a ocupado.

using Firmeza.Domain.Common;

namespace Firmeza.Domain.Events;

// ReSharper disable NotAccessedPositionalProperty.Global

// Datos del aviso que informa sobre un nuevo contrato de alquiler de maquinaria con un cliente.
[System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "NotAccessedPositionalProperty.Global")]
public sealed record VehiculoAlquiladoDomainEvent(
    int AlquilerId,
    string NumeroContrato,
    int VehiculoId,
    string PlacaVehiculo,
    int ClienteId,
    int DiasAlquiler,
    decimal Total,
    DateTime OccurredOn
) : IDomainEvent;

// ReSharper restore NotAccessedPositionalProperty.Global
