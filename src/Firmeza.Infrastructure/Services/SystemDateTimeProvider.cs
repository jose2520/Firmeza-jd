// Servicio sencillo que entrega la fecha y hora actual del reloj del servidor.
// Implementa la interfaz IDateTimeProvider para que todos los servicios y repositorios del sistema
// consulten el tiempo actual de manera uniforme.
using Firmeza.Domain.Abstractions;

namespace Firmeza.Infrastructure.Services;

public class SystemDateTimeProvider : IDateTimeProvider
{
    // Devuelve la fecha y hora universal actual (UTC).
    public DateTime UtcNow => DateTime.UtcNow;

    // Devuelve la fecha y hora local del servidor.
    public DateTime Now => DateTime.Now;

    // Devuelve únicamente la fecha del día actual sin la hora.
    public DateOnly Today => DateOnly.FromDateTime(DateTime.UtcNow);
}
