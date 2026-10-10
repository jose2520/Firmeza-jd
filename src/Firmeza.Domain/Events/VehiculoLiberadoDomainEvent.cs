using System;
using Firmeza.Domain.Common;

namespace Firmeza.Domain.Events;

/// <summary>
/// Evento de Dominio emitido cuando finaliza un contrato de alquiler y el vehículo vuelve a estar Disponible.
/// </summary>
public sealed record VehiculoLiberadoDomainEvent(
    int VehiculoId,
    string PlacaVehiculo,
    string NumeroContrato,
    DateTime OccurredOn
) : IDomainEvent;
