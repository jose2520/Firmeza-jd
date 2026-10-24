// =================================================================================================
// SERVICIO: ExcelService.cs
// CAPA: Firmeza.Infrastructure (Capa de Infraestructura y Persistencia)
// CONTRATO: IExcelService (Firmeza.Application)
// PROPÓSITO:
//   Motor corporativo de generación e importación de libros de cálculo Excel (.xlsx) con estándar
//   empresarial moderno (EPPlus 8). Incorpora la identidad visual oficial de Firmeza S.A.S.:
//   - Paleta corporativa: Azul Noche (#091522), Dorado Institucional (#E5A93C), Pizarra (#1E293B).
//   - Tableros analíticos ejecutivos en hojas dedicadas con KPIs financieros y operativos.
//   - Formato contable formal en pesos colombianos (COP), autofiltros interactivos y paneles fijos.
//   - Badges visuales de estado, alertas de existencias en bodega e importaciones masivas seguras.
// =================================================================================================

using System.Drawing;
using System.Globalization;
using Firmeza.Application.DTOs;
using Firmeza.Application.Interfaces;
using Firmeza.Domain.Entities;
using Firmeza.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace Firmeza.Infrastructure.Services;

public class ExcelService : IExcelService
{
    private readonly ApplicationDbContext _context;

    // Paleta oficial de marca Firmeza S.A.S. adaptada para visualización de alta definición en Excel
    private static readonly Color BrandNavy = Color.FromArgb(9, 21, 34);          // #091522 Azul Noche Corporativo
    private static readonly Color BrandGold = Color.FromArgb(229, 169, 60);        // #E5A93C Dorado Firmeza
    private static readonly Color BrandGoldLight = Color.FromArgb(253, 243, 231);  // #FDF3E7 Fondo Dorado Suave
    private static readonly Color BrandSlateLight = Color.FromArgb(71, 85, 105);   // #475569 Pizarra Medio
    private static readonly Color BrandGray = Color.FromArgb(100, 116, 139);       // #64748B Gris Muted
    private static readonly Color BrandBorder = Color.FromArgb(226, 232, 240);     // #E2E8F0 Borde Suave
    private static readonly Color BrandZebra = Color.FromArgb(248, 250, 252);      // #F8FAFC Fila Alternada
    private static readonly Color BrandSurfaceCard = Color.FromArgb(241, 245, 249);// #F1F5F9 Fondo Tarjeta

    // Estados con diseño de etiqueta (Badge / Pill)
    private static readonly Color BadgeGreenBg = Color.FromArgb(220, 252, 231);    // #DCFCE7 Verde Suave
    private static readonly Color BadgeGreenText = Color.FromArgb(22, 101, 52);    // #166534 Verde Oscuro
    private static readonly Color BadgeRedBg = Color.FromArgb(254, 226, 226);      // #FEE2E2 Rojo Suave
    private static readonly Color BadgeRedText = Color.FromArgb(153, 27, 27);      // #991B1B Rojo Oscuro
    private static readonly Color BadgeAmberBg = Color.FromArgb(254, 243, 199);    // #FEF3C7 Ámbar Suave
    private static readonly Color BadgeAmberText = Color.FromArgb(146, 64, 14);    // #92400E Ámbar Oscuro

    // Formatos numéricos oficiales
    private const string CurrencyFormat = "$#,##0.00;($#,##0.00);\"$ 0.00\"";
    private const string IntegerFormat = "#,##0";
    private const string PercentFormat = "0.0%";

    // Inicializa el servicio con la base de datos y activa la licencia gratuita no comercial para EPPlus 8.
    public ExcelService(ApplicationDbContext context)
    {
        _context = context;
        ExcelPackage.License.SetNonCommercialPersonal("Firmeza");
    }

    // Obtiene la ruta física del logotipo corporativo según el entorno de ejecución.
    private static string? GetLogoPath(bool dark = false)
    {
        var fileName = dark ? "firme.png" : "firme2.png";
        var candidates = new[]
        {
            Path.Combine(AppContext.BaseDirectory, "Resources", "Assets", fileName),
            Path.Combine(Directory.GetCurrentDirectory(), "Resources", "Assets", fileName),
            Path.Combine(Directory.GetCurrentDirectory(), "src", "Firmeza.Infrastructure", "Resources", "Assets", fileName),
            $"/home/cohorte5/Documentos/jose/Firmeza.app/Firmeza/assets/expo.icon/Assets/{fileName}",
            Path.Combine(AppContext.BaseDirectory, fileName)
        };

        return candidates.FirstOrDefault(File.Exists);
    }

    // Inserta el logotipo corporativo oficial con fondo transparente sobre la superficie limpia
    private static void InsertCorporateLogo(ExcelWorksheet ws, string pictureName, int fromRow = 0, int fromRowOffset = 3, int fromCol = 0, int fromColOffset = 8, int width = 42, int height = 42)
    {
        var logoPath = GetLogoPath(dark: false);
        if (!string.IsNullOrEmpty(logoPath) && File.Exists(logoPath))
        {
            try
            {
                var logoPic = ws.Drawings.AddPicture(pictureName, new FileInfo(logoPath));
                logoPic.SetPosition(fromRow, fromRowOffset, fromCol, fromColOffset);
                logoPic.SetSize(width, height);
            }
            catch
            {
                // Si la imagen ya existe o el formato del host no lo soporta, continúa sin bloquear la exportación
            }
        }
    }

    // Aplica el estilo empresarial unificado a las tarjetas de métricas clave (KPI)
    private static void RenderKpiCard(ExcelWorksheet ws, int labelRow, int valStartRow, int valEndRow,
        int startCol, int endCol, string label, string formulaOrVal, string numFormat, Color accentColor)
    {
        ws.Cells[labelRow, startCol, labelRow, endCol].Merge = true;
        var lblCell = ws.Cells[labelRow, startCol];
        lblCell.Value = label;
        lblCell.Style.Font.Name = "Segoe UI";
        lblCell.Style.Font.Size = 8f;
        lblCell.Style.Font.Bold = true;
        lblCell.Style.Font.Color.SetColor(BrandSlateLight);
        lblCell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        lblCell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
        lblCell.Style.Fill.PatternType = ExcelFillStyle.Solid;
        lblCell.Style.Fill.BackgroundColor.SetColor(BrandSurfaceCard);

        ws.Cells[valStartRow, startCol, valEndRow, endCol].Merge = true;
        var valCell = ws.Cells[valStartRow, startCol];
        if (formulaOrVal.StartsWith('='))
        {
            valCell.Formula = formulaOrVal.Substring(1);
        }
        else
        {
            valCell.Value = formulaOrVal;
        }

        valCell.Style.Font.Name = "Segoe UI";
        valCell.Style.Font.Size = 13.5f;
        valCell.Style.Font.Bold = true;
        valCell.Style.Font.Color.SetColor(BrandNavy);
        valCell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        valCell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
        valCell.Style.Fill.PatternType = ExcelFillStyle.Solid;
        valCell.Style.Fill.BackgroundColor.SetColor(Color.White);

        if (!string.IsNullOrEmpty(numFormat))
        {
            valCell.Style.Numberformat.Format = numFormat;
        }

        for (int r = labelRow; r <= valEndRow; r++)
        {
            for (int c = startCol; c <= endCol; c++)
            {
                ws.Cells[r, c].Style.Border.BorderAround(ExcelBorderStyle.Thin, BrandBorder);
            }
        }

        for (int c = startCol; c <= endCol; c++)
        {
            ws.Cells[labelRow, c].Style.Border.Top.Style = ExcelBorderStyle.Medium;
            ws.Cells[labelRow, c].Style.Border.Top.Color.SetColor(accentColor);
        }
    }

    // =============================================================================================
    // 1. EXPORTACIÓN: CATÁLOGO DE PRODUCTOS E INVENTARIO CON TABLERO ANALÍTICO
    // =============================================================================================
    public async Task<byte[]> ExportProductsToExcelAsync()
    {
        var productos = await _context.Productos
            .AsNoTracking()
            .OrderBy(p => p.Id)
            .ToListAsync();

        using var package = new ExcelPackage();

        // -----------------------------------------------------------------------------------------
        // HOJA 1: CATÁLOGO DETALLADO DE PRODUCTOS (Requisito contractual y funcional)
        // -----------------------------------------------------------------------------------------
        var ws = package.Workbook.Worksheets.Add("Catálogo de Productos");
        ws.TabColor = BrandNavy;

        // Configuración de página horizontal (Landscape)
        ws.PrinterSettings.Orientation = eOrientation.Landscape;
        ws.PrinterSettings.PaperSize = ePaperSize.Letter;
        ws.PrinterSettings.FitToPage = true;
        ws.PrinterSettings.FitToWidth = 1;
        ws.PrinterSettings.FitToHeight = 0;
        ws.View.ShowGridLines = true;

        // Inserción de Logotipo oficial con fondo blanco para contraste impecable de azul y dorado
        InsertCorporateLogo(ws, "LogoFirmezaProd", fromRow: 0, fromRowOffset: 3, fromCol: 0, fromColOffset: 8, width: 42, height: 42);

        // 1. Cabecera ejecutiva con fondo blanco pulcro que resalta el logotipo oficial azul oscuro y dorado
        ws.Row(1).Height = 48;
        ws.Cells["A1:H1"].Merge = true;
        ws.Cells["A1"].Value = "            FIRMEZA S.A.S.   |   CATÁLOGO GENERAL DE PRODUCTOS";
        ws.Cells["A1"].Style.Font.Name = "Segoe UI";
        ws.Cells["A1"].Style.Font.Size = 14.5f;
        ws.Cells["A1"].Style.Font.Bold = true;
        ws.Cells["A1"].Style.Font.Color.SetColor(BrandNavy);
        ws.Cells["A1"].Style.Fill.PatternType = ExcelFillStyle.Solid;
        ws.Cells["A1"].Style.Fill.BackgroundColor.SetColor(Color.White);
        ws.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
        ws.Cells["A1"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
        ws.Cells["A1:H1"].Style.Border.Top.Style = ExcelBorderStyle.Medium;
        ws.Cells["A1:H1"].Style.Border.Top.Color.SetColor(BrandNavy);

        // 2. Línea decorativa dorada de acento institucional
        ws.Row(2).Height = 5;
        ws.Cells["A2:H2"].Merge = true;
        ws.Cells["A2"].Style.Fill.PatternType = ExcelFillStyle.Solid;
        ws.Cells["A2"].Style.Fill.BackgroundColor.SetColor(BrandGold);

        // 3. Subtítulo con fecha y resumen de emisión
        ws.Row(3).Height = 24;
        ws.Cells["A3:H3"].Merge = true;
        ws.Cells["A3"].Value = $"Reporte Oficial emitido el {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC   •   Total artículos: {productos.Count}   •   Firmeza Ferretería y Materiales de Construcción   •   NIT: 901.458.789-0";
        ws.Cells["A3"].Style.Font.Name = "Segoe UI";
        ws.Cells["A3"].Style.Font.Size = 9.5f;
        ws.Cells["A3"].Style.Font.Italic = true;
        ws.Cells["A3"].Style.Font.Color.SetColor(BrandNavy);
        ws.Cells["A3"].Style.Fill.PatternType = ExcelFillStyle.Solid;
        ws.Cells["A3"].Style.Fill.BackgroundColor.SetColor(BrandGoldLight);
        ws.Cells["A3"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        ws.Cells["A3"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
        ws.Cells["A3:H3"].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
        ws.Cells["A3:H3"].Style.Border.Bottom.Color.SetColor(BrandGold);

        // 4. Encabezados de columnas de la tabla maestra
        string[] headers = { "ID", "Código", "Nombre del Producto", "Descripción", "Precio (COP)", "Stock", "Estado", "Fecha Registro" };
        ws.Row(4).Height = 28;

        for (int i = 0; i < headers.Length; i++)
        {
            var cell = ws.Cells[4, i + 1];
            cell.Value = headers[i];
            cell.Style.Font.Name = "Segoe UI";
            cell.Style.Font.Size = 10.5f;
            cell.Style.Font.Bold = true;
            cell.Style.Font.Color.SetColor(Color.White);
            cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
            cell.Style.Fill.BackgroundColor.SetColor(BrandNavy);
            cell.Style.HorizontalAlignment = (i == 4 || i == 5) ? ExcelHorizontalAlignment.Right : (i == 2 || i == 3) ? ExcelHorizontalAlignment.Left : ExcelHorizontalAlignment.Center;
            cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            cell.Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.FromArgb(51, 65, 85));
            cell.Style.Border.Bottom.Style = ExcelBorderStyle.Medium;
            cell.Style.Border.Bottom.Color.SetColor(BrandGold);
        }

        // 5. Filas de datos
        int startDataRow = 5;
        int row = startDataRow;

        if (productos.Count == 0)
        {
            ws.Cells[row, 1, row, 8].Merge = true;
            ws.Cells[row, 1].Value = "No se encontraron productos registrados en el catálogo.";
            ws.Cells[row, 1].Style.Font.Name = "Segoe UI";
            ws.Cells[row, 1].Style.Font.Italic = true;
            ws.Cells[row, 1].Style.Font.Color.SetColor(BrandGray);
            ws.Cells[row, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            ws.Cells[row, 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            ws.Row(row).Height = 26;
            ws.Cells[row, 1, row, 8].Style.Border.BorderAround(ExcelBorderStyle.Thin, BrandBorder);
            row++;
        }
        else
        {
            foreach (var p in productos)
            {
                ws.Row(row).Height = 22;
                var isEven = (row % 2 == 0);
                var currentBg = isEven ? Color.White : BrandZebra;

                // Col 1: ID
                var cellId = ws.Cells[row, 1];
                cellId.Value = p.Id;
                cellId.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                // Col 2: Código
                var cellCodigo = ws.Cells[row, 2];
                cellCodigo.Value = p.Codigo;
                cellCodigo.Style.Font.Bold = true;
                cellCodigo.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                // Col 3: Nombre
                var cellNombre = ws.Cells[row, 3];
                cellNombre.Value = p.Nombre;
                cellNombre.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                // Col 4: Descripción
                var cellDesc = ws.Cells[row, 4];
                cellDesc.Value = p.Descripcion ?? "-";
                cellDesc.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                // Col 5: Precio
                var cellPrecio = ws.Cells[row, 5];
                cellPrecio.Value = p.Precio;
                cellPrecio.Style.Numberformat.Format = CurrencyFormat;
                cellPrecio.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                // Col 6: Stock con alerta visual sutil
                var cellStock = ws.Cells[row, 6];
                cellStock.Value = p.Stock;
                cellStock.Style.Numberformat.Format = IntegerFormat;
                cellStock.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                if (p.Stock <= 0)
                {
                    cellStock.Style.Font.Bold = true;
                    cellStock.Style.Font.Color.SetColor(BadgeRedText);
                    cellStock.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    cellStock.Style.Fill.BackgroundColor.SetColor(BadgeRedBg);
                }
                else if (p.Stock <= 5)
                {
                    cellStock.Style.Font.Bold = true;
                    cellStock.Style.Font.Color.SetColor(BadgeAmberText);
                    cellStock.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    cellStock.Style.Fill.BackgroundColor.SetColor(BadgeAmberBg);
                }

                // Col 7: Estado con estilo de etiqueta (Badge / Pill)
                var cellEstado = ws.Cells[row, 7];
                cellEstado.Value = p.Activo ? "Activo" : "Inactivo";
                cellEstado.Style.Font.Bold = true;
                cellEstado.Style.Font.Size = 9.5f;
                cellEstado.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                cellEstado.Style.Fill.PatternType = ExcelFillStyle.Solid;
                cellEstado.Style.Fill.BackgroundColor.SetColor(p.Activo ? BadgeGreenBg : BadgeRedBg);
                cellEstado.Style.Font.Color.SetColor(p.Activo ? BadgeGreenText : BadgeRedText);

                // Col 8: Fecha Registro
                var cellFecha = ws.Cells[row, 8];
                cellFecha.Value = p.CreadoEn.ToString("yyyy-MM-dd HH:mm");
                cellFecha.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                // Estilo general para la fila: tipografía, fondo alternado y bordes suaves
                for (int col = 1; col <= 8; col++)
                {
                    var cell = ws.Cells[row, col];
                    cell.Style.Font.Name = "Segoe UI";
                    cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    if (col != 7 && (col != 6 || p.Stock > 5))
                    {
                        cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        cell.Style.Fill.BackgroundColor.SetColor(currentBg);
                    }

                    cell.Style.Border.BorderAround(ExcelBorderStyle.Thin, BrandBorder);
                }

                row++;
            }

            // 6. Fila de Totales y Resumen Contable
            int totalRow = row;
            ws.Row(totalRow).Height = 26;

            ws.Cells[totalRow, 1, totalRow, 4].Merge = true;
            ws.Cells[totalRow, 1].Value = "PROMEDIO / TOTAL INVENTARIO";
            ws.Cells[totalRow, 1].Style.Font.Name = "Segoe UI";
            ws.Cells[totalRow, 1].Style.Font.Bold = true;
            ws.Cells[totalRow, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

            ws.Cells[totalRow, 5].Formula = $"AVERAGE(E{startDataRow}:E{totalRow - 1})";
            ws.Cells[totalRow, 5].Style.Numberformat.Format = CurrencyFormat;
            ws.Cells[totalRow, 5].Style.Font.Bold = true;
            ws.Cells[totalRow, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

            ws.Cells[totalRow, 6].Formula = $"SUM(F{startDataRow}:F{totalRow - 1})";
            ws.Cells[totalRow, 6].Style.Numberformat.Format = IntegerFormat;
            ws.Cells[totalRow, 6].Style.Font.Bold = true;
            ws.Cells[totalRow, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

            ws.Cells[totalRow, 7].Value = $"{productos.Count(p => p.Activo)} Activos";
            ws.Cells[totalRow, 7].Style.Font.Bold = true;
            ws.Cells[totalRow, 7].Style.Font.Size = 9.5f;
            ws.Cells[totalRow, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            ws.Cells[totalRow, 8].Value = $"{productos.Count} Registros";
            ws.Cells[totalRow, 8].Style.Font.Italic = true;
            ws.Cells[totalRow, 8].Style.Font.Size = 9.5f;
            ws.Cells[totalRow, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            for (int col = 1; col <= 8; col++)
            {
                var c = ws.Cells[totalRow, col];
                c.Style.Font.Name = "Segoe UI";
                c.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                c.Style.Fill.PatternType = ExcelFillStyle.Solid;
                c.Style.Fill.BackgroundColor.SetColor(BrandGoldLight);
                c.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                c.Style.Border.Top.Color.SetColor(BrandGold);
                c.Style.Border.Bottom.Style = ExcelBorderStyle.Double;
                c.Style.Border.Bottom.Color.SetColor(BrandNavy);
            }

            // Habilitar Autofiltro dinámico interactivo en los encabezados de la tabla
            ws.Cells[4, 1, totalRow - 1, 8].AutoFilter = true;

            // 7. Tarjetas KPI de Resumen Ejecutivo al pie de la tabla
            int kpiLabelRow = totalRow + 2;
            int kpiValRow = totalRow + 3;
            int kpiValEndRow = totalRow + 4;

            ws.Row(totalRow + 1).Height = 14;
            ws.Row(kpiLabelRow).Height = 18;
            ws.Row(kpiValRow).Height = 22;
            ws.Row(kpiValEndRow).Height = 22;

            int criticosCount = productos.Count(p => p.Stock <= 5);

            RenderKpiCard(ws, kpiLabelRow, kpiValRow, kpiValEndRow, 2, 3, "TOTAL ARTÍCULOS CATÁLOGO", $"{productos.Count} Ítems", "", BrandGold);
            RenderKpiCard(ws, kpiLabelRow, kpiValRow, kpiValEndRow, 4, 5, "EXISTENCIAS EN BODEGA", $"=SUM(F{startDataRow}:F{totalRow - 1})", IntegerFormat, BrandNavy);
            RenderKpiCard(ws, kpiLabelRow, kpiValRow, kpiValEndRow, 6, 7, "PRECIO PROMEDIO MATERIALES", $"=AVERAGE(E{startDataRow}:E{totalRow - 1})", CurrencyFormat, BrandGold);
            RenderKpiCard(ws, kpiLabelRow, kpiValRow, kpiValEndRow, 8, 8, "STOCK CRÍTICO (≤ 5)", $"{criticosCount} Ítems", "", criticosCount > 0 ? Color.FromArgb(239, 68, 68) : BrandNavy);
        }

        // Fijar paneles para que los encabezados permanezcan visibles al desplazarse
        ws.View.FreezePanes(5, 1);

        // Autoajustar ancho de columnas agregando margen de respiración
        ws.Cells[4, 1, Math.Max(4, row), 8].AutoFitColumns();
        for (int col = 1; col <= 8; col++)
        {
            ws.Column(col).Width = Math.Max(ws.Column(col).Width + 4, 13);
        }

        // -----------------------------------------------------------------------------------------
        // HOJA 2: TABLERO ANALÍTICO DE INVENTARIO Y SALUD DE STOCK (Vista Ejecutiva)
        // -----------------------------------------------------------------------------------------
        if (productos.Count > 0)
        {
            var wsDash = package.Workbook.Worksheets.Add("📊 Resumen de Inventario");
            wsDash.TabColor = BrandGold;
            wsDash.View.ShowGridLines = true;
            wsDash.PrinterSettings.Orientation = eOrientation.Landscape;

            // Encabezado del Dashboard con logotipo corporativo sobre fondo blanco
            InsertCorporateLogo(wsDash, "LogoFirmezaDashProd", fromRow: 0, fromRowOffset: 3, fromCol: 1, fromColOffset: 6, width: 42, height: 42);

            wsDash.Row(1).Height = 48;
            wsDash.Cells["B1:G1"].Merge = true;
            wsDash.Cells["B1"].Value = "            FIRMEZA S.A.S.  |  TABLERO ANALÍTICO Y SALUD DE INVENTARIO";
            wsDash.Cells["B1"].Style.Font.Name = "Segoe UI";
            wsDash.Cells["B1"].Style.Font.Size = 13.5f;
            wsDash.Cells["B1"].Style.Font.Bold = true;
            wsDash.Cells["B1"].Style.Font.Color.SetColor(BrandNavy);
            wsDash.Cells["B1"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            wsDash.Cells["B1"].Style.Fill.BackgroundColor.SetColor(Color.White);
            wsDash.Cells["B1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            wsDash.Cells["B1"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            wsDash.Cells["B1:G1"].Style.Border.Top.Style = ExcelBorderStyle.Medium;
            wsDash.Cells["B1:G1"].Style.Border.Top.Color.SetColor(BrandNavy);

            wsDash.Row(2).Height = 5;
            wsDash.Cells["B2:G2"].Merge = true;
            wsDash.Cells["B2"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            wsDash.Cells["B2"].Style.Fill.BackgroundColor.SetColor(BrandGold);

            // Métricas KPI Ejecutivas Superiores
            int dashKpiLabel = 4;
            int dashKpiValStart = 5;
            int dashKpiValEnd = 6;
            wsDash.Row(dashKpiLabel).Height = 18;
            wsDash.Row(dashKpiValStart).Height = 20;
            wsDash.Row(dashKpiValEnd).Height = 20;

            decimal valorTotalInv = productos.Sum(p => p.Precio * p.Stock);
            int totalUnidades = productos.Sum(p => p.Stock);
            int articulosActivos = productos.Count(p => p.Activo);
            int articulosCriticos = productos.Count(p => p.Stock <= 5 && p.Stock > 0);
            int articulosAgotados = productos.Count(p => p.Stock <= 0);

            RenderKpiCard(wsDash, dashKpiLabel, dashKpiValStart, dashKpiValEnd, 2, 3, "VALORIZACIÓN ESTIMADA BODEGA", valorTotalInv.ToString(CurrencyFormat, CultureInfo.InvariantCulture), CurrencyFormat, BrandGold);
            RenderKpiCard(wsDash, dashKpiLabel, dashKpiValStart, dashKpiValEnd, 4, 4, "UNIDADES TOTALES", totalUnidades.ToString(IntegerFormat), IntegerFormat, BrandNavy);
            RenderKpiCard(wsDash, dashKpiLabel, dashKpiValStart, dashKpiValEnd, 5, 5, "PRODUCTOS ACTIVOS", $"{articulosActivos} de {productos.Count}", "", BrandGold);
            RenderKpiCard(wsDash, dashKpiLabel, dashKpiValStart, dashKpiValEnd, 6, 7, "ALERTAS CRÍTICAS / AGOTADOS", $"{articulosCriticos + articulosAgotados} Referencias", "", articulosAgotados > 0 ? Color.FromArgb(220, 38, 38) : BrandNavy);

            // Tabla de Desglose de Niveles de Abastecimiento
            wsDash.Row(8).Height = 14;
            int tableHeaderRow = 9;
            wsDash.Row(tableHeaderRow).Height = 24;

            string[] dashHeaders = { "Nivel de Abastecimiento", "Condición de Existencia", "Nº Referencias", "% Portafolio", "Unidades Físicas", "Valor Estimado (COP)" };
            for (int i = 0; i < dashHeaders.Length; i++)
            {
                var cell = wsDash.Cells[tableHeaderRow, i + 2];
                cell.Value = dashHeaders[i];
                cell.Style.Font.Name = "Segoe UI";
                cell.Style.Font.Size = 9.5f;
                cell.Style.Font.Bold = true;
                cell.Style.Font.Color.SetColor(Color.White);
                cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                cell.Style.Fill.BackgroundColor.SetColor(BrandNavy);
                cell.Style.HorizontalAlignment = (i >= 2) ? ExcelHorizontalAlignment.Right : ExcelHorizontalAlignment.Left;
                cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                cell.Style.Border.Bottom.Style = ExcelBorderStyle.Medium;
                cell.Style.Border.Bottom.Color.SetColor(BrandGold);
            }

            var categorias = new[]
            {
                new { Nombre = "🟢 Abastecimiento Óptimo", Condicion = "Stock mayor a 15 unidades", Items = productos.Where(p => p.Stock > 15).ToList() },
                new { Nombre = "🟡 Abastecimiento Moderado", Condicion = "Stock entre 6 y 15 unidades", Items = productos.Where(p => p.Stock >= 6 && p.Stock <= 15).ToList() },
                new { Nombre = "🟠 Nivel de Alerta Crítica", Condicion = "Stock entre 1 y 5 unidades", Items = productos.Where(p => p.Stock >= 1 && p.Stock <= 5).ToList() },
                new { Nombre = "🔴 Agotado en Bodega", Condicion = "Stock igual a 0 unidades", Items = productos.Where(p => p.Stock == 0).ToList() }
            };

            int dashRow = tableHeaderRow + 1;
            foreach (var cat in categorias)
            {
                wsDash.Row(dashRow).Height = 21;
                var isEven = (dashRow % 2 == 0);
                var rowBg = isEven ? Color.White : BrandZebra;

                var countCat = cat.Items.Count;
                var pct = productos.Count > 0 ? (double)countCat / productos.Count : 0.0;
                var stockCat = cat.Items.Sum(p => p.Stock);
                var valCat = cat.Items.Sum(p => p.Precio * p.Stock);

                wsDash.Cells[dashRow, 2].Value = cat.Nombre;
                wsDash.Cells[dashRow, 2].Style.Font.Bold = true;
                wsDash.Cells[dashRow, 3].Value = cat.Condicion;
                wsDash.Cells[dashRow, 4].Value = countCat;
                wsDash.Cells[dashRow, 4].Style.Numberformat.Format = IntegerFormat;
                wsDash.Cells[dashRow, 5].Value = pct;
                wsDash.Cells[dashRow, 5].Style.Numberformat.Format = PercentFormat;
                wsDash.Cells[dashRow, 6].Value = stockCat;
                wsDash.Cells[dashRow, 6].Style.Numberformat.Format = IntegerFormat;
                wsDash.Cells[dashRow, 7].Value = valCat;
                wsDash.Cells[dashRow, 7].Style.Numberformat.Format = CurrencyFormat;

                for (int c = 2; c <= 7; c++)
                {
                    var cell = wsDash.Cells[dashRow, c];
                    cell.Style.Font.Name = "Segoe UI";
                    cell.Style.Font.Size = 9.5f;
                    cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    cell.Style.Fill.BackgroundColor.SetColor(rowBg);
                    cell.Style.Border.BorderAround(ExcelBorderStyle.Thin, BrandBorder);
                }

                dashRow++;
            }

            // Fila Total de la tabla resumen
            wsDash.Row(dashRow).Height = 24;
            wsDash.Cells[dashRow, 2, dashRow, 3].Merge = true;
            wsDash.Cells[dashRow, 2].Value = "TOTAL CONSOLIDADO EN BODEGA";
            wsDash.Cells[dashRow, 2].Style.Font.Bold = true;
            wsDash.Cells[dashRow, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

            wsDash.Cells[dashRow, 4].Value = productos.Count;
            wsDash.Cells[dashRow, 4].Style.Numberformat.Format = IntegerFormat;
            wsDash.Cells[dashRow, 4].Style.Font.Bold = true;

            wsDash.Cells[dashRow, 5].Value = 1.0;
            wsDash.Cells[dashRow, 5].Style.Numberformat.Format = PercentFormat;
            wsDash.Cells[dashRow, 5].Style.Font.Bold = true;

            wsDash.Cells[dashRow, 6].Value = totalUnidades;
            wsDash.Cells[dashRow, 6].Style.Numberformat.Format = IntegerFormat;
            wsDash.Cells[dashRow, 6].Style.Font.Bold = true;

            wsDash.Cells[dashRow, 7].Value = valorTotalInv;
            wsDash.Cells[dashRow, 7].Style.Numberformat.Format = CurrencyFormat;
            wsDash.Cells[dashRow, 7].Style.Font.Bold = true;

            for (int c = 2; c <= 7; c++)
            {
                var cell = wsDash.Cells[dashRow, c];
                cell.Style.Font.Name = "Segoe UI";
                cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                cell.Style.Fill.BackgroundColor.SetColor(BrandGoldLight);
                cell.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                cell.Style.Border.Top.Color.SetColor(BrandGold);
                cell.Style.Border.Bottom.Style = ExcelBorderStyle.Double;
                cell.Style.Border.Bottom.Color.SetColor(BrandNavy);
            }

            // Nota fiscal y comercial al pie
            int noteRow = dashRow + 2;
            wsDash.Cells[noteRow, 2, noteRow, 7].Merge = true;
            wsDash.Cells[noteRow, 2].Value = "FIRMEZA S.A.S.  •  Tablero automatizado para toma de decisiones de compras, rotación de activos y abastecimiento de obra.";
            wsDash.Cells[noteRow, 2].Style.Font.Name = "Segoe UI";
            wsDash.Cells[noteRow, 2].Style.Font.Size = 8.5f;
            wsDash.Cells[noteRow, 2].Style.Font.Italic = true;
            wsDash.Cells[noteRow, 2].Style.Font.Color.SetColor(BrandGray);
            wsDash.Cells[noteRow, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            wsDash.Column(1).Width = 4;
            wsDash.Column(2).Width = 30;
            wsDash.Column(3).Width = 32;
            wsDash.Column(4).Width = 16;
            wsDash.Column(5).Width = 16;
            wsDash.Column(6).Width = 20;
            wsDash.Column(7).Width = 26;
        }

        return await package.GetAsByteArrayAsync();
    }

    // =============================================================================================
    // 2. EXPORTACIÓN: HISTÓRICO Y CONTROL GENERAL DE VENTAS CON TABLERO GERENCIAL
    // =============================================================================================
    public async Task<byte[]> ExportSalesToExcelAsync()
    {
        var ventas = await _context.Ventas
            .Include(v => v.Cliente)
            .Include(v => v.DetallesVenta)
                .ThenInclude(d => d.Producto)
            .AsNoTracking()
            .OrderByDescending(v => v.Fecha)
            .ToListAsync();

        using var package = new ExcelPackage();

        // -----------------------------------------------------------------------------------------
        // HOJA 1: REGISTRO DE VENTAS DETALLADAS (Requisito contractual y funcional)
        // -----------------------------------------------------------------------------------------
        var ws = package.Workbook.Worksheets.Add("Ventas Registradas");
        ws.TabColor = BrandNavy;

        // Configuración de página horizontal
        ws.PrinterSettings.Orientation = eOrientation.Landscape;
        ws.PrinterSettings.PaperSize = ePaperSize.Letter;
        ws.PrinterSettings.FitToPage = true;
        ws.PrinterSettings.FitToWidth = 1;
        ws.PrinterSettings.FitToHeight = 0;
        ws.View.ShowGridLines = true;

        // Inserción de Logotipo oficial con fondo blanco para contraste impecable de azul y dorado
        InsertCorporateLogo(ws, "LogoFirmezaVentas", fromRow: 0, fromRowOffset: 3, fromCol: 0, fromColOffset: 8, width: 42, height: 42);

        // 1. Cabecera ejecutiva con fondo blanco pulcro que resalta el logotipo oficial azul oscuro y dorado
        ws.Row(1).Height = 48;
        ws.Cells["A1:I1"].Merge = true;
        ws.Cells["A1"].Value = "            FIRMEZA S.A.S.   |   HISTÓRICO Y CONTROL GENERAL DE VENTAS";
        ws.Cells["A1"].Style.Font.Name = "Segoe UI";
        ws.Cells["A1"].Style.Font.Size = 14.5f;
        ws.Cells["A1"].Style.Font.Bold = true;
        ws.Cells["A1"].Style.Font.Color.SetColor(BrandNavy);
        ws.Cells["A1"].Style.Fill.PatternType = ExcelFillStyle.Solid;
        ws.Cells["A1"].Style.Fill.BackgroundColor.SetColor(Color.White);
        ws.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
        ws.Cells["A1"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
        ws.Cells["A1:I1"].Style.Border.Top.Style = ExcelBorderStyle.Medium;
        ws.Cells["A1:I1"].Style.Border.Top.Color.SetColor(BrandNavy);

        // 2. Línea decorativa dorada de acento institucional
        ws.Row(2).Height = 5;
        ws.Cells["A2:I2"].Merge = true;
        ws.Cells["A2"].Style.Fill.PatternType = ExcelFillStyle.Solid;
        ws.Cells["A2"].Style.Fill.BackgroundColor.SetColor(BrandGold);

        // 3. Subtítulo con fecha y resumen de ventas
        ws.Row(3).Height = 24;
        ws.Cells["A3:I3"].Merge = true;
        ws.Cells["A3"].Value = $"Reporte Oficial emitido el {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC   •   Total órdenes registradas: {ventas.Count}   •   Facturación Electrónica Firmeza   •   NIT: 901.458.789-0";
        ws.Cells["A3"].Style.Font.Name = "Segoe UI";
        ws.Cells["A3"].Style.Font.Size = 9.5f;
        ws.Cells["A3"].Style.Font.Italic = true;
        ws.Cells["A3"].Style.Font.Color.SetColor(BrandNavy);
        ws.Cells["A3"].Style.Fill.PatternType = ExcelFillStyle.Solid;
        ws.Cells["A3"].Style.Fill.BackgroundColor.SetColor(BrandGoldLight);
        ws.Cells["A3"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        ws.Cells["A3"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
        ws.Cells["A3:I3"].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
        ws.Cells["A3:I3"].Style.Border.Bottom.Color.SetColor(BrandGold);

        // 4. Encabezados de la tabla
        string[] headers = { "ID", "Nº Factura", "Fecha de Emisión", "Cliente", "Identificación", "Subtotal (COP)", "IVA (19%)", "Total Facturado", "Estado" };
        ws.Row(4).Height = 28;

        for (int i = 0; i < headers.Length; i++)
        {
            var cell = ws.Cells[4, i + 1];
            cell.Value = headers[i];
            cell.Style.Font.Name = "Segoe UI";
            cell.Style.Font.Size = 10.5f;
            cell.Style.Font.Bold = true;
            cell.Style.Font.Color.SetColor(Color.White);
            cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
            cell.Style.Fill.BackgroundColor.SetColor(BrandNavy);
            cell.Style.HorizontalAlignment = (i >= 5 && i <= 7) ? ExcelHorizontalAlignment.Right : (i == 3) ? ExcelHorizontalAlignment.Left : ExcelHorizontalAlignment.Center;
            cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            cell.Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.FromArgb(51, 65, 85));
            cell.Style.Border.Bottom.Style = ExcelBorderStyle.Medium;
            cell.Style.Border.Bottom.Color.SetColor(BrandGold);
        }

        // 5. Filas de ventas
        int startDataRow = 5;
        int row = startDataRow;

        if (ventas.Count == 0)
        {
            ws.Cells[row, 1, row, 9].Merge = true;
            ws.Cells[row, 1].Value = "No se encontraron ventas registradas en el período consultado.";
            ws.Cells[row, 1].Style.Font.Name = "Segoe UI";
            ws.Cells[row, 1].Style.Font.Italic = true;
            ws.Cells[row, 1].Style.Font.Color.SetColor(BrandGray);
            ws.Cells[row, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            ws.Cells[row, 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            ws.Row(row).Height = 26;
            ws.Cells[row, 1, row, 9].Style.Border.BorderAround(ExcelBorderStyle.Thin, BrandBorder);
            row++;
        }
        else
        {
            foreach (var v in ventas)
            {
                ws.Row(row).Height = 22;
                var isEven = (row % 2 == 0);
                var currentBg = isEven ? Color.White : BrandZebra;

                // Col 1: ID
                var cellId = ws.Cells[row, 1];
                cellId.Value = v.Id;
                cellId.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                // Col 2: Nº Factura
                var cellNumero = ws.Cells[row, 2];
                cellNumero.Value = v.NumeroVenta;
                cellNumero.Style.Font.Bold = true;
                cellNumero.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                // Col 3: Fecha
                var cellFecha = ws.Cells[row, 3];
                cellFecha.Value = v.Fecha.ToString("yyyy-MM-dd HH:mm");
                cellFecha.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                // Col 4: Cliente
                var cellCliente = ws.Cells[row, 4];
                cellCliente.Value = $"{v.Cliente.Nombres} {v.Cliente.Apellidos}".Trim();
                cellCliente.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                // Col 5: Identificación
                var cellDoc = ws.Cells[row, 5];
                cellDoc.Value = v.Cliente.Identificacion;
                cellDoc.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                // Col 6: Subtotal
                var cellSub = ws.Cells[row, 6];
                cellSub.Value = v.Subtotal;
                cellSub.Style.Numberformat.Format = CurrencyFormat;
                cellSub.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                // Col 7: IVA
                var cellIva = ws.Cells[row, 7];
                cellIva.Value = v.Iva;
                cellIva.Style.Numberformat.Format = CurrencyFormat;
                cellIva.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                // Col 8: Total Facturado
                var cellTotal = ws.Cells[row, 8];
                cellTotal.Value = v.Total;
                cellTotal.Style.Numberformat.Format = CurrencyFormat;
                cellTotal.Style.Font.Bold = true;
                cellTotal.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                // Col 9: Estado con badge / pill
                var cellEstado = ws.Cells[row, 9];
                cellEstado.Value = v.Estado;
                cellEstado.Style.Font.Bold = true;
                cellEstado.Style.Font.Size = 9.5f;
                cellEstado.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                cellEstado.Style.Fill.PatternType = ExcelFillStyle.Solid;

                var isCompleted = v.Estado.Equals("Completada", StringComparison.OrdinalIgnoreCase) ||
                                  v.Estado.Equals("Aprobada", StringComparison.OrdinalIgnoreCase);
                var isCancelled = v.Estado.Equals("Anulada", StringComparison.OrdinalIgnoreCase) ||
                                  v.Estado.Equals("Cancelada", StringComparison.OrdinalIgnoreCase) ||
                                  v.Estado.Equals("Denegada", StringComparison.OrdinalIgnoreCase);

                if (isCompleted)
                {
                    cellEstado.Style.Fill.BackgroundColor.SetColor(BadgeGreenBg);
                    cellEstado.Style.Font.Color.SetColor(BadgeGreenText);
                }
                else if (isCancelled)
                {
                    cellEstado.Style.Fill.BackgroundColor.SetColor(BadgeRedBg);
                    cellEstado.Style.Font.Color.SetColor(BadgeRedText);
                }
                else
                {
                    cellEstado.Style.Fill.BackgroundColor.SetColor(BadgeAmberBg);
                    cellEstado.Style.Font.Color.SetColor(BadgeAmberText);
                }

                // Estilo general para la fila
                for (int col = 1; col <= 9; col++)
                {
                    var cell = ws.Cells[row, col];
                    cell.Style.Font.Name = "Segoe UI";
                    cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    if (col != 9)
                    {
                        cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        cell.Style.Fill.BackgroundColor.SetColor(currentBg);
                    }

                    cell.Style.Border.BorderAround(ExcelBorderStyle.Thin, BrandBorder);
                }

                row++;
            }

            // 6. Fila de Totales y Resumen Contable
            int totalRow = row;
            ws.Row(totalRow).Height = 26;

            ws.Cells[totalRow, 1, totalRow, 5].Merge = true;
            ws.Cells[totalRow, 1].Value = "TOTALES FACTURADOS";
            ws.Cells[totalRow, 1].Style.Font.Name = "Segoe UI";
            ws.Cells[totalRow, 1].Style.Font.Bold = true;
            ws.Cells[totalRow, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

            ws.Cells[totalRow, 6].Formula = $"SUM(F{startDataRow}:F{totalRow - 1})";
            ws.Cells[totalRow, 6].Style.Numberformat.Format = CurrencyFormat;
            ws.Cells[totalRow, 6].Style.Font.Bold = true;
            ws.Cells[totalRow, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

            ws.Cells[totalRow, 7].Formula = $"SUM(G{startDataRow}:G{totalRow - 1})";
            ws.Cells[totalRow, 7].Style.Numberformat.Format = CurrencyFormat;
            ws.Cells[totalRow, 7].Style.Font.Bold = true;
            ws.Cells[totalRow, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

            ws.Cells[totalRow, 8].Formula = $"SUM(H{startDataRow}:H{totalRow - 1})";
            ws.Cells[totalRow, 8].Style.Numberformat.Format = CurrencyFormat;
            ws.Cells[totalRow, 8].Style.Font.Bold = true;
            ws.Cells[totalRow, 8].Style.Font.Size = 11f;
            ws.Cells[totalRow, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

            ws.Cells[totalRow, 9].Value = $"{ventas.Count} Ventas";
            ws.Cells[totalRow, 9].Style.Font.Bold = true;
            ws.Cells[totalRow, 9].Style.Font.Size = 9.5f;
            ws.Cells[totalRow, 9].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            for (int col = 1; col <= 9; col++)
            {
                var c = ws.Cells[totalRow, col];
                c.Style.Font.Name = "Segoe UI";
                c.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                c.Style.Fill.PatternType = ExcelFillStyle.Solid;
                c.Style.Fill.BackgroundColor.SetColor(BrandGoldLight);
                c.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                c.Style.Border.Top.Color.SetColor(BrandGold);
                c.Style.Border.Bottom.Style = ExcelBorderStyle.Double;
                c.Style.Border.Bottom.Color.SetColor(BrandNavy);
            }

            // Habilitar Autofiltro dinámico interactivo en los encabezados
            ws.Cells[4, 1, totalRow - 1, 9].AutoFilter = true;

            // 7. Tarjetas KPI de Resumen Ejecutivo al pie de la tabla
            int kpiLabelRow = totalRow + 2;
            int kpiValRow = totalRow + 3;
            int kpiValEndRow = totalRow + 4;

            ws.Row(totalRow + 1).Height = 14;
            ws.Row(kpiLabelRow).Height = 18;
            ws.Row(kpiValRow).Height = 22;
            ws.Row(kpiValEndRow).Height = 22;

            RenderKpiCard(ws, kpiLabelRow, kpiValRow, kpiValEndRow, 2, 3, "FACTURACIÓN TOTAL (COP)", $"=SUM(H{startDataRow}:H{totalRow - 1})", CurrencyFormat, BrandGold);
            RenderKpiCard(ws, kpiLabelRow, kpiValRow, kpiValEndRow, 4, 5, "FACTURAS EMITIDAS", $"{ventas.Count} Órdenes", "", BrandNavy);
            RenderKpiCard(ws, kpiLabelRow, kpiValRow, kpiValEndRow, 6, 7, "RECAUDO IMPUESTO IVA (19%)", $"=SUM(G{startDataRow}:G{totalRow - 1})", CurrencyFormat, BrandGold);
            RenderKpiCard(ws, kpiLabelRow, kpiValRow, kpiValEndRow, 8, 9, "TICKET PROMEDIO", $"=AVERAGE(H{startDataRow}:H{totalRow - 1})", CurrencyFormat, BrandNavy);
        }

        // Fijar paneles
        ws.View.FreezePanes(5, 1);

        // Autoajustar ancho de columnas
        ws.Cells[4, 1, Math.Max(4, row), 9].AutoFitColumns();
        for (int col = 1; col <= 9; col++)
        {
            ws.Column(col).Width = Math.Max(ws.Column(col).Width + 4, 13);
        }

        // -----------------------------------------------------------------------------------------
        // HOJA 2: TABLERO GERENCIAL DE VENTAS Y RECAUDO (Vista Ejecutiva)
        // -----------------------------------------------------------------------------------------
        if (ventas.Count > 0)
        {
            var wsDash = package.Workbook.Worksheets.Add("📊 Tablero Ejecutivo");
            wsDash.TabColor = BrandGold;
            wsDash.View.ShowGridLines = true;
            wsDash.PrinterSettings.Orientation = eOrientation.Landscape;

            // Banner Ejecutivo con logotipo corporativo sobre fondo blanco
            InsertCorporateLogo(wsDash, "LogoFirmezaDashVentas", fromRow: 0, fromRowOffset: 3, fromCol: 1, fromColOffset: 6, width: 42, height: 42);

            wsDash.Row(1).Height = 48;
            wsDash.Cells["B1:G1"].Merge = true;
            wsDash.Cells["B1"].Value = "            FIRMEZA S.A.S.  |  TABLERO GERENCIAL DE VENTAS Y RECAUDO";
            wsDash.Cells["B1"].Style.Font.Name = "Segoe UI";
            wsDash.Cells["B1"].Style.Font.Size = 13.5f;
            wsDash.Cells["B1"].Style.Font.Bold = true;
            wsDash.Cells["B1"].Style.Font.Color.SetColor(BrandNavy);
            wsDash.Cells["B1"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            wsDash.Cells["B1"].Style.Fill.BackgroundColor.SetColor(Color.White);
            wsDash.Cells["B1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            wsDash.Cells["B1"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            wsDash.Cells["B1:G1"].Style.Border.Top.Style = ExcelBorderStyle.Medium;
            wsDash.Cells["B1:G1"].Style.Border.Top.Color.SetColor(BrandNavy);

            wsDash.Row(2).Height = 5;
            wsDash.Cells["B2:G2"].Merge = true;
            wsDash.Cells["B2"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            wsDash.Cells["B2"].Style.Fill.BackgroundColor.SetColor(BrandGold);

            // Métricas KPI Superiores
            int dashKpiLabel = 4;
            int dashKpiValStart = 5;
            int dashKpiValEnd = 6;
            wsDash.Row(dashKpiLabel).Height = 18;
            wsDash.Row(dashKpiValStart).Height = 20;
            wsDash.Row(dashKpiValEnd).Height = 20;

            decimal totalFacturado = ventas.Sum(v => v.Total);
            decimal totalSubtotal = ventas.Sum(v => v.Subtotal);
            decimal ticketPromedio = ventas.Count > 0 ? totalFacturado / ventas.Count : 0m;
            int aprobadasCount = ventas.Count(v => v.Estado.Equals("Completada", StringComparison.OrdinalIgnoreCase) ||
                                                  v.Estado.Equals("Aprobada", StringComparison.OrdinalIgnoreCase));
            double tasaEfectividad = ventas.Count > 0 ? (double)aprobadasCount / ventas.Count : 0.0;

            RenderKpiCard(wsDash, dashKpiLabel, dashKpiValStart, dashKpiValEnd, 2, 3, "FACTURACIÓN TOTAL BRUTA", totalFacturado.ToString(CurrencyFormat, CultureInfo.InvariantCulture), CurrencyFormat, BrandGold);
            RenderKpiCard(wsDash, dashKpiLabel, dashKpiValStart, dashKpiValEnd, 4, 4, "ÓRDENES TOTALES", ventas.Count.ToString(IntegerFormat), IntegerFormat, BrandNavy);
            RenderKpiCard(wsDash, dashKpiLabel, dashKpiValStart, dashKpiValEnd, 5, 5, "EFECTIVIDAD DE VENTAS", tasaEfectividad.ToString(PercentFormat), PercentFormat, BrandGold);
            RenderKpiCard(wsDash, dashKpiLabel, dashKpiValStart, dashKpiValEnd, 6, 7, "TICKET PROMEDIO ORDEN", ticketPromedio.ToString(CurrencyFormat, CultureInfo.InvariantCulture), CurrencyFormat, BrandNavy);

            // Tabla de Desglose por Estado Contable
            wsDash.Row(8).Height = 14;
            int tableHeaderRow = 9;
            wsDash.Row(tableHeaderRow).Height = 24;

            string[] dashHeaders = { "Estado Contable", "Significado / Gestión", "Nº Órdenes", "% Participación", "Subtotal (COP)", "Total Facturado (COP)" };
            for (int i = 0; i < dashHeaders.Length; i++)
            {
                var cell = wsDash.Cells[tableHeaderRow, i + 2];
                cell.Value = dashHeaders[i];
                cell.Style.Font.Name = "Segoe UI";
                cell.Style.Font.Size = 9.5f;
                cell.Style.Font.Bold = true;
                cell.Style.Font.Color.SetColor(Color.White);
                cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                cell.Style.Fill.BackgroundColor.SetColor(BrandNavy);
                cell.Style.HorizontalAlignment = (i >= 2) ? ExcelHorizontalAlignment.Right : ExcelHorizontalAlignment.Left;
                cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                cell.Style.Border.Bottom.Style = ExcelBorderStyle.Medium;
                cell.Style.Border.Bottom.Color.SetColor(BrandGold);
            }

            var estados = ventas.GroupBy(v => v.Estado)
                .Select(g => new
                {
                    Estado = g.Key,
                    Count = g.Count(),
                    Subtotal = g.Sum(x => x.Subtotal),
                    Total = g.Sum(x => x.Total)
                })
                .OrderByDescending(g => g.Total)
                .ToList();

            int dashRow = tableHeaderRow + 1;
            foreach (var est in estados)
            {
                wsDash.Row(dashRow).Height = 21;
                var isEven = (dashRow % 2 == 0);
                var rowBg = isEven ? Color.White : BrandZebra;
                var pct = ventas.Count > 0 ? (double)est.Count / ventas.Count : 0.0;

                string desc = est.Estado switch
                {
                    "Completada" => "Ventas despachadas y cobro registrado",
                    "Aprobada" => "Factura aprobada y en trámite de entrega",
                    "Anulada" => "Operación cancelada sin efecto fiscal",
                    "Pendiente" => "En espera de confirmación de pago",
                    _ => "Transacción en gestión comercial"
                };

                wsDash.Cells[dashRow, 2].Value = est.Estado;
                wsDash.Cells[dashRow, 2].Style.Font.Bold = true;
                wsDash.Cells[dashRow, 3].Value = desc;
                wsDash.Cells[dashRow, 4].Value = est.Count;
                wsDash.Cells[dashRow, 4].Style.Numberformat.Format = IntegerFormat;
                wsDash.Cells[dashRow, 5].Value = pct;
                wsDash.Cells[dashRow, 5].Style.Numberformat.Format = PercentFormat;
                wsDash.Cells[dashRow, 6].Value = est.Subtotal;
                wsDash.Cells[dashRow, 6].Style.Numberformat.Format = CurrencyFormat;
                wsDash.Cells[dashRow, 7].Value = est.Total;
                wsDash.Cells[dashRow, 7].Style.Numberformat.Format = CurrencyFormat;

                for (int c = 2; c <= 7; c++)
                {
                    var cell = wsDash.Cells[dashRow, c];
                    cell.Style.Font.Name = "Segoe UI";
                    cell.Style.Font.Size = 9.5f;
                    cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    cell.Style.Fill.BackgroundColor.SetColor(rowBg);
                    cell.Style.Border.BorderAround(ExcelBorderStyle.Thin, BrandBorder);
                }

                dashRow++;
            }

            // Fila de Totales
            wsDash.Row(dashRow).Height = 24;
            wsDash.Cells[dashRow, 2, dashRow, 3].Merge = true;
            wsDash.Cells[dashRow, 2].Value = "TOTAL CONSOLIDADO GENERAL";
            wsDash.Cells[dashRow, 2].Style.Font.Bold = true;
            wsDash.Cells[dashRow, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

            wsDash.Cells[dashRow, 4].Value = ventas.Count;
            wsDash.Cells[dashRow, 4].Style.Numberformat.Format = IntegerFormat;
            wsDash.Cells[dashRow, 4].Style.Font.Bold = true;

            wsDash.Cells[dashRow, 5].Value = 1.0;
            wsDash.Cells[dashRow, 5].Style.Numberformat.Format = PercentFormat;
            wsDash.Cells[dashRow, 5].Style.Font.Bold = true;

            wsDash.Cells[dashRow, 6].Value = totalSubtotal;
            wsDash.Cells[dashRow, 6].Style.Numberformat.Format = CurrencyFormat;
            wsDash.Cells[dashRow, 6].Style.Font.Bold = true;

            wsDash.Cells[dashRow, 7].Value = totalFacturado;
            wsDash.Cells[dashRow, 7].Style.Numberformat.Format = CurrencyFormat;
            wsDash.Cells[dashRow, 7].Style.Font.Bold = true;

            for (int c = 2; c <= 7; c++)
            {
                var cell = wsDash.Cells[dashRow, c];
                cell.Style.Font.Name = "Segoe UI";
                cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                cell.Style.Fill.BackgroundColor.SetColor(BrandGoldLight);
                cell.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                cell.Style.Border.Top.Color.SetColor(BrandGold);
                cell.Style.Border.Bottom.Style = ExcelBorderStyle.Double;
                cell.Style.Border.Bottom.Color.SetColor(BrandNavy);
            }

            // Nota fiscal DIAN
            int noteRow = dashRow + 2;
            wsDash.Cells[noteRow, 2, noteRow, 7].Merge = true;
            wsDash.Cells[noteRow, 2].Value = "FIRMEZA S.A.S.  •  NIT: 901.458.789-0  •  Resolución DIAN No. 18764000001 de 2026/01/15  •  Régimen Común IVA  •  Bogotá D.C., Colombia";
            wsDash.Cells[noteRow, 2].Style.Font.Name = "Segoe UI";
            wsDash.Cells[noteRow, 2].Style.Font.Size = 8.5f;
            wsDash.Cells[noteRow, 2].Style.Font.Italic = true;
            wsDash.Cells[noteRow, 2].Style.Font.Color.SetColor(BrandGray);
            wsDash.Cells[noteRow, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            wsDash.Column(1).Width = 4;
            wsDash.Column(2).Width = 24;
            wsDash.Column(3).Width = 38;
            wsDash.Column(4).Width = 16;
            wsDash.Column(5).Width = 16;
            wsDash.Column(6).Width = 24;
            wsDash.Column(7).Width = 26;
        }

        return await package.GetAsByteArrayAsync();
    }

    // =============================================================================================
    // 3. PLANTILLA EMPRESARIAL DE PRODUCTOS (Para Carga Masiva)
    // =============================================================================================
    public byte[] GenerateProductsTemplate()
    {
        using var package = new ExcelPackage();
        var ws = package.Workbook.Worksheets.Add("Plantilla Carga Productos");
        ws.TabColor = BrandNavy;
        ws.View.ShowGridLines = true;

        string[] headers = { "Codigo", "Nombre", "Descripcion", "Precio", "Stock" };
        string[] tooltips = {
            "Código único de identificación del producto (Obligatorio, Ej: PROD-100)",
            "Nombre comercial formal del producto o material (Obligatorio)",
            "Descripción técnica o comercial detallada (Opcional)",
            "Precio de venta al público en COP sin puntuación especial (Obligatorio, >= 0)",
            "Cantidad de existencias en bodega (Obligatorio, entero >= 0)"
        };

        ws.Row(1).Height = 30;

        for (int i = 0; i < headers.Length; i++)
        {
            var cell = ws.Cells[1, i + 1];
            cell.Value = headers[i];
            cell.Style.Font.Name = "Segoe UI";
            cell.Style.Font.Bold = true;
            cell.Style.Font.Size = 10.5f;
            cell.Style.Font.Color.SetColor(Color.White);
            cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
            cell.Style.Fill.BackgroundColor.SetColor(BrandNavy);
            cell.Style.HorizontalAlignment = (i == 3 || i == 4) ? ExcelHorizontalAlignment.Right : (i == 0) ? ExcelHorizontalAlignment.Center : ExcelHorizontalAlignment.Left;
            cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            cell.Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.FromArgb(51, 65, 85));
            cell.Style.Border.Bottom.Style = ExcelBorderStyle.Medium;
            cell.Style.Border.Bottom.Color.SetColor(BrandGold);

            try
            {
                var comment = cell.AddComment(tooltips[i], "Firmeza S.A.S.");
                comment.AutoFit = true;
            }
            catch
            {
                // Si la adición de comentarios no está soportada en el entorno, continúa sin error
            }
        }

        // Fila 1 de ejemplo profesional
        ws.Row(2).Height = 23;
        ws.Cells[2, 1].Value = "PROD-100";
        ws.Cells[2, 2].Value = "Pala Redonda con Mango de Madera";
        ws.Cells[2, 3].Value = "Herramienta agrícola y de construcción forjada de alta resistencia";
        ws.Cells[2, 4].Value = 48500.00;
        ws.Cells[2, 5].Value = 40;

        // Fila 2 de ejemplo profesional
        ws.Row(3).Height = 23;
        ws.Cells[3, 1].Value = "PROD-101";
        ws.Cells[3, 2].Value = "Disco de Corte Metal 4.5 pulg";
        ws.Cells[3, 3].Value = "Disco abrasivo para esmeriladora angular de corte rápido";
        ws.Cells[3, 4].Value = 6200.00;
        ws.Cells[3, 5].Value = 100;

        // Fila 3 de ejemplo profesional
        ws.Row(4).Height = 23;
        ws.Cells[4, 1].Value = "PROD-102";
        ws.Cells[4, 2].Value = "Cemento Gris Tipo UG 50kg";
        ws.Cells[4, 3].Value = "Cemento de uso general para estructuras y mezclas de concreto";
        ws.Cells[4, 4].Value = 32500.00;
        ws.Cells[4, 5].Value = 150;

        for (int r = 2; r <= 4; r++)
        {
            var isEven = (r % 2 == 0);
            var rowBg = isEven ? Color.White : BrandZebra;

            for (int c = 1; c <= 5; c++)
            {
                var cell = ws.Cells[r, c];
                cell.Style.Font.Name = "Segoe UI";
                cell.Style.Font.Size = 9.5f;
                cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                cell.Style.Fill.BackgroundColor.SetColor(rowBg);
                cell.Style.Border.BorderAround(ExcelBorderStyle.Thin, BrandBorder);

                if (c == 1)
                {
                    cell.Style.Font.Bold = true;
                    cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                }
                else if (c == 4)
                {
                    cell.Style.Numberformat.Format = CurrencyFormat;
                    cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                }
                else if (c == 5)
                {
                    cell.Style.Numberformat.Format = IntegerFormat;
                    cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                }
            }
        }

        ws.View.FreezePanes(2, 1);
        ws.Cells[1, 1, 4, 5].AutoFitColumns();
        for (int c = 1; c <= 5; c++)
        {
            ws.Column(c).Width = Math.Max(ws.Column(c).Width + 5, 16);
        }

        return package.GetAsByteArray();
    }

    // =============================================================================================
    // 4. PLANTILLA EMPRESARIAL COMBINADA (Productos y Clientes en Una Hoja)
    // =============================================================================================
    public byte[] GenerateDesnormalizedTemplate()
    {
        using var package = new ExcelPackage();
        var ws = package.Workbook.Worksheets.Add("Carga Desnormalizada");
        ws.TabColor = BrandNavy;
        ws.View.ShowGridLines = true;

        var prodHeaderBg = BrandNavy;           // Azul Noche Corporativo para Productos
        var cliHeaderBg = Color.FromArgb(15, 118, 110); // Teal / Verde Azulado Ejecutivo para Clientes

        string[] headers = {
            "Codigo_Producto", "Nombre_Producto", "Descripcion_Producto", "Precio", "Stock",
            "Identificacion_Cliente", "Nombres_Cliente", "Apellidos_Cliente", "Email_Cliente", "Telefono_Cliente", "Direccion_Cliente", "Edad_Cliente"
        };

        ws.Row(1).Height = 30;

        for (int i = 0; i < headers.Length; i++)
        {
            var cell = ws.Cells[1, i + 1];
            cell.Value = headers[i];
            cell.Style.Font.Name = "Segoe UI";
            cell.Style.Font.Bold = true;
            cell.Style.Font.Size = 10f;
            cell.Style.Font.Color.SetColor(Color.White);
            cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
            cell.Style.Fill.BackgroundColor.SetColor(i < 5 ? prodHeaderBg : cliHeaderBg);
            cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            cell.Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.FromArgb(51, 65, 85));
            cell.Style.Border.Bottom.Style = ExcelBorderStyle.Medium;
            cell.Style.Border.Bottom.Color.SetColor(BrandGold);
        }

        // Fila 1 de ejemplo
        ws.Row(2).Height = 23;
        ws.Cells[2, 1].Value = "PROD-201";
        ws.Cells[2, 2].Value = "Pintura Anticorrosiva Gris (Galón)";
        ws.Cells[2, 3].Value = "Esmalte para protección de estructuras metálicas";
        ws.Cells[2, 4].Value = 72000.00;
        ws.Cells[2, 5].Value = 45;
        ws.Cells[2, 6].Value = "1098765432";
        ws.Cells[2, 7].Value = "Mauricio";
        ws.Cells[2, 8].Value = "Cárdenas Ruiz";
        ws.Cells[2, 9].Value = "mauricio.cardenas@example.com";
        ws.Cells[2, 10].Value = "3154567890";
        ws.Cells[2, 11].Value = "Calle 80 # 24-10";
        ws.Cells[2, 12].Value = 39;

        // Fila 2 de ejemplo
        ws.Row(3).Height = 23;
        ws.Cells[3, 1].Value = "PROD-202";
        ws.Cells[3, 2].Value = "Malla Electrosoldada 2x3m";
        ws.Cells[3, 3].Value = "Malla de refuerzo para losas de concreto";
        ws.Cells[3, 4].Value = 54000.00;
        ws.Cells[3, 5].Value = 60;
        ws.Cells[3, 6].Value = "1087654321";
        ws.Cells[3, 7].Value = "Patricia";
        ws.Cells[3, 8].Value = "Gutiérrez Peñaloza";
        ws.Cells[3, 9].Value = "patricia.gutierrez@example.com";
        ws.Cells[3, 10].Value = "3178901234";
        ws.Cells[3, 11].Value = "Carrera 13 # 45-67";
        ws.Cells[3, 12].Value = 34;

        for (int r = 2; r <= 3; r++)
        {
            var isEven = (r % 2 == 0);
            var rowBg = isEven ? Color.White : BrandZebra;

            for (int c = 1; c <= headers.Length; c++)
            {
                var cell = ws.Cells[r, c];
                cell.Style.Font.Name = "Segoe UI";
                cell.Style.Font.Size = 9.5f;
                cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                cell.Style.Fill.BackgroundColor.SetColor(rowBg);
                cell.Style.Border.BorderAround(ExcelBorderStyle.Thin, BrandBorder);

                if (c == 4)
                {
                    cell.Style.Numberformat.Format = CurrencyFormat;
                    cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                }
                else if (c == 5 || c == 12)
                {
                    cell.Style.Numberformat.Format = IntegerFormat;
                    cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                }
                else if (c == 1 || c == 6)
                {
                    cell.Style.Font.Bold = true;
                    cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                }
            }
        }

        ws.View.FreezePanes(2, 1);
        ws.Cells[1, 1, 3, headers.Length].AutoFitColumns();
        for (int c = 1; c <= headers.Length; c++)
        {
            ws.Column(c).Width = Math.Max(ws.Column(c).Width + 4, 15);
        }

        return package.GetAsByteArray();
    }

    // =============================================================================================
    // 5. IMPORTACIÓN: PRODUCTOS DESDE ARCHIVO EXCEL
    // =============================================================================================
    public async Task<ExcelImportResultDto> ImportProductsFromExcelAsync(Stream fileStream)
    {
        var errores = new List<string>();
        int totalFilas = 0;
        int totalImportados = 0;
        int totalIgnorados = 0;

        using var package = new ExcelPackage(fileStream);
        var worksheet = package.Workbook.Worksheets.FirstOrDefault();

        if (worksheet == null || worksheet.Dimension == null)
        {
            return new ExcelImportResultDto(0, 0, 0, new List<string> { "El archivo Excel se encuentra vacío o no contiene hojas de cálculo válidas." });
        }

        int rowCount = worksheet.Dimension.Rows;

        // Detección de columnas según los encabezados en la fila 1
        int colCodigo = 1, colNombre = 2, colDesc = 3, colPrecio = 4, colStock = 5;

        for (int col = 1; col <= worksheet.Dimension.Columns; col++)
        {
            var header = worksheet.Cells[1, col].Text.Trim().ToLowerInvariant();
            if (header.Contains("código") || header.Contains("codigo")) colCodigo = col;
            else if (header.Contains("nombre")) colNombre = col;
            else if (header.Contains("descrip")) colDesc = col;
            else if (header.Contains("precio")) colPrecio = col;
            else if (header.Contains("stock") || header.Contains("cant")) colStock = col;
        }

        // Guardar en memoria los productos actuales de la base de datos para buscarlos rápido
        var codigosExistentes = await _context.Productos.ToDictionaryAsync(p => p.Codigo, StringComparer.OrdinalIgnoreCase);

        for (int row = 2; row <= rowCount; row++)
        {
            var codigo = worksheet.Cells[row, colCodigo].Text.Trim();
            var nombre = worksheet.Cells[row, colNombre].Text.Trim();
            var desc = worksheet.Cells[row, colDesc].Text.Trim();
            var precioStr = worksheet.Cells[row, colPrecio].Text.Trim();
            var stockStr = worksheet.Cells[row, colStock].Text.Trim();

            // Saltar filas completamente vacías al final de la hoja
            if (string.IsNullOrWhiteSpace(codigo) && string.IsNullOrWhiteSpace(nombre))
            {
                continue;
            }

            totalFilas++;

            if (string.IsNullOrWhiteSpace(codigo))
            {
                errores.Add($"Fila {row}: El código del producto es obligatorio.");
                totalIgnorados++;
                continue;
            }

            if (string.IsNullOrWhiteSpace(nombre))
            {
                errores.Add($"Fila {row} ({codigo}): El nombre del producto es obligatorio.");
                totalIgnorados++;
                continue;
            }

            // Limpieza y conversión de precio numérico (soporta signo $ y decimales)
            precioStr = precioStr.Replace("$", "").Replace(" ", "");
            if (!decimal.TryParse(precioStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var precio) &&
                !decimal.TryParse(precioStr, NumberStyles.Any, new CultureInfo("es-CO"), out precio))
            {
                errores.Add($"Fila {row} ({codigo}): El precio '{precioStr}' no tiene un formato numérico válido.");
                totalIgnorados++;
                continue;
            }

            if (precio < 0)
            {
                errores.Add($"Fila {row} ({codigo}): El precio no puede ser negativo.");
                totalIgnorados++;
                continue;
            }

            if (!int.TryParse(stockStr, out var stock) || stock < 0)
            {
                errores.Add($"Fila {row} ({codigo}): El stock '{stockStr}' debe ser un número entero no negativo.");
                totalIgnorados++;
                continue;
            }

            var codigoKey = codigo.ToUpperInvariant();

            if (codigosExistentes.TryGetValue(codigoKey, out var productoExistente))
            {
                // Si el producto ya existe en la base de datos, se actualizan sus datos y stock
                productoExistente.Nombre = nombre;
                productoExistente.Descripcion = string.IsNullOrWhiteSpace(desc) ? productoExistente.Descripcion : desc;
                productoExistente.Precio = precio;
                productoExistente.Stock = stock;
                productoExistente.Activo = true;
                totalImportados++;
            }
            else
            {
                // Si es un producto nuevo, se crea y se agrega para guardarlo
                var nuevo = new Producto
                {
                    Codigo = codigoKey,
                    Nombre = nombre,
                    Descripcion = string.IsNullOrWhiteSpace(desc) ? null : desc,
                    Precio = precio,
                    Stock = stock,
                    Activo = true,
                    CreadoEn = DateTime.UtcNow
                };

                _context.Productos.Add(nuevo);
                codigosExistentes[codigoKey] = nuevo;
                totalImportados++;
            }
        }

        // Guardar todos los productos nuevos y actualizados en la base de datos
        await _context.SaveChangesAsync();

        return new ExcelImportResultDto(totalFilas, totalImportados, totalIgnorados, errores);
    }

    // =============================================================================================
    // 6. IMPORTACIÓN: CARGA COMBINADA (Productos y Clientes en Una Hoja)
    // =============================================================================================
    public async Task<DesnormalizedImportResultDto> ImportDesnormalizedDataAsync(Stream fileStream)
    {
        var errores = new List<string>();
        int totalFilas = 0;
        int prodsCreados = 0;
        int prodsActualizados = 0;
        int clientesCreados = 0;
        int clientesActualizados = 0;
        int totalOmitidos = 0;

        using var package = new ExcelPackage(fileStream);
        var worksheet = package.Workbook.Worksheets.FirstOrDefault();

        if (worksheet == null || worksheet.Dimension == null)
        {
            return new DesnormalizedImportResultDto(0, 0, 0, 0, 0, 0, new List<string> { "El archivo Excel está vacío o no contiene hojas válidas." });
        }

        int rowCount = worksheet.Dimension.Rows;

        // Detección de columnas según el título de cada una
        int colProdCodigo = -1, colProdNombre = -1, colProdDesc = -1, colProdPrecio = -1, colProdStock = -1;
        int colCliId = -1, colCliNombres = -1, colCliApellidos = -1, colCliEmail = -1, colCliTel = -1, colCliDir = -1, colCliEdad = -1;

        for (int col = 1; col <= worksheet.Dimension.Columns; col++)
        {
            var h = worksheet.Cells[1, col].Text.Trim().ToLowerInvariant();
            // Detección de producto
            if (h.Contains("codigo_producto") || h.Contains("código_producto") || h == "codigo" || h == "código") colProdCodigo = col;
            else if (h.Contains("nombre_producto") || h == "producto" || (h == "nombre" && colProdNombre == -1)) colProdNombre = col;
            else if (h.Contains("descrip")) colProdDesc = col;
            else if (h.Contains("precio")) colProdPrecio = col;
            else if (h.Contains("stock") || h == "cantidad") colProdStock = col;

            // Detección de cliente
            if (h.Contains("identificacion") || h.Contains("cedula") || h.Contains("cédula") || h.Contains("documento")) colCliId = col;
            else if (h.Contains("nombres_cliente") || h == "nombres" || h == "cliente") colCliNombres = col;
            else if (h.Contains("apellidos")) colCliApellidos = col;
            else if (h.Contains("email") || h.Contains("correo")) colCliEmail = col;
            else if (h.Contains("telefono") || h.Contains("teléfono") || h.Contains("celular")) colCliTel = col;
            else if (h.Contains("direccion") || h.Contains("dirección")) colCliDir = col;
            else if (h.Contains("edad")) colCliEdad = col;
        }

        // Cargar en memoria los productos y clientes existentes de la base de datos
        var productosExistentes = await _context.Productos.ToDictionaryAsync(p => p.Codigo, StringComparer.OrdinalIgnoreCase);
        var clientesPorIdentificacion = await _context.Clientes.ToDictionaryAsync(c => c.Identificacion, StringComparer.OrdinalIgnoreCase);
        var emailsClientes = new HashSet<string>(
            clientesPorIdentificacion.Values
                .Where(c => !string.IsNullOrWhiteSpace(c.Email))
                .Select(c => c.Email.Trim()),
            StringComparer.OrdinalIgnoreCase);

        for (int row = 2; row <= rowCount; row++)
        {
            string prodCodigo = colProdCodigo > 0 ? worksheet.Cells[row, colProdCodigo].Text.Trim() : "";
            string prodNombre = colProdNombre > 0 ? worksheet.Cells[row, colProdNombre].Text.Trim() : "";
            string prodDesc = colProdDesc > 0 ? worksheet.Cells[row, colProdDesc].Text.Trim() : "";
            string prodPrecioStr = colProdPrecio > 0 ? worksheet.Cells[row, colProdPrecio].Text.Trim() : "";
            string prodStockStr = colProdStock > 0 ? worksheet.Cells[row, colProdStock].Text.Trim() : "";

            string cliId = colCliId > 0 ? worksheet.Cells[row, colCliId].Text.Trim() : "";
            string cliNombres = colCliNombres > 0 ? worksheet.Cells[row, colCliNombres].Text.Trim() : "";
            string cliApellidos = colCliApellidos > 0 ? worksheet.Cells[row, colCliApellidos].Text.Trim() : "";
            string cliEmail = colCliEmail > 0 ? worksheet.Cells[row, colCliEmail].Text.Trim() : "";
            string cliTel = colCliTel > 0 ? worksheet.Cells[row, colCliTel].Text.Trim() : "";
            string cliDir = colCliDir > 0 ? worksheet.Cells[row, colCliDir].Text.Trim() : "";
            string cliEdadStr = colCliEdad > 0 ? worksheet.Cells[row, colCliEdad].Text.Trim() : "";

            if (string.IsNullOrWhiteSpace(prodCodigo) && string.IsNullOrWhiteSpace(cliId))
            {
                continue; // fila vacía
            }

            totalFilas++;
            bool rowHasError = false;

            // 1. Procesar Producto si contiene código o nombre
            if (!string.IsNullOrWhiteSpace(prodCodigo) || !string.IsNullOrWhiteSpace(prodNombre))
            {
                if (string.IsNullOrWhiteSpace(prodCodigo) || string.IsNullOrWhiteSpace(prodNombre))
                {
                    errores.Add($"Fila {row}: Producto incompleto (el código y nombre son obligatorios).");
                    rowHasError = true;
                }
                else
                {
                    prodPrecioStr = prodPrecioStr.Replace("$", "").Replace(" ", "");
                    if (!decimal.TryParse(prodPrecioStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var precio) &&
                        !decimal.TryParse(prodPrecioStr, NumberStyles.Any, new CultureInfo("es-CO"), out precio))
                    {
                        errores.Add($"Fila {row} (Prod {prodCodigo}): Precio '{prodPrecioStr}' inválido.");
                        rowHasError = true;
                    }
                    else if (precio < 0)
                    {
                        errores.Add($"Fila {row} (Prod {prodCodigo}): El precio no puede ser negativo.");
                        rowHasError = true;
                    }
                    else if (!int.TryParse(prodStockStr, out var stock) || stock < 0)
                    {
                        errores.Add($"Fila {row} (Prod {prodCodigo}): Stock '{prodStockStr}' debe ser entero >= 0.");
                        rowHasError = true;
                    }
                    else
                    {
                        var key = prodCodigo.ToUpperInvariant();
                        if (productosExistentes.TryGetValue(key, out var exist))
                        {
                            exist.Nombre = prodNombre;
                            exist.Descripcion = string.IsNullOrWhiteSpace(prodDesc) ? exist.Descripcion : prodDesc;
                            exist.Precio = precio;
                            exist.Stock = stock;
                            exist.Activo = true;
                            prodsActualizados++;
                        }
                        else
                        {
                            var nuevo = new Producto
                            {
                                Codigo = key,
                                Nombre = prodNombre,
                                Descripcion = string.IsNullOrWhiteSpace(prodDesc) ? null : prodDesc,
                                Precio = precio,
                                Stock = stock,
                                Activo = true,
                                CreadoEn = DateTime.UtcNow
                            };
                            _context.Productos.Add(nuevo);
                            productosExistentes[key] = nuevo;
                            prodsCreados++;
                        }
                    }
                }
            }

            // 2. Procesar Cliente si contiene identificación o nombres
            if (!string.IsNullOrWhiteSpace(cliId) || !string.IsNullOrWhiteSpace(cliNombres))
            {
                if (string.IsNullOrWhiteSpace(cliId) || string.IsNullOrWhiteSpace(cliNombres))
                {
                    errores.Add($"Fila {row}: Cliente incompleto (la identificación y nombres son obligatorios).");
                    rowHasError = true;
                }
                else
                {
                    int? edad = null;
                    if (!string.IsNullOrWhiteSpace(cliEdadStr))
                    {
                        if (int.TryParse(cliEdadStr, out var e) && e >= 0)
                        {
                            edad = e;
                        }
                        else
                        {
                            errores.Add($"Fila {row} (Cliente {cliId}): Edad '{cliEdadStr}' inválida (debe ser número entero >= 0).");
                            rowHasError = true;
                        }
                    }

                    if (!rowHasError)
                    {
                        var key = cliId.Trim();
                        var candidateEmail = string.IsNullOrWhiteSpace(cliEmail) ? $"cliente_{key}@firmeza.com" : cliEmail.Trim();

                        if (clientesPorIdentificacion.TryGetValue(key, out var existCli))
                        {
                            if (!string.IsNullOrWhiteSpace(cliEmail) &&
                                !string.Equals(existCli.Email, candidateEmail, StringComparison.OrdinalIgnoreCase) &&
                                emailsClientes.Contains(candidateEmail))
                            {
                                errores.Add($"Fila {row} (Cliente {cliId}): El email '{candidateEmail}' ya pertenece a otro cliente.");
                                rowHasError = true;
                            }
                            else
                            {
                                emailsClientes.Remove(existCli.Email);
                                existCli.Nombres = cliNombres.Trim();
                                existCli.Apellidos = string.IsNullOrWhiteSpace(cliApellidos) ? existCli.Apellidos : cliApellidos.Trim();
                                existCli.Email = string.IsNullOrWhiteSpace(cliEmail) ? existCli.Email : candidateEmail;
                                existCli.Telefono = string.IsNullOrWhiteSpace(cliTel) ? existCli.Telefono : cliTel.Trim();
                                existCli.Direccion = string.IsNullOrWhiteSpace(cliDir) ? existCli.Direccion : cliDir.Trim();
                                if (edad.HasValue) existCli.Edad = edad.Value;
                                existCli.Activo = true;
                                emailsClientes.Add(existCli.Email);
                                clientesActualizados++;
                            }
                        }
                        else
                        {
                            if (emailsClientes.Contains(candidateEmail))
                            {
                                errores.Add($"Fila {row} (Cliente {cliId}): El email '{candidateEmail}' ya se encuentra registrado en otro cliente.");
                                rowHasError = true;
                            }
                            else
                            {
                                var nuevoCli = new Cliente
                                {
                                    Identificacion = key,
                                    Nombres = cliNombres.Trim(),
                                    Apellidos = string.IsNullOrWhiteSpace(cliApellidos) ? "Consumidor Final" : cliApellidos.Trim(),
                                    Email = candidateEmail,
                                    Telefono = string.IsNullOrWhiteSpace(cliTel) ? null : cliTel.Trim(),
                                    Direccion = string.IsNullOrWhiteSpace(cliDir) ? null : cliDir.Trim(),
                                    Edad = edad,
                                    Activo = true,
                                    CreadoEn = DateTime.UtcNow
                                };
                                _context.Clientes.Add(nuevoCli);
                                clientesPorIdentificacion[key] = nuevoCli;
                                emailsClientes.Add(candidateEmail);
                                clientesCreados++;
                            }
                        }
                    }
                }
            }

            if (rowHasError)
            {
                totalOmitidos++;
            }
        }

        await _context.SaveChangesAsync();
        return new DesnormalizedImportResultDto(totalFilas, prodsCreados, prodsActualizados, clientesCreados, clientesActualizados, totalOmitidos, errores);
    }
}
