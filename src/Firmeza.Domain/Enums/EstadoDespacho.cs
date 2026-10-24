// Archivo: EstadoDespacho.cs
// Define las etapas del envío de materiales hacia la obra o dirección del cliente.
// Sirve para hacer seguimiento al transporte desde que se alista en bodega hasta que se entrega.

namespace Firmeza.Domain.Enums;

// Estados que muestran el progreso de una entrega de materiales a domicilio o en obra.
public enum EstadoDespacho
{
    // El pedido está programado y esperando ser cargado en el vehículo de transporte.
    Pendiente = 1,

    // Los materiales ya van en el camión camino hacia la obra de construcción.
    EnRuta = 2,

    // El pedido fue descargado y recibido conforme por el encargado en la obra.
    Entregado = 3,

    // El envío fue suspendido por problemas en la entrega o cancelación del pedido.
    Cancelado = 4
}
