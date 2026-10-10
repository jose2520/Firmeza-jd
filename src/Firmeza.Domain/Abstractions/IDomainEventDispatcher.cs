using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Firmeza.Domain.Common;

namespace Firmeza.Domain.Abstractions;

/// <summary>
/// Contrato para el despachador de eventos de dominio.
/// Permite publicar y enrutar eventos a sus respectivos suscriptores o manejadores.
/// </summary>
public interface IDomainEventDispatcher
{
    /// <summary>
    /// Despacha una colección de eventos de dominio a sus respectivos manejadores.
    /// </summary>
    /// <param name="domainEvents">Lista de eventos de dominio acumulados.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    Task DispatchEventsAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default);

    /// <summary>
    /// Despacha un evento de dominio individual a su manejador correspondiente.
    /// </summary>
    /// <param name="domainEvent">Evento de dominio a publicar.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    Task DispatchEventAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default);
}
