// Pruebas para verificar la generación de la factura corporativa en formato PDF.
// Comprueba que el documento se construya correctamente con los datos de la venta, cliente,
// productos y el logotipo institucional de Firmeza.

using Firmeza.Domain.Entities;
using Firmeza.Infrastructure.Persistence;
using Firmeza.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.Tests;

public class PdfServiceTests
{
    private ApplicationDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task GenerateSaleInvoicePdfAsync_ShouldGenerateValidCorporatePdf()
    {
        using var context = CreateInMemoryDbContext();

        var cliente = new Cliente
        {
            Id = 1,
            Identificacion = "901234567-8",
            Nombres = "Construcciones",
            Apellidos = "Andinas S.A.S.",
            Email = "contacto@andinas.com",
            Telefono = "3109876543",
            Direccion = "Av. El Dorado #68C-61, Bogotá",
            Activo = true,
            CreadoEn = DateTime.UtcNow
        };

        var producto1 = new Producto
        {
            Id = 1,
            Codigo = "MAT-001",
            Nombre = "Cemento Estructural 50kg",
            Precio = 35000m,
            Stock = 100,
            Activo = true
        };

        var producto2 = new Producto
        {
            Id = 2,
            Codigo = "HER-042",
            Nombre = "Varilla Corrugada 1/2 pulgada",
            Precio = 48000m,
            Stock = 80,
            Activo = true
        };

        var metodoPago = new MetodoPago
        {
            Id = 1,
            Codigo = "TRANSF",
            Nombre = "Transferencia Bancaria Bancolombia",
            Activo = true
        };

        var venta = new Venta
        {
            Id = 1,
            NumeroVenta = "VENTA-20261022-0042",
            ClienteId = cliente.Id,
            Cliente = cliente,
            MetodoPagoId = metodoPago.Id,
            MetodoPago = metodoPago,
            Fecha = DateTime.UtcNow,
            Subtotal = 118000m,
            Iva = 22420m,
            Total = 140420m,
            Estado = "Completada",
            DetallesVenta = new List<DetalleVenta>
            {
                new()
                {
                    Id = 1,
                    VentaId = 1,
                    ProductoId = producto1.Id,
                    Producto = producto1,
                    Cantidad = 2,
                    PrecioUnitario = 35000m,
                    Subtotal = 70000m
                },
                new()
                {
                    Id = 2,
                    VentaId = 1,
                    ProductoId = producto2.Id,
                    Producto = producto2,
                    Cantidad = 1,
                    PrecioUnitario = 48000m,
                    Subtotal = 48000m
                }
            }
        };

        context.Clientes.Add(cliente);
        context.Productos.AddRange(producto1, producto2);
        context.MetodosPago.Add(metodoPago);
        context.Ventas.Add(venta);
        await context.SaveChangesAsync();

        var pdfService = new PdfService(context);

        // Generar factura en PDF
        byte[] pdfBytes = await pdfService.GenerateSaleInvoicePdfAsync(venta.Id);

        // Verificaciones
        pdfBytes.Should().NotBeNull();
        pdfBytes.Length.Should().BeGreaterThan(1000);

        // Verificar la firma inicial estándar del formato PDF: "%PDF-"
        string header = System.Text.Encoding.ASCII.GetString(pdfBytes.Take(5).ToArray());
        header.Should().Be("%PDF-");
    }

    [Fact]
    public async Task GenerateSaleInvoicePdfAsync_WhenSaleNotFound_ShouldThrowKeyNotFoundException()
    {
        using var context = CreateInMemoryDbContext();
        var pdfService = new PdfService(context);

        Func<Task> act = () => pdfService.GenerateSaleInvoicePdfAsync(99999);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*99999*");
    }
}
