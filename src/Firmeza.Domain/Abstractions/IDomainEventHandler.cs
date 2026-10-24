// Este archivo define la estructura que debe tener cualquier encargado de responder a un aviso del negocio.
// En Firmeza, sirve para organizar las tareas automáticas que se disparan tras un suceso, como actualizar
// el estado de un vehículo cuando se inicia o termina un alquiler.

using Firmeza.Domain.Common;

namespace Firmeza.Domain.Abstractions;

// Define la plantilla para procesar un tipo específico de aviso o suceso ocurrido en el negocio.
public interface IDomainEventHandler<in TEvent> where TEvent : IDomainEvent
{
    // Ejecuta la tarea o acción correspondiente cuando recibe el aviso de que ocurrió el suceso.
    Task HandleAsync(TEvent domainEvent, CancellationToken cancellationToken = default);
}
