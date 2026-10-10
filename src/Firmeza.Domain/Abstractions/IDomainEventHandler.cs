using System.Threading;
using System.Threading.Tasks;
using Firmeza.Domain.Common;

namespace Firmeza.Domain.Abstractions;

/// <summary>
/// Contrato para un manejador de un evento de dominio específico.
/// </summary>
/// <typeparam name="TEvent">Tipo del evento de dominio manejado.</typeparam>
public interface IDomainEventHandler<in TEvent> where TEvent : IDomainEvent
{
    /// <summary>
    /// Maneja la ocurrencia del evento de dominio de manera asíncrona.
    /// </summary>
    /// <param name="domainEvent">Instancia del evento ocurrido.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    Task HandleAsync(TEvent domainEvent, CancellationToken cancellationToken = default);
}
