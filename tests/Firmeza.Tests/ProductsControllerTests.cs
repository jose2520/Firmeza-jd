using Firmeza.API.Controllers;
using Firmeza.Application.DTOs;
using Firmeza.Application.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Firmeza.Tests;

public class ProductsControllerTests
{
    private readonly Mock<IProductoService> _productoServiceMock;
    private readonly ProductsController _controller;

    public ProductsControllerTests()
    {
        _productoServiceMock = new Mock<IProductoService>();
        _controller = new ProductsController(_productoServiceMock.Object);
    }

    [Fact]
    public async Task GetAll_ShouldReturnOkWithProductsList()
    {
        // Arrange
        var productos = new List<ProductoDto>
        {
            new(1, "PROD-001", "Tornillo", "Desc", 100m, 10, true, DateTime.UtcNow),
            new(2, "PROD-002", "Cemento", "Desc", 200m, 20, true, DateTime.UtcNow)
        };
        _productoServiceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(productos);

        // Act
        var result = await _controller.GetAll();

        // Assert
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);
        var returnedProducts = okResult.Value as IEnumerable<ProductoDto>;
        returnedProducts.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetById_WhenProductExists_ShouldReturnOk()
    {
        // Arrange
        var producto = new ProductoDto(1, "PROD-001", "Tornillo", "Desc", 100m, 10, true, DateTime.UtcNow);
        _productoServiceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(producto);

        // Act
        var result = await _controller.GetById(1);

        // Assert
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.Value.Should().BeEquivalentTo(producto);
    }

    [Fact]
    public async Task GetById_WhenProductNotFound_ShouldReturnNotFound()
    {
        // Arrange
        _productoServiceMock.Setup(s => s.GetByIdAsync(999)).ReturnsAsync((ProductoDto?)null);

        // Act
        var result = await _controller.GetById(999);

        // Assert
        var notFoundResult = result.Result as NotFoundObjectResult;
        notFoundResult.Should().NotBeNull();
        notFoundResult!.StatusCode.Should().Be(404);
    }
}
