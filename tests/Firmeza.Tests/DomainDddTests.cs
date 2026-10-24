// Pruebas de las reglas de negocio principales del núcleo de Firmeza.
// Verifica que los formatos de correo, NIT, teléfono, placa, precios y códigos de producto sean válidos,
// además de comprobar el descuento de existencias, el registro y anulación de ventas y los alquileres.

using Firmeza.Domain.Abstractions;
using Firmeza.Domain.Entities;
using Firmeza.Domain.Enums;
using Firmeza.Domain.Errors;
using Firmeza.Domain.Events;
using Firmeza.Domain.Exceptions;
using Firmeza.Domain.ValueObjects;
using FluentAssertions;

namespace Firmeza.Tests;

public class DomainDddTests
{
    // 1. Pruebas de objetos de valor y formatos de datos

    // Comprueba que el correo electrónico se convierta a minúsculas y que dos correos iguales se reconozcan como equivalentes.
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

    // Verifica que se rechacen correos vacíos o mal escritos que no tengan arroba o dominio válido.
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

    // Comprueba que un NIT con formato colombiano válido se guarde y compare correctamente.
    [Fact]
    public void Nit_ShouldValidateColombianFormatAndBeEqualByValue()
    {
        var nit1 = new Nit("860002130-1");
        var nit2 = new Nit("860002130-1");

        nit1.Valor.Should().Be("860002130-1");
        (nit1 == nit2).Should().BeTrue();
        (nit1 != nit2).Should().BeFalse();
    }

    // Verifica que se lance un error cuando el NIT esté vacío, sea muy corto o tenga un dígito de verificación inválido.
    [Theory]
    [InlineData("")]
    [InlineData("123")] // Menor a 5 dígitos
    [InlineData("860002130-123")] // Dígito de verificación inválido
    public void Nit_ShouldThrowValorInvalidoException_WhenFormatIsInvalid(string nitInvalido)
    {
        var act = () => new Nit(nitInvalido);
        act.Should().Throw<ValorInvalidoException>();
    }

    // Comprueba que el teléfono acepte números válidos y rechace aquellos que tengan menos de 7 dígitos.
    [Fact]
    public void Telefono_ShouldValidateDigitLength()
    {
        var tel = new Telefono("310 456 7890");
        tel.Valor.Should().Be("310 456 7890");

        var act = () => new Telefono("12345"); // Menor a 7 dígitos
        act.Should().Throw<ValorInvalidoException>();
    }

    // Verifica que la placa de un vehículo se convierta a mayúsculas y rechace textos demasiado largos.
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

    // Comprueba que los valores monetarios permitan sumar, restar y multiplicar correctamente y no acepten montos negativos.
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

    // Verifica que el código del producto se convierta a mayúsculas y no permita valores en blanco.
    [Fact]
    public void CodigoSku_ShouldNormalizeUppercaseAndValidateFormat()
    {
        var sku1 = new CodigoSku("prod-001");
        sku1.Valor.Should().Be("PROD-001");

        var actVacio = () => new CodigoSku(" ");
        actVacio.Should().Throw<ValorInvalidoException>();
    }

    // 2. Pruebas del catálogo de errores de negocio

    // Comprueba que cada código de error del negocio tenga el identificador y la categoría que le corresponde.
    [Fact]
    public void DomainErrors_ShouldContainStandardizedCodesAndDescriptions()
    {
        DomainErrors.Cliente.EmailInvalido.Code.Should().Be("Cliente.EmailInvalido");
        DomainErrors.Cliente.EmailInvalido.Description.Should().Be("El formato del correo electrónico suministrado no es válido.");
        DomainErrors.Cliente.EmailInvalido.Type.Should().Be(ErrorType.Validation);

        DomainErrors.Producto.StockInsuficiente.Code.Should().Be("Producto.StockInsuficiente");
        DomainErrors.Producto.StockInsuficiente.Description.Should().Be("No hay existencias físicas suficientes en almacén para despachar el pedido.");
        DomainErrors.Producto.StockInsuficiente.Type.Should().Be(ErrorType.Failure);

        DomainErrors.Venta.YaAnulada.Code.Should().Be("Venta.YaAnulada");
        DomainErrors.Venta.YaAnulada.Description.Should().Be("La venta seleccionada ya se encuentra en estado anulada.");
        DomainErrors.Venta.YaAnulada.Type.Should().Be(ErrorType.Conflict);

        DomainErrors.Vehiculo.NoDisponible.Code.Should().Be("Vehiculo.NoDisponible");
        DomainErrors.Vehiculo.NoDisponible.Description.Should().Be("El vehículo o maquinaria solicitada no se encuentra disponible para alquiler.");

        DomainErrors.Alquiler.DiasInvalidos.Code.Should().Be("Alquiler.DiasInvalidos");
        DomainErrors.Alquiler.DiasInvalidos.Description.Should().Be("La duración del contrato de alquiler debe ser de al menos 1 día.");

        DomainErrors.Despacho.DireccionInvalida.Code.Should().Be("Despacho.DireccionInvalida");
        DomainErrors.Despacho.DireccionInvalida.Description.Should().Be("La dirección de entrega en obra no puede estar vacía.");
    }

    // 3. Pruebas de avisos internos del negocio

    // Verifica que los avisos de venta registrada, vehículo alquilado y despachos guarden todos los datos de la operación y su fecha.
    [Fact]
    public void DomainEvents_ShouldCaptureContextAndTimestamp()
    {
        var fecha = DateTime.UtcNow;
        var eventoVenta = new VentaRegistradaDomainEvent(1, "VENTA-2026-001", 10, 150000m, fecha);

        eventoVenta.VentaId.Should().Be(1);
        eventoVenta.NumeroVenta.Should().Be("VENTA-2026-001");
        eventoVenta.ClienteId.Should().Be(10);
        eventoVenta.Total.Should().Be(150000m);
        eventoVenta.OccurredOn.Should().Be(fecha);

        var eventoVehiculo = new VehiculoAlquiladoDomainEvent(1, "ALQ-001", 5, "FIR-101", 10, 4, 1000000m, fecha);
        eventoVehiculo.AlquilerId.Should().Be(1);
        eventoVehiculo.NumeroContrato.Should().Be("ALQ-001");
        eventoVehiculo.VehiculoId.Should().Be(5);
        eventoVehiculo.PlacaVehiculo.Should().Be("FIR-101");
        eventoVehiculo.ClienteId.Should().Be(10);
        eventoVehiculo.DiasAlquiler.Should().Be(4);
        eventoVehiculo.Total.Should().Be(1000000m);

        var eventoDespacho = new DespachoEnRutaDomainEvent(1, 10, "Calle 100 # 15-20", "Carlos Perez", fecha);
        eventoDespacho.DespachoId.Should().Be(1);
        eventoDespacho.VentaId.Should().Be(10);
        eventoDespacho.DireccionEntrega.Should().Be("Calle 100 # 15-20");
        eventoDespacho.ContactoRecibe.Should().Be("Carlos Perez");
        eventoDespacho.OccurredOn.Should().Be(fecha);

        var eventoEntregado = new DespachoEntregadoDomainEvent(1, 10, "Bogotá", fecha.AddHours(2), fecha);
        eventoEntregado.DespachoId.Should().Be(1);
        eventoEntregado.VentaId.Should().Be(10);
        eventoEntregado.Ciudad.Should().Be("Bogotá");
        eventoEntregado.FechaEntrega.Should().Be(fecha.AddHours(2));
        eventoEntregado.OccurredOn.Should().Be(fecha);
    }

    // 4. Pruebas de excepciones de reglas del negocio

    // Comprueba que el error de falta de existencias informe el código del producto, cuántas unidades hay y cuántas se pidieron.
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

    // Verifica que el error de regla de negocio conserve el código de la regla incumplida y su mensaje explicativo.
    [Fact]
    public void ReglaNegocioException_ShouldContainRuleCode()
    {
        var ex = new ReglaNegocioException("Alquiler.DiasInvalidos", "Mínimo 1 día de alquiler");
        ex.CodigoRegla.Should().Be("Alquiler.DiasInvalidos");
        ex.Message.Should().Be("Mínimo 1 día de alquiler");
        ex.Should().BeAssignableTo<DomainException>();
    }

    // 5. Pruebas de acciones sobre productos, ventas y alquileres

    // Comprueba que al descontar unidades de un producto baje su inventario y se genere una alerta cuando queden 10 o menos.
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

        // Descontar 8 unidades (quedan 7, lo que activa la alerta de pocas existencias)
        producto.DescontarStock(8);

        // Comprobar que el inventario bajó a 7 y se registró la alerta
        producto.Stock.Should().Be(7);
        producto.DomainEvents.Should().ContainSingle();
        var evento = producto.DomainEvents.First().Should().BeOfType<StockBajoDomainEvent>().Subject;
        evento.ProductoId.Should().Be(1);
        evento.CodigoProducto.Should().Be("PROD-002");
        evento.NombreProducto.Should().Be("Cemento Gris");
        evento.StockRestante.Should().Be(7);

        // Limpiar la lista de avisos
        producto.ClearDomainEvents();
        producto.DomainEvents.Should().BeEmpty();
    }

    // Verifica que al intentar descontar más unidades de las que existen en bodega se lance un error de inventario insuficiente.
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

    // Comprueba que al confirmar una venta su estado cambie a 'Completada' y se registre el aviso correspondiente.
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
        evt.VentaId.Should().Be(10);
        evt.NumeroVenta.Should().Be("VENTA-2026-999");
        evt.ClienteId.Should().Be(5);
        evt.Total.Should().Be(119000m);
    }

    // Verifica que al cancelar una venta pase a estado 'Anulada' y que no se permita volver a anularla por segunda vez.
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
        evt.VentaId.Should().Be(10);
        evt.NumeroVenta.Should().Be("VENTA-2026-999");
        evt.Motivo.Should().Be("Cliente solicitó retracto");

        // Intentar anular de nuevo la misma venta debe lanzar error
        var actDuplicada = () => venta.CancelarVenta("Otra anulación");
        actDuplicada.Should().Throw<ReglaNegocioException>()
                    .And.CodigoRegla.Should().Be("Venta.YaAnulada");
    }

    // Comprueba que al iniciar un alquiler se calcule el monto total y al finalizarlo cambie su estado a 'Finalizado'.
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

        // Iniciar el alquiler
        alquiler.IniciarAlquiler();

        alquiler.Total.Should().Be(1000000m);
        alquiler.Estado.Should().Be("Activo");
        alquiler.DomainEvents.Should().ContainSingle(e => e is VehiculoAlquiladoDomainEvent);

        // Finalizar el alquiler
        alquiler.FinalizarAlquiler();
        alquiler.Estado.Should().Be("Finalizado");
        var eventoLiberado = alquiler.DomainEvents.OfType<VehiculoLiberadoDomainEvent>().Single();
        eventoLiberado.VehiculoId.Should().Be(3);
        eventoLiberado.PlacaVehiculo.Should().Be("FIR-101");
        eventoLiberado.NumeroContrato.Should().Be("ALQ-2026-100");
    }

    // 6. Pruebas de listados de opciones del negocio

    // Verifica que todas las listas de estados, tipos de vehículo, unidades de medida y medios de pago contengan los valores esperados.
    [Fact]
    public void DomainEnums_ShouldHaveExpectedValuesAndConsistency()
    {
        // Estados de venta
        Enum.IsDefined(typeof(EstadoVenta), EstadoVenta.Pendiente).Should().BeTrue();
        Enum.IsDefined(typeof(EstadoVenta), EstadoVenta.Completada).Should().BeTrue();
        Enum.IsDefined(typeof(EstadoVenta), EstadoVenta.Anulada).Should().BeTrue();

        // Estados de despacho
        Enum.GetNames<EstadoDespacho>().Should().Contain(new[] { "Pendiente", "EnRuta", "Entregado", "Cancelado" });

        // Estados de vehículo
        Enum.GetNames<EstadoVehiculo>().Should().Contain(new[] { "Disponible", "Alquilado", "EnMantenimiento" });

        // Estados de alquiler
        Enum.GetNames<EstadoAlquiler>().Should().Contain(new[] { "Activo", "Finalizado", "Cancelado" });

        // Tipos de movimiento de inventario
        Enum.GetNames<TipoMovimientoInventario>().Should().Contain(new[] { "Entrada", "Salida", "Ajuste" });

        // Tipos de vehículo
        Enum.GetNames<TipoVehiculo>().Should().Contain("VolquetaSencilla");
        Enum.GetNames<TipoVehiculo>().Should().Contain("Retroexcavadora");

        // Tipos de método de pago
        Enum.GetNames<TipoMetodoPago>().Should().Contain(new[] { "Efectivo", "TransferenciaBancaria", "TarjetaCredito", "Nequi" });

        // Tipos de documento de identificación
        Enum.GetNames<TipoIdentificacion>().Should().Contain(new[] { "CedulaCiudadania", "Nit", "Rut" });

        // Unidades de medida de materiales
        Enum.GetNames<UnidadMedida>().Should().Contain(new[] { "Bulto", "MetroCubico", "Tonelada", "Galon" });

        // Niveles de prioridad de despacho
        Enum.GetNames<PrioridadDespacho>().Should().Contain(new[] { "Baja", "Normal", "Alta", "Urgente" });

        // Estados de cotización
        Enum.GetNames<EstadoCotizacion>().Should().Contain(new[] { "Borrador", "Enviada", "Aprobada", "Rechazada", "Vencida" });
    }

    // 7. Pruebas de las interfaces de acceso a datos y servicios base

    // Comprueba que todos los repositorios específicos hereden las operaciones básicas de consulta y guardado.
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

    // Verifica que el coordinador de guardado en base de datos incluya los métodos para iniciar, confirmar y deshacer operaciones.
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

    // Comprueba que el distribuidor de avisos del negocio defina los métodos para enviar notificaciones internas.
    [Fact]
    public void DomainEventsDispatcher_ShouldDefineDispatchContracts()
    {
        var dispatcherType = typeof(IDomainEventDispatcher);
        dispatcherType.IsInterface.Should().BeTrue();
        dispatcherType.GetMethod(nameof(IDomainEventDispatcher.DispatchEventsAsync)).Should().NotBeNull();
        dispatcherType.GetMethod(nameof(IDomainEventDispatcher.DispatchEventAsync)).Should().NotBeNull();
    }

    // Verifica que el proveedor de fecha y hora del sistema exponga la hora actual y la fecha del día.
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
