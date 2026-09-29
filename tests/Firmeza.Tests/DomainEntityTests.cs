using Firmeza.Domain.Entities;
using FluentAssertions;

namespace Firmeza.Tests;

public class DomainEntityTests
{
    [Fact]
    public void Producto_ShouldInitializePropertiesCorrectly()
    {
        // Arrange & Act
        var producto = new Producto
        {
            Id = 1,
            Codigo = "PROD-001",
            Nombre = "Tornillo de Acero 2 pulg",
            Precio = 15000m,
            Stock = 100,
            Activo = true
        };

        // Assert
        producto.Id.Should().Be(1);
        producto.Codigo.Should().Be("PROD-001");
        producto.Precio.Should().Be(15000m);
        producto.Stock.Should().Be(100);
        producto.Activo.Should().BeTrue();
        producto.DetallesVenta.Should().BeEmpty();
    }

    [Fact]
    public void Venta_ShouldLinkToClientAndDetails()
    {
        // Arrange
        var cliente = new Cliente
        {
            Id = 1,
            Identificacion = "1020304050",
            Nombres = "Carlos",
            Apellidos = "Gómez",
            Email = "carlos@example.com"
        };

        var venta = new Venta
        {
            Id = 1,
            NumeroVenta = "VTA-0001",
            ClienteId = cliente.Id,
            Cliente = cliente,
            Subtotal = 100000m,
            Iva = 19000m,
            Total = 119000m,
            Estado = "Completada"
        };

        // Assert
        venta.Cliente.Should().BeSameAs(cliente);
        venta.Total.Should().Be(venta.Subtotal + venta.Iva);
        venta.DetallesVenta.Should().BeEmpty();
    }
}
