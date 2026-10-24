// Pruebas para verificar la generación de plantillas y la carga masiva desde archivos de Excel.
// Comprueba que una hoja de cálculo que trae productos y clientes mezclados en una misma fila
// los separe y guarde correctamente en sus respectivas tablas.

using Firmeza.Application.DTOs;
using Firmeza.Infrastructure.Persistence;
using Firmeza.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

namespace Firmeza.Tests;

public class DesnormalizedExcelTests
{
    // Crea una base de datos temporal en memoria para probar la importación de archivos Excel.
    private ApplicationDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    // Verifica que la plantilla de Excel combinada se genere correctamente y devuelva un archivo con contenido válido.
    [Fact]
    public void GenerateDesnormalizedTemplate_ShouldReturnValidBytes()
    {
        // Preparar el servicio de Excel
        using var context = CreateInMemoryDbContext();
        var service = new ExcelService(context);

        // Generar la plantilla de Excel
        var bytes = service.GenerateDesnormalizedTemplate();

        // Comprobar que el archivo generado no esté vacío
        bytes.Should().NotBeNull();
        bytes.Length.Should().BeGreaterThan(100);
    }

    // Comprueba que al importar un archivo Excel con datos de producto y cliente en la misma fila, ambos se creen en la base de datos.
    [Fact]
    public async Task ImportDesnormalizedDataAsync_ShouldImportProductsAndClientsSuccessfully()
    {
        // Preparar un archivo Excel en memoria con una fila de prueba
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

        // Ejecutar la importación del archivo Excel
        var result = await service.ImportDesnormalizedDataAsync(stream);

        // Comprobar que se crearon tanto el producto como el cliente sin errores
        result.Should().NotBeNull();
        result.TotalFilas.Should().Be(1);
        result.ProductosCreados.Should().Be(1);
        result.ProductosActualizados.Should().Be(0);
        result.ClientesCreados.Should().Be(1);
        result.ClientesActualizados.Should().Be(0);
        result.TotalOmitidos.Should().Be(0);
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

    // Comprueba que el resultado de importar productos desde Excel guarde adecuadamente las filas procesadas y los errores.
    [Fact]
    public void ExcelImportResultDto_ShouldHoldAssignedValues()
    {
        var dto = new ExcelImportResultDto(10, 8, 2, new List<string> { "Fila 3 inválida" });
        dto.TotalFilas.Should().Be(10);
        dto.TotalImportados.Should().Be(8);
        dto.TotalIgnorados.Should().Be(2);
        dto.Errores.Should().ContainSingle();
    }

    // Verifica que la exportación de catálogo a Excel genere un libro con diseño corporativo, encabezados y totales.
    [Fact]
    public async Task ExportProductsToExcelAsync_ShouldGenerateStyledWorkbook()
    {
        using var context = CreateInMemoryDbContext();
        context.Productos.Add(new Firmeza.Domain.Entities.Producto
        {
            Codigo = "PROD-A1",
            Nombre = "Cemento Gris 50kg",
            Precio = 32000m,
            Stock = 25,
            Activo = true,
            CreadoEn = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var service = new ExcelService(context);
        var bytes = await service.ExportProductsToExcelAsync();

        bytes.Should().NotBeNull();
        bytes.Length.Should().BeGreaterThan(500);

        using var ms = new MemoryStream(bytes);
        using var package = new ExcelPackage(ms);
        var ws = package.Workbook.Worksheets["Catálogo de Productos"];
        ws.Should().NotBeNull();
        ws.Cells["A1"].Text.Should().Contain("CATÁLOGO GENERAL DE PRODUCTOS");
        ws.Cells[4, 2].Text.Should().Be("Código");
        ws.Cells[5, 2].Text.Should().Be("PROD-A1");
        ws.View.ShowGridLines.Should().BeTrue();
        ws.Drawings.Count.Should().BeGreaterThanOrEqualTo(1);

        // Verifica que también se genere la hoja ejecutiva de resumen analítico
        var wsDash = package.Workbook.Worksheets["📊 Resumen de Inventario"];
        wsDash.Should().NotBeNull();
        wsDash.Cells["B1"].Text.Should().Contain("TABLERO ANALÍTICO Y SALUD DE INVENTARIO");
        wsDash.View.ShowGridLines.Should().BeTrue();
    }

    // Verifica que la exportación de ventas a Excel genere un reporte estilizado con todas las columnas comerciales.
    [Fact]
    public async Task ExportSalesToExcelAsync_ShouldGenerateStyledWorkbook()
    {
        using var context = CreateInMemoryDbContext();
        var cliente = new Firmeza.Domain.Entities.Cliente
        {
            Identificacion = "123456789",
            Nombres = "Carlos",
            Apellidos = "Pérez",
            Email = "carlos@test.com",
            Activo = true
        };
        context.Clientes.Add(cliente);
        await context.SaveChangesAsync();

        context.Ventas.Add(new Firmeza.Domain.Entities.Venta
        {
            NumeroVenta = "VENTA-20261022-001",
            ClienteId = cliente.Id,
            Cliente = cliente,
            Fecha = DateTime.UtcNow,
            Subtotal = 100000m,
            Iva = 19000m,
            Total = 119000m,
            Estado = "Completada"
        });
        await context.SaveChangesAsync();

        var service = new ExcelService(context);
        var bytes = await service.ExportSalesToExcelAsync();

        bytes.Should().NotBeNull();
        bytes.Length.Should().BeGreaterThan(500);

        using var ms = new MemoryStream(bytes);
        using var package = new ExcelPackage(ms);
        var ws = package.Workbook.Worksheets["Ventas Registradas"];
        ws.Should().NotBeNull();
        ws.Cells["A1"].Text.Should().Contain("HISTÓRICO Y CONTROL GENERAL DE VENTAS");
        ws.Cells[4, 2].Text.Should().Be("Nº Factura");
        ws.Cells[5, 2].Text.Should().Be("VENTA-20261022-001");
        ws.View.ShowGridLines.Should().BeTrue();
        ws.Drawings.Count.Should().BeGreaterThanOrEqualTo(1);

        // Verifica que también se genere la hoja ejecutiva de tablero gerencial de ventas
        var wsDash = package.Workbook.Worksheets["📊 Tablero Ejecutivo"];
        wsDash.Should().NotBeNull();
        wsDash.Cells["B1"].Text.Should().Contain("TABLERO GERENCIAL DE VENTAS Y RECAUDO");
        wsDash.View.ShowGridLines.Should().BeTrue();
    }

    // Verifica que la plantilla simple de carga de productos se genere con las columnas esperadas.
    [Fact]
    public void GenerateProductsTemplate_ShouldReturnValidBytes()
    {
        using var context = CreateInMemoryDbContext();
        var service = new ExcelService(context);
        var bytes = service.GenerateProductsTemplate();

        bytes.Should().NotBeNull();
        bytes.Length.Should().BeGreaterThan(100);

        using var ms = new MemoryStream(bytes);
        using var package = new ExcelPackage(ms);
        var ws = package.Workbook.Worksheets["Plantilla Carga Productos"];
        ws.Should().NotBeNull();
        ws.Cells[1, 1].Text.Should().Be("Codigo");
    }

    // Verifica que cuando no hay productos registrados, el archivo se genere con mensaje informativo corporativo.
    [Fact]
    public async Task ExportProductsToExcelAsync_WhenEmpty_ShouldGenerateEmptyCatalogNotice()
    {
        using var context = CreateInMemoryDbContext();
        var service = new ExcelService(context);
        var bytes = await service.ExportProductsToExcelAsync();

        bytes.Should().NotBeNull();
        using var ms = new MemoryStream(bytes);
        using var package = new ExcelPackage(ms);
        var ws = package.Workbook.Worksheets["Catálogo de Productos"];
        ws.Should().NotBeNull();
        ws.Cells[5, 1].Text.Should().Contain("No se encontraron productos registrados");
    }

    // Verifica que cuando no hay ventas registradas, el archivo se genere con mensaje informativo corporativo.
    [Fact]
    public async Task ExportSalesToExcelAsync_WhenEmpty_ShouldGenerateEmptySalesNotice()
    {
        using var context = CreateInMemoryDbContext();
        var service = new ExcelService(context);
        var bytes = await service.ExportSalesToExcelAsync();

        bytes.Should().NotBeNull();
        using var ms = new MemoryStream(bytes);
        using var package = new ExcelPackage(ms);
        var ws = package.Workbook.Worksheets["Ventas Registradas"];
        ws.Should().NotBeNull();
        ws.Cells[5, 1].Text.Should().Contain("No se encontraron ventas registradas");
    }
}
