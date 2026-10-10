using Firmeza.Application.DTOs;
using Firmeza.Domain.Entities;
using Firmeza.Infrastructure.Persistence;
using Firmeza.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.Tests;

public class ClienteServiceTests
{
    private ApplicationDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task CreateAsync_WhenIdentificacionAlreadyExists_ShouldThrowInvalidOperationException()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        context.Clientes.Add(new Cliente
        {
            Id = 1,
            Identificacion = "1020304050",
            Nombres = "Carlos",
            Apellidos = "Gómez",
            Email = "carlos@example.com",
            Activo = true
        });
        await context.SaveChangesAsync();

        var service = new ClienteService(context);

        // Act
        var act = async () => await service.CreateAsync(new CreateClienteDto(
            Identificacion: "1020304050",
            Nombres: "Otro",
            Apellidos: "Usuario",
            Email: "otro@example.com",
            Telefono: null,
            Direccion: null
        ));

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Ya existe un cliente con la identificación*");
    }

    [Fact]
    public async Task DeleteAsync_WhenClientHasSales_ShouldPerformSoftDelete()
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
        var venta = new Venta
        {
            Id = 1,
            NumeroVenta = "VENTA-001",
            ClienteId = 1,
            Subtotal = 100m,
            Iva = 19m,
            Total = 119m,
            Estado = "Completada"
        };

        context.Clientes.Add(cliente);
        context.Ventas.Add(venta);
        await context.SaveChangesAsync();

        var service = new ClienteService(context);

        // Act
        var result = await service.DeleteAsync(1);

        // Assert
        result.Should().BeTrue();
        var clienteDb = await context.Clientes.FindAsync(1);
        clienteDb.Should().NotBeNull();
        clienteDb!.Activo.Should().BeFalse();
    }

    [Fact]
    public async Task CreateAsync_WhenEdadIsNegative_ShouldThrowInvalidOperationException()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var service = new ClienteService(context);

        // Act
        var act = async () => await service.CreateAsync(new CreateClienteDto(
            Identificacion: "1099887766",
            Nombres: "Laura",
            Apellidos: "Castro",
            Email: "laura@example.com",
            Telefono: null,
            Direccion: null,
            UserId: "USER-123",
            Edad: -5
        ));

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*edad*no puede ser un número negativo*");
    }

    [Fact]
    public async Task CreateAsync_WithValidEdadAndUserId_ShouldPersistCorrectly()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var service = new ClienteService(context);

        // Act
        var result = await service.CreateAsync(new CreateClienteDto(
            Identificacion: "1099887766",
            Nombres: "Laura",
            Apellidos: "Castro",
            Email: "laura@example.com",
            Telefono: "3001234567",
            Direccion: "Calle 10 # 20-30",
            UserId: "USER-123",
            Edad: 25
        ));

        // Assert
        result.Should().NotBeNull();
        result.Edad.Should().Be(25);
        result.UserId.Should().Be("USER-123");

        var dbClient = await context.Clientes.FindAsync(result.Id);
        dbClient.Should().NotBeNull();
        dbClient!.Edad.Should().Be(25);
        dbClient.UserId.Should().Be("USER-123");
    }
}
