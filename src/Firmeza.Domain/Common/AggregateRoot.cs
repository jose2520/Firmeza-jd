// Este archivo proporciona una estructura base para las entidades principales del sistema (como Ventas, Productos y Alquileres).
// En el negocio de Firmeza, sirve para guardar temporalmente los avisos de sucesos importantes (por ejemplo, que se hizo una venta
// o que un producto quedó con poco stock) para procesarlos justo después de guardar en la base de datos.

namespace Firmeza.Domain.Common;

// Clase base para las entidades principales del negocio que necesitan registrar avisos de actividades importantes.
public abstract class AggregateRoot
{
    // Lista interna donde se van guardando los avisos pendientes de enviar.
    private readonly List<IDomainEvent> _domainEvents = new();

    // Lista de consulta con todos los avisos acumulados durante una operación comercial.
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    // Registra un nuevo aviso de negocio en la lista de pendientes para que sea enviado más adelante.
    public void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    // Vacía la lista de avisos una vez que todos ya fueron atendidos y procesados por el sistema.
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
