using System;

namespace Firmeza.Domain.Abstractions;

/// <summary>
/// Contrato para el proveedor de fecha y hora del sistema.
/// Desacopla las reglas de negocio y entidades de llamadas directas a DateTime.UtcNow,
/// permitiendo tests unitarios deterministas y reproducibles.
/// </summary>
public interface IDateTimeProvider
{
    /// <summary>
    /// Fecha y hora actual en formato UTC.
    /// </summary>
    DateTime UtcNow { get; }

    /// <summary>
    /// Fecha y hora actual en la zona horaria del servidor.
    /// </summary>
    DateTime Now { get; }

    /// <summary>
    /// Fecha del día actual (sin componente horaria).
    /// </summary>
    DateOnly Today { get; }
}
