using Firmeza.API.Controllers;
using Firmeza.Application.DTOs;
using Firmeza.Application.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Firmeza.Tests;

public class ProductsControllerTests
{
    private readonly Mock<IProductoService> _productoServiceMock;
    private readonly Mock<IExcelService> _excelServiceMock;
    private readonly ProductsController _controller;

    public ProductsControllerTests()
    {
        _productoServiceMock = new Mock<IProductoService>();
        _excelServiceMock = new Mock<IExcelService>();
        _controller = new ProductsController(_productoServiceMock.Object, _excelServiceMock.Object);
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

    [Fact]
    public async Task ExportExcel_ShouldReturnFileContent()
    {
        // Arrange
        var dummyBytes = new byte[] { 1, 2, 3, 4 };
        _excelServiceMock.Setup(e => e.ExportProductsToExcelAsync()).ReturnsAsync(dummyBytes);

        // Act
        var result = await _controller.ExportExcel();

        // Assert
        var fileResult = result as FileContentResult;
        fileResult.Should().NotBeNull();
        fileResult!.ContentType.Should().Be("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        fileResult.FileContents.Should().BeEquivalentTo(dummyBytes);
    }

    [Fact]
    public void DownloadTemplate_ShouldReturnTemplateFile()
    {
        // Arrange
        var dummyBytes = new byte[] { 5, 6, 7, 8 };
        _excelServiceMock.Setup(e => e.GenerateProductsTemplate()).Returns(dummyBytes);

        // Act
        var result = _controller.DownloadTemplate();

        // Assert
        var fileResult = result as FileContentResult;
        fileResult.Should().NotBeNull();
        fileResult!.FileContents.Should().BeEquivalentTo(dummyBytes);
    }
}
