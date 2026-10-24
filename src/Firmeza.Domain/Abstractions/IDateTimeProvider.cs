// Este archivo define un servicio para obtener la fecha y hora actual dentro del sistema.
// En el negocio de Firmeza, sirve para registrar con exactitud cuándo se hacen las ventas,
// alquileres o movimientos de bodega, facilitando también las pruebas automáticas del sistema.

namespace Firmeza.Domain.Abstractions;

// Define cómo el sistema consulta la fecha y hora actual de manera centralizada.
public interface IDateTimeProvider
{
    // Devuelve la fecha y hora actual en horario universal estándar (UTC).
    DateTime UtcNow { get; }

    // Devuelve la fecha y hora actual según el reloj local del servidor.
    DateTime Now { get; }

    // Devuelve únicamente la fecha del día de hoy, sin incluir la hora.
    DateOnly Today { get; }
}
