using System;

namespace Firmeza.Application.DTOs;

// =============================================================================
// DTOs PARA PROVEEDORES
// =============================================================================
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

public record CreateProveedorDto(
    string Nit,
    string RazonSocial,
    string? ContactoNombre = null,
    string? Telefono = null,
    string? Email = null,
    string? Direccion = null,
    string? Ciudad = null
);

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

// =============================================================================
// DTOs PARA MÉTODOS DE PAGO
// =============================================================================
public record MetodoPagoDto(
    int Id = 0,
    string Codigo = "",
    string Nombre = "",
    string? Descripcion = null,
    bool Activo = true
);

public record CreateMetodoPagoDto(
    string Codigo,
    string Nombre,
    string? Descripcion = null
);

// =============================================================================
// DTOs PARA VEHÍCULOS Y MAQUINARIA DE CONSTRUCCIÓN
// =============================================================================
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

public record CreateVehiculoDto(
    string Placa,
    string Marca,
    string Modelo,
    string TipoVehiculo,
    decimal? CapacidadCargaTon = null,
    decimal TarifaDiaria = 0
);

public record UpdateVehiculoDto(
    string Marca,
    string Modelo,
    string TipoVehiculo,
    decimal? CapacidadCargaTon = null,
    decimal TarifaDiaria = 0,
    string Estado = "Disponible",
    bool Activo = true
);

// =============================================================================
// DTOs PARA CONTRATOS DE ALQUILER DE MAQUINARIA (US 4)
// =============================================================================
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

public record CreateAlquilerDto(
    string NumeroContrato,
    int ClienteId,
    int VehiculoId,
    DateTime FechaInicio,
    DateTime FechaFin,
    int DiasAlquiler,
    decimal ValorDiario
);

// =============================================================================
// DTOs PARA LOGÍSTICA Y DESPACHOS
// =============================================================================
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

// =============================================================================
// DTOs PARA TRAZABILIDAD Y KARDEX DE INVENTARIO
// =============================================================================
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

public record CreateMovimientoInventarioDto(
    int ProductoId,
    string TipoMovimiento,
    int Cantidad,
    string? Referencia = null,
    string? Motivo = null
);
