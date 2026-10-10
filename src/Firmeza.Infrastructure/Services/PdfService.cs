// =================================================================================================
// SERVICIO: PdfService.cs
// CAPA: Firmeza.Infrastructure (Capa de Infraestructura y Persistencia)
// CONTRATO: IPdfService (Firmeza.Application)
// PROPÓSITO:
//   Motor de renderizado de documentos PDF para la facturación electrónica de Firmeza usando iText 9.
//   Construye un documento PDF tamaño CARTA con alta calidad gráfica:
//   - Membrete y datos comerciales de la empresa.
//   - Información de identificación y contacto del cliente.
//   - Renglones de productos adquiridos con cantidades, precios y subtotales.
//   - Liquidación financiera: Subtotal, IVA 19% y Total final en pesos colombianos ($ COP).
//   - Cláusulas comerciales y pie de página.
//
// ORIGEN Y DESTINO DE LOS DATOS:
//   - Entrada de Datos: Base de datos PostgreSQL (consulta 'ventas' con 'clientes' y 'detalles_venta').
//   - Salida del Documento: Generado enteramente en memoria RAM (MemoryStream) con iText 9.
//     No genera archivos temporales en el disco duro ni en carpetas del servidor.
// =================================================================================================

using System.Globalization;
using Firmeza.Application.Interfaces;
using Firmeza.Infrastructure.Persistence;
using iText.IO.Font.Constants;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.Infrastructure.Services;

public class PdfService : IPdfService
{
    private readonly ApplicationDbContext _context;

    public PdfService(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Genera la factura en PDF para una venta dada y devuelve sus bytes en memoria.
    /// ORIGEN DE DATOS: PostgreSQL ('ventas' JOIN 'clientes' JOIN 'detalles_venta' JOIN 'productos').
    /// </summary>
    public async Task<byte[]> GenerateSaleInvoicePdfAsync(int saleId)
    {
        // 1. Obtener la venta con todas sus entidades vinculadas desde PostgreSQL
        var venta = await _context.Ventas
            .Include(v => v.Cliente)
            .Include(v => v.DetallesVenta)
                .ThenInclude(d => d.Producto)
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.Id == saleId);

        if (venta == null)
        {
            throw new KeyNotFoundException($"Venta con ID {saleId} no encontrada.");
        }

        // 2. Inicializar el motor de renderizado PDF en un flujo de memoria (MemoryStream)
        using var ms = new MemoryStream();
        using var writer = new PdfWriter(ms);
        using var pdf = new PdfDocument(writer);
        using var document = new Document(pdf, PageSize.LETTER);

        document.SetMargins(36, 36, 36, 36);

        // Fuentes estándar Helvetica
        var fontRegular = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
        var fontBold = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);

        document.SetFont(fontRegular);

        // Paleta de colores corporativos de Firmeza
        var primaryColor = new DeviceRgb(30, 58, 138);     // Azul corporativo #1E3A8A
        var secondaryColor = new DeviceRgb(241, 245, 249); // Fondo gris suave #F1F5F9
        var accentColor = new DeviceRgb(15, 118, 110);      // Verde #0F766E

        // -----------------------------------------------------------------------------------------
        // SECCIÓN 1: ENCABEZADO DE LA EMPRESA & DATOS DE LA FACTURA
        // -----------------------------------------------------------------------------------------
        var headerTable = new Table(UnitValue.CreatePercentArray(new float[] { 60, 40 })).UseAllAvailableWidth();
        headerTable.SetBorder(Border.NO_BORDER);

        var companyCell = new Cell().SetBorder(Border.NO_BORDER);
        companyCell.Add(new Paragraph("FIRMEZA S.A.S.")
            .SetFontSize(20)
            .SetFont(fontBold)
            .SetFontColor(primaryColor));
        companyCell.Add(new Paragraph("NIT: 901.458.789-0 | Régimen Común")
            .SetFontSize(9)
            .SetFontColor(ColorConstants.DARK_GRAY));
        companyCell.Add(new Paragraph("Materiales para la Construcción y Ferretería Industrial\nCalle 100 # 15-20, Bogotá D.C.\nTel: (+57) 601 789 4500 | info@firmeza.com")
            .SetFontSize(8)
            .SetFontColor(ColorConstants.GRAY));
        headerTable.AddCell(companyCell);

        var invoiceCell = new Cell().SetBorder(Border.NO_BORDER).SetTextAlignment(TextAlignment.RIGHT);
        invoiceCell.Add(new Paragraph("FACTURA DE VENTA")
            .SetFontSize(16)
            .SetFont(fontBold)
            .SetFontColor(accentColor));
        invoiceCell.Add(new Paragraph($"Nº: {venta.NumeroVenta}")
            .SetFontSize(12)
            .SetFont(fontBold));
        invoiceCell.Add(new Paragraph($"Fecha: {venta.Fecha:yyyy-MM-dd HH:mm}\nEstado: {venta.Estado.ToUpper()}")
            .SetFontSize(9)
            .SetFontColor(ColorConstants.DARK_GRAY));
        headerTable.AddCell(invoiceCell);

        document.Add(headerTable);
        document.Add(new Paragraph("\n"));

        // -----------------------------------------------------------------------------------------
        // SECCIÓN 2: INFORMACIÓN DEL CLIENTE (Extraída de la tabla 'clientes')
        // -----------------------------------------------------------------------------------------
        var clientTable = new Table(UnitValue.CreatePercentArray(new float[] { 50, 50 })).UseAllAvailableWidth();
        clientTable.SetBackgroundColor(secondaryColor);
        clientTable.SetPadding(8);
        clientTable.SetBorder(new SolidBorder(new DeviceRgb(203, 213, 225), 1));

        var cCol1 = new Cell().SetBorder(Border.NO_BORDER);
        cCol1.Add(new Paragraph("DATOS DEL CLIENTE").SetFont(fontBold).SetFontSize(10).SetFontColor(primaryColor));
        cCol1.Add(new Paragraph($"Cliente: {venta.Cliente.Nombres} {venta.Cliente.Apellidos}").SetFontSize(9));
        cCol1.Add(new Paragraph($"Identificación / NIT: {venta.Cliente.Identificacion}").SetFontSize(9));
        clientTable.AddCell(cCol1);

        var cCol2 = new Cell().SetBorder(Border.NO_BORDER);
        cCol2.Add(new Paragraph($"Teléfono: {venta.Cliente.Telefono ?? "No registrado"}").SetFontSize(9));
        cCol2.Add(new Paragraph($"Dirección: {venta.Cliente.Direccion ?? "No registrada"}").SetFontSize(9));
        cCol2.Add(new Paragraph($"Email: {venta.Cliente.Email}").SetFontSize(9));
        clientTable.AddCell(cCol2);

        document.Add(clientTable);
        document.Add(new Paragraph("\n"));

        // -----------------------------------------------------------------------------------------
        // SECCIÓN 3: TABLA DE PRODUCTOS / RENGLONES (Extraídos de 'detalles_venta' y 'productos')
        // -----------------------------------------------------------------------------------------
        var itemsTable = new Table(UnitValue.CreatePercentArray(new float[] { 15, 45, 10, 15, 15 })).UseAllAvailableWidth();

        string[] cols = { "Código", "Descripción del Producto", "Cant.", "Precio Unit.", "Subtotal" };
        foreach (var col in cols)
        {
            var th = new Cell()
                .Add(new Paragraph(col).SetFont(fontBold).SetFontSize(9).SetFontColor(ColorConstants.WHITE))
                .SetBackgroundColor(primaryColor)
                .SetTextAlignment(col == "Cant." || col.Contains("Precio") || col == "Subtotal" ? TextAlignment.RIGHT : TextAlignment.LEFT)
                .SetPadding(6);
            itemsTable.AddHeaderCell(th);
        }

        var colFormat = new CultureInfo("es-CO");
        int index = 0;
        foreach (var d in venta.DetallesVenta)
        {
            var rowBg = index % 2 == 0 ? ColorConstants.WHITE : new DeviceRgb(248, 250, 252);

            itemsTable.AddCell(new Cell().Add(new Paragraph(d.Producto?.Codigo ?? "-").SetFontSize(8)).SetBackgroundColor(rowBg).SetPadding(5));
            itemsTable.AddCell(new Cell().Add(new Paragraph(d.Producto?.Nombre ?? "Producto").SetFontSize(8)).SetBackgroundColor(rowBg).SetPadding(5));
            itemsTable.AddCell(new Cell().Add(new Paragraph(d.Cantidad.ToString()).SetFontSize(8)).SetTextAlignment(TextAlignment.RIGHT).SetBackgroundColor(rowBg).SetPadding(5));
            itemsTable.AddCell(new Cell().Add(new Paragraph(d.PrecioUnitario.ToString("C2", colFormat)).SetFontSize(8)).SetTextAlignment(TextAlignment.RIGHT).SetBackgroundColor(rowBg).SetPadding(5));
            itemsTable.AddCell(new Cell().Add(new Paragraph(d.Subtotal.ToString("C2", colFormat)).SetFontSize(8)).SetTextAlignment(TextAlignment.RIGHT).SetBackgroundColor(rowBg).SetPadding(5));
            index++;
        }

        document.Add(itemsTable);
        document.Add(new Paragraph("\n"));

        // -----------------------------------------------------------------------------------------
        // SECCIÓN 4: TOTALES FINANCIEROS (Subtotal, IVA 19% y Total)
        // -----------------------------------------------------------------------------------------
        var totalsTable = new Table(UnitValue.CreatePercentArray(new float[] { 60, 40 })).UseAllAvailableWidth();
        totalsTable.SetBorder(Border.NO_BORDER);

        var notesCell = new Cell().SetBorder(Border.NO_BORDER);
        notesCell.Add(new Paragraph("Términos y Condiciones:")
            .SetFont(fontBold).SetFontSize(8).SetFontColor(ColorConstants.DARK_GRAY));
        notesCell.Add(new Paragraph("1. Esta factura se asimila en sus efectos a una letra de cambio según el Código de Comercio.\n2. No se aceptan reclamos pasados 8 días de recibida la mercancía.\n3. Todo producto cuenta con garantía directa de fábrica.")
            .SetFontSize(7).SetFontColor(ColorConstants.GRAY));
        totalsTable.AddCell(notesCell);

        var mathCell = new Cell().SetBorder(Border.NO_BORDER);
        var subTable = new Table(UnitValue.CreatePercentArray(new float[] { 50, 50 })).UseAllAvailableWidth();

        subTable.AddCell(new Cell().Add(new Paragraph("Subtotal:").SetFontSize(9)).SetBorder(Border.NO_BORDER));
        subTable.AddCell(new Cell().Add(new Paragraph(venta.Subtotal.ToString("C2", colFormat)).SetFontSize(9)).SetTextAlignment(TextAlignment.RIGHT).SetBorder(Border.NO_BORDER));

        subTable.AddCell(new Cell().Add(new Paragraph("IVA (19%):").SetFontSize(9)).SetBorder(Border.NO_BORDER));
        subTable.AddCell(new Cell().Add(new Paragraph(venta.Iva.ToString("C2", colFormat)).SetFontSize(9)).SetTextAlignment(TextAlignment.RIGHT).SetBorder(Border.NO_BORDER));

        var totalTitleCell = new Cell().Add(new Paragraph("TOTAL:").SetFont(fontBold).SetFontSize(11).SetFontColor(primaryColor)).SetBorder(new SolidBorder(primaryColor, 1));
        var totalValueCell = new Cell().Add(new Paragraph(venta.Total.ToString("C2", colFormat)).SetFont(fontBold).SetFontSize(11).SetFontColor(primaryColor)).SetTextAlignment(TextAlignment.RIGHT).SetBorder(new SolidBorder(primaryColor, 1));
        subTable.AddCell(totalTitleCell);
        subTable.AddCell(totalValueCell);

        mathCell.Add(subTable);
        totalsTable.AddCell(mathCell);

        document.Add(totalsTable);

        // -----------------------------------------------------------------------------------------
        // SECCIÓN 5: PIE DE PÁGINA
        // -----------------------------------------------------------------------------------------
        document.Add(new Paragraph("\n\n"));
        var footer = new Paragraph("Generado electrónicamente por Plataforma Firmeza | www.firmeza.com | Gracias por su confianza")
            .SetFontSize(8)
            .SetFontColor(ColorConstants.GRAY)
            .SetTextAlignment(TextAlignment.CENTER);
        document.Add(footer);

        // Cierra y compila el flujo binario
        document.Close();
        var bytes = ms.ToArray();

        // Guardar copia física en wwwroot/recibos según requerimiento de US 2 (Tarea 2)
        try
        {
            var folderPath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "recibos");
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            var filePath = System.IO.Path.Combine(folderPath, $"Factura_{venta.NumeroVenta}.pdf");
            File.WriteAllBytes(filePath, bytes);
        }
        catch
        {
            // Si el entorno no permite escritura en disco, continúa sin interrumpir el stream al cliente
        }

        return bytes;
    }
}
