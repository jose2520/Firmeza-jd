// Este archivo representa el servicio de envío y entrega a domicilio o en obra de una venta.
// En el negocio de Firmeza, sirve para anotar la dirección de destino, quién recibe los materiales,
// el costo del transporte, las fechas de entrega y el estado actual del envío.

namespace Firmeza.Domain.Entities;

// Representa los datos logísticos de un envío o entrega de materiales en la tabla 'despachos'.
public class Despacho
{
    // Número identificador único del despacho en la base de datos.
    public int Id { get; set; }

    // Identificador de la venta o factura cuyos materiales se van a enviar.
    public int VentaId { get; set; }

    // Dirección exacta de la obra o lugar donde se deben entregar los materiales.
    public string DireccionEntrega { get; set; } = null!;

    // Ciudad o municipio de destino del envío.
    public string Ciudad { get; set; } = null!;

    // Nombre completo de la persona encargada de recibir la mercancía en la obra.
    public string ContactoRecibe { get; set; } = null!;

    // Número de teléfono de la persona que recibe el pedido para coordinar la llegada.
    public string TelefonoContacto { get; set; } = null!;

    // Fecha y hora programada en la que se calcula que llegará el pedido.
    public DateTime FechaEstimada { get; set; }

    // Fecha y hora real en la que se entregaron los materiales al cliente.
    public DateTime? FechaEntrega { get; set; }

    // Valor cobrado por el servicio de transporte o flete hasta el lugar de entrega.
    public decimal CostoFlete { get; set; }

    // Situación actual del envío (por ejemplo: Pendiente, En Ruta o Entregado).
    public string Estado { get; set; } = "Pendiente";

    // Notas o indicaciones adicionales para el conductor o el equipo de bodega.
    public string? Observaciones { get; set; }

    // Fecha y hora en que se registró la orden de despacho en el sistema.
    public DateTime CreadoEn { get; set; }

    // Información completa de la venta a la que pertenece este envío.
    public virtual Venta Venta { get; set; } = null!;
}
