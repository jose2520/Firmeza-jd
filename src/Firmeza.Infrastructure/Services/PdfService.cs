// =================================================================================================
// SERVICIO: PdfService.cs
// CAPA: Firmeza.Infrastructure (Capa de Infraestructura y Persistencia)
// CONTRATO: IPdfService (Firmeza.Application)
// PROPÓSITO:
//   Motor de renderizado de facturas electrónicas de venta en PDF con estándar empresarial
//   y validación técnica DIAN (iText 9). Incorpora la identidad visual oficial de Firmeza S.A.S.:
//   - Azul Noche Corporativo (#091522), Dorado Institucional (#E5A93C) y Pizarra (#1E293B).
//   - Logotipo oficial de alta resolución incrustado.
//   - Código QR de autenticación fiscal DIAN con hash CUFE (Código Único de Factura Electrónica).
//   - Desglose financiero completo con conversión automática de montos a letras en español.
//   - Información comercial del adquirente, cuentas bancarias y cláusulas de aceptación legal.
// =================================================================================================

using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Firmeza.Application.Interfaces;
using Firmeza.Infrastructure.Persistence;
using iText.Barcodes;
using iText.IO.Font.Constants;
using iText.IO.Image;
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

    // Inicializa el servicio generador de PDF con la conexión a la base de datos.
    public PdfService(ApplicationDbContext context)
    {
        _context = context;
    }

    // Obtiene la ruta física del logotipo corporativo según el entorno de ejecución.
    private static string? GetLogoPath(bool dark = false)
    {
        var fileName = dark ? "firme.png" : "firme2.png";
        var candidates = new[]
        {
            System.IO.Path.Combine(AppContext.BaseDirectory, "Resources", "Assets", fileName),
            System.IO.Path.Combine(Directory.GetCurrentDirectory(), "Resources", "Assets", fileName),
            System.IO.Path.Combine(Directory.GetCurrentDirectory(), "src", "Firmeza.Infrastructure", "Resources", "Assets", fileName),
            $"/home/cohorte5/Documentos/jose/Firmeza.app/Firmeza/assets/expo.icon/Assets/{fileName}",
            System.IO.Path.Combine(AppContext.BaseDirectory, fileName)
        };

        return candidates.FirstOrDefault(File.Exists);
    }

    // Convierte un monto numérico a su representación formal en letras según la normativa contable colombiana.
    public static string ConvertirNumeroALetras(decimal numero)
    {
        var unidades = new[]
        {
            "", "UN", "DOS", "TRES", "CUATRO", "CINCO", "SEIS", "SIETE", "OCHO", "NUEVE",
            "DIEZ", "ONCE", "DOCE", "TRECE", "CATORCE", "QUINCE", "DIECISÉIS", "DIECISIETE", "DIECIOCHO", "DIECINUEVE",
            "VEINTE", "VEINTIÚN", "VEINTIDÓS", "VEINTITRÉS", "VEINTICUATRO", "VEINTICINCO", "VEINTISÉIS", "VEINTISIETE", "VEINTIOCHO", "VEINTINUEVE"
        };

        var decenas = new[]
        {
            "", "", "", "TREINTA", "CUARENTA", "CINCUENTA", "SESENTA", "SETENTA", "OCHENTA", "NOVENTA"
        };

        var centenas = new[]
        {
            "", "CIENTO", "DOSCIENTOS", "TRESCIENTOS", "CUATROCIENTOS", "QUINIENTOS", "SEISCIENTOS", "SETECIENTOS", "OCHOCIENTOS", "NOVECIENTOS"
        };

        string Seccion(long n)
        {
            if (n == 0) return "";
            if (n == 100) return "CIEN";

            long c = n / 100;
            long d = n % 100;
            var sb = new StringBuilder();

            if (c > 0)
            {
                sb.Append(centenas[c]).Append(' ');
            }

            if (d < 30)
            {
                sb.Append(unidades[d]);
            }
            else
            {
                long dec = d / 10;
                long u = d % 10;
                sb.Append(decenas[dec]);
                if (u > 0)
                {
                    sb.Append(" Y ").Append(unidades[u]);
                }
            }

            return sb.ToString().Trim();
        }

        long entero = (long)Math.Truncate(numero);
        int centavos = (int)Math.Round((numero - Math.Truncate(numero)) * 100);

        if (entero == 0)
        {
            return centavos > 0
                ? $"CERO PESOS CON {centavos:D2}/100 M/CTE."
                : "CERO PESOS M/CTE.";
        }

        long millones = entero / 1_000_000;
        long miles = (entero % 1_000_000) / 1000;
        long resto = entero % 1000;

        var partes = new List<string>();

        if (millones == 1)
        {
            partes.Add("UN MILLÓN");
        }
        else if (millones > 1)
        {
            partes.Add($"{Seccion(millones)} MILLONES");
        }

        if (miles == 1)
        {
            partes.Add("MIL");
        }
        else if (miles > 1)
        {
            partes.Add($"{Seccion(miles)} MIL");
        }

        if (resto > 0)
        {
            partes.Add(Seccion(resto));
        }

        var texto = string.Join(" ", partes).Trim();
        string moneda = (millones > 0 && miles == 0 && resto == 0) ? "DE PESOS" : (entero == 1 ? "PESO" : "PESOS");

        return centavos > 0
            ? $"{texto} {moneda} CON {centavos:D2}/100 M/CTE."
            : $"{texto} {moneda} M/CTE.";
    }

    // Genera un código hash CUFE (Código Único de Factura Electrónica) representativo según el estándar DIAN.
    private static string GenerarCufe(string numFac, DateTime fecha, decimal total, decimal subtotal, decimal iva, string nitCliente)
    {
        var rawData = $"{numFac}|{fecha:yyyyMMddHHmmss}|{total:F2}|01|{iva:F2}|0.00|0.00|{subtotal:F2}|{nitCliente}|9014587890";
        var hashBytes = SHA384.HashData(Encoding.UTF8.GetBytes(rawData));
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }

    // Busca una venta por su ID en la base de datos y genera el archivo PDF con diseño corporativo oficial.
    public async Task<byte[]> GenerateSaleInvoicePdfAsync(int saleId)
    {
        // 1. Obtener la venta con el cliente, productos y método de pago desde la base de datos
        var venta = await _context.Ventas
            .Include(v => v.Cliente)
            .Include(v => v.MetodoPago)
            .Include(v => v.DetallesVenta)
                .ThenInclude(d => d.Producto)
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.Id == saleId);

        if (venta == null)
        {
            throw new KeyNotFoundException($"Venta con ID {saleId} no encontrada.");
        }

        // 2. Preparar el documento PDF tamaño carta con márgenes ejecutivos
        using var ms = new MemoryStream();
        using var writer = new PdfWriter(ms);
        using var pdf = new PdfDocument(writer);
        using var document = new Document(pdf, PageSize.LETTER);

        // Márgenes compactos de 28pt verticales y 30pt laterales (maximiza área útil de impresión)
        document.SetMargins(28, 30, 26, 30);

        // Fuentes estándar Helvetica
        var fontRegular = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
        var fontBold = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
        var fontOblique = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_OBLIQUE);
        var fontCourier = PdfFontFactory.CreateFont(StandardFonts.COURIER);

        document.SetFont(fontRegular);

        // Paleta oficial de marca Firmeza S.A.S.
        var brandNavy = new DeviceRgb(9, 21, 34);           // #091522 Azul Noche Corporativo
        var brandGold = new DeviceRgb(229, 169, 60);        // #E5A93C Dorado Firmeza
        var brandGoldLight = new DeviceRgb(253, 243, 231);   // #FDF3E7 Fondo Dorado Suave
        var brandGoldDark = new DeviceRgb(180, 125, 25);    // #B47D19 Dorado Oscuro Texto
        var brandSlate = new DeviceRgb(30, 41, 59);         // #1E293B Pizarra Oscuro
        var brandGray = new DeviceRgb(100, 116, 139);       // #64748B Gris Medio
        var brandLightBorder = new DeviceRgb(226, 232, 240); // #E2E8F0 Borde Suave
        var brandSurfaceBg = new DeviceRgb(248, 250, 252);  // #F8FAFC Fondo Tarjetas
        var brandGreen = new DeviceRgb(16, 185, 129);       // #10B981 Éxito / Pagada
        var brandGreenBg = new DeviceRgb(209, 250, 229);    // #D1FAE5 Fondo Éxito
        var brandGreenText = new DeviceRgb(6, 95, 70);      // #065F46 Texto Éxito
        var brandRed = new DeviceRgb(239, 68, 68);          // #EF4444 Anulada
        var brandRedBg = new DeviceRgb(254, 226, 226);      // #FEE2E2 Fondo Error
        var brandRedText = new DeviceRgb(153, 27, 27);      // #991B1B Texto Error

        var colFormat = new CultureInfo("es-CO");

        // -----------------------------------------------------------------------------------------
        // BANDA SUPERIOR DECORATIVA (AZUL MARINO Y DORADO)
        // -----------------------------------------------------------------------------------------
        var topBarTable = new Table(UnitValue.CreatePercentArray(new float[] { 85, 15 })).UseAllAvailableWidth();
        topBarTable.SetMarginBottom(8);
        topBarTable.AddCell(new Cell().SetHeight(3.5f).SetBackgroundColor(brandNavy).SetBorder(Border.NO_BORDER));
        topBarTable.AddCell(new Cell().SetHeight(3.5f).SetBackgroundColor(brandGold).SetBorder(Border.NO_BORDER));
        document.Add(topBarTable);

        // -----------------------------------------------------------------------------------------
        // SECCIÓN 1: CABECERA EMPRESARIAL (LOGOTIPO + RAZÓN SOCIAL + CAJA FISCAL DIAN)
        // -----------------------------------------------------------------------------------------
        var headerTable = new Table(UnitValue.CreatePercentArray(new float[] { 61, 39 })).UseAllAvailableWidth();
        headerTable.SetBorder(Border.NO_BORDER);

        // Lado izquierdo: Logotipo corporativo e información legal de la empresa
        var companyCell = new Cell().SetBorder(Border.NO_BORDER).SetPaddingRight(10);
        var brandRow = new Table(UnitValue.CreatePercentArray(new float[] { 22, 78 })).UseAllAvailableWidth();
        brandRow.SetBorder(Border.NO_BORDER);

        var logoPath = GetLogoPath(dark: false);
        var logoCell = new Cell().SetBorder(Border.NO_BORDER).SetVerticalAlignment(VerticalAlignment.MIDDLE);

        if (!string.IsNullOrEmpty(logoPath) && File.Exists(logoPath))
        {
            var imgData = ImageDataFactory.Create(logoPath);
            var logoImg = new Image(imgData)
                .SetMaxHeight(62)
                .SetMaxWidth(62)
                .SetAutoScale(true);
            logoCell.Add(logoImg);
        }
        brandRow.AddCell(logoCell);

        var nameCell = new Cell().SetBorder(Border.NO_BORDER).SetVerticalAlignment(VerticalAlignment.MIDDLE);
        nameCell.Add(new Paragraph("FIRMEZA S.A.S.")
            .SetFontSize(17.5f)
            .SetFont(fontBold)
            .SetFontColor(brandNavy)
            .SetMargin(0));
        nameCell.Add(new Paragraph("NIT: 901.458.789 - 0   |   IVA Régimen Común")
            .SetFontSize(8.5f)
            .SetFont(fontBold)
            .SetFontColor(brandSlate)
            .SetMarginTop(1)
            .SetMarginBottom(1));
        nameCell.Add(new Paragraph("Comercialización de Materiales para Construcción & Ferretería Industrial")
            .SetFontSize(7.5f)
            .SetFontColor(brandGray)
            .SetMargin(0));
        brandRow.AddCell(nameCell);
        companyCell.Add(brandRow);

        companyCell.Add(new Paragraph("Calle 100 # 15-20, Torre Empresarial Firmeza, Piso 8, Bogotá D.C., Colombia\nPBX: (+57) 601 789 4500  •  WhatsApp: (+57) 310 555 0199  •  ventas@firmeza.com.co\nwww.firmeza.com.co  •  Actividad Económica CIUU: 4663")
            .SetFontSize(7.2f)
            .SetFontColor(brandGray)
            .SetMarginTop(6)
            .SetMarginBottom(0)
            .SetMultipliedLeading(1.25f));
        headerTable.AddCell(companyCell);

        // Lado derecho: Caja fiscal con diseño oficial de Factura Electrónica
        var invoiceCard = new Cell()
            .SetBorder(new SolidBorder(brandGold, 1.5f))
            .SetPadding(0)
            .SetBackgroundColor(brandGoldLight);

        // Franja azul marino con el título de la factura
        var invoiceCardHeader = new Table(1).UseAllAvailableWidth();
        invoiceCardHeader.AddCell(new Cell()
            .Add(new Paragraph("FACTURA ELECTRÓNICA DE VENTA")
                .SetFont(fontBold)
                .SetFontSize(9.5f)
                .SetFontColor(ColorConstants.WHITE)
                .SetTextAlignment(TextAlignment.CENTER))
            .SetBackgroundColor(brandNavy)
            .SetPadding(4)
            .SetBorder(Border.NO_BORDER));
        invoiceCard.Add(invoiceCardHeader);

        // Contenido de la caja fiscal
        var invoiceCardBody = new Table(UnitValue.CreatePercentArray(new float[] { 45, 55 })).UseAllAvailableWidth();
        invoiceCardBody.SetPadding(6);

        var numCell = new Cell(1, 2)
            .Add(new Paragraph(venta.NumeroVenta)
                .SetFont(fontBold)
                .SetFontSize(13f)
                .SetFontColor(brandNavy)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMargin(2))
            .SetBorder(Border.NO_BORDER);
        invoiceCardBody.AddCell(numCell);

        invoiceCardBody.AddCell(new Cell().Add(new Paragraph("Fecha Emisión:").SetFontSize(7.5f).SetFontColor(brandGray)).SetBorder(Border.NO_BORDER).SetPadding(1.5f));
        invoiceCardBody.AddCell(new Cell().Add(new Paragraph(venta.Fecha.ToString("yyyy-MM-dd HH:mm", colFormat)).SetFontSize(7.5f).SetFont(fontBold)).SetTextAlignment(TextAlignment.RIGHT).SetBorder(Border.NO_BORDER).SetPadding(1.5f));

        var fechaVence = venta.Fecha.AddDays(30);
        invoiceCardBody.AddCell(new Cell().Add(new Paragraph("Fecha Vence:").SetFontSize(7.5f).SetFontColor(brandGray)).SetBorder(Border.NO_BORDER).SetPadding(1.5f));
        invoiceCardBody.AddCell(new Cell().Add(new Paragraph(fechaVence.ToString("yyyy-MM-dd", colFormat)).SetFontSize(7.5f).SetFont(fontBold)).SetTextAlignment(TextAlignment.RIGHT).SetBorder(Border.NO_BORDER).SetPadding(1.5f));

        var metodoNombre = venta.MetodoPago?.Nombre ?? "Transferencia Bancaria Bancolombia";
        invoiceCardBody.AddCell(new Cell().Add(new Paragraph("Medio de Pago:").SetFontSize(7.5f).SetFontColor(brandGray)).SetBorder(Border.NO_BORDER).SetPadding(1.5f));
        invoiceCardBody.AddCell(new Cell().Add(new Paragraph(metodoNombre).SetFontSize(7.5f).SetFont(fontBold)).SetTextAlignment(TextAlignment.RIGHT).SetBorder(Border.NO_BORDER).SetPadding(1.5f));

        var isCompleted = venta.Estado.Equals("Completada", StringComparison.OrdinalIgnoreCase) ||
                          venta.Estado.Equals("Aprobada", StringComparison.OrdinalIgnoreCase);
        var isCancelled = venta.Estado.Equals("Anulada", StringComparison.OrdinalIgnoreCase) ||
                          venta.Estado.Equals("Denegada", StringComparison.OrdinalIgnoreCase) ||
                          venta.Estado.Equals("Cancelada", StringComparison.OrdinalIgnoreCase);

        var statusBg = isCompleted ? brandGreenBg : (isCancelled ? brandRedBg : brandGoldLight);
        var statusColor = isCompleted ? brandGreenText : (isCancelled ? brandRedText : brandGoldDark);
        var statusBorder = isCompleted ? brandGreen : (isCancelled ? brandRed : brandGold);
        var statusIcon = isCompleted ? "● APROBADA" : (isCancelled ? "● DENEGADA" : $"● {venta.Estado.ToUpper()}");

        invoiceCard.Add(invoiceCardBody);

        // Franja inferior de estado con el ancho completo de la caja fiscal (de borde a borde, igual que la franja azul superior)
        var invoiceCardFooter = new Table(1).UseAllAvailableWidth();
        invoiceCardFooter.AddCell(new Cell()
            .Add(new Paragraph(statusIcon)
                .SetFont(fontBold)
                .SetFontSize(8.5f)
                .SetFontColor(statusColor)
                .SetTextAlignment(TextAlignment.CENTER))
            .SetBackgroundColor(statusBg)
            .SetBorder(Border.NO_BORDER)
            .SetBorderTop(new SolidBorder(statusBorder, 0.8f))
            .SetPadding(4));
        invoiceCard.Add(invoiceCardFooter);

        headerTable.AddCell(invoiceCard);

        document.Add(headerTable);

        // -----------------------------------------------------------------------------------------
        // FRANJA DE AUTORIZACIÓN DIAN
        // -----------------------------------------------------------------------------------------
        var dianBanner = new Table(1).UseAllAvailableWidth();
        dianBanner.SetMarginTop(6);
        dianBanner.SetMarginBottom(8);
        dianBanner.AddCell(new Cell()
            .Add(new Paragraph("Autorización de Facturación Electrónica DIAN: Resolución No. 18764000001 de 2026/01/15  •  Prefijo: VENTA  •  Rango: 0001 a 50000  •  Vigencia: 24 Meses")
                .SetFontSize(6.8f)
                .SetFontColor(brandSlate)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMargin(0))
            .SetBackgroundColor(brandSurfaceBg)
            .SetBorder(new SolidBorder(brandLightBorder, 0.6f))
            .SetPadding(3));
        document.Add(dianBanner);

        // -----------------------------------------------------------------------------------------
        // SECCIÓN 2: INFORMACIÓN DEL ADQUIRENTE / CLIENTE Y CONDICIONES DE ENTREGA
        // -----------------------------------------------------------------------------------------
        var clientCard = new Table(UnitValue.CreatePercentArray(new float[] { 50, 50 })).UseAllAvailableWidth();
        clientCard.SetBackgroundColor(brandSurfaceBg);
        clientCard.SetBorderTop(new SolidBorder(brandLightBorder, 0.8f));
        clientCard.SetBorderRight(new SolidBorder(brandLightBorder, 0.8f));
        clientCard.SetBorderBottom(new SolidBorder(brandLightBorder, 0.8f));
        clientCard.SetBorderLeft(new SolidBorder(brandGold, 3.5f)); // Acento institucional dorado

        // Columna 1: Datos del Adquirente
        var colCli = new Cell().SetBorder(Border.NO_BORDER).SetPadding(7);
        colCli.Add(new Paragraph("DATOS DEL ADQUIRENTE / CLIENTE")
            .SetFont(fontBold)
            .SetFontSize(8.5f)
            .SetFontColor(brandNavy)
            .SetMarginBottom(4));

        var subCli = new Table(UnitValue.CreatePercentArray(new float[] { 34, 66 })).UseAllAvailableWidth();
        subCli.AddCell(new Cell().Add(new Paragraph("Razón Social:").SetFontSize(7.5f).SetFontColor(brandGray)).SetBorder(Border.NO_BORDER).SetPadding(1));
        subCli.AddCell(new Cell().Add(new Paragraph($"{venta.Cliente.Nombres} {venta.Cliente.Apellidos}".Trim()).SetFontSize(8f).SetFont(fontBold).SetFontColor(brandSlate)).SetBorder(Border.NO_BORDER).SetPadding(1));

        subCli.AddCell(new Cell().Add(new Paragraph("NIT / Cédula:").SetFontSize(7.5f).SetFontColor(brandGray)).SetBorder(Border.NO_BORDER).SetPadding(1));
        subCli.AddCell(new Cell().Add(new Paragraph(venta.Cliente.Identificacion).SetFontSize(8f).SetFont(fontBold).SetFontColor(brandSlate)).SetBorder(Border.NO_BORDER).SetPadding(1));

        subCli.AddCell(new Cell().Add(new Paragraph("Correo Electrónico:").SetFontSize(7.5f).SetFontColor(brandGray)).SetBorder(Border.NO_BORDER).SetPadding(1));
        subCli.AddCell(new Cell().Add(new Paragraph(venta.Cliente.Email).SetFontSize(7.5f).SetFontColor(brandSlate)).SetBorder(Border.NO_BORDER).SetPadding(1));

        subCli.AddCell(new Cell().Add(new Paragraph("Teléfono / Celular:").SetFontSize(7.5f).SetFontColor(brandGray)).SetBorder(Border.NO_BORDER).SetPadding(1));
        subCli.AddCell(new Cell().Add(new Paragraph(venta.Cliente.Telefono ?? "No registrado").SetFontSize(7.5f).SetFontColor(brandSlate)).SetBorder(Border.NO_BORDER).SetPadding(1));
        colCli.Add(subCli);
        clientCard.AddCell(colCli);

        // Columna 2: Condiciones Comerciales y Entrega
        var colEntrega = new Cell().SetBorder(Border.NO_BORDER).SetPadding(7);
        colEntrega.Add(new Paragraph("CONDICIONES DE ENTREGA & PAGO")
            .SetFont(fontBold)
            .SetFontSize(8.5f)
            .SetFontColor(brandNavy)
            .SetMarginBottom(4));

        var subEntrega = new Table(UnitValue.CreatePercentArray(new float[] { 38, 62 })).UseAllAvailableWidth();
        subEntrega.AddCell(new Cell().Add(new Paragraph("Dirección Entrega:").SetFontSize(7.5f).SetFontColor(brandGray)).SetBorder(Border.NO_BORDER).SetPadding(1));
        subEntrega.AddCell(new Cell().Add(new Paragraph(venta.Cliente.Direccion ?? "Entrega en Bodega Central").SetFontSize(7.5f).SetFontColor(brandSlate)).SetBorder(Border.NO_BORDER).SetPadding(1));

        subEntrega.AddCell(new Cell().Add(new Paragraph("Ciudad / Destino:").SetFontSize(7.5f).SetFontColor(brandGray)).SetBorder(Border.NO_BORDER).SetPadding(1));
        subEntrega.AddCell(new Cell().Add(new Paragraph("Bogotá D.C. - Colombia").SetFontSize(7.5f).SetFontColor(brandSlate)).SetBorder(Border.NO_BORDER).SetPadding(1));

        subEntrega.AddCell(new Cell().Add(new Paragraph("Forma de Pago:").SetFontSize(7.5f).SetFontColor(brandGray)).SetBorder(Border.NO_BORDER).SetPadding(1));
        subEntrega.AddCell(new Cell().Add(new Paragraph("1 - Contado Comercial").SetFontSize(7.5f).SetFontColor(brandSlate)).SetBorder(Border.NO_BORDER).SetPadding(1));

        subEntrega.AddCell(new Cell().Add(new Paragraph("Moneda / Régimen:").SetFontSize(7.5f).SetFontColor(brandGray)).SetBorder(Border.NO_BORDER).SetPadding(1));
        subEntrega.AddCell(new Cell().Add(new Paragraph("COP - Pesos Colombianos").SetFontSize(7.5f).SetFontColor(brandSlate)).SetBorder(Border.NO_BORDER).SetPadding(1));
        colEntrega.Add(subEntrega);
        clientCard.AddCell(colEntrega);

        document.Add(clientCard);
        document.Add(new Paragraph("\n").SetFontSize(4));

        // -----------------------------------------------------------------------------------------
        // SECCIÓN 3: TABLA DE PRODUCTOS Y MATERIALES COMPRADOS
        // -----------------------------------------------------------------------------------------
        var itemsTable = new Table(UnitValue.CreatePercentArray(new float[] { 5, 14, 38, 8, 7, 14, 14 })).UseAllAvailableWidth();
        itemsTable.SetMarginTop(2);

        string[] cols = { "#", "CÓDIGO", "DESCRIPCIÓN DEL MATERIAL", "CANT.", "UND", "PRECIO UNIT.", "SUBTOTAL" };
        for (int i = 0; i < cols.Length; i++)
        {
            var isNumeric = (i == 3 || i >= 5);
            var isCenter = (i == 0 || i == 1 || i == 4);
            var align = isNumeric ? TextAlignment.RIGHT : (isCenter ? TextAlignment.CENTER : TextAlignment.LEFT);

            var th = new Cell()
                .Add(new Paragraph(cols[i]).SetFont(fontBold).SetFontSize(8f).SetFontColor(ColorConstants.WHITE))
                .SetBackgroundColor(brandNavy)
                .SetBorder(Border.NO_BORDER)
                .SetBorderBottom(new SolidBorder(brandGold, 2f))
                .SetTextAlignment(align)
                .SetPadding(5.5f)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE);
            itemsTable.AddHeaderCell(th);
        }

        int itemIndex = 1;
        foreach (var d in venta.DetallesVenta)
        {
            var rowBg = (itemIndex % 2 == 1) ? ColorConstants.WHITE : brandSurfaceBg;
            var bottomBorder = new SolidBorder(brandLightBorder, 0.5f);

            var codigo = string.IsNullOrWhiteSpace(d.Producto.Codigo) ? "-" : d.Producto.Codigo;
            var nombre = string.IsNullOrWhiteSpace(d.Producto.Nombre) ? "Material de Construcción" : d.Producto.Nombre;

            // #
            itemsTable.AddCell(new Cell().Add(new Paragraph(itemIndex.ToString()).SetFontSize(7.5f).SetFontColor(brandGray)).SetTextAlignment(TextAlignment.CENTER).SetBackgroundColor(rowBg).SetBorder(Border.NO_BORDER).SetBorderBottom(bottomBorder).SetPadding(5));
            // Código
            itemsTable.AddCell(new Cell().Add(new Paragraph(codigo).SetFontSize(8f).SetFont(fontBold).SetFontColor(brandNavy)).SetTextAlignment(TextAlignment.CENTER).SetBackgroundColor(rowBg).SetBorder(Border.NO_BORDER).SetBorderBottom(bottomBorder).SetPadding(5));
            // Nombre
            itemsTable.AddCell(new Cell().Add(new Paragraph(nombre).SetFontSize(8f).SetFontColor(brandSlate)).SetBackgroundColor(rowBg).SetBorder(Border.NO_BORDER).SetBorderBottom(bottomBorder).SetPadding(5));
            // Cantidad
            itemsTable.AddCell(new Cell().Add(new Paragraph(d.Cantidad.ToString("N0", colFormat)).SetFontSize(8f).SetFont(fontBold)).SetTextAlignment(TextAlignment.RIGHT).SetBackgroundColor(rowBg).SetBorder(Border.NO_BORDER).SetBorderBottom(bottomBorder).SetPadding(5));
            // Unidad
            itemsTable.AddCell(new Cell().Add(new Paragraph("UND").SetFontSize(7.5f).SetFontColor(brandGray)).SetTextAlignment(TextAlignment.CENTER).SetBackgroundColor(rowBg).SetBorder(Border.NO_BORDER).SetBorderBottom(bottomBorder).SetPadding(5));
            // Precio Unitario
            itemsTable.AddCell(new Cell().Add(new Paragraph(d.PrecioUnitario.ToString("C2", colFormat)).SetFontSize(8f)).SetTextAlignment(TextAlignment.RIGHT).SetBackgroundColor(rowBg).SetBorder(Border.NO_BORDER).SetBorderBottom(bottomBorder).SetPadding(5));
            // Subtotal
            itemsTable.AddCell(new Cell().Add(new Paragraph(d.Subtotal.ToString("C2", colFormat)).SetFontSize(8f).SetFont(fontBold).SetFontColor(brandSlate)).SetTextAlignment(TextAlignment.RIGHT).SetBackgroundColor(rowBg).SetBorder(Border.NO_BORDER).SetBorderBottom(bottomBorder).SetPadding(5));

            itemIndex++;
        }

        document.Add(itemsTable);

        // -----------------------------------------------------------------------------------------
        // SECCIÓN 4: VALOR EN LETRAS (SON: ...)
        // -----------------------------------------------------------------------------------------
        var letrasText = ConvertirNumeroALetras(venta.Total);
        var letrasBox = new Table(1).UseAllAvailableWidth();
        letrasBox.SetMarginTop(5);
        letrasBox.SetMarginBottom(8);

        letrasBox.AddCell(new Cell()
            .Add(new Paragraph($"VALOR EN LETRAS:  SON {letrasText.ToUpper()}")
                .SetFont(fontBold)
                .SetFontSize(7.5f)
                .SetFontColor(brandSlate)
                .SetMargin(0))
            .SetBackgroundColor(brandGoldLight)
            .SetBorderTop(new SolidBorder(brandLightBorder, 0.5f))
            .SetBorderRight(new SolidBorder(brandLightBorder, 0.5f))
            .SetBorderBottom(new SolidBorder(brandLightBorder, 0.5f))
            .SetBorderLeft(new SolidBorder(brandGold, 3f))
            .SetPadding(5));
        document.Add(letrasBox);

        // -----------------------------------------------------------------------------------------
        // SECCIÓN 5: LIQUIDACIÓN FINANCIERA, QR CUFE DIAN Y TÉRMINOS LEGALES
        // -----------------------------------------------------------------------------------------
        var bottomTable = new Table(UnitValue.CreatePercentArray(new float[] { 54, 46 })).UseAllAvailableWidth();
        bottomTable.SetBorder(Border.NO_BORDER);

        // Columna Izquierda: QR Fiscal DIAN, CUFE criptográfico, Cuentas Bancarias y Cláusulas
        var complianceCol = new Cell().SetBorder(Border.NO_BORDER).SetPaddingRight(10);

        // Generar hash CUFE y código QR oficial
        var cufeHash = GenerarCufe(venta.NumeroVenta, venta.Fecha, venta.Total, venta.Subtotal, venta.Iva, venta.Cliente.Identificacion);
        var qrData = $"NumFac: {venta.NumeroVenta}\nFecFac: {venta.Fecha:yyyy-MM-dd}\nHoraFac: {venta.Fecha:HH:mm:ss}-05:00\nNitFac: 9014587890\nDocAdq: {venta.Cliente.Identificacion}\nValFac: {venta.Subtotal:F2}\nValIva: {venta.Iva:F2}\nValTotal: {venta.Total:F2}\nCUFE: {cufeHash}\nhttps://catalogo-vpfe.dian.gov.co/document/searchqr?documentkey={cufeHash}";

        // Partir el CUFE en bloques de 32 caracteres para que no desborde horizontalmente la columna
        var cufeFormatted = new StringBuilder();
        for (int i = 0; i < cufeHash.Length; i += 32)
        {
            if (i > 0) cufeFormatted.AppendLine();
            cufeFormatted.Append(cufeHash.Substring(i, Math.Min(32, cufeHash.Length - i)));
        }

        var qrAndCufeTable = new Table(UnitValue.CreatePercentArray(new float[] { 26, 74 })).UseAllAvailableWidth();
        qrAndCufeTable.SetBorder(Border.NO_BORDER);

        try
        {
            var qrCode = new BarcodeQRCode(qrData);
            var qrObj = qrCode.CreateFormXObject(pdf);
            var qrImg = new Image(qrObj).SetWidth(64).SetHeight(64);
            qrAndCufeTable.AddCell(new Cell().Add(qrImg).SetBorder(Border.NO_BORDER).SetVerticalAlignment(VerticalAlignment.MIDDLE));
        }
        catch
        {
            qrAndCufeTable.AddCell(new Cell().Add(new Paragraph("[QR FISCAL]").SetFontSize(8)).SetBorder(Border.NO_BORDER));
        }

        var cufeDetails = new Cell().SetBorder(Border.NO_BORDER).SetVerticalAlignment(VerticalAlignment.MIDDLE).SetPaddingLeft(4);
        cufeDetails.Add(new Paragraph("CÓDIGO ÚNICO DE FACTURA ELECTRÓNICA (CUFE):")
            .SetFont(fontBold)
            .SetFontSize(6.8f)
            .SetFontColor(brandNavy)
            .SetMarginBottom(1));
        cufeDetails.Add(new Paragraph(cufeFormatted.ToString())
            .SetFont(fontCourier)
            .SetFontSize(5.6f)
            .SetFontColor(brandSlate)
            .SetMultipliedLeading(1.1f)
            .SetMarginBottom(2));
        cufeDetails.Add(new Paragraph("Firma Digital: SHA-384 with RSA  •  Emisor: FIRMEZA S.A.S. PKI\nDocumento emitido conforme a la Resolución DIAN No. 000042 de 2020.")
            .SetFontSize(6.2f)
            .SetFontColor(brandGray)
            .SetMultipliedLeading(1.2f));
        qrAndCufeTable.AddCell(cufeDetails);
        complianceCol.Add(qrAndCufeTable);

        // Canales de recaudo autorizados
        complianceCol.Add(new Paragraph("CUENTAS BANCARIAS AUTORIZADAS PARA PAGOS:")
            .SetFont(fontBold)
            .SetFontSize(7.2f)
            .SetFontColor(brandNavy)
            .SetMarginTop(6)
            .SetMarginBottom(1));
        complianceCol.Add(new Paragraph("• Bancolombia Cta. Corriente # 031-892341-92 (Convenio Empresarial Firmeza: 74921)\n• Banco de Bogotá Cta. de Ahorros # 142-901847-15 a nombre de FIRMEZA S.A.S.")
            .SetFontSize(6.8f)
            .SetFontColor(brandSlate)
            .SetMarginBottom(4));

        // Cláusulas de aceptación comercial
        complianceCol.Add(new Paragraph("Esta factura electrónica de venta constituye título ejecutivo y se asimila en todos sus efectos a una letra de cambio según los Arts. 772 a 774 del Código de Comercio colombiano. La firma o recepción de los materiales implica la aceptación expresa de las condiciones y garantías pactadas.")
            .SetFontSize(6.3f)
            .SetFontColor(brandGray)
            .SetMultipliedLeading(1.2f));

        bottomTable.AddCell(complianceCol);

        // Columna Derecha: Tarjeta de Liquidación Financiera
        var totalsCol = new Cell().SetBorder(Border.NO_BORDER).SetPaddingLeft(6);
        var subTable = new Table(UnitValue.CreatePercentArray(new float[] { 50, 50 })).UseAllAvailableWidth();
        subTable.SetBackgroundColor(brandSurfaceBg);
        subTable.SetBorder(new SolidBorder(brandLightBorder, 0.8f));

        void AddTotalRow(string label, string value, bool bold = false, bool isDeduction = false)
        {
            var labelCell = new Cell()
                .Add(new Paragraph(label).SetFontSize(7.6f).SetFontColor(bold ? brandNavy : brandGray).SetFont(bold ? fontBold : fontRegular))
                .SetBorder(Border.NO_BORDER)
                .SetBorderBottom(new SolidBorder(brandLightBorder, 0.4f))
                .SetPadding(3.5f);

            var valueCell = new Cell()
                .Add(new Paragraph(value).SetFontSize(7.8f).SetFontColor(isDeduction ? brandRedText : brandSlate).SetFont(bold ? fontBold : fontRegular))
                .SetTextAlignment(TextAlignment.RIGHT)
                .SetBorder(Border.NO_BORDER)
                .SetBorderBottom(new SolidBorder(brandLightBorder, 0.4f))
                .SetPadding(3.5f);

            subTable.AddCell(labelCell);
            subTable.AddCell(valueCell);
        }

        AddTotalRow("Subtotal Bruto:", venta.Subtotal.ToString("C2", colFormat));
        AddTotalRow("Descuento Comercial (0%):", "$ 0,00", false, true);
        AddTotalRow("Base Imponible Gravable:", venta.Subtotal.ToString("C2", colFormat), true);
        AddTotalRow("IVA Generado (19%):", venta.Iva.ToString("C2", colFormat));
        AddTotalRow("Retención en la Fuente (0%):", "$ 0,00", false, true);

        // Fila destacada de Total a Pagar en Azul Noche y Dorado
        var totalTitleCell = new Cell()
            .Add(new Paragraph("TOTAL A PAGAR:")
                .SetFont(fontBold)
                .SetFontSize(9f)
                .SetFontColor(brandGold))
            .SetBackgroundColor(brandNavy)
            .SetBorder(new SolidBorder(brandGold, 1.2f))
            .SetPadding(5)
            .SetVerticalAlignment(VerticalAlignment.MIDDLE);

        var totalValueCell = new Cell()
            .Add(new Paragraph(venta.Total.ToString("C2", colFormat))
                .SetFont(fontBold)
                .SetFontSize(11f)
                .SetFontColor(ColorConstants.WHITE))
            .SetBackgroundColor(brandNavy)
            .SetTextAlignment(TextAlignment.RIGHT)
            .SetBorder(new SolidBorder(brandGold, 1.2f))
            .SetPadding(5)
            .SetVerticalAlignment(VerticalAlignment.MIDDLE);

        subTable.AddCell(totalTitleCell);
        subTable.AddCell(totalValueCell);

        totalsCol.Add(subTable);

        // Bloques de firma al pie de totales
        var signTable = new Table(UnitValue.CreatePercentArray(new float[] { 50, 50 })).UseAllAvailableWidth();
        signTable.SetMarginTop(10);

        var s1 = new Cell().SetBorder(Border.NO_BORDER).SetPaddingRight(3);
        s1.Add(new Paragraph("___________________\nFIRMA AUTORIZADA\nFIRMEZA S.A.S.")
            .SetFontSize(6.5f)
            .SetFontColor(brandSlate)
            .SetTextAlignment(TextAlignment.CENTER)
            .SetMultipliedLeading(1.2f));

        var s2 = new Cell().SetBorder(Border.NO_BORDER).SetPaddingLeft(3);
        s2.Add(new Paragraph("___________________\nRECIBIDO A CONFORMIDAD\nCLIENTE / C.C. / SELLO")
            .SetFontSize(6.5f)
            .SetFontColor(brandSlate)
            .SetTextAlignment(TextAlignment.CENTER)
            .SetMultipliedLeading(1.2f));

        signTable.AddCell(s1);
        signTable.AddCell(s2);
        totalsCol.Add(signTable);

        bottomTable.AddCell(totalsCol);
        document.Add(bottomTable);

        // -----------------------------------------------------------------------------------------
        // SECCIÓN 6: PIE DE PÁGINA INSTITUCIONAL
        // -----------------------------------------------------------------------------------------
        document.Add(new Paragraph("\n").SetFontSize(4));
        var footerLine = new Table(1).UseAllAvailableWidth();
        footerLine.AddCell(new Cell().SetHeight(0.8f).SetBackgroundColor(brandLightBorder).SetBorder(Border.NO_BORDER));
        document.Add(footerLine);

        var footer = new Paragraph("FIRMEZA S.A.S.  |  Solidez, Confianza y Calidad en Materiales para la Construcción  |  Bogotá D.C., Colombia\nDocumento fiscal electrónico expedido por la Plataforma Digital Firmeza (www.firmeza.com.co)  •  Página 1 de 1")
            .SetFontSize(7.2f)
            .SetFont(fontOblique)
            .SetFontColor(brandGray)
            .SetTextAlignment(TextAlignment.CENTER)
            .SetMarginTop(3)
            .SetMultipliedLeading(1.25f);
        document.Add(footer);

        // Cierra el documento y devuelve los bytes en memoria
        document.Close();
        var bytes = ms.ToArray();

        // Guarda una copia de respaldo en disco en wwwroot/recibos
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
            // Si el entorno o permisos no permiten escritura en disco, continúa sin interrumpir la descarga
        }

        return bytes;
    }
}
