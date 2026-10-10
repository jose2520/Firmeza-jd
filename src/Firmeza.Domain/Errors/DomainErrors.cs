namespace Firmeza.Domain.Errors;

/// <summary>
/// Catálogo centralizado de errores de negocio del dominio Firmeza.
/// Permite tipar y estandarizar las respuestas de fallo entre capas.
/// </summary>
public static class DomainErrors
{
    public static class Cliente
    {
        public static readonly DomainError EmailInvalido = DomainError.Validation(
            "Cliente.EmailInvalido", "El formato del correo electrónico suministrado no es válido.");

        public static readonly DomainError IdentificacionInvalida = DomainError.Validation(
            "Cliente.IdentificacionInvalida", "El número de identificación o NIT debe contener entre 5 y 20 caracteres.");

        public static readonly DomainError EdadNegativa = DomainError.Validation(
            "Cliente.EdadNegativa", "La edad del cliente no puede ser un número negativo.");

        public static readonly DomainError NoEncontrado = DomainError.NotFound(
            "Cliente.NoEncontrado", "El cliente solicitado no existe en los registros de la empresa.");

        public static readonly DomainError IdentificacionDuplicada = DomainError.Conflict(
            "Cliente.IdentificacionDuplicada", "Ya existe un cliente registrado con ese número de identificación.");
    }

    public static class Producto
    {
        public static readonly DomainError StockInsuficiente = DomainError.Failure(
            "Producto.StockInsuficiente", "No hay existencias físicas suficientes en almacén para despachar el pedido.");

        public static readonly DomainError PrecioInvalido = DomainError.Validation(
            "Producto.PrecioInvalido", "El precio del producto debe ser un importe mayor o igual a cero.");

        public static readonly DomainError CodigoInvalido = DomainError.Validation(
            "Producto.CodigoInvalido", "El código de referencia SKU contiene un formato o caracteres inválidos.");

        public static readonly DomainError NoEncontrado = DomainError.NotFound(
            "Producto.NoEncontrado", "El producto no fue encontrado en el catálogo comercial.");

        public static readonly DomainError CodigoDuplicado = DomainError.Conflict(
            "Producto.CodigoDuplicado", "Ya existe un producto registrado con ese código SKU.");
    }

    public static class Venta
    {
        public static readonly DomainError SinDetalles = DomainError.Validation(
            "Venta.SinDetalles", "Una transacción comercial debe incluir al menos un renglón o producto facturado.");

        public static readonly DomainError TotalInvalido = DomainError.Validation(
            "Venta.TotalInvalido", "El valor total facturado no coincide con la suma del subtotal más el IVA correspondiente.");

        public static readonly DomainError YaAnulada = DomainError.Conflict(
            "Venta.YaAnulada", "La venta seleccionada ya se encuentra en estado anulada.");

        public static readonly DomainError NoEncontrada = DomainError.NotFound(
            "Venta.NoEncontrada", "La orden de venta solicitada no existe.");
    }

    public static class Vehiculo
    {
        public static readonly DomainError PlacaInvalida = DomainError.Validation(
            "Vehiculo.PlacaInvalida", "La placa vehicular no posee una estructura válida.");

        public static readonly DomainError NoDisponible = DomainError.Conflict(
            "Vehiculo.NoDisponible", "El vehículo o maquinaria solicitada no se encuentra disponible para alquiler.");

        public static readonly DomainError NoEncontrado = DomainError.NotFound(
            "Vehiculo.NoEncontrado", "El vehículo no existe en el parque automotor de la empresa.");
    }

    public static class Alquiler
    {
        public static readonly DomainError DiasInvalidos = DomainError.Validation(
            "Alquiler.DiasInvalidos", "La duración del contrato de alquiler debe ser de al menos 1 día.");

        public static readonly DomainError FechasInconsistentes = DomainError.Validation(
            "Alquiler.FechasInconsistentes", "La fecha de finalización del contrato debe ser posterior a la fecha de inicio.");

        public static readonly DomainError NoEncontrado = DomainError.NotFound(
            "Alquiler.NoEncontrado", "El contrato de alquiler solicitado no existe.");
    }

    public static class Despacho
    {
        public static readonly DomainError DireccionInvalida = DomainError.Validation(
            "Despacho.DireccionInvalida", "La dirección de entrega en obra no puede estar vacía.");

        public static readonly DomainError FleteNegativo = DomainError.Validation(
            "Despacho.FleteNegativo", "El costo del flete de transporte no puede ser un valor negativo.");

        public static readonly DomainError YaEntregado = DomainError.Conflict(
            "Despacho.YaEntregado", "El despacho ya ha sido marcado como entregado en la obra.");
    }
}
