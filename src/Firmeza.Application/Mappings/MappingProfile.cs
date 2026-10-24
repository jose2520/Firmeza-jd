// Archivo: MappingProfile.cs
// Qué es: Configuración de conversión automática de datos entre las tablas de la base de datos y los objetos que se envían por pantalla.
// Para qué sirve: Ahorra copiar campo por campo manualmente al pasar información de clientes, productos, ventas, alquileres y envíos de un formato a otro.

using AutoMapper;
using Firmeza.Application.DTOs;
using Firmeza.Domain.Entities;

namespace Firmeza.Application.Mappings;

// Clase que reúne todas las reglas para copiar datos automáticamente entre las entidades del sistema y las estructuras de envío.
public class MappingProfile : Profile
{
    // Define paso a paso cómo se deben copiar los campos para cada tipo de registro del negocio.
    public MappingProfile()
    {
        // 1. Reglas para convertir datos de Cliente
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

        // 2. Reglas para convertir datos de Producto
        CreateMap<Producto, ProductoDto>()
            .ForCtorParam(nameof(ProductoDto.CategoriaNombre), opt => opt.MapFrom(src => src.Categoria != null ? src.Categoria.Nombre : null))
            .ForCtorParam(nameof(ProductoDto.ProveedorNombre), opt => opt.MapFrom(src => src.Proveedor != null ? src.Proveedor.RazonSocial : null))
            .ForMember(dest => dest.CategoriaNombre, opt => opt.MapFrom(src => src.Categoria != null ? src.Categoria.Nombre : null))
            .ForMember(dest => dest.ProveedorNombre, opt => opt.MapFrom(src => src.Proveedor != null ? src.Proveedor.RazonSocial : null));
        CreateMap<CreateProductoDto, Producto>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Activo, opt => opt.MapFrom(_ => true))
            .ForMember(dest => dest.CreadoEn, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.Categoria, opt => opt.Ignore())
            .ForMember(dest => dest.Proveedor, opt => opt.Ignore())
            .ForMember(dest => dest.DetallesVenta, opt => opt.Ignore())
            .ForMember(dest => dest.MovimientosInventario, opt => opt.Ignore());
        CreateMap<UpdateProductoDto, Producto>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Codigo, opt => opt.Ignore())
            .ForMember(dest => dest.CreadoEn, opt => opt.Ignore())
            .ForMember(dest => dest.Categoria, opt => opt.Ignore())
            .ForMember(dest => dest.Proveedor, opt => opt.Ignore())
            .ForMember(dest => dest.DetallesVenta, opt => opt.Ignore())
            .ForMember(dest => dest.MovimientosInventario, opt => opt.Ignore());

        // 3. Reglas para convertir cada renglón de una venta
        CreateMap<DetalleVenta, DetalleVentaDto>()
            .ForMember(dest => dest.CodigoProducto, opt => opt.MapFrom(src => src.Producto != null ? src.Producto.Codigo : string.Empty))
            .ForMember(dest => dest.NombreProducto, opt => opt.MapFrom(src => src.Producto != null ? src.Producto.Nombre : string.Empty));

        // 4. Reglas para convertir la factura de Venta completa
        CreateMap<Venta, VentaDto>()
            .ForMember(dest => dest.NombreCliente, opt => opt.MapFrom(src => src.Cliente != null ? $"{src.Cliente.Nombres} {src.Cliente.Apellidos}".Trim() : string.Empty))
            .ForMember(dest => dest.IdentificacionCliente, opt => opt.MapFrom(src => src.Cliente != null ? src.Cliente.Identificacion : string.Empty))
            .ForMember(dest => dest.Detalles, opt => opt.MapFrom(src => src.DetallesVenta));

        // 5. Reglas para convertir datos de Categoría
        CreateMap<Categoria, CategoriaDto>()
            .ForCtorParam(nameof(CategoriaDto.TotalProductos), opt => opt.MapFrom(src => src.Productos != null ? src.Productos.Count : 0))
            .ForMember(dest => dest.TotalProductos, opt => opt.MapFrom(src => src.Productos != null ? src.Productos.Count : 0));
        CreateMap<CreateCategoriaDto, Categoria>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Activo, opt => opt.MapFrom(_ => true))
            .ForMember(dest => dest.CreadoEn, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.Productos, opt => opt.Ignore());

        // 6. Reglas para convertir datos de Proveedor
        CreateMap<Proveedor, ProveedorDto>();
        CreateMap<CreateProveedorDto, Proveedor>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Activo, opt => opt.MapFrom(_ => true))
            .ForMember(dest => dest.CreadoEn, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.Productos, opt => opt.Ignore());

        // 7. Reglas para convertir Formas de Pago
        CreateMap<MetodoPago, MetodoPagoDto>();
        CreateMap<CreateMetodoPagoDto, MetodoPago>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Activo, opt => opt.MapFrom(_ => true))
            .ForMember(dest => dest.Ventas, opt => opt.Ignore());
        CreateMap<UpdateMetodoPagoDto, MetodoPago>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Ventas, opt => opt.Ignore());

        // 8. Reglas para convertir datos de Vehículos y Maquinaria
        CreateMap<Vehiculo, VehiculoDto>();
        CreateMap<CreateVehiculoDto, Vehiculo>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Estado, opt => opt.MapFrom(_ => "Disponible"))
            .ForMember(dest => dest.Activo, opt => opt.MapFrom(_ => true))
            .ForMember(dest => dest.CreadoEn, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.Alquileres, opt => opt.Ignore());

        // 9. Reglas para convertir Contratos de Alquiler
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

        // 10. Reglas para convertir datos de Envíos y Despachos
        CreateMap<Despacho, DespachoDto>();
        CreateMap<CreateDespachoDto, Despacho>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.FechaEntrega, opt => opt.Ignore())
            .ForMember(dest => dest.Estado, opt => opt.MapFrom(_ => "Pendiente"))
            .ForMember(dest => dest.CreadoEn, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.Venta, opt => opt.Ignore());

        // 11. Reglas para convertir Movimientos de Inventario
        CreateMap<MovimientoInventario, MovimientoInventarioDto>()
            .ForMember(dest => dest.CodigoProducto, opt => opt.MapFrom(src => src.Producto != null ? src.Producto.Codigo : string.Empty))
            .ForMember(dest => dest.NombreProducto, opt => opt.MapFrom(src => src.Producto != null ? src.Producto.Nombre : string.Empty));
    }
}
