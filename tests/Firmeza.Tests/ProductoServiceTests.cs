// Pruebas para verificar las reglas del servicio de productos.
// Comprueba que no se permita registrar dos productos con el mismo código y que, al eliminar
// un artículo que ya tiene ventas registradas, solo se desactive para no afectar facturas pasadas.

using Firmeza.Application.DTOs;
using Firmeza.Domain.Entities;
using Firmeza.Infrastructure.Persistence;
using Firmeza.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.Tests;

public class ProductoServiceTests
{
    // Crea una base de datos temporal en memoria para ejecutar cada prueba de forma independiente.
    private ApplicationDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    // Verifica que si se intenta crear un producto con un código que ya existe, el sistema lance un error.
    [Fact]
    public async Task CreateAsync_WhenCodeAlreadyExists_ShouldThrowInvalidOperationException()
    {
        // Preparar un producto existente en la base de datos
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

        // Intentar crear otro producto con el mismo código
        var act = async () => await service.CreateAsync(new CreateProductoDto(
            Codigo: "PROD-100",
            Nombre: "Varilla Duplicada",
            Descripcion: "Desc",
            Precio: 50000m,
            Stock: 10
        ));

        // Comprobar que se rechace por código repetido
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Ya existe un producto con el código*");
    }

    // Comprueba que al eliminar un producto que ya figura en ventas anteriores, no se borre de la tabla sino que pase a inactivo.
    [Fact]
    public async Task DeleteAsync_WhenProductHasSales_ShouldPerformSoftDelete()
    {
        // Preparar un producto que ya tiene un detalle de venta registrado
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

        // Solicitar la eliminación del producto con historial de ventas
        var result = await service.DeleteAsync(1);

        // Comprobar que el producto permanezca guardado pero con estado inactivo
        result.Should().BeTrue();
        var productoDb = await context.Productos.FindAsync(1);
        productoDb.Should().NotBeNull();
        productoDb!.Activo.Should().BeFalse();
    }
}
