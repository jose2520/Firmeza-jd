// Servicio encargado de repartir los avisos o eventos internos que ocurren en las entidades del negocio.
// Trabaja junto con DispatchDomainEventsInterceptor cuando se guardan cambios en la base de datos.
// Registra en el historial del sistema cada evento ocurrido y avisa a las clases encargadas de procesarlo.
using Firmeza.Domain.Abstractions;
using Firmeza.Domain.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Firmeza.Infrastructure.Services;

public class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DomainEventDispatcher> _logger;

    // Recibe el proveedor de servicios del sistema y el registrador de mensajes de seguimiento.
    public DomainEventDispatcher(IServiceProvider serviceProvider, ILogger<DomainEventDispatcher> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // Recorre una lista de eventos ocurridos y los envía uno por uno para que sean atendidos.
    public async Task DispatchEventsAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(domainEvents);

        foreach (var domainEvent in domainEvents)
        {
            await DispatchEventAsync(domainEvent, cancellationToken);
        }
    }

    // Registra un evento en la consola de seguimiento y ejecuta las acciones asociadas a ese tipo de evento.
    public async Task DispatchEventAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);

        var eventType = domainEvent.GetType();
        _logger.LogInformation("Evento de dominio emitido: {EventType} en {OccurredOn}", eventType.Name, domainEvent.OccurredOn);

        var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(eventType);
        var handlers = _serviceProvider.GetServices(handlerType);

        foreach (var handler in handlers)
        {
            if (handler == null) continue;
            var method = handlerType.GetMethod(nameof(IDomainEventHandler<IDomainEvent>.HandleAsync));
            if (method != null)
            {
                var task = (Task?)method.Invoke(handler, new object[] { domainEvent, cancellationToken });
                if (task != null)
                {
                    await task;
                }
            }
        }
    }
}
