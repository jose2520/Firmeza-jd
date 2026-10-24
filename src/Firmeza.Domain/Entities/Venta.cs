// Este archivo representa la factura o recibo principal de una venta realizada en la ferretería.
// En el negocio de Firmeza, sirve para agrupar quién compró, cómo pagó, en qué fecha, cuánto suma
// el subtotal, el impuesto (IVA) y el total final, además de permitir confirmar o anular la compra.

using Firmeza.Domain.Common;
using Firmeza.Domain.Events;
using Firmeza.Domain.Exceptions;

namespace Firmeza.Domain.Entities;

// Representa el encabezado de una factura o transacción de venta en la tabla 'ventas' de la base de datos.
public class Venta : AggregateRoot
{
    // Número identificador único de la venta en la base de datos.
    public int Id { get; set; }

    // Número consecutivo único de la factura o recibo de venta (por ejemplo: VENTA-2026-0001).
    public string NumeroVenta { get; set; } = null!;

    // Identificador del cliente que realizó la compra.
    public int ClienteId { get; set; }

    // Identificador del medio de pago elegido por el cliente para cancelar la factura.
    public int? MetodoPagoId { get; set; }

    // Fecha y hora exacta en que se realizó la venta.
    public DateTime Fecha { get; set; }

    // Suma del valor de todos los productos comprados antes de aplicar el impuesto.
    public decimal Subtotal { get; set; }

    // Valor en dinero correspondiente al impuesto sobre las ventas (IVA del 19%).
    public decimal Iva { get; set; }

    // Monto final a pagar por el cliente, sumando el subtotal más el impuesto (IVA).
    public decimal Total { get; set; }

    // Estado actual de la factura (puede ser Completada, Pendiente o Anulada).
    public string Estado { get; set; } = null!;

    // Información completa del cliente que hizo la compra.
    public virtual Cliente Cliente { get; set; } = null!;

    // Información completa de la forma de pago utilizada en la venta.
    public virtual MetodoPago? MetodoPago { get; set; }

    // Datos del envío a domicilio o a la obra, en caso de que la venta incluya despacho.
    public virtual Despacho? Despacho { get; set; }

    // Lista de los renglones detallados con cada producto, cantidad y precio incluidos en esta factura.
    public virtual ICollection<DetalleVenta> DetallesVenta { get; set; } = new List<DetalleVenta>();

    // Verifica que el total a cobrar sea válido, marca la factura como completada y envía el aviso de nueva venta registrada.
    public void RegistrarVenta()
    {
        if (Total < 0)
            throw new ValorInvalidoException(nameof(Total), "El total de la venta no puede ser negativo.");

        Estado = "Completada";
        AddDomainEvent(new VentaRegistradaDomainEvent(Id, NumeroVenta, ClienteId, Total, DateTime.UtcNow));
    }

    // Cambia el estado de la factura a anulada cuando se cancela la operación y envía el aviso con el motivo de la cancelación.
    public void CancelarVenta(string motivo)
    {
        if (Estado == "Anulada")
            throw new ReglaNegocioException("Venta.YaAnulada", "La venta ya se encuentra en estado anulada.");

        Estado = "Anulada";
        AddDomainEvent(new VentaCanceladaDomainEvent(Id, NumeroVenta, motivo, DateTime.UtcNow));
    }
}
