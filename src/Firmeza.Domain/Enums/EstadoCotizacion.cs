namespace Firmeza.Domain.Enums;

/// <summary>
/// Estados del ciclo de vida comercial de una cotización o presupuesto para clientes.
/// </summary>
public enum EstadoCotizacion
{
    /// <summary>
    /// Cotización en elaboración por el asesor comercial.
    /// </summary>
    Borrador = 1,

    /// <summary>
    /// Cotización enviada al cliente pendiente de revisión o respuesta.
    /// </summary>
    Enviada = 2,

    /// <summary>
    /// Cotización aprobada por el cliente y lista para convertirse en orden de venta.
    /// </summary>
    Aprobada = 3,

    /// <summary>
    /// Cotización rechazada o declinada por el cliente.
    /// </summary>
    Rechazada = 4,

    /// <summary>
    /// Cotización que superó los días de validez de precios (ej. variación precio del acero/cemento).
    /// </summary>
    Vencida = 5
}
