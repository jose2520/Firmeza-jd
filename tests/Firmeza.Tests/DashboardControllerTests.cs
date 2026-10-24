// Pruebas para verificar el funcionamiento del controlador del panel de control (Dashboard).
// Comprueba que la ruta de resumen entregue correctamente los totales de clientes,
// productos, ventas e ingresos del negocio con código de éxito 200.

using Firmeza.API.Controllers;
using Firmeza.Application.DTOs;
using Firmeza.Application.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Firmeza.Tests;

public class DashboardControllerTests
{
    // Verifica que al solicitar el resumen del panel de control se devuelvan las cifras esperadas con estado exitoso.
    [Fact]
    public async Task GetSummary_ShouldReturnOkWithSummaryData()
    {
        // Preparar datos de ejemplo para el resumen
        var mockService = new Mock<IDashboardService>();
        var summary = new DashboardSummaryDto(
            TotalClientes: 12,
            TotalProductos: 13,
            TotalVentas: 12,
            TotalIngresos: 2000000m,
            VentasHoy: 2,
            IngresosHoy: 350000m,
            ProductosBajoStock: new List<ProductoBajoStockDto>(),
            TopProductosVendidos: new List<TopProductoVendidoDto>()
        );
        mockService.Setup(s => s.GetSummaryAsync()).ReturnsAsync(summary);

        var controller = new DashboardController(mockService.Object);

        // Ejecutar la consulta del resumen
        var result = await controller.GetSummary();

        // Comprobar que la respuesta sea exitosa y contenga el resumen completo
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);
        okResult.Value.Should().BeEquivalentTo(summary);

        summary.TotalClientes.Should().Be(12);
        summary.TotalProductos.Should().Be(13);
        summary.TotalVentas.Should().Be(12);
        summary.TotalIngresos.Should().Be(2000000m);
        summary.VentasHoy.Should().Be(2);
        summary.IngresosHoy.Should().Be(350000m);
        summary.ProductosBajoStock.Should().BeEmpty();
        summary.TopProductosVendidos.Should().BeEmpty();
    }

    // Comprueba que los objetos de productos con poco inventario y más vendidos conserven sus valores correctamente.
    [Fact]
    public void DashboardDtos_ShouldHoldAssignedValues()
    {
        var bajoStock = new ProductoBajoStockDto(1, "PROD-1", "Martillo", 3, 25000m);
        bajoStock.Id.Should().Be(1);
        bajoStock.Codigo.Should().Be("PROD-1");
        bajoStock.Nombre.Should().Be("Martillo");
        bajoStock.Stock.Should().Be(3);
        bajoStock.Precio.Should().Be(25000m);

        var topVendido = new TopProductoVendidoDto(2, "PROD-2", "Pala", 50, 1500000m);
        topVendido.ProductoId.Should().Be(2);
        topVendido.Codigo.Should().Be("PROD-2");
        topVendido.Nombre.Should().Be("Pala");
        topVendido.TotalUnidadesVendidas.Should().Be(50);
        topVendido.TotalMontoGenerado.Should().Be(1500000m);
    }
}
