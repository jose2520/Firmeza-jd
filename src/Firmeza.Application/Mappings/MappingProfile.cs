// =================================================================================================
// PERFILES DE MAPEO: MappingProfile.cs
// CAPA: Firmeza.Application (Mapeos y Transformaciones)
// PROPÓSITO:
//   Define las reglas de AutoMapper para transformar automáticamente entidades del Dominio
//   hacia Data Transfer Objects (DTOs) y viceversa, satisfaciendo el requerimiento de M6.3S3 (Tarea 4).
// =================================================================================================

using System;
using AutoMapper;
using Firmeza.Application.DTOs;
using Firmeza.Domain.Entities;

namespace Firmeza.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // 1. Mapeos de Cliente
        CreateMap<Cliente, ClienteDto>();
        CreateMap<CreateClienteDto, Cliente>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Activo, opt => opt.MapFrom(_ => true))
            .ForMember(dest => dest.CreadoEn, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.Ventas, opt => opt.Ignore())
            .ForMember(dest => dest.Alquileres, opt => opt.Ignore());
        CreateMap<UpdateClienteDto, Cliente>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Identificacion, opt => opt.Ignore())
            .ForMember(dest => dest.CreadoEn, opt => opt.Ignore())
            .ForMember(dest => dest.Ventas, opt => opt.Ignore())
            .ForMember(dest => dest.Alquileres, opt => opt.Ignore());

        // 2. Mapeos de Producto
        CreateMap<Producto, ProductoDto>()
            .ForMember(dest => dest.CategoriaNombre, opt => opt.MapFrom(src => src.Categoria != null ? src.Categoria.Nombre : null));
        CreateMap<CreateProductoDto, Producto>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Activo, opt => opt.MapFrom(_ => true))
            .ForMember(dest => dest.CreadoEn, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.ProveedorId, opt => opt.Ignore())
            .ForMember(dest => dest.Categoria, opt => opt.Ignore())
            .ForMember(dest => dest.Proveedor, opt => opt.Ignore())
            .ForMember(dest => dest.DetallesVenta, opt => opt.Ignore())
            .ForMember(dest => dest.MovimientosInventario, opt => opt.Ignore());
        CreateMap<UpdateProductoDto, Producto>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Codigo, opt => opt.Ignore())
            .ForMember(dest => dest.CreadoEn, opt => opt.Ignore())
            .ForMember(dest => dest.ProveedorId, opt => opt.Ignore())
            .ForMember(dest => dest.Categoria, opt => opt.Ignore())
            .ForMember(dest => dest.Proveedor, opt => opt.Ignore())
            .ForMember(dest => dest.DetallesVenta, opt => opt.Ignore())
            .ForMember(dest => dest.MovimientosInventario, opt => opt.Ignore());

        // 3. Mapeos de DetalleVenta
        CreateMap<DetalleVenta, DetalleVentaDto>()
            .ForMember(dest => dest.CodigoProducto, opt => opt.MapFrom(src => src.Producto != null ? src.Producto.Codigo : string.Empty))
            .ForMember(dest => dest.NombreProducto, opt => opt.MapFrom(src => src.Producto != null ? src.Producto.Nombre : string.Empty));

        // 4. Mapeos de Venta
        CreateMap<Venta, VentaDto>()
            .ForMember(dest => dest.NombreCliente, opt => opt.MapFrom(src => src.Cliente != null ? $"{src.Cliente.Nombres} {src.Cliente.Apellidos}".Trim() : string.Empty))
            .ForMember(dest => dest.IdentificacionCliente, opt => opt.MapFrom(src => src.Cliente != null ? src.Cliente.Identificacion : string.Empty))
            .ForMember(dest => dest.Detalles, opt => opt.MapFrom(src => src.DetallesVenta));

        // 5. Mapeos de Categoria
        CreateMap<Categoria, CategoriaDto>()
            .ForCtorParam(nameof(CategoriaDto.TotalProductos), opt => opt.MapFrom(src => src.Productos != null ? src.Productos.Count : 0))
            .ForMember(dest => dest.TotalProductos, opt => opt.MapFrom(src => src.Productos != null ? src.Productos.Count : 0));
        CreateMap<CreateCategoriaDto, Categoria>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Activo, opt => opt.MapFrom(_ => true))
            .ForMember(dest => dest.CreadoEn, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.Productos, opt => opt.Ignore());

        // 6. Mapeos de Proveedor
        CreateMap<Proveedor, ProveedorDto>();
        CreateMap<CreateProveedorDto, Proveedor>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Activo, opt => opt.MapFrom(_ => true))
            .ForMember(dest => dest.CreadoEn, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.Productos, opt => opt.Ignore());

        // 7. Mapeos de MetodoPago
        CreateMap<MetodoPago, MetodoPagoDto>();
        CreateMap<CreateMetodoPagoDto, MetodoPago>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Activo, opt => opt.MapFrom(_ => true))
            .ForMember(dest => dest.Ventas, opt => opt.Ignore());

        // 8. Mapeos de Vehiculo
        CreateMap<Vehiculo, VehiculoDto>();
        CreateMap<CreateVehiculoDto, Vehiculo>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Estado, opt => opt.MapFrom(_ => "Disponible"))
            .ForMember(dest => dest.Activo, opt => opt.MapFrom(_ => true))
            .ForMember(dest => dest.CreadoEn, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.Alquileres, opt => opt.Ignore());

        // 9. Mapeos de Alquiler
        CreateMap<Alquiler, AlquilerDto>()
            .ForCtorParam(nameof(AlquilerDto.NombreCliente), opt => opt.MapFrom(src => src.Cliente != null ? $"{src.Cliente.Nombres} {src.Cliente.Apellidos}".Trim() : string.Empty))
            .ForCtorParam(nameof(AlquilerDto.IdentificacionCliente), opt => opt.MapFrom(src => src.Cliente != null ? src.Cliente.Identificacion : string.Empty))
            .ForCtorParam(nameof(AlquilerDto.PlacaVehiculo), opt => opt.MapFrom(src => src.Vehiculo != null ? src.Vehiculo.Placa : string.Empty))
            .ForCtorParam(nameof(AlquilerDto.TipoVehiculo), opt => opt.MapFrom(src => src.Vehiculo != null ? src.Vehiculo.TipoVehiculo : string.Empty))
            .ForMember(dest => dest.NombreCliente, opt => opt.MapFrom(src => src.Cliente != null ? $"{src.Cliente.Nombres} {src.Cliente.Apellidos}".Trim() : string.Empty))
            .ForMember(dest => dest.IdentificacionCliente, opt => opt.MapFrom(src => src.Cliente != null ? src.Cliente.Identificacion : string.Empty))
            .ForMember(dest => dest.PlacaVehiculo, opt => opt.MapFrom(src => src.Vehiculo != null ? src.Vehiculo.Placa : string.Empty))
            .ForMember(dest => dest.TipoVehiculo, opt => opt.MapFrom(src => src.Vehiculo != null ? src.Vehiculo.TipoVehiculo : string.Empty));
        CreateMap<CreateAlquilerDto, Alquiler>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Total, opt => opt.MapFrom(src => src.DiasAlquiler * src.ValorDiario))
            .ForMember(dest => dest.Estado, opt => opt.MapFrom(_ => "Activo"))
            .ForMember(dest => dest.CreadoEn, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.Cliente, opt => opt.Ignore())
            .ForMember(dest => dest.Vehiculo, opt => opt.Ignore());

        // 10. Mapeos de Despacho
        CreateMap<Despacho, DespachoDto>();
        CreateMap<CreateDespachoDto, Despacho>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.FechaEntrega, opt => opt.Ignore())
            .ForMember(dest => dest.Estado, opt => opt.MapFrom(_ => "Pendiente"))
            .ForMember(dest => dest.CreadoEn, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.Venta, opt => opt.Ignore());

        // 11. Mapeos de MovimientoInventario
        CreateMap<MovimientoInventario, MovimientoInventarioDto>()
            .ForMember(dest => dest.CodigoProducto, opt => opt.MapFrom(src => src.Producto != null ? src.Producto.Codigo : string.Empty))
            .ForMember(dest => dest.NombreProducto, opt => opt.MapFrom(src => src.Producto != null ? src.Producto.Nombre : string.Empty));
    }
}
