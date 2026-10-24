// Pruebas para verificar las rutas del controlador de productos.
// Comprueba la consulta del listado completo, la búsqueda de un producto por su identificador,
// la respuesta cuando no existe y la descarga del catálogo y de la plantilla en Excel.

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

    // Prepara los servicios de productos y de Excel antes de ejecutar cada prueba del controlador.
    public ProductsControllerTests()
    {
        _productoServiceMock = new Mock<IProductoService>();
        _excelServiceMock = new Mock<IExcelService>();
        _controller = new ProductsController(_productoServiceMock.Object, _excelServiceMock.Object);
    }

    // Verifica que al pedir todos los productos se devuelva el código 200 junto con la lista completa.
    [Fact]
    public async Task GetAll_ShouldReturnOkWithProductsList()
    {
        // Preparar una lista de dos productos de prueba
        var productos = new List<ProductoDto>
        {
            new(1, "PROD-001", "Tornillo", "Desc", 100m, 10, true, DateTime.UtcNow),
            new(2, "PROD-002", "Cemento", "Desc", 200m, 20, true, DateTime.UtcNow)
        };
        _productoServiceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(productos);

        // Ejecutar la consulta de todos los productos
        var result = await _controller.GetAll();

        // Comprobar que devuelva estado 200 y los dos productos
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);
        var returnedProducts = okResult.Value as IEnumerable<ProductoDto>;
        returnedProducts.Should().HaveCount(2);
    }

    // Comprueba que al buscar un producto existente por su número identificador se devuelvan sus datos correctamente.
    [Fact]
    public async Task GetById_WhenProductExists_ShouldReturnOk()
    {
        // Preparar un producto existente
        var producto = new ProductoDto(1, "PROD-001", "Tornillo", "Desc", 100m, 10, true, DateTime.UtcNow);
        _productoServiceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(producto);

        // Buscar el producto con identificador 1
        var result = await _controller.GetById(1);

        // Comprobar que devuelva el producto esperado
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.Value.Should().BeEquivalentTo(producto);
    }

    // Verifica que al buscar un producto con un identificador que no existe se responda con código 404.
    [Fact]
    public async Task GetById_WhenProductNotFound_ShouldReturnNotFound()
    {
        // Configurar que el producto 999 no existe
        _productoServiceMock.Setup(s => s.GetByIdAsync(999)).ReturnsAsync((ProductoDto?)null);

        // Buscar el producto inexistente
        var result = await _controller.GetById(999);

        // Comprobar que responda 404 (no encontrado)
        var notFoundResult = result.Result as NotFoundObjectResult;
        notFoundResult.Should().NotBeNull();
        notFoundResult!.StatusCode.Should().Be(404);
    }

    // Comprueba que la ruta de exportar productos a Excel devuelva un archivo descargable con el formato correcto.
    [Fact]
    public async Task ExportExcel_ShouldReturnFileContent()
    {
        // Preparar el contenido del archivo Excel de prueba
        var dummyBytes = new byte[] { 1, 2, 3, 4 };
        _excelServiceMock.Setup(e => e.ExportProductsToExcelAsync()).ReturnsAsync(dummyBytes);

        // Solicitar la exportación a Excel
        var result = await _controller.ExportExcel();

        // Comprobar que devuelva el archivo Excel con sus bytes exactos
        var fileResult = result as FileContentResult;
        fileResult.Should().NotBeNull();
        fileResult!.ContentType.Should().Be("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        fileResult.FileContents.Should().BeEquivalentTo(dummyBytes);
    }

    // Verifica que la ruta de descarga de plantilla entregue el archivo de Excel vacío listo para diligenciar.
    [Fact]
    public void DownloadTemplate_ShouldReturnTemplateFile()
    {
        // Preparar los bytes de la plantilla de prueba
        var dummyBytes = new byte[] { 5, 6, 7, 8 };
        _excelServiceMock.Setup(e => e.GenerateProductsTemplate()).Returns(dummyBytes);

        // Solicitar la descarga de la plantilla
        var result = _controller.DownloadTemplate();

        // Comprobar que se entregue el archivo esperado
        var fileResult = result as FileContentResult;
        fileResult.Should().NotBeNull();
        fileResult!.FileContents.Should().BeEquivalentTo(dummyBytes);
    }
}
