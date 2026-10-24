// Archivo: EstadoVenta.cs
// Define los estados por los que pasa una venta o factura comercial en el sistema.
// Sirve para distinguir entre compras por pagar, ventas ya cobradas y facturas anuladas.

namespace Firmeza.Domain.Enums;

// Estados principales de una factura o compra realizada por un cliente.
public enum EstadoVenta
{
    // La venta fue registrada pero aún está pendiente de pago o confirmación final.
    Pendiente = 1,

    // La venta fue cobrada y procesada por completo de manera exitosa.
    Completada = 2,

    // La venta fue cancelada y los productos fueron devueltos al inventario de la bodega.
    Anulada = 3
}
