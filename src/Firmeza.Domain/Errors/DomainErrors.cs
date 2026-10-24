// Archivo: DomainErrors.cs
// Reúne en un solo lugar todos los mensajes de error predefinidos del negocio.
// Sirve para que todas las pantallas y servicios muestren textos uniformes y claros al usuario.

namespace Firmeza.Domain.Errors;

// Catálogo organizado por módulos con los errores más comunes que pueden ocurrir en Firmeza.
public static class DomainErrors
{
    // Errores relacionados con el registro y búsqueda de clientes.
    public static class Cliente
    {
        // Se usa cuando el correo del cliente no tiene un formato válido.
        public static readonly DomainError EmailInvalido = DomainError.Validation(
            "Cliente.EmailInvalido", "El formato del correo electrónico suministrado no es válido.");

        // Se usa cuando el documento o NIT es demasiado corto o demasiado largo.
        public static readonly DomainError IdentificacionInvalida = DomainError.Validation(
            "Cliente.IdentificacionInvalida", "El número de identificación o NIT debe contener entre 5 y 20 caracteres.");

        // Se usa cuando se intenta registrar una edad menor a cero.
        public static readonly DomainError EdadNegativa = DomainError.Validation(
            "Cliente.EdadNegativa", "La edad del cliente no puede ser un número negativo.");

        // Se usa cuando se busca un cliente que no está registrado en el sistema.
        public static readonly DomainError NoEncontrado = DomainError.NotFound(
            "Cliente.NoEncontrado", "El cliente solicitado no existe en los registros de la empresa.");

        // Se usa cuando se intenta crear un cliente con una cédula o NIT que ya existe.
        public static readonly DomainError IdentificacionDuplicada = DomainError.Conflict(
            "Cliente.IdentificacionDuplicada", "Ya existe un cliente registrado con ese número de identificación.");
    }

    // Errores relacionados con el catálogo de materiales y control de bodega.
    public static class Producto
    {
        // Se usa cuando se intenta vender más cantidad de la que hay guardada en el almacén.
        public static readonly DomainError StockInsuficiente = DomainError.Failure(
            "Producto.StockInsuficiente", "No hay existencias físicas suficientes en almacén para despachar el pedido.");

        // Se usa cuando se intenta asignar un precio menor a cero a un producto.
        public static readonly DomainError PrecioInvalido = DomainError.Validation(
            "Producto.PrecioInvalido", "El precio del producto debe ser un importe mayor o igual a cero.");

        // Se usa cuando el código de referencia del producto tiene símbolos no permitidos.
        public static readonly DomainError CodigoInvalido = DomainError.Validation(
            "Producto.CodigoInvalido", "El código de referencia SKU contiene un formato o caracteres inválidos.");

        // Se usa cuando el producto buscado no existe en el listado de la empresa.
        public static readonly DomainError NoEncontrado = DomainError.NotFound(
            "Producto.NoEncontrado", "El producto no fue encontrado en el catálogo comercial.");

        // Se usa cuando se intenta registrar un código de producto que ya tiene otro artículo.
        public static readonly DomainError CodigoDuplicado = DomainError.Conflict(
            "Producto.CodigoDuplicado", "Ya existe un producto registrado con ese código SKU.");
    }

    // Errores relacionados con la facturación y órdenes de venta.
    public static class Venta
    {
        // Se usa cuando se intenta guardar una venta vacía sin ningún producto.
        public static readonly DomainError SinDetalles = DomainError.Validation(
            "Venta.SinDetalles", "Una transacción comercial debe incluir al menos un renglón o producto facturado.");

        // Se usa cuando la suma del subtotal más el IVA no cuadra con el total cobrado.
        public static readonly DomainError TotalInvalido = DomainError.Validation(
            "Venta.TotalInvalido", "El valor total facturado no coincide con la suma del subtotal más el IVA correspondiente.");

        // Se usa cuando se intenta cancelar una factura que ya había sido anulada antes.
        public static readonly DomainError YaAnulada = DomainError.Conflict(
            "Venta.YaAnulada", "La venta seleccionada ya se encuentra en estado anulada.");

        // Se usa cuando el número de venta buscado no aparece en el sistema.
        public static readonly DomainError NoEncontrada = DomainError.NotFound(
            "Venta.NoEncontrada", "La orden de venta solicitada no existe.");
    }

    // Errores relacionados con la flota de camiones y maquinaria pesada.
    public static class Vehiculo
    {
        // Se usa cuando la placa ingresada no cumple con el formato de letras y números.
        public static readonly DomainError PlacaInvalida = DomainError.Validation(
            "Vehiculo.PlacaInvalida", "La placa vehicular no posee una estructura válida.");

        // Se usa cuando se intenta alquilar una máquina que ya está ocupada o en taller.
        public static readonly DomainError NoDisponible = DomainError.Conflict(
            "Vehiculo.NoDisponible", "El vehículo o maquinaria solicitada no se encuentra disponible para alquiler.");

        // Se usa cuando el vehículo buscado no está registrado en la empresa.
        public static readonly DomainError NoEncontrado = DomainError.NotFound(
            "Vehiculo.NoEncontrado", "El vehículo no existe en el parque automotor de la empresa.");
    }

    // Errores relacionados con los contratos de alquiler de maquinaria.
    public static class Alquiler
    {
        // Se usa cuando se intenta crear un alquiler por cero días o un número negativo.
        public static readonly DomainError DiasInvalidos = DomainError.Validation(
            "Alquiler.DiasInvalidos", "La duración del contrato de alquiler debe ser de al menos 1 día.");

        // Se usa cuando la fecha de entrega es anterior a la fecha de inicio del alquiler.
        public static readonly DomainError FechasInconsistentes = DomainError.Validation(
            "Alquiler.FechasInconsistentes", "La fecha de finalización del contrato debe ser posterior a la fecha de inicio.");

        // Se usa cuando no se encuentra el contrato de alquiler consultado.
        public static readonly DomainError NoEncontrado = DomainError.NotFound(
            "Alquiler.NoEncontrado", "El contrato de alquiler solicitado no existe.");
    }

    // Errores relacionados con el envío y transporte de materiales a la obra.
    public static class Despacho
    {
        // Se usa cuando no se escribe la dirección a donde debe llegar el camión.
        public static readonly DomainError DireccionInvalida = DomainError.Validation(
            "Despacho.DireccionInvalida", "La dirección de entrega en obra no puede estar vacía.");

        // Se usa cuando el valor cobrado por el transporte es menor a cero.
        public static readonly DomainError FleteNegativo = DomainError.Validation(
            "Despacho.FleteNegativo", "El costo del flete de transporte no puede ser un valor negativo.");

        // Se usa cuando se intenta modificar o volver a entregar un envío que ya finalizó.
        public static readonly DomainError YaEntregado = DomainError.Conflict(
            "Despacho.YaEntregado", "El despacho ya ha sido marcado como entregado en la obra.");
    }
}
