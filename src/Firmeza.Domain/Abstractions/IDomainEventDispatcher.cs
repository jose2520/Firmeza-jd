// Este archivo define el encargado de enviar avisos internos cuando ocurre algo importante en el negocio.
// En Firmeza, sirve para notificar automáticamente a otras partes del sistema cuando se realiza una venta,
// se agota un producto o se alquila un vehículo.

using Firmeza.Domain.Common;

namespace Firmeza.Domain.Abstractions;

// Define las funciones para repartir avisos de actividades importantes del negocio a quienes deban procesarlos.
public interface IDomainEventDispatcher
{
    // Envía una lista de varios avisos acumulados para que el sistema ejecute las acciones correspondientes.
    Task DispatchEventsAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default);

    // Envía un único aviso del negocio para que sea atendido de inmediato por su encargado.
    Task DispatchEventAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default);
}
