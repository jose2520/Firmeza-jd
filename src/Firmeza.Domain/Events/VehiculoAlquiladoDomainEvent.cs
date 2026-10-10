using System;
using Firmeza.Domain.Common;

namespace Firmeza.Domain.Events;

/// <summary>
/// Evento de Dominio emitido cuando se suscribe un contrato de alquiler de maquinaria o vehículo.
/// </summary>
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
