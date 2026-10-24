// Pruebas sobre el servicio encargado de procesar las ventas y controlar el inventario.
// Verifica el cálculo exacto del impuesto del 19%, el descuento de unidades al vender,
// el rechazo cuando faltan existencias o el cliente está inactivo, y la devolución de productos al anular.

using Firmeza.Application.DTOs;
using Firmeza.Domain.Entities;
using Firmeza.Infrastructure.Persistence;
using Firmeza.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.Tests;

public class VentaServiceTests
{
    // Crea una base de datos temporal en memoria para probar el flujo completo de ventas sin alterar datos reales.
    private ApplicationDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    // Comprueba que al registrar una venta se calcule el 19% de IVA, se sume al total y se descuenten las unidades vendidas de la bodega.
    [Fact]
    public async Task CreateAsync_ShouldCalculate19PercentIvaAndDeductStockCorrectly()
    {
        // Preparar un cliente activo y un producto con 50 unidades disponibles
        using var context = CreateInMemoryDbContext();
        var cliente = new Cliente
        {
            Id = 1,
            Identificacion = "1020304050",
            Nombres = "Carlos",
            Apellidos = "Gómez",
            Email = "carlos@example.com",
            Activo = true
        };
        var producto = new Producto
        {
            Id = 1,
            Codigo = "PROD-001",
            Nombre = "Tornillo de Acero",
            Precio = 10000m,
            Stock = 50,
            Activo = true
        };

        context.Clientes.Add(cliente);
        context.Productos.Add(producto);
        await context.SaveChangesAsync();

        var service = new VentaService(context);
        var dto = new CreateVentaDto(
            ClienteId: 1,
            Detalles: new List<CreateDetalleVentaDto>
            {
                new(ProductoId: 1, Cantidad: 3) // 3 * 10,000 = 30,000
            }
        );

        // Registrar la venta de 3 unidades
        var result = await service.CreateAsync(dto);

        // Comprobar los valores de subtotal, IVA, total y estado final
        result.Should().NotBeNull();
        result.Subtotal.Should().Be(30000m);
        result.Iva.Should().Be(5700m); // 30,000 * 0.19 = 5,700
        result.Total.Should().Be(35700m); // 30,000 + 5,700 = 35,700
        result.Estado.Should().Be("Completada");

        // Verificar que las existencias del producto hayan bajado de 50 a 47
        var productoActualizado = await context.Productos.FindAsync(1);
        productoActualizado!.Stock.Should().Be(47); // 50 - 3 = 47
    }

    // Verifica que si un cliente pide más unidades de las que hay disponibles en bodega, la venta sea rechazada.
    [Fact]
    public async Task CreateAsync_WhenStockIsInsufficient_ShouldThrowInvalidOperationException()
    {
        // Preparar un producto que solo tiene 2 unidades en bodega
        using var context = CreateInMemoryDbContext();
        var cliente = new Cliente
        {
            Id = 1,
            Identificacion = "1020304050",
            Nombres = "Carlos",
            Apellidos = "Gómez",
            Email = "carlos@example.com",
            Activo = true
        };
        var producto = new Producto
        {
            Id = 1,
            Codigo = "PROD-001",
            Nombre = "Cemento",
            Precio = 32000m,
            Stock = 2,
            Activo = true
        };

        context.Clientes.Add(cliente);
        context.Productos.Add(producto);
        await context.SaveChangesAsync();

        var service = new VentaService(context);
        var dto = new CreateVentaDto(
            ClienteId: 1,
            Detalles: new List<CreateDetalleVentaDto>
            {
                new(ProductoId: 1, Cantidad: 10) // Se piden 10 pero solo hay 2 disponibles
            }
        );

        // Intentar registrar la venta
        var act = async () => await service.CreateAsync(dto);

        // Comprobar que arroje error por falta de inventario
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Stock insuficiente*");
    }

    // Comprueba que no se permita registrar una venta si el cliente seleccionado se encuentra inactivo.
    [Fact]
    public async Task CreateAsync_WhenClientIsInactive_ShouldThrowInvalidOperationException()
    {
        // Preparar un cliente desactivado y un producto con existencias
        using var context = CreateInMemoryDbContext();
        var clienteInactivo = new Cliente
        {
            Id = 1,
            Identificacion = "1020304050",
            Nombres = "Inactivo",
            Apellidos = "User",
            Email = "inactivo@example.com",
            Activo = false
        };
        var producto = new Producto
        {
            Id = 1,
            Codigo = "PROD-001",
            Nombre = "Pintura",
            Precio = 65000m,
            Stock = 20,
            Activo = true
        };

        context.Clientes.Add(clienteInactivo);
        context.Productos.Add(producto);
        await context.SaveChangesAsync();

        var service = new VentaService(context);
        var dto = new CreateVentaDto(
            ClienteId: 1,
            Detalles: new List<CreateDetalleVentaDto>
            {
                new(ProductoId: 1, Cantidad: 1)
            }
        );

        // Intentar crear la venta para el cliente inactivo
        var act = async () => await service.CreateAsync(dto);

        // Comprobar que el sistema rechace la venta indicando que el cliente está inactivo
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*inactivo*");
    }

    // Verifica que al anular una venta se marque como 'Anulada', se devuelvan las unidades al inventario y quede registro de ambos movimientos.
    [Fact]
    public async Task CancelAsync_ShouldRestoreProductStockAndMarkSaleAsAnulada()
    {
        // Preparar un cliente y un producto con 10 unidades iniciales
        using var context = CreateInMemoryDbContext();
        var cliente = new Cliente
        {
            Id = 1,
            Identificacion = "1020304050",
            Nombres = "Carlos",
            Apellidos = "Gómez",
            Email = "carlos@example.com",
            Activo = true
        };
        var producto = new Producto
        {
            Id = 1,
            Codigo = "PROD-001",
            Nombre = "Tornillo",
            Precio = 1000m,
            Stock = 10,
            Activo = true
        };

        context.Clientes.Add(cliente);
        context.Productos.Add(producto);
        await context.SaveChangesAsync();

        var service = new VentaService(context);
        var venta = await service.CreateAsync(new CreateVentaDto(
            ClienteId: 1,
            Detalles: new List<CreateDetalleVentaDto> { new(1, 4) }
        ));

        // Al vender 4 unidades, deben quedar 6 en bodega
        (await context.Productos.FindAsync(1))!.Stock.Should().Be(6);

        // Anular la venta recién creada
        var anulada = await service.CancelAsync(venta.Id);

        // Comprobar que la venta quede con estado 'Anulada'
        anulada.Should().BeTrue();
        var ventaDb = await context.Ventas.FindAsync(venta.Id);
        ventaDb!.Estado.Should().Be("Anulada");

        // Comprobar que las 4 unidades hayan regresado al inventario (6 + 4 = 10)
        var productoRestaurado = await context.Productos.FindAsync(1);
        productoRestaurado!.Stock.Should().Be(10);

        // Verificar que en el historial de movimientos aparezcan tanto la salida por venta como la entrada por anulación
        var movimientos = await context.MovimientosInventario
            .Where(m => m.ProductoId == 1)
            .OrderBy(m => m.Id)
            .ToListAsync();
        movimientos.Should().HaveCount(2);
        movimientos[0].TipoMovimiento.Should().Be("Salida");
        movimientos[0].StockPrevio.Should().Be(10);
        movimientos[0].StockPosterior.Should().Be(6);
        movimientos[1].TipoMovimiento.Should().Be("Entrada");
        movimientos[1].StockPrevio.Should().Be(6);
        movimientos[1].StockPosterior.Should().Be(10);
    }

    // Comprueba que al indicar un método de pago válido en la venta, este quede guardado correctamente en el registro.
    [Fact]
    public async Task CreateAsync_WithValidMetodoPagoId_ShouldPersistMetodoPago()
    {
        // Preparar cliente, producto y método de pago activos
        using var context = CreateInMemoryDbContext();
        context.Clientes.Add(new Cliente { Id = 1, Identificacion = "1020", Nombres = "Ana", Apellidos = "Paz", Email = "ana@example.com", Activo = true });
        context.Productos.Add(new Producto { Id = 1, Codigo = "PROD-001", Nombre = "Martillo", Precio = 20000m, Stock = 15, Activo = true });
        context.MetodosPago.Add(new MetodoPago { Id = 2, Codigo = "TRANSF", Nombre = "Transferencia Bancaria", Activo = true });
        await context.SaveChangesAsync();

        var service = new VentaService(context);
        var dto = new CreateVentaDto(
            ClienteId: 1,
            Detalles: new List<CreateDetalleVentaDto> { new(1, 2) },
            MetodoPagoId: 2
        );

        // Registrar la venta con el método de pago 2
        var result = await service.CreateAsync(dto);

        // Comprobar que la venta guardó el método de pago seleccionado
        var ventaDb = await context.Ventas.FindAsync(result.Id);
        ventaDb!.MetodoPagoId.Should().Be(2);
    }
}
