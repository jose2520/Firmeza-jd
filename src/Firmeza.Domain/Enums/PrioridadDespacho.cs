namespace Firmeza.Domain.Enums;

/// <summary>
/// Nivel de urgencia o prioridad para la entrega en obra de un despacho de materiales o maquinaria.
/// </summary>
public enum PrioridadDespacho
{
    /// <summary>
    /// Despacho normal con ventana de entrega estándar (24-48 horas).
    /// </summary>
    Baja = 1,

    /// <summary>
    /// Despacho prioritario programado para el mismo día.
    /// </summary>
    Normal = 2,

    /// <summary>
    /// Despacho de alta prioridad requerido con prelación de ruta.
    /// </summary>
    Alta = 3,

    /// <summary>
    /// Despacho urgente inmediato (obra detenida por falta de material).
    /// </summary>
    Urgente = 4
}
