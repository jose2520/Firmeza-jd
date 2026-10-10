using Firmeza.Application.DTOs;
using Firmeza.Domain.Entities;
using Firmeza.Infrastructure.Persistence;
using Firmeza.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.Tests;

public class VentaServiceTests
{
    private ApplicationDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task CreateAsync_ShouldCalculate19PercentIvaAndDeductStockCorrectly()
    {
        // Arrange
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

        // Act
        var result = await service.CreateAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.Subtotal.Should().Be(30000m);
        result.Iva.Should().Be(5700m); // 30,000 * 0.19 = 5,700
        result.Total.Should().Be(35700m); // 30,000 + 5,700 = 35,700
        result.Estado.Should().Be("Completada");

        // Verificar descuento en base de datos
        var productoActualizado = await context.Productos.FindAsync(1);
        productoActualizado!.Stock.Should().Be(47); // 50 - 3 = 47
    }

    [Fact]
    public async Task CreateAsync_WhenStockIsInsufficient_ShouldThrowInvalidOperationException()
    {
        // Arrange
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
                new(ProductoId: 1, Cantidad: 10) // Solicitado 10, disponible 2
            }
        );

        // Act
        var act = async () => await service.CreateAsync(dto);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Stock insuficiente*");
    }

    [Fact]
    public async Task CreateAsync_WhenClientIsInactive_ShouldThrowInvalidOperationException()
    {
        // Arrange
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

        // Act
        var act = async () => await service.CreateAsync(dto);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*inactivo*");
    }

    [Fact]
    public async Task CancelAsync_ShouldRestoreProductStockAndMarkSaleAsAnulada()
    {
        // Arrange
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

        // Stock debe haber bajado a 6
        (await context.Productos.FindAsync(1))!.Stock.Should().Be(6);

        // Act: Anular venta
        var anulada = await service.CancelAsync(venta.Id);

        // Assert
        anulada.Should().BeTrue();
        var ventaDb = await context.Ventas.FindAsync(venta.Id);
        ventaDb!.Estado.Should().Be("Anulada");

        // El stock debe haberse reincorporado (6 + 4 = 10)
        var productoRestaurado = await context.Productos.FindAsync(1);
        productoRestaurado!.Stock.Should().Be(10);
    }
}
