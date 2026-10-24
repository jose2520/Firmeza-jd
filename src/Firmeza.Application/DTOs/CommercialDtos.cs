// Archivo: CommercialDtos.cs
// Qué es: Conjunto de estructuras de datos para las operaciones comerciales adicionales del negocio.
// Para qué sirve: Permite enviar y recibir información de proveedores, formas de pago, vehículos, alquileres de maquinaria, envíos y movimientos de bodega.

namespace Firmeza.Application.DTOs;

// ReSharper disable NotAccessedPositionalProperty.Global

// Muestra la información completa de un proveedor registrado en el sistema.
public record ProveedorDto(
    int Id = 0,
    string Nit = "",
    string RazonSocial = "",
    string? ContactoNombre = null,
    string? Telefono = null,
    string? Email = null,
    string? Direccion = null,
    string? Ciudad = null,
    bool Activo = true,
    DateTime CreadoEn = default
);

// Datos necesarios para registrar un nuevo proveedor comercial.
[System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "NotAccessedPositionalProperty.Global")]
public record CreateProveedorDto(
    string Nit,
    string RazonSocial,
    string? ContactoNombre = null,
    string? Telefono = null,
    string? Email = null,
    string? Direccion = null,
    string? Ciudad = null
);

// Datos para modificar la información de contacto o el estado de un proveedor existente.
public record UpdateProveedorDto(
    string Nit,
    string RazonSocial,
    string? ContactoNombre = null,
    string? Telefono = null,
    string? Email = null,
    string? Direccion = null,
    string? Ciudad = null,
    bool Activo = true
);

// Muestra los datos de una forma de pago disponible (efectivo, tarjeta, transferencia, etc.).
public record MetodoPagoDto(
    int Id = 0,
    string Codigo = "",
    string Nombre = "",
    string? Descripcion = null,
    bool Activo = true
);

// Datos requeridos para agregar una nueva forma de pago al sistema.
[System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "NotAccessedPositionalProperty.Global")]
public record CreateMetodoPagoDto(
    string Codigo,
    string Nombre,
    string? Descripcion = null
);

// Datos para actualizar el nombre, descripción o estado de una forma de pago.
public record UpdateMetodoPagoDto(
    string Codigo,
    string Nombre,
    string? Descripcion = null,
    bool Activo = true
);

// Muestra la ficha completa de un vehículo o maquinaria de construcción y su tarifa por día.
public record VehiculoDto(
    int Id = 0,
    string Placa = "",
    string Marca = "",
    string Modelo = "",
    string TipoVehiculo = "",
    decimal? CapacidadCargaTon = null,
    decimal TarifaDiaria = 0,
    string Estado = "Disponible",
    bool Activo = true,
    DateTime CreadoEn = default
);

// Datos necesarios para registrar un nuevo vehículo o maquinaria en la flota.
[System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "NotAccessedPositionalProperty.Global")]
public record CreateVehiculoDto(
    string Placa,
    string Marca,
    string Modelo,
    string TipoVehiculo,
    decimal? CapacidadCargaTon = null,
    decimal TarifaDiaria = 0
);

// Datos para actualizar las características, tarifa o disponibilidad de un vehículo.
public record UpdateVehiculoDto(
    string Marca,
    string Modelo,
    string TipoVehiculo,
    decimal? CapacidadCargaTon = null,
    decimal TarifaDiaria = 0,
    string Estado = "Disponible",
    bool Activo = true
);

// Muestra todos los detalles de un contrato de alquiler de maquinaria, incluyendo cliente, vehículo, fechas y costo total.
public record AlquilerDto(
    int Id = 0,
    string NumeroContrato = "",
    int ClienteId = 0,
    int VehiculoId = 0,
    string? NombreCliente = null,
    string? IdentificacionCliente = null,
    string? PlacaVehiculo = null,
    string? TipoVehiculo = null,
    DateTime FechaInicio = default,
    DateTime FechaFin = default,
    int DiasAlquiler = 0,
    decimal ValorDiario = 0,
    decimal Total = 0,
    string Estado = "Activo",
    DateTime CreadoEn = default
);

// Datos necesarios para crear un nuevo contrato de alquiler de un vehículo o maquinaria.
public record CreateAlquilerDto(
    string NumeroContrato,
    int ClienteId,
    int VehiculoId,
    DateTime FechaInicio,
    DateTime FechaFin,
    int DiasAlquiler,
    decimal ValorDiario
);

// Muestra la información de envío a domicilio de una venta, como dirección, contacto, costo de transporte y estado de entrega.
public record DespachoDto(
    int Id = 0,
    int VentaId = 0,
    string DireccionEntrega = "",
    string Ciudad = "",
    string ContactoRecibe = "",
    string TelefonoContacto = "",
    DateTime FechaEstimada = default,
    DateTime? FechaEntrega = null,
    decimal CostoFlete = 0,
    string Estado = "Pendiente",
    string? Observaciones = null,
    DateTime CreadoEn = default
);

// Datos que se piden para programar un nuevo envío a domicilio asociado a una venta.
[System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "NotAccessedPositionalProperty.Global")]
public record CreateDespachoDto(
    int VentaId,
    string DireccionEntrega,
    string Ciudad,
    string ContactoRecibe,
    string TelefonoContacto,
    DateTime FechaEstimada,
    decimal CostoFlete,
    string? Observaciones = null
);

// Muestra el registro de una entrada o salida de mercancía en bodega, indicando cuánto había antes y cuánto quedó después.
public record MovimientoInventarioDto(
    int Id = 0,
    int ProductoId = 0,
    string? CodigoProducto = null,
    string? NombreProducto = null,
    string TipoMovimiento = "",
    int Cantidad = 0,
    int StockPrevio = 0,
    int StockPosterior = 0,
    string? Referencia = null,
    string? Motivo = null,
    string? UsuarioId = null,
    DateTime Fecha = default
);

// Datos necesarios para registrar manualmente una entrada o salida de unidades de un producto en el inventario.
public record CreateMovimientoInventarioDto(
    int ProductoId,
    string TipoMovimiento,
    int Cantidad,
    string? Referencia = null,
    string? Motivo = null
);

// ReSharper restore NotAccessedPositionalProperty.Global

