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
    private readonly Mock<IPdfService> _pdfServiceMock;
    private readonly Mock<IExcelService> _excelServiceMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<IClienteService> _clienteServiceMock;
    private readonly SalesController _controller;

    public SalesControllerTests()
    {
        _ventaServiceMock = new Mock<IVentaService>();
        _pdfServiceMock = new Mock<IPdfService>();
        _excelServiceMock = new Mock<IExcelService>();
        _emailServiceMock = new Mock<IEmailService>();
        _clienteServiceMock = new Mock<IClienteService>();
        _controller = new SalesController(
            _ventaServiceMock.Object,
            _pdfServiceMock.Object,
            _excelServiceMock.Object,
            _emailServiceMock.Object,
            _clienteServiceMock.Object);
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

    [Fact]
    public async Task DownloadInvoicePdf_WhenSaleExists_ShouldReturnPdfFile()
    {
        // Arrange
        var dummyPdf = new byte[] { 37, 80, 68, 70 }; // %PDF
        var sale = new VentaDto(1, "VENTA-2026-0001", 1, "Carlos", "102", DateTime.UtcNow, 100, 19, 119, "Completada", new List<DetalleVentaDto>());
        _pdfServiceMock.Setup(p => p.GenerateSaleInvoicePdfAsync(1)).ReturnsAsync(dummyPdf);
        _ventaServiceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(sale);

        // Act
        var result = await _controller.DownloadInvoicePdf(1);

        // Assert
        var fileResult = result as FileContentResult;
        fileResult.Should().NotBeNull();
        fileResult!.ContentType.Should().Be("application/pdf");
        fileResult.FileDownloadName.Should().Contain("VENTA-2026-0001");
        fileResult.FileContents.Should().BeEquivalentTo(dummyPdf);
    }

    [Fact]
    public async Task ExportSalesExcel_ShouldReturnExcelFile()
    {
        // Arrange
        var dummyExcel = new byte[] { 10, 20, 30 };
        _excelServiceMock.Setup(e => e.ExportSalesToExcelAsync()).ReturnsAsync(dummyExcel);

        // Act
        var result = await _controller.ExportSalesExcel();

        // Assert
        var fileResult = result as FileContentResult;
        fileResult.Should().NotBeNull();
        fileResult!.ContentType.Should().Be("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        fileResult.FileContents.Should().BeEquivalentTo(dummyExcel);
    }
}
