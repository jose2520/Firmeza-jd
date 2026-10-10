using Firmeza.API.Controllers;
using Firmeza.Application.DTOs;
using Firmeza.Application.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Firmeza.Tests;

public class DashboardControllerTests
{
    [Fact]
    public async Task GetSummary_ShouldReturnOkWithSummaryData()
    {
        // Arrange
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

        // Act
        var result = await controller.GetSummary();

        // Assert
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);
        okResult.Value.Should().BeEquivalentTo(summary);
    }
}
