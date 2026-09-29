using Firmeza.API.Controllers;
using Firmeza.Application.DTOs;
using Firmeza.Application.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Firmeza.Tests;

public class SalesControllerTests
{
    private readonly Mock<IVentaService> _ventaServiceMock;
    private readonly SalesController _controller;

    public SalesControllerTests()
    {
        _ventaServiceMock = new Mock<IVentaService>();
        _controller = new SalesController(_ventaServiceMock.Object);
    }

    [Fact]
    public async Task GetAll_ShouldReturnOkWithSalesList()
    {
        // Arrange
        var sales = new List<VentaDto>
        {
            new(1, "VENTA-2026-0001", 1, "Carlos Gómez", "1020304050", DateTime.UtcNow, 100000m, 19000m, 119000m, "Completada", new List<DetalleVentaDto>())
        };
        _ventaServiceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(sales);

        // Act
        var result = await _controller.GetAll();

        // Assert
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);
        var returnedSales = okResult.Value as IEnumerable<VentaDto>;
        returnedSales.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetById_WhenSaleExists_ShouldReturnOk()
    {
        // Arrange
        var sale = new VentaDto(1, "VENTA-2026-0001", 1, "Carlos Gómez", "1020304050", DateTime.UtcNow, 100000m, 19000m, 119000m, "Completada", new List<DetalleVentaDto>());
        _ventaServiceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(sale);

        // Act
        var result = await _controller.GetById(1);

        // Assert
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.Value.Should().BeEquivalentTo(sale);
    }

    [Fact]
    public async Task GetById_WhenNotFound_ShouldReturnNotFound()
    {
        // Arrange
        _ventaServiceMock.Setup(s => s.GetByIdAsync(999)).ReturnsAsync((VentaDto?)null);

        // Act
        var result = await _controller.GetById(999);

        // Assert
        var notFoundResult = result.Result as NotFoundObjectResult;
        notFoundResult.Should().NotBeNull();
        notFoundResult!.StatusCode.Should().Be(404);
    }
}
