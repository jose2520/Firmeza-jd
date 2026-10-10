using System.Collections.Generic;

namespace Firmeza.Domain.Common;

/// <summary>
/// Clase base para Agregados Raíz (Aggregate Roots) en Domain-Driven Design.
/// Permite acumular eventos de dominio para ser publicados de manera diferida
/// tras completarse la transacción de persistencia.
/// </summary>
public abstract class AggregateRoot
{
    private readonly List<IDomainEvent> _domainEvents = new();

    /// <summary>
    /// Lista de solo lectura con los eventos de dominio acumulados por esta entidad.
    /// </summary>
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Agrega un evento de dominio a la colección pendiente de despacho.
    /// </summary>
    public void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Limpia todos los eventos de dominio una vez que han sido procesados y publicados.
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
