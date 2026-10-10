using Firmeza.Application.DTOs;
using Firmeza.Domain.Entities;
using Firmeza.Infrastructure.Persistence;
using Firmeza.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.Tests;

public class ProductoServiceTests
{
    private ApplicationDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task CreateAsync_WhenCodeAlreadyExists_ShouldThrowInvalidOperationException()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        context.Productos.Add(new Producto
        {
            Id = 1,
            Codigo = "PROD-100",
            Nombre = "Varilla",
            Precio = 45000m,
            Stock = 20,
            Activo = true
        });
        await context.SaveChangesAsync();

        var service = new ProductoService(context);

        // Act
        var act = async () => await service.CreateAsync(new CreateProductoDto(
            Codigo: "PROD-100",
            Nombre: "Varilla Duplicada",
            Descripcion: "Desc",
            Precio: 50000m,
            Stock: 10
        ));

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Ya existe un producto con el código*");
    }

    [Fact]
    public async Task DeleteAsync_WhenProductHasSales_ShouldPerformSoftDelete()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var producto = new Producto
        {
            Id = 1,
            Codigo = "PROD-100",
            Nombre = "Tornillo",
            Precio = 1000m,
            Stock = 50,
            Activo = true
        };
        var detalle = new DetalleVenta
        {
            Id = 1,
            VentaId = 1,
            ProductoId = 1,
            Cantidad = 5,
            PrecioUnitario = 1000m,
            Subtotal = 5000m
        };

        context.Productos.Add(producto);
        context.DetallesVenta.Add(detalle);
        await context.SaveChangesAsync();

        var service = new ProductoService(context);

        // Act: Delete product with existing sales history
        var result = await service.DeleteAsync(1);

        // Assert: Product should NOT be deleted from DB, but marked as Activo = false
        result.Should().BeTrue();
        var productoDb = await context.Productos.FindAsync(1);
        productoDb.Should().NotBeNull();
        productoDb!.Activo.Should().BeFalse();
    }
}
