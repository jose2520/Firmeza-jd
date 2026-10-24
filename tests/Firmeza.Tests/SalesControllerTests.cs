// Pruebas para verificar las rutas del controlador de ventas.
// Comprueba la consulta de ventas, la descarga de facturas en PDF, la exportación del historial a Excel
// y la protección de seguridad para que un cliente no pueda ver las compras de otra persona.

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

    // Inicializa los servicios simulados de ventas, PDF, Excel, correo y clientes para probar el controlador.
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
            _clienteServiceMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext()
            }
        };
    }

    // Verifica que al consultar todas las ventas se responda con código 200 y la lista de compras registradas.
    [Fact]
    public async Task GetAll_ShouldReturnOkWithSalesList()
    {
        // Preparar una lista con una venta de ejemplo
        var sales = new List<VentaDto>
        {
            new(1, "VENTA-2026-0001", 1, "Carlos Gómez", "1020304050", DateTime.UtcNow, 100000m, 19000m, 119000m, "Completada", new List<DetalleVentaDto>())
        };
        _ventaServiceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(sales);

        // Ejecutar la consulta del listado
        var result = await _controller.GetAll();

        // Comprobar que responda 200 con la venta esperada
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);
        var returnedSales = okResult.Value as IEnumerable<VentaDto>;
        returnedSales.Should().HaveCount(1);
    }

    // Comprueba que al consultar una venta existente por su identificador se devuelva su información completa.
    [Fact]
    public async Task GetById_WhenSaleExists_ShouldReturnOk()
    {
        // Preparar los datos de la venta 1
        var sale = new VentaDto(1, "VENTA-2026-0001", 1, "Carlos Gómez", "1020304050", DateTime.UtcNow, 100000m, 19000m, 119000m, "Completada", new List<DetalleVentaDto>());
        _ventaServiceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(sale);

        // Consultar la venta 1
        var result = await _controller.GetById(1);

        // Comprobar que los datos devueltos coincidan con la venta
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.Value.Should().BeEquivalentTo(sale);
    }

    // Verifica que al buscar una venta que no existe se devuelva el código 404 (no encontrada).
    [Fact]
    public async Task GetById_WhenNotFound_ShouldReturnNotFound()
    {
        // Configurar que la venta 999 no existe
        _ventaServiceMock.Setup(s => s.GetByIdAsync(999)).ReturnsAsync((VentaDto?)null);

        // Consultar la venta 999
        var result = await _controller.GetById(999);

        // Comprobar que responda 404
        var notFoundResult = result.Result as NotFoundObjectResult;
        notFoundResult.Should().NotBeNull();
        notFoundResult!.StatusCode.Should().Be(404);
    }

    // Comprueba que al descargar la factura de una venta existente se entregue el archivo PDF con el número de venta en su nombre.
    [Fact]
    public async Task DownloadInvoicePdf_WhenSaleExists_ShouldReturnPdfFile()
    {
        // Preparar un archivo PDF de prueba y los datos de la venta
        var dummyPdf = new byte[] { 37, 80, 68, 70 }; // %PDF
        var sale = new VentaDto(1, "VENTA-2026-0001", 1, "Carlos", "102", DateTime.UtcNow, 100, 19, 119, "Completada", new List<DetalleVentaDto>());
        _pdfServiceMock.Setup(p => p.GenerateSaleInvoicePdfAsync(1)).ReturnsAsync(dummyPdf);
        _ventaServiceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(sale);

        // Solicitar la descarga del PDF
        var result = await _controller.DownloadInvoicePdf(1);

        // Comprobar que se entregue un archivo PDF válido con el nombre correcto
        var fileResult = result as FileContentResult;
        fileResult.Should().NotBeNull();
        fileResult!.ContentType.Should().Be("application/pdf");
        fileResult.FileDownloadName.Should().Contain("VENTA-2026-0001");
        fileResult.FileContents.Should().BeEquivalentTo(dummyPdf);
    }

    // Verifica que la ruta de exportar ventas a Excel entregue el archivo descargable correctamente.
    [Fact]
    public async Task ExportSalesExcel_ShouldReturnExcelFile()
    {
        // Preparar los bytes del archivo Excel de prueba
        var dummyExcel = new byte[] { 10, 20, 30 };
        _excelServiceMock.Setup(e => e.ExportSalesToExcelAsync()).ReturnsAsync(dummyExcel);

        // Solicitar la exportación del historial de ventas
        var result = await _controller.ExportSalesExcel();

        // Comprobar que devuelva el archivo Excel esperado
        var fileResult = result as FileContentResult;
        fileResult.Should().NotBeNull();
        fileResult!.ContentType.Should().Be("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        fileResult.FileContents.Should().BeEquivalentTo(dummyExcel);
    }

    // Comprueba que si un cliente conectado intenta ver la factura de otro cliente distinto, el sistema le bloquee el acceso.
    [Fact]
    public async Task GetById_WhenAuthenticatedClientRequestsAnotherClientsSale_ShouldReturnForbid()
    {
        // Preparar una venta que pertenece al cliente 2 mientras quien consulta es el cliente 1
        var saleOfClient2 = new VentaDto(10, "VENTA-2026-0010", 2, "Otro Cliente", "999", DateTime.UtcNow, 100000m, 19000m, 119000m, "Completada", new List<DetalleVentaDto>());
        _ventaServiceMock.Setup(s => s.GetByIdAsync(10)).ReturnsAsync(saleOfClient2);
        _clienteServiceMock.Setup(c => c.GetAllAsync()).ReturnsAsync(new List<ClienteDto>
        {
            new(1, "1020", "Cliente Uno", "Gómez", "cliente1@example.com", "300", "Dir", true, DateTime.UtcNow, "user-1", 30)
        });

        var claims = new[]
        {
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "user-1"),
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Email, "cliente1@example.com"),
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, "Cliente")
        };
        var identity = new System.Security.Claims.ClaimsIdentity(claims, "TestAuth");
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext
            {
                User = new System.Security.Claims.ClaimsPrincipal(identity)
            }
        };

        // Intentar consultar la venta ajena
        var result = await _controller.GetById(10);

        // Comprobar que se deniegue el acceso
        result.Result.Should().BeOfType<ForbidResult>();
    }
}
