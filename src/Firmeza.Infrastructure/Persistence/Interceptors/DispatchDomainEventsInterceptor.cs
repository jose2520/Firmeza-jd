// Clase que revisa si las entidades modificadas tienen avisos o eventos pendientes al guardar en la base de datos.
// Trabaja con ApplicationDbContext y envía los eventos acumulados usando el servicio DomainEventDispatcher.
// Limpia la lista de eventos de cada entidad después de enviarlos para que no se repitan.
using Firmeza.Domain.Abstractions;
using Firmeza.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Firmeza.Infrastructure.Persistence.Interceptors;

public class DispatchDomainEventsInterceptor : SaveChangesInterceptor
{
    private readonly IDomainEventDispatcher? _dispatcher;

    // Recibe el encargado de enviar los eventos del sistema cuando se guardan datos.
    public DispatchDomainEventsInterceptor(IDomainEventDispatcher? dispatcher = null)
    {
        _dispatcher = dispatcher;
    }

    // Revisa y envía los eventos pendientes justo antes de un guardado normal en la base de datos.
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        DispatchEvents(eventData.Context).GetAwaiter().GetResult();
        return base.SavingChanges(eventData, result);
    }

    // Revisa y envía los eventos pendientes justo antes de un guardado asíncrono en la base de datos.
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        await DispatchEvents(eventData.Context, cancellationToken);
        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    // Busca todas las entidades con eventos pendientes, limpia su lista y los envía al despachador.
    private async Task DispatchEvents(DbContext? context, CancellationToken cancellationToken = default)
    {
        if (context == null || _dispatcher == null) return;

        var aggregateRoots = context.ChangeTracker
            .Entries<AggregateRoot>()
            .Where(e => e.Entity.DomainEvents.Any())
            .Select(e => e.Entity)
            .ToList();

        var domainEvents = aggregateRoots
            .SelectMany(a => a.DomainEvents)
            .ToList();

        foreach (var aggregateRoot in aggregateRoots)
        {
            aggregateRoot.ClearDomainEvents();
        }

        if (domainEvents.Any())
        {
            await _dispatcher.DispatchEventsAsync(domainEvents, cancellationToken);
        }
    }
}
