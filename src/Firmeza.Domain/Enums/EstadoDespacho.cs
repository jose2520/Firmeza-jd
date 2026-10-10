namespace Firmeza.Domain.Enums;

/// <summary>
/// Representa el estado del despacho logístico y flete de materiales a pie de obra.
/// </summary>
public enum EstadoDespacho
{
    /// <summary>
    /// Despacho programado en espera de cargue en bodega.
    /// </summary>
    Pendiente = 1,

    /// <summary>
    /// Materiales cargados en vehículo y en trayecto hacia la obra de construcción.
    /// </summary>
    EnRuta = 2,

    /// <summary>
    /// Pedido descargado y recibido a satisfacción por el responsable en obra.
    /// </summary>
    Entregado = 3,

    /// <summary>
    /// Entrega cancelada por novedades en la obra o retracto comercial.
    /// </summary>
    Cancelado = 4
}
