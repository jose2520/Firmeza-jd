// Pruebas sobre el servicio encargado de gestionar los clientes de Firmeza.
// Verifica que no se permitan documentos repetidos ni edades negativas, y que al borrar
// un cliente con compras previas solo se desactive en lugar de eliminarse por completo.

using Firmeza.Application.DTOs;
using Firmeza.Domain.Entities;
using Firmeza.Infrastructure.Persistence;
using Firmeza.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.Tests;

public class ClienteServiceTests
{
    // Crea una base de datos temporal en memoria para realizar cada prueba sin afectar datos reales.
    private ApplicationDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    // Comprueba que el sistema rechace el registro de un cliente nuevo cuando su número de identificación ya existe.
    [Fact]
    public async Task CreateAsync_WhenIdentificacionAlreadyExists_ShouldThrowInvalidOperationException()
    {
        // Preparar los datos de prueba
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

        // Intentar registrar otro cliente con la misma identificación
        var act = async () => await service.CreateAsync(new CreateClienteDto(
            Identificacion: "1020304050",
            Nombres: "Otro",
            Apellidos: "Usuario",
            Email: "otro@example.com",
            Telefono: null,
            Direccion: null
        ));

        // Comprobar que arroje el error esperado
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Ya existe un cliente con la identificación*");
    }

    // Verifica que al intentar eliminar un cliente que ya tiene ventas, se conserve su registro pero quede marcado como inactivo.
    [Fact]
    public async Task DeleteAsync_WhenClientHasSales_ShouldPerformSoftDelete()
    {
        // Preparar un cliente con una venta registrada
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

        // Ejecutar la eliminación del cliente
        var result = await service.DeleteAsync(1);

        // Comprobar que sigue en la base de datos pero inactivo
        result.Should().BeTrue();
        var clienteDb = await context.Clientes.FindAsync(1);
        clienteDb.Should().NotBeNull();
        clienteDb!.Activo.Should().BeFalse();
    }

    // Comprueba que no se permita registrar un cliente si se ingresa un número negativo en su edad.
    [Fact]
    public async Task CreateAsync_WhenEdadIsNegative_ShouldThrowInvalidOperationException()
    {
        // Preparar el servicio
        using var context = CreateInMemoryDbContext();
        var service = new ClienteService(context);

        // Intentar crear un cliente con edad negativa
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

        // Comprobar que el sistema lance un error por la edad inválida
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*edad*no puede ser un número negativo*");
    }

    // Verifica que un cliente con edad válida y cuenta de usuario asociada se guarde correctamente en la base de datos.
    [Fact]
    public async Task CreateAsync_WithValidEdadAndUserId_ShouldPersistCorrectly()
    {
        // Preparar el servicio
        using var context = CreateInMemoryDbContext();
        var service = new ClienteService(context);

        // Crear el cliente con datos válidos
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

        // Comprobar que se guardaron tanto la edad como el identificador de usuario
        result.Should().NotBeNull();
        result.Edad.Should().Be(25);
        result.UserId.Should().Be("USER-123");

        var dbClient = await context.Clientes.FindAsync(result.Id);
        dbClient.Should().NotBeNull();
        dbClient!.Edad.Should().Be(25);
        dbClient.UserId.Should().Be("USER-123");
    }
}
