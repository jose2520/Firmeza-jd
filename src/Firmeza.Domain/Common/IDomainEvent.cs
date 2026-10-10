using System;

namespace Firmeza.Domain.Common;

/// <summary>
/// Contrato que define un Evento de Dominio (Domain Event).
/// Representa un acontecimiento relevante que ya sucedió en el negocio (hecho inmutable).
/// </summary>
public interface IDomainEvent
{
    /// <summary>
    /// Momento exacto en tiempo UTC en que ocurrió el evento de dominio.
    /// </summary>
    DateTime OccurredOn { get; }
}
