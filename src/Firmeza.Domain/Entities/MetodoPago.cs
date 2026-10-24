// Este archivo representa los medios de pago aceptados por la ferretería en sus ventas.
// En el negocio de Firmeza, sirve para registrar si el cliente pagó en efectivo, con tarjeta,
// por transferencia bancaria o a crédito, llevando un control claro del recaudo en caja.

namespace Firmeza.Domain.Entities;

// Representa una forma de pago disponible en el sistema dentro de la tabla 'metodos_pago'.
public class MetodoPago
{
    // Número identificador único del método de pago en la base de datos.
    public int Id { get; set; }

    // Código corto o abreviatura para identificar rápido el medio de pago (por ejemplo: EFE, TAR, TRANS).
    public string Codigo { get; set; } = null!;

    // Nombre claro del medio de pago (por ejemplo: Efectivo, Tarjeta Débito/Crédito, Transferencia).
    public string Nombre { get; set; } = null!;

    // Explicación o instrucciones adicionales sobre cómo se recibe este medio de pago.
    public string? Descripcion { get; set; }

    // Indica si este medio de pago está habilitado (true) para usarse en caja o si está inactivo (false).
    public bool Activo { get; set; } = true;

    // Lista de todas las ventas que han sido pagadas utilizando este medio de pago.
    public virtual ICollection<Venta> Ventas { get; set; } = new List<Venta>();
}
