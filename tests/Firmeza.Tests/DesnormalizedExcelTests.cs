using Firmeza.Infrastructure.Persistence;
using Firmeza.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

namespace Firmeza.Tests;

public class DesnormalizedExcelTests
{
    private ApplicationDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    [Fact]
    public void GenerateDesnormalizedTemplate_ShouldReturnValidBytes()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var service = new ExcelService(context);

        // Act
        var bytes = service.GenerateDesnormalizedTemplate();

        // Assert
        bytes.Should().NotBeNull();
        bytes.Length.Should().BeGreaterThan(100);
    }

    [Fact]
    public async Task ImportDesnormalizedDataAsync_ShouldImportProductsAndClientsSuccessfully()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var service = new ExcelService(context);

        using var package = new ExcelPackage();
        var ws = package.Workbook.Worksheets.Add("Sheet1");
        ws.Cells[1, 1].Value = "Codigo_Producto";
        ws.Cells[1, 2].Value = "Nombre_Producto";
        ws.Cells[1, 3].Value = "Precio";
        ws.Cells[1, 4].Value = "Stock";
        ws.Cells[1, 5].Value = "Identificacion_Cliente";
        ws.Cells[1, 6].Value = "Nombres_Cliente";
        ws.Cells[1, 7].Value = "Email_Cliente";
        ws.Cells[1, 8].Value = "Edad_Cliente";

        ws.Cells[2, 1].Value = "PROD-X1";
        ws.Cells[2, 2].Value = "Taladro de Banco 550W";
        ws.Cells[2, 3].Value = "380000";
        ws.Cells[2, 4].Value = "15";
        ws.Cells[2, 5].Value = "1122334455";
        ws.Cells[2, 6].Value = "Andrés Gómez";
        ws.Cells[2, 7].Value = "andres.gomez@test.com";
        ws.Cells[2, 8].Value = "32";

        var bytes = package.GetAsByteArray();
        using var stream = new MemoryStream(bytes);

        // Act
        var result = await service.ImportDesnormalizedDataAsync(stream);

        // Assert
        result.Should().NotBeNull();
        result.TotalFilas.Should().Be(1);
        result.ProductosCreados.Should().Be(1);
        result.ClientesCreados.Should().Be(1);
        result.Errores.Should().BeEmpty();

        var prodInDb = await context.Productos.FirstOrDefaultAsync(p => p.Codigo == "PROD-X1");
        prodInDb.Should().NotBeNull();
        prodInDb!.Nombre.Should().Be("Taladro de Banco 550W");
        prodInDb.Precio.Should().Be(380000m);

        var cliInDb = await context.Clientes.FirstOrDefaultAsync(c => c.Identificacion == "1122334455");
        cliInDb.Should().NotBeNull();
        cliInDb!.Nombres.Should().Be("Andrés Gómez");
        cliInDb.Edad.Should().Be(32);
    }
}
