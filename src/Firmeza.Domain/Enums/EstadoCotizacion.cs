// Archivo: EstadoCotizacion.cs
// Define las etapas por las que pasa una cotización o presupuesto enviado a un cliente.
// Sirve para saber si una oferta comercial sigue en preparación, fue aceptada o ya venció.

namespace Firmeza.Domain.Enums;

// Estados comerciales que indican el avance de una cotización de materiales o alquiler.
public enum EstadoCotizacion
{
    // El presupuesto aún está siendo redactado por el vendedor y no se ha enviado.
    Borrador = 1,

    // La cotización ya fue entregada al cliente y se espera su respuesta.
    Enviada = 2,

    // El cliente aceptó los precios y condiciones, lista para convertirse en una venta.
    Aprobada = 3,

    // El cliente decidió no aceptar la oferta comercial.
    Rechazada = 4,

    // La oferta superó el tiempo límite de validez de los precios ofrecidos.
    Vencida = 5
}
