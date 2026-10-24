// Archivo: VentaDto.cs
// Qué es: Estructuras de datos para el registro y consulta de ventas y sus productos.
// Para qué sirve: Transporta los datos de las facturas, incluyendo qué cliente compró, qué artículos llevó, las cantidades y los totales cobrados.

namespace Firmeza.Application.DTOs;

// Representa un renglón dentro de una factura, mostrando el producto comprado, la cantidad, el precio por unidad y el subtotal.
public record DetalleVentaDto(
    int Id,
    int ProductoId,
    string CodigoProducto,
    string NombreProducto,
    int Cantidad,
    decimal PrecioUnitario,
    decimal Subtotal
);

// Contiene la información completa de una factura de venta con los datos del cliente, impuestos, total y la lista de productos.
public record VentaDto(
    int Id,
    string NumeroVenta,
    int ClienteId,
    string NombreCliente,
    string IdentificacionCliente,
    DateTime Fecha,
    decimal Subtotal,
    decimal Iva,
    decimal Total,
    string Estado,
    IEnumerable<DetalleVentaDto> Detalles
);

// Indica cuál producto y cuántas unidades se quieren incluir al momento de registrar una compra nueva.
public record CreateDetalleVentaDto(
    int ProductoId,
    int Cantidad
);

// Agrupa el cliente comprador, la forma de pago y la lista de productos elegidos para crear una nueva venta.
public record CreateVentaDto(
    int ClienteId,
    List<CreateDetalleVentaDto> Detalles,
    int? MetodoPagoId = null
);
