namespace Firmeza.Application.DTOs;

public record DetalleVentaDto(
    int Id,
    int ProductoId,
    string CodigoProducto,
    string NombreProducto,
    int Cantidad,
    decimal PrecioUnitario,
    decimal Subtotal
);

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

public record CreateDetalleVentaDto(
    int ProductoId,
    int Cantidad
);

public record CreateVentaDto(
    int ClienteId,
    List<CreateDetalleVentaDto> Detalles
);
