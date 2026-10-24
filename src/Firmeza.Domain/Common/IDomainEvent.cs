// Este archivo define la estructura mínima de cualquier aviso o suceso importante ocurrido en el sistema.
// En el negocio de Firmeza, sirve para dejar constancia de hechos reales que ya pasaron (como una venta
// confirmada, un alquiler iniciado o una alerta de pocas existencias) junto con la hora exacta en que ocurrieron.

namespace Firmeza.Domain.Common;

// Representa un aviso sobre un suceso importante que acaba de ocurrir en la operación del negocio.
public interface IDomainEvent
{
    // Guarda la fecha y hora exacta (en horario universal UTC) en que ocurrió el suceso.
    DateTime OccurredOn { get; }
}
