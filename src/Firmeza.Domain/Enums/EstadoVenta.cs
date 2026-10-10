namespace Firmeza.Domain.Enums;

/// <summary>
/// Representa el estado del ciclo de vida de una factura o transacción comercial.
/// </summary>
public enum EstadoVenta
{
    /// <summary>
    /// Venta creada pero con pago o confirmación pendiente.
    /// </summary>
    Pendiente = 1,

    /// <summary>
    /// Venta finalizada, cobrada y procesada exitosamente.
    /// </summary>
    Completada = 2,

    /// <summary>
    /// Venta cancelada o retractada; su stock ha sido reincorporado al almacén.
    /// </summary>
    Anulada = 3
}
