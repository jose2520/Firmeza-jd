namespace Firmeza.Application.DTOs;

/// <summary>
/// DTO de respuesta para representar un renglón o partida dentro de una venta.
/// 
/// PARA QUÉ SIRVE:
///   Muestra el detalle del producto adquirido, la cantidad y precios pactados en la transacción.
/// 
/// ORIGEN DE LOS DATOS:
///   Proyectado desde la tabla 'detalles_venta' con JOIN a 'productos' en PostgreSQL.
/// </summary>
public record DetalleVentaDto(
    int Id,
    int ProductoId,
    string CodigoProducto,
    string NombreProducto,
    int Cantidad,
    decimal PrecioUnitario,
    decimal Subtotal
);

/// <summary>
/// DTO de respuesta para la consulta de una factura de venta completa con su cliente y partidas.
/// 
/// PARA QUÉ SIRVE:
///   Transfiere la información global de la venta (totales, impuestos, cliente y renglones).
/// 
/// ORIGEN DE LOS DATOS:
///   Proyectado desde las tablas 'ventas', 'clientes' y 'detalles_venta' en PostgreSQL.
/// </summary>
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

/// <summary>
/// DTO de solicitud para un renglón de compra al registrar una nueva venta.
/// 
/// PARA QUÉ SIRVE:
///   Especifica qué producto y cuántas unidades desea adquirir el cliente.
/// 
/// ORIGEN DE LOS DATOS:
///   Recibido dentro de la lista 'Detalles' en el cuerpo JSON de peticiones POST /api/sales.
/// </summary>
public record CreateDetalleVentaDto(
    int ProductoId,
    int Cantidad
);

/// <summary>
/// DTO de solicitud para la creación integral de una orden de venta.
/// 
/// PARA QUÉ SIRVE:
///   Agrupa el identificador del cliente comprador y la lista de artículos a facturar.
/// 
/// ORIGEN DE LOS DATOS:
///   Enviado en formato JSON en el cuerpo (Body) de peticiones POST /api/sales.
/// </summary>
public record CreateVentaDto(
    int ClienteId,
    List<CreateDetalleVentaDto> Detalles
);
