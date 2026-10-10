using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using Firmeza.API.Controllers;
using Firmeza.Application.DTOs;
using Firmeza.Application.Mappings;
using Firmeza.Domain.Entities;
using Firmeza.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Firmeza.Tests;

public class CommercialDomainTests
{
    private readonly IMapper _mapper;

    public CommercialDomainTests()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        }, NullLoggerFactory.Instance);

        _mapper = config.CreateMapper();
    }

    private ApplicationDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    [Fact]
    public void Categoria_ShouldInitializePropertiesCorrectly()
    {
        var categoria = new Categoria
        {
            Id = 1,
            Nombre = "Cementos y Agregados",
            Descripcion = "Materiales para cimentación",
            Activo = true
        };

        categoria.Id.Should().Be(1);
        categoria.Nombre.Should().Be("Cementos y Agregados");
        categoria.Activo.Should().BeTrue();
        categoria.Productos.Should().BeEmpty();
    }

    [Fact]
    public void Proveedor_ShouldInitializePropertiesCorrectly()
    {
        var proveedor = new Proveedor
        {
            Id = 1,
            Nit = "860002130-1",
            RazonSocial = "Cementos Argos S.A.",
            Ciudad = "Medellín",
            Telefono = "3104567890",
            Activo = true
        };

        proveedor.Nit.Should().Be("860002130-1");
        proveedor.RazonSocial.Should().Be("Cementos Argos S.A.");
        proveedor.Productos.Should().BeEmpty();
    }

    [Fact]
    public void MetodoPago_ShouldInitializePropertiesCorrectly()
    {
        var metodo = new MetodoPago
        {
            Id = 1,
            Codigo = "CREDITO_COMERCIAL",
            Nombre = "Crédito Comercial a 30 Días",
            Activo = true
        };

        metodo.Codigo.Should().Be("CREDITO_COMERCIAL");
        metodo.Nombre.Should().Be("Crédito Comercial a 30 Días");
        metodo.Ventas.Should().BeEmpty();
    }

    [Fact]
    public void Vehiculo_ShouldInitializePropertiesCorrectly()
    {
        var vehiculo = new Vehiculo
        {
            Id = 1,
            Placa = "FIR-101",
            Marca = "Hino",
            Modelo = "Dutro 300",
            TipoVehiculo = "Camión Estacas",
            CapacidadCargaTon = 4.5m,
            TarifaDiaria = 250000m,
            Estado = "Disponible"
        };

        vehiculo.Placa.Should().Be("FIR-101");
        vehiculo.TarifaDiaria.Should().Be(250000m);
        vehiculo.Estado.Should().Be("Disponible");
        vehiculo.Alquileres.Should().BeEmpty();
    }

    [Fact]
    public void Alquiler_ShouldCalculateTotalAndRelateEntities()
    {
        var cliente = new Cliente { Id = 1, Identificacion = "1020304050", Nombres = "Carlos", Apellidos = "Gómez", Email = "carlos@firmeza.com" };
        var vehiculo = new Vehiculo { Id = 2, Placa = "FIR-202", Marca = "Chevrolet", Modelo = "NKR", TipoVehiculo = "Volqueta", TarifaDiaria = 320000m };

        var alquiler = new Alquiler
        {
            Id = 1,
            NumeroContrato = "ALQ-2026-001",
            ClienteId = cliente.Id,
            Cliente = cliente,
            VehiculoId = vehiculo.Id,
            Vehiculo = vehiculo,
            DiasAlquiler = 5,
            ValorDiario = 320000m,
            Total = 5 * 320000m,
            Estado = "Activo"
        };

        alquiler.Total.Should().Be(1600000m);
        alquiler.Cliente.Nombres.Should().Be("Carlos");
        alquiler.Vehiculo.Placa.Should().Be("FIR-202");
    }

    [Fact]
    public void Despacho_ShouldLinkToVenta()
    {
        var venta = new Venta { Id = 1, NumeroVenta = "VENTA-001", Subtotal = 100000m, Iva = 19000m, Total = 119000m, Estado = "Completada" };
        var despacho = new Despacho
        {
            Id = 1,
            VentaId = venta.Id,
            Venta = venta,
            DireccionEntrega = "Obra Calle 128 # 58-20",
            Ciudad = "Bogotá",
            ContactoRecibe = "Ing. Ramírez",
            TelefonoContacto = "3123456789",
            CostoFlete = 35000m,
            Estado = "En Ruta"
        };

        despacho.Venta.NumeroVenta.Should().Be("VENTA-001");
        despacho.CostoFlete.Should().Be(35000m);
        despacho.Estado.Should().Be("En Ruta");
    }

    [Fact]
    public void MovimientoInventario_ShouldTrackKardexDeltas()
    {
        var producto = new Producto { Id = 1, Codigo = "PROD-002", Nombre = "Cemento Gris", Stock = 40, Precio = 32000m };
        var movimiento = new MovimientoInventario
        {
            Id = 1,
            ProductoId = producto.Id,
            Producto = producto,
            TipoMovimiento = "Salida",
            Cantidad = 10,
            StockPrevio = 50,
            StockPosterior = 40,
            Referencia = "VENTA-007",
            Motivo = "Despacho a obra"
        };

        movimiento.StockPosterior.Should().Be(movimiento.StockPrevio - movimiento.Cantidad);
        movimiento.Producto.Nombre.Should().Be("Cemento Gris");
    }

    [Fact]
    public void Mapper_ShouldMapCategoriaToCategoriaDto()
    {
        var cat = new Categoria { Id = 5, Nombre = "Herramientas Eléctricas", Descripcion = "Taladros y pulidoras", Activo = true };
        var dto = _mapper.Map<CategoriaDto>(cat);

        dto.Should().NotBeNull();
        dto.Id.Should().Be(5);
        dto.Nombre.Should().Be("Herramientas Eléctricas");
    }

    [Fact]
    public void Mapper_ShouldMapProveedorToProveedorDto()
    {
        var prov = new Proveedor { Id = 2, Nit = "890900161-5", RazonSocial = "Gerdau Diaco S.A.", Ciudad = "Bogotá", Activo = true };
        var dto = _mapper.Map<ProveedorDto>(prov);

        dto.Should().NotBeNull();
        dto.Nit.Should().Be("890900161-5");
        dto.RazonSocial.Should().Be("Gerdau Diaco S.A.");
    }

    [Fact]
    public void Mapper_ShouldMapVehiculoToVehiculoDto()
    {
        var vh = new Vehiculo { Id = 3, Placa = "FIR-303", Marca = "Caterpillar", Modelo = "Mixer", TipoVehiculo = "Mezcladora", TarifaDiaria = 120000m };
        var dto = _mapper.Map<VehiculoDto>(vh);

        dto.Should().NotBeNull();
        dto.Placa.Should().Be("FIR-303");
        dto.TarifaDiaria.Should().Be(120000m);
    }

    [Fact]
    public void Mapper_ShouldMapAlquilerToAlquilerDtoWithRelations()
    {
        var alq = new Alquiler
        {
            Id = 1,
            NumeroContrato = "ALQ-999",
            Cliente = new Cliente { Nombres = "Pedro", Apellidos = "Picapiedra", Identificacion = "112233" },
            Vehiculo = new Vehiculo { Placa = "FIR-404", TipoVehiculo = "Camioneta" },
            DiasAlquiler = 3,
            ValorDiario = 150000m,
            Total = 450000m,
            Estado = "Activo"
        };

        var dto = _mapper.Map<AlquilerDto>(alq);

        dto.Should().NotBeNull();
        dto.NumeroContrato.Should().Be("ALQ-999");
        dto.NombreCliente.Should().Be("Pedro Picapiedra");
        dto.IdentificacionCliente.Should().Be("112233");
        dto.PlacaVehiculo.Should().Be("FIR-404");
        dto.TipoVehiculo.Should().Be("Camioneta");
        dto.Total.Should().Be(450000m);
    }

    [Fact]
    public async Task CategoriesController_CreateAndGetAll_ShouldWorkCorrectly()
    {
        using var context = CreateInMemoryDbContext();
        var controller = new CategoriesController(context, _mapper);

        var createResult = await controller.Create(new CreateCategoriaDto("Drywall y Yesos", "Sistemas livianos"));
        createResult.Result.Should().BeOfType<CreatedAtActionResult>();

        var getAllResult = await controller.GetAll();
        var okResult = getAllResult.Result.Should().BeOfType<OkObjectResult>().Subject;
        var list = okResult.Value as IEnumerable<CategoriaDto>;
        list.Should().NotBeNull();
        list.Should().ContainSingle(c => c.Nombre == "Drywall y Yesos");
    }

    [Fact]
    public async Task CategoriesController_Delete_WithAssignedProducts_ShouldApplySoftDelete()
    {
        using var context = CreateInMemoryDbContext();
        var categoria = new Categoria { Nombre = "Fijaciones y Pernos", Activo = true };
        context.Categorias.Add(categoria);
        await context.SaveChangesAsync();

        var producto = new Producto { Codigo = "TORN-01", Nombre = "Tornillo 2 pulg", Stock = 10, Precio = 500m, CategoriaId = categoria.Id };
        context.Productos.Add(producto);
        await context.SaveChangesAsync();

        var controller = new CategoriesController(context, _mapper);
        var deleteResult = await controller.Delete(categoria.Id);

        var okResult = deleteResult.Should().BeOfType<OkObjectResult>().Subject;
        var catActualizada = await context.Categorias.FindAsync(categoria.Id);
        catActualizada!.Activo.Should().BeFalse();
    }

    [Fact]
    public async Task CategoriesController_GetProducts_ShouldReturnAssociatedProducts()
    {
        using var context = CreateInMemoryDbContext();
        var categoria = new Categoria { Nombre = "Maquinaria de Carga", Activo = true };
        context.Categorias.Add(categoria);
        await context.SaveChangesAsync();

        context.Productos.Add(new Producto { Codigo = "PROD-M1", Nombre = "Mezcladora 2 Bultos", Stock = 5, Precio = 1200000m, CategoriaId = categoria.Id, Activo = true });
        context.Productos.Add(new Producto { Codigo = "PROD-M2", Nombre = "Inactivo", Stock = 0, Precio = 500000m, CategoriaId = categoria.Id, Activo = false });
        await context.SaveChangesAsync();

        var controller = new CategoriesController(context, _mapper);
        var result = await controller.GetProducts(categoria.Id);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var productos = okResult.Value as IEnumerable<ProductoDto>;
        productos.Should().NotBeNull();
        productos.Should().ContainSingle(p => p.Codigo == "PROD-M1");
    }

    [Fact]
    public async Task VehiclesController_CreateAndGetAll_ShouldWorkCorrectly()
    {
        using var context = CreateInMemoryDbContext();
        var controller = new VehiclesController(context, _mapper);

        var createResult = await controller.Create(new CreateVehiculoDto("FIR-888", "Komatsu", "PC200", "Retroexcavadora", 20.0m, 800000m));
        createResult.Result.Should().BeOfType<CreatedAtActionResult>();

        var getAllResult = await controller.GetAll();
        var okResult = getAllResult.Result.Should().BeOfType<OkObjectResult>().Subject;
        var list = okResult.Value as IEnumerable<VehiculoDto>;
        list.Should().NotBeNull();
        list.Should().ContainSingle(v => v.Placa == "FIR-888");
    }

    [Fact]
    public async Task RentalsController_Create_ShouldUpdateVehicleStateToAlquilado()
    {
        using var context = CreateInMemoryDbContext();
        var cliente = new Cliente { Identificacion = "556677", Nombres = "Andrés", Apellidos = "Torres", Email = "andres@test.com" };
        var vehiculo = new Vehiculo { Placa = "FIR-555", Marca = "Ford", Modelo = "F-150", TipoVehiculo = "Camioneta", TarifaDiaria = 200000m, Estado = "Disponible" };
        context.Clientes.Add(cliente);
        context.Vehiculos.Add(vehiculo);
        await context.SaveChangesAsync();

        var controller = new RentalsController(context, _mapper);
        var createDto = new CreateAlquilerDto("ALQ-NEW-01", cliente.Id, vehiculo.Id, DateTime.UtcNow, DateTime.UtcNow.AddDays(4), 4, 200000m);

        var result = await controller.Create(createDto);
        result.Result.Should().BeOfType<CreatedAtActionResult>();

        var vehiculoActualizado = await context.Vehiculos.FindAsync(vehiculo.Id);
        vehiculoActualizado!.Estado.Should().Be("Alquilado");
    }

    [Fact]
    public async Task SuppliersController_CreateAndNitDuplicate_ShouldReturnBadRequest()
    {
        using var context = CreateInMemoryDbContext();
        var controller = new SuppliersController(context, _mapper);

        var supplierDto = new CreateProveedorDto("860002130-1", "Cementos Argos S.A.", "Carlos Restrepo", "3104567890", "contacto@argos.co", "Calle 7Sur", "Medellín");
        var result1 = await controller.Create(supplierDto);
        result1.Result.Should().BeOfType<CreatedAtActionResult>();

        // Intento duplicado con el mismo NIT
        var result2 = await controller.Create(supplierDto);
        result2.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task PaymentMethodsController_CreateAndGetAll_ShouldWorkCorrectly()
    {
        using var context = CreateInMemoryDbContext();
        var controller = new PaymentMethodsController(context, _mapper);

        var createResult = await controller.Create(new CreateMetodoPagoDto("EFECTIVO", "Efectivo", "Pago en mostrador"));
        createResult.Result.Should().BeOfType<CreatedAtActionResult>();

        var getAllResult = await controller.GetAll();
        var okResult = getAllResult.Result.Should().BeOfType<OkObjectResult>().Subject;
        var list = okResult.Value as IEnumerable<MetodoPagoDto>;
        list.Should().NotBeNull();
        list.Should().ContainSingle(m => m.Codigo == "EFECTIVO");
    }

    [Fact]
    public async Task DispatchesController_CreateAndStatusUpdate_ShouldSetFechaEntrega()
    {
        using var context = CreateInMemoryDbContext();
        var cliente = new Cliente { Identificacion = "102030", Nombres = "Juan", Apellidos = "Pérez", Email = "juan@test.com" };
        var venta = new Venta { NumeroVenta = "VENTA-TEST-01", Cliente = cliente, Subtotal = 50000m, Iva = 9500m, Total = 59500m, Estado = "Completada" };
        context.Clientes.Add(cliente);
        context.Ventas.Add(venta);
        await context.SaveChangesAsync();

        var controller = new DispatchesController(context, _mapper);
        var createDto = new CreateDespachoDto(venta.Id, "Calle 100 # 20-30", "Bogotá", "Juan Pérez", "3110001122", DateTime.UtcNow.AddDays(1), 25000m, "Piso 2");

        var createResult = await controller.Create(createDto);
        var createdAction = createResult.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        var despachoCreado = createdAction.Value as DespachoDto;
        despachoCreado.Should().NotBeNull();

        // Actualizar estado a 'Entregado'
        var updateResult = await controller.UpdateStatus(despachoCreado!.Id, "Entregado");
        var okResult = updateResult.Result.Should().BeOfType<OkObjectResult>().Subject;
        var despachoFinal = okResult.Value as DespachoDto;
        despachoFinal.Should().NotBeNull();
        despachoFinal!.Estado.Should().Be("Entregado");
        despachoFinal.FechaEntrega.Should().NotBeNull();
    }

    [Fact]
    public async Task InventoryMovementsController_EntradaAndSalida_ShouldUpdateProductStockCorrectly()
    {
        using var context = CreateInMemoryDbContext();
        var producto = new Producto { Codigo = "PROD-KARDEX-01", Nombre = "Cemento Gris Especial", Stock = 20, Precio = 30000m };
        context.Productos.Add(producto);
        await context.SaveChangesAsync();

        var controller = new InventoryMovementsController(context, _mapper);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "USER-ADMIN-ID") }))
            }
        };

        // 1. Registrar Entrada de 30 unidades
        var entradaDto = new CreateMovimientoInventarioDto(producto.Id, "Entrada", 30, "Factura Compra FC-100", "Llegada de camión");
        var resEntrada = await controller.Create(entradaDto);
        resEntrada.Result.Should().BeOfType<CreatedAtActionResult>();

        var prodAfterEntrada = await context.Productos.FindAsync(producto.Id);
        prodAfterEntrada!.Stock.Should().Be(50); // 20 + 30 = 50

        // 2. Registrar Salida de 15 unidades
        var salidaDto = new CreateMovimientoInventarioDto(producto.Id, "Salida", 15, "VENTA-001", "Venta a cliente");
        var resSalida = await controller.Create(salidaDto);
        resSalida.Result.Should().BeOfType<CreatedAtActionResult>();

        var prodAfterSalida = await context.Productos.FindAsync(producto.Id);
        prodAfterSalida!.Stock.Should().Be(35); // 50 - 15 = 35

        // 3. Salida con cantidad superior al stock debe retornar BadRequest
        var salidaExcesivaDto = new CreateMovimientoInventarioDto(producto.Id, "Salida", 100, "VENTA-002", "Venta excesiva");
        var resInvalida = await controller.Create(salidaExcesivaDto);
        resInvalida.Result.Should().BeOfType<BadRequestObjectResult>();
    }
}
