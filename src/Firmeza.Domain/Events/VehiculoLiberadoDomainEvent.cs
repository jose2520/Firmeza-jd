// Archivo: VehiculoLiberadoDomainEvent.cs
// Representa el aviso automático cuando termina un contrato de alquiler y devuelven el equipo.
// Sirve para informar que el vehículo o máquina ya regresó al patio y vuelve a estar disponible.

using Firmeza.Domain.Common;

namespace Firmeza.Domain.Events;

// ReSharper disable NotAccessedPositionalProperty.Global

// Datos del aviso que se emite cuando un vehículo alquilado queda libre nuevamente.
[System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "NotAccessedPositionalProperty.Global")]
public sealed record VehiculoLiberadoDomainEvent(
    int VehiculoId,
    string PlacaVehiculo,
    string NumeroContrato,
    DateTime OccurredOn
) : IDomainEvent;

// ReSharper restore NotAccessedPositionalProperty.Global
