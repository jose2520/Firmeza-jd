// Pruebas para verificar la conversión automática de datos entre las tablas del sistema y los objetos de respuesta.
// Asegura que toda la información de clientes y productos se copie de forma exacta
// sin perder ningún campo al preparar los datos para enviarlos al usuario.

using AutoMapper;
using Firmeza.Application.DTOs;
using Firmeza.Application.Mappings;
using Firmeza.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;

namespace Firmeza.Tests;

public class AutoMapperTests
{
    private readonly IMapper _mapper;

    // Configura las reglas de conversión de datos antes de ejecutar cada prueba.
    public AutoMapperTests()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        }, NullLoggerFactory.Instance);

        _mapper = config.CreateMapper();
    }

    // Comprueba que un registro de cliente se convierta correctamente con todos sus datos personales, edad y usuario asociado.
    [Fact]
    public void Mapper_ShouldMapClienteToClienteDtoCorrectly()
    {
        // Preparar los datos de prueba
        var cliente = new Cliente
        {
            Id = 1,
            Identificacion = "1020304050",
            Nombres = "Carlos",
            Apellidos = "Gómez",
            Email = "carlos@example.com",
            Telefono = "3001234567",
            Direccion = "Calle 100",
            UserId = "USER-01",
            Edad = 35,
            Activo = true,
            CreadoEn = DateTime.UtcNow
        };

        // Ejecutar la conversión
        var dto = _mapper.Map<ClienteDto>(cliente);

        // Comprobar el resultado
        dto.Should().NotBeNull();
        dto.Id.Should().Be(1);
        dto.Identificacion.Should().Be("1020304050");
        dto.Nombres.Should().Be("Carlos");
        dto.Apellidos.Should().Be("Gómez");
        dto.Email.Should().Be("carlos@example.com");
        dto.UserId.Should().Be("USER-01");
        dto.Edad.Should().Be(35);
    }

    // Verifica que un producto conserve su código, nombre, precio y cantidad en bodega al convertirse en objeto de respuesta.
    [Fact]
    public void Mapper_ShouldMapProductoToProductoDtoCorrectly()
    {
        // Preparar los datos de prueba
        var producto = new Producto
        {
            Id = 10,
            Codigo = "PROD-999",
            Nombre = "Martillo de Uña",
            Descripcion = "Herramienta de impacto",
            Precio = 35000m,
            Stock = 50,
            Activo = true
        };

        // Ejecutar la conversión
        var dto = _mapper.Map<ProductoDto>(producto);

        // Comprobar el resultado
        dto.Should().NotBeNull();
        dto.Id.Should().Be(10);
        dto.Codigo.Should().Be("PROD-999");
        dto.Nombre.Should().Be("Martillo de Uña");
        dto.Precio.Should().Be(35000m);
        dto.Stock.Should().Be(50);
    }

    // Comprueba que los datos para crear un proveedor se copien correctamente en la ficha del proveedor.
    [Fact]
    public void Mapper_ShouldMapCreateProveedorDtoToProveedorCorrectly()
    {
        var dto = new CreateProveedorDto("900123456-7", "Distribuidora Nacional", "Laura Restrepo", "3151234567", "laura@distribuidora.com", "Carrera 45", "Cali");
        var proveedor = _mapper.Map<Proveedor>(dto);

        proveedor.Should().NotBeNull();
        proveedor.Nit.Should().Be(dto.Nit);
        proveedor.RazonSocial.Should().Be(dto.RazonSocial);
        proveedor.ContactoNombre.Should().Be(dto.ContactoNombre);
        proveedor.Telefono.Should().Be(dto.Telefono);
        proveedor.Email.Should().Be(dto.Email);
        proveedor.Direccion.Should().Be(dto.Direccion);
        proveedor.Ciudad.Should().Be(dto.Ciudad);
        proveedor.Activo.Should().BeTrue();
    }

    // Comprueba que los datos para registrar un método de pago se copien fielmente a la entidad del sistema.
    [Fact]
    public void Mapper_ShouldMapCreateMetodoPagoDtoToMetodoPagoCorrectly()
    {
        var dto = new CreateMetodoPagoDto("NEQUI", "Pago Nequi", "Transferencia por aplicación móvil");
        var metodo = _mapper.Map<MetodoPago>(dto);

        metodo.Should().NotBeNull();
        metodo.Codigo.Should().Be(dto.Codigo);
        metodo.Nombre.Should().Be(dto.Nombre);
        metodo.Descripcion.Should().Be(dto.Descripcion);
        metodo.Activo.Should().BeTrue();
    }

    // Comprueba que los datos para registrar un vehículo se asignen de forma precisa a la maquinaria o vehículo.
    [Fact]
    public void Mapper_ShouldMapCreateVehiculoDtoToVehiculoCorrectly()
    {
        var dto = new CreateVehiculoDto("XYZ-123", "Caterpillar", "2024", "Retroexcavadora", 5.5m, 250000m);
        var vehiculo = _mapper.Map<Vehiculo>(dto);

        vehiculo.Should().NotBeNull();
        vehiculo.Placa.Should().Be(dto.Placa);
        vehiculo.Marca.Should().Be(dto.Marca);
        vehiculo.Modelo.Should().Be(dto.Modelo);
        vehiculo.TipoVehiculo.Should().Be(dto.TipoVehiculo);
        vehiculo.CapacidadCargaTon.Should().Be(dto.CapacidadCargaTon);
        vehiculo.TarifaDiaria.Should().Be(dto.TarifaDiaria);
        vehiculo.Estado.Should().Be("Disponible");
        vehiculo.Activo.Should().BeTrue();
    }

    // Comprueba que los datos de un envío a domicilio se guarden de manera correcta en el despacho.
    [Fact]
    public void Mapper_ShouldMapCreateDespachoDtoToDespachoCorrectly()
    {
        var fechaEstimada = DateTime.UtcNow.AddDays(2);
        var dto = new CreateDespachoDto(5, "Avenida Las Palmas 20", "Medellín", "Andrés Pérez", "3209876543", fechaEstimada, 30000m, "Entregar en portería");
        var despacho = _mapper.Map<Despacho>(dto);

        despacho.Should().NotBeNull();
        despacho.VentaId.Should().Be(dto.VentaId);
        despacho.DireccionEntrega.Should().Be(dto.DireccionEntrega);
        despacho.Ciudad.Should().Be(dto.Ciudad);
        despacho.ContactoRecibe.Should().Be(dto.ContactoRecibe);
        despacho.TelefonoContacto.Should().Be(dto.TelefonoContacto);
        despacho.FechaEstimada.Should().Be(dto.FechaEstimada);
        despacho.CostoFlete.Should().Be(dto.CostoFlete);
        despacho.Observaciones.Should().Be(dto.Observaciones);
        despacho.Estado.Should().Be("Pendiente");
    }

    // Comprueba que los datos para registrar una categoría nueva se transfieran fielmente a la entidad del sistema.
    [Fact]
    public void Mapper_ShouldMapCreateCategoriaDtoToCategoriaCorrectly()
    {
        var dto = new CreateCategoriaDto("Tuberías y Accesorios", "Tubos de PVC y conexiones sanitarias");
        var categoria = _mapper.Map<Categoria>(dto);

        categoria.Should().NotBeNull();
        categoria.Nombre.Should().Be(dto.Nombre);
        categoria.Descripcion.Should().Be(dto.Descripcion);
        categoria.Activo.Should().BeTrue();
    }
}
