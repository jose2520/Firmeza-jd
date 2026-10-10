using Firmeza.Domain.Abstractions;
using Firmeza.Domain.Entities;
using Firmeza.Domain.Enums;
using Firmeza.Domain.Errors;
using Firmeza.Domain.Events;
using Firmeza.Domain.Exceptions;
using Firmeza.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

namespace Firmeza.Tests;

public class DomainDddTests
{
    // =========================================================================
    // 1. VALUE OBJECTS TESTS
    // =========================================================================

    [Fact]
    public void Email_ShouldNormalizeToLowercaseAndBeEqualByValue()
    {
        var email1 = new Email("Cliente.Prueba@Firmeza.COM");
        var email2 = new Email("cliente.prueba@firmeza.com");

        email1.Valor.Should().Be("cliente.prueba@firmeza.com");
        (email1 == email2).Should().BeTrue();
        email1.Equals(email2).Should().BeTrue();
        email1.GetHashCode().Should().Be(email2.GetHashCode());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("correo_sin_arroba")]
    [InlineData("correo@")]
    [InlineData("@dominio.com")]
    public void Email_ShouldThrowValorInvalidoException_WhenFormatIsInvalid(string emailInvalido)
    {
        var act = () => new Email(emailInvalido);
        act.Should().Throw<ValorInvalidoException>()
           .And.NombreCampo.Should().Be("Email");
    }

    [Fact]
    public void Nit_ShouldValidateColombianFormatAndBeEqualByValue()
    {
        var nit1 = new Nit("860002130-1");
        var nit2 = new Nit("860002130-1");

        nit1.Valor.Should().Be("860002130-1");
        (nit1 == nit2).Should().BeTrue();
        (nit1 != nit2).Should().BeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData("123")] // Menor a 5 dígitos
    [InlineData("860002130-123")] // DV inválido
    public void Nit_ShouldThrowValorInvalidoException_WhenFormatIsInvalid(string nitInvalido)
    {
        var act = () => new Nit(nitInvalido);
        act.Should().Throw<ValorInvalidoException>();
    }

    [Fact]
    public void Telefono_ShouldValidateDigitLength()
    {
        var tel = new Telefono("310 456 7890");
        tel.Valor.Should().Be("310 456 7890");

        var act = () => new Telefono("12345"); // Menor a 7 dígitos
        act.Should().Throw<ValorInvalidoException>();
    }

    [Fact]
    public void Placa_ShouldNormalizeToUppercaseAndValidateStructure()
    {
        var placa1 = new Placa("fir-101");
        var placa2 = new Placa("FIR-101");

        placa1.Valor.Should().Be("FIR-101");
        (placa1 == placa2).Should().BeTrue();

        var actInvalida = () => new Placa("PLACA_LARGA_INVALIDA");
        actInvalida.Should().Throw<ValorInvalidoException>();
    }

    [Fact]
    public void Dinero_ShouldEnforceNonNegativeAndPerformArithmetic()
    {
        var precio1 = new Dinero(100000m);
        var precio2 = new Dinero(50000m);

        var suma = precio1 + precio2;
        suma.Monto.Should().Be(150000m);

        var resta = precio1 - precio2;
        resta.Monto.Should().Be(50000m);

        var multiplicado = precio2 * 2;
        multiplicado.Monto.Should().Be(100000m);

        (precio1 > precio2).Should().BeTrue();

        var actNegativo = () => new Dinero(-10m);
        actNegativo.Should().Throw<ValorInvalidoException>();
    }

    [Fact]
    public void CodigoSku_ShouldNormalizeUppercaseAndValidateFormat()
    {
        var sku1 = new CodigoSku("prod-001");
        sku1.Valor.Should().Be("PROD-001");

        var actVacio = () => new CodigoSku(" ");
        actVacio.Should().Throw<ValorInvalidoException>();
    }

    // =========================================================================
    // 2. DOMAIN ERRORS TESTS
    // =========================================================================

    [Fact]
    public void DomainErrors_ShouldContainStandardizedCodesAndDescriptions()
    {
        DomainErrors.Cliente.EmailInvalido.Code.Should().Be("Cliente.EmailInvalido");
        DomainErrors.Cliente.EmailInvalido.Type.Should().Be(ErrorType.Validation);

        DomainErrors.Producto.StockInsuficiente.Code.Should().Be("Producto.StockInsuficiente");
        DomainErrors.Producto.StockInsuficiente.Type.Should().Be(ErrorType.Failure);

        DomainErrors.Venta.YaAnulada.Code.Should().Be("Venta.YaAnulada");
        DomainErrors.Venta.YaAnulada.Type.Should().Be(ErrorType.Conflict);

        DomainErrors.Vehiculo.NoDisponible.Code.Should().Be("Vehiculo.NoDisponible");
        DomainErrors.Alquiler.DiasInvalidos.Code.Should().Be("Alquiler.DiasInvalidos");
        DomainErrors.Despacho.DireccionInvalida.Code.Should().Be("Despacho.DireccionInvalida");
    }

    // =========================================================================
    // 3. DOMAIN EVENTS TESTS
    // =========================================================================

    [Fact]
    public void DomainEvents_ShouldCaptureContextAndTimestamp()
    {
        var fecha = DateTime.UtcNow;
        var eventoVenta = new VentaRegistradaDomainEvent(1, "VENTA-2026-001", 10, 150000m, fecha);

        eventoVenta.VentaId.Should().Be(1);
        eventoVenta.NumeroVenta.Should().Be("VENTA-2026-001");
        eventoVenta.Total.Should().Be(150000m);
        eventoVenta.OccurredOn.Should().Be(fecha);

        var eventoVehiculo = new VehiculoAlquiladoDomainEvent(1, "ALQ-001", 5, "FIR-101", 10, 4, 1000000m, fecha);
        eventoVehiculo.PlacaVehiculo.Should().Be("FIR-101");
        eventoVehiculo.Total.Should().Be(1000000m);
    }

    // =========================================================================
    // 4. DOMAIN EXCEPTIONS TESTS
    // =========================================================================

    [Fact]
    public void StockInsuficienteException_ShouldExposeDiagnosticProperties()
    {
        var ex = new StockInsuficienteException("PROD-002", 5, 20);

        ex.CodigoProducto.Should().Be("PROD-002");
        ex.StockDisponible.Should().Be(5);
        ex.CantidadSolicitada.Should().Be(20);
        ex.Message.Should().Contain("Existencias disponibles: 5");
        ex.Should().BeAssignableTo<DomainException>();
    }

    [Fact]
    public void ReglaNegocioException_ShouldContainRuleCode()
    {
        var ex = new ReglaNegocioException("Alquiler.DiasInvalidos", "Mínimo 1 día de alquiler");
        ex.CodigoRegla.Should().Be("Alquiler.DiasInvalidos");
        ex.Message.Should().Be("Mínimo 1 día de alquiler");
        ex.Should().BeAssignableTo<DomainException>();
    }

    // =========================================================================
    // 5. AGGREGATE ROOT & ENTITY DOMAIN METHODS TESTS
    // =========================================================================

    [Fact]
    public void Producto_DescontarStock_ShouldDecreaseStockAndEmitStockBajoEvent()
    {
        var producto = new Producto
        {
            Id = 1,
            Codigo = "PROD-002",
            Nombre = "Cemento Gris",
            Stock = 15,
            Precio = 32000m
        };

        // Act: descontar 8 unidades (quedan 7, que es <= 10)
        producto.DescontarStock(8);

        // Assert
        producto.Stock.Should().Be(7);
        producto.DomainEvents.Should().ContainSingle();
        var evento = producto.DomainEvents.First().Should().BeOfType<StockBajoDomainEvent>().Subject;
        evento.CodigoProducto.Should().Be("PROD-002");
        evento.StockRestante.Should().Be(7);

        // Limpiar eventos
        producto.ClearDomainEvents();
        producto.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void Producto_DescontarStock_ShouldThrowStockInsuficienteException_WhenExceedingStock()
    {
        var producto = new Producto
        {
            Id = 1,
            Codigo = "PROD-002",
            Nombre = "Cemento Gris",
            Stock = 5,
            Precio = 32000m
        };

        var act = () => producto.DescontarStock(10);
        act.Should().Throw<StockInsuficienteException>()
           .And.StockDisponible.Should().Be(5);
    }

    [Fact]
    public void Venta_RegistrarVenta_ShouldSetCompletadaAndEmitDomainEvent()
    {
        var venta = new Venta
        {
            Id = 10,
            NumeroVenta = "VENTA-2026-999",
            ClienteId = 5,
            Subtotal = 100000m,
            Iva = 19000m,
            Total = 119000m
        };

        venta.RegistrarVenta();

        venta.Estado.Should().Be("Completada");
        venta.DomainEvents.Should().ContainSingle();
        var evt = venta.DomainEvents.First().Should().BeOfType<VentaRegistradaDomainEvent>().Subject;
        evt.NumeroVenta.Should().Be("VENTA-2026-999");
        evt.Total.Should().Be(119000m);
    }

    [Fact]
    public void Venta_CancelarVenta_ShouldSetAnuladaAndEmitDomainEvent()
    {
        var venta = new Venta
        {
            Id = 10,
            NumeroVenta = "VENTA-2026-999",
            Estado = "Completada"
        };

        venta.CancelarVenta("Cliente solicitó retracto");

        venta.Estado.Should().Be("Anulada");
        venta.DomainEvents.Should().ContainSingle();
        var evt = venta.DomainEvents.First().Should().BeOfType<VentaCanceladaDomainEvent>().Subject;
        evt.Motivo.Should().Be("Cliente solicitó retracto");

        // Intentar anular de nuevo debe lanzar excepción
        var actDuplicada = () => venta.CancelarVenta("Otra anulación");
        actDuplicada.Should().Throw<ReglaNegocioException>()
                    .And.CodigoRegla.Should().Be("Venta.YaAnulada");
    }

    [Fact]
    public void Alquiler_IniciarYFinalizar_ShouldEmitDomainEvents()
    {
        var vehiculo = new Vehiculo { Id = 3, Placa = "FIR-101", TarifaDiaria = 250000m, Estado = "Disponible" };
        var alquiler = new Alquiler
        {
            Id = 1,
            NumeroContrato = "ALQ-2026-100",
            ClienteId = 2,
            VehiculoId = vehiculo.Id,
            Vehiculo = vehiculo,
            FechaInicio = DateTime.UtcNow,
            FechaFin = DateTime.UtcNow.AddDays(4),
            DiasAlquiler = 4,
            ValorDiario = 250000m
        };

        // Act: Iniciar
        alquiler.IniciarAlquiler();

        alquiler.Total.Should().Be(1000000m);
        alquiler.Estado.Should().Be("Activo");
        alquiler.DomainEvents.Should().ContainSingle(e => e is VehiculoAlquiladoDomainEvent);

        // Act: Finalizar
        alquiler.FinalizarAlquiler();
        alquiler.Estado.Should().Be("Finalizado");
        alquiler.DomainEvents.Should().Contain(e => e is VehiculoLiberadoDomainEvent);
    }

    // =========================================================================
    // 6. DOMAIN ENUMS TESTS
    // =========================================================================

    [Fact]
    public void DomainEnums_ShouldHaveExpectedValuesAndConsistency()
    {
        // EstadoVenta
        Enum.IsDefined(typeof(EstadoVenta), EstadoVenta.Pendiente).Should().BeTrue();
        Enum.IsDefined(typeof(EstadoVenta), EstadoVenta.Completada).Should().BeTrue();
        Enum.IsDefined(typeof(EstadoVenta), EstadoVenta.Anulada).Should().BeTrue();

        // EstadoDespacho
        Enum.GetNames<EstadoDespacho>().Should().Contain(new[] { "Pendiente", "EnRuta", "Entregado", "Cancelado" });

        // EstadoVehiculo
        Enum.GetNames<EstadoVehiculo>().Should().Contain(new[] { "Disponible", "Alquilado", "EnMantenimiento" });

        // EstadoAlquiler
        Enum.GetNames<EstadoAlquiler>().Should().Contain(new[] { "Activo", "Finalizado", "Cancelado" });

        // TipoMovimientoInventario
        Enum.GetNames<TipoMovimientoInventario>().Should().Contain(new[] { "Entrada", "Salida", "Ajuste" });

        // TipoVehiculo
        Enum.GetNames<TipoVehiculo>().Should().Contain("VolquetaSencilla");
        Enum.GetNames<TipoVehiculo>().Should().Contain("Retroexcavadora");

        // TipoMetodoPago
        Enum.GetNames<TipoMetodoPago>().Should().Contain(new[] { "Efectivo", "TransferenciaBancaria", "TarjetaCredito", "Nequi" });

        // TipoIdentificacion
        Enum.GetNames<TipoIdentificacion>().Should().Contain(new[] { "CedulaCiudadania", "NIT", "RUT" });

        // UnidadMedida
        Enum.GetNames<UnidadMedida>().Should().Contain(new[] { "Bulto", "MetroCubico", "Tonelada", "Galon" });

        // PrioridadDespacho
        Enum.GetNames<PrioridadDespacho>().Should().Contain(new[] { "Baja", "Normal", "Alta", "Urgente" });

        // EstadoCotizacion
        Enum.GetNames<EstadoCotizacion>().Should().Contain(new[] { "Borrador", "Enviada", "Aprobada", "Rechazada", "Vencida" });
    }

    // =========================================================================
    // 7. DOMAIN ABSTRACTIONS CONTRACT TESTS
    // =========================================================================

    [Fact]
    public void DomainRepositories_ShouldInheritFromGenericRepository()
    {
        typeof(IRepository<Cliente>).IsAssignableFrom(typeof(IClienteRepository)).Should().BeTrue();
        typeof(IRepository<Producto>).IsAssignableFrom(typeof(IProductoRepository)).Should().BeTrue();
        typeof(IRepository<Venta>).IsAssignableFrom(typeof(IVentaRepository)).Should().BeTrue();
        typeof(IRepository<Vehiculo>).IsAssignableFrom(typeof(IVehiculoRepository)).Should().BeTrue();
        typeof(IRepository<Alquiler>).IsAssignableFrom(typeof(IAlquilerRepository)).Should().BeTrue();
        typeof(IRepository<Despacho>).IsAssignableFrom(typeof(IDespachoRepository)).Should().BeTrue();
        typeof(IRepository<MovimientoInventario>).IsAssignableFrom(typeof(IMovimientoInventarioRepository)).Should().BeTrue();
        typeof(IRepository<Categoria>).IsAssignableFrom(typeof(ICategoriaRepository)).Should().BeTrue();
        typeof(IRepository<Proveedor>).IsAssignableFrom(typeof(IProveedorRepository)).Should().BeTrue();
    }

    [Fact]
    public void UnitOfWork_ShouldDefineTransactionAndPersistenceContracts()
    {
        var uowType = typeof(IUnitOfWork);
        uowType.IsInterface.Should().BeTrue();
        typeof(IDisposable).IsAssignableFrom(uowType).Should().BeTrue();
        uowType.GetMethod(nameof(IUnitOfWork.SaveChangesAsync)).Should().NotBeNull();
        uowType.GetMethod(nameof(IUnitOfWork.BeginTransactionAsync)).Should().NotBeNull();
        uowType.GetMethod(nameof(IUnitOfWork.CommitTransactionAsync)).Should().NotBeNull();
        uowType.GetMethod(nameof(IUnitOfWork.RollbackTransactionAsync)).Should().NotBeNull();
    }

    [Fact]
    public void DomainEventsDispatcher_ShouldDefineDispatchContracts()
    {
        var dispatcherType = typeof(IDomainEventDispatcher);
        dispatcherType.IsInterface.Should().BeTrue();
        dispatcherType.GetMethod(nameof(IDomainEventDispatcher.DispatchEventsAsync)).Should().NotBeNull();
        dispatcherType.GetMethod(nameof(IDomainEventDispatcher.DispatchEventAsync)).Should().NotBeNull();
    }

    [Fact]
    public void DateTimeProvider_ShouldExposeRequiredTimeProperties()
    {
        var providerType = typeof(IDateTimeProvider);
        providerType.IsInterface.Should().BeTrue();
        providerType.GetProperty(nameof(IDateTimeProvider.UtcNow)).Should().NotBeNull();
        providerType.GetProperty(nameof(IDateTimeProvider.Now)).Should().NotBeNull();
        providerType.GetProperty(nameof(IDateTimeProvider.Today)).Should().NotBeNull();
    }
}

