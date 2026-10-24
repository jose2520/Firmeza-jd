// Pruebas básicas sobre las clases principales de productos y ventas.
// Verifica que al crear un producto o una venta se guarden correctamente sus valores
// iniciales y se mantenga la relación con el cliente y sus detalles de compra.

using Firmeza.Domain.Entities;
using FluentAssertions;

namespace Firmeza.Tests;

public class DomainEntityTests
{
    // Comprueba que un producto nuevo guarde su código, nombre, precio, existencias e inicie sin ventas asociadas.
    [Fact]
    public void Producto_ShouldInitializePropertiesCorrectly()
    {
        // Crear el producto de prueba
        var producto = new Producto
        {
            Id = 1,
            Codigo = "PROD-001",
            Nombre = "Tornillo de Acero 2 pulg",
            Precio = 15000m,
            Stock = 100,
            Activo = true
        };

        // Comprobar que cada campo tenga el valor asignado
        producto.Id.Should().Be(1);
        producto.Codigo.Should().Be("PROD-001");
        producto.Precio.Should().Be(15000m);
        producto.Stock.Should().Be(100);
        producto.Activo.Should().BeTrue();
        producto.DetallesVenta.Should().BeEmpty();
    }

    // Verifica que una venta quede asociada a su cliente y que su total coincida con la suma del subtotal más el impuesto.
    [Fact]
    public void Venta_ShouldLinkToClientAndDetails()
    {
        // Preparar un cliente y una venta asociada
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

        // Comprobar el vínculo con el cliente y el cálculo del total
        venta.Cliente.Should().BeSameAs(cliente);
        venta.Total.Should().Be(venta.Subtotal + venta.Iva);
        venta.DetallesVenta.Should().BeEmpty();
    }
}
