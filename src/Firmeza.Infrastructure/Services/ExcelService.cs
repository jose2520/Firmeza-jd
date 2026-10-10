// =================================================================================================
// SERVICIO: ExcelService.cs
// CAPA: Firmeza.Infrastructure (Capa de Infraestructura y Persistencia)
// CONTRATO: IExcelService (Firmeza.Application)
// PROPÓSITO:
//   Gestiona la integración con hojas de cálculo Microsoft Excel (.xlsx) usando EPPlus 8:
//   - Carga masiva e importación de productos desde un flujo de bytes (Stream).
//   - Exportación estilizada del catálogo de productos.
//   - Exportación del historial financiero de ventas.
//   - Generación de la plantilla base descargable para el usuario.
//
// ORIGEN Y DESTINO DE LOS DATOS:
//   - Importación: Lee el flujo de bytes (Stream) proveniente de la solicitud HTTP multipart,
//     analiza las celdas en memoria e inserta/actualiza en PostgreSQL (tabla 'productos').
//   - Exportaciones: Consulta PostgreSQL (tablas 'productos' o 'ventas'), renderiza el libro .xlsx
//     en memoria RAM y retorna los bytes directamente sin escribir archivos basura en el disco del servidor.
//   - Plantilla: Generada 100% en memoria mediante código C#.
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

    public ExcelService(ApplicationDbContext context)
    {
        _context = context;
        // Configura la licencia no comercial para EPPlus 8
        ExcelPackage.License.SetNonCommercialPersonal("Firmeza");
    }

    /// <summary>
    /// Lee un archivo Excel (.xlsx) desde un Stream y carga/actualiza productos por lote.
    /// ORIGEN DE DATOS: Stream binario recibido vía HTTP -> Procesado en memoria -> Persistido en PostgreSQL ('productos').
    /// </summary>
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

        // Detección dinámica de columnas según los encabezados en la fila 1
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

        // Cachear en memoria los productos existentes en PostgreSQL para evitar consultas repetitivas N+1
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

            // Normalización y conversión de precio numérico (soporta formato $ y decimales)
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
                // Si el producto ya existe en PostgreSQL, se actualizan sus datos y stock
                productoExistente.Nombre = nombre;
                productoExistente.Descripcion = string.IsNullOrWhiteSpace(desc) ? productoExistente.Descripcion : desc;
                productoExistente.Precio = precio;
                productoExistente.Stock = stock;
                productoExistente.Activo = true;
                totalImportados++;
            }
            else
            {
                // Si es un producto nuevo, se instancia y se agrega al contexto de EF Core
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

        // Persistir todas las inserciones y modificaciones en una sola transacción en PostgreSQL
        await _context.SaveChangesAsync();

        return new ExcelImportResultDto(totalFilas, totalImportados, totalIgnorados, errores);
    }

    /// <summary>
    /// Consulta los productos en PostgreSQL y genera un archivo Excel (.xlsx) con diseño profesional.
    /// ORIGEN DE DATOS: Lectura de la tabla 'productos' en PostgreSQL -> Renderizado a byte[] con EPPlus.
    /// </summary>
    public async Task<byte[]> ExportProductsToExcelAsync()
    {
        var productos = await _context.Productos
            .AsNoTracking()
            .OrderBy(p => p.Id)
            .ToListAsync();

        using var package = new ExcelPackage();
        var ws = package.Workbook.Worksheets.Add("Catálogo de Productos");

        // 1. Título corporativo del reporte
        ws.Cells["A1:H1"].Merge = true;
        ws.Cells["A1"].Value = "PLATAFORMA FIRMEZA - CATÁLOGO DE PRODUCTOS";
        ws.Cells["A1"].Style.Font.Size = 16;
        ws.Cells["A1"].Style.Font.Bold = true;
        ws.Cells["A1"].Style.Font.Color.SetColor(Color.White);
        ws.Cells["A1"].Style.Fill.PatternType = ExcelFillStyle.Solid;
        ws.Cells["A1"].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(30, 58, 138)); // Azul marino
        ws.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        ws.Row(1).Height = 35;

        // 2. Encabezados de columnas
        string[] headers = { "ID", "Código", "Nombre", "Descripción", "Precio (COP)", "Stock", "Estado", "Fecha Registro" };
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = ws.Cells[3, i + 1];
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
            cell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(226, 232, 240));
            cell.Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.FromArgb(203, 213, 225));
        }

        // 3. Filas con los datos extraídos de la base de datos
        int row = 4;
        foreach (var p in productos)
        {
            ws.Cells[row, 1].Value = p.Id;
            ws.Cells[row, 2].Value = p.Codigo;
            ws.Cells[row, 3].Value = p.Nombre;
            ws.Cells[row, 4].Value = p.Descripcion ?? "-";
            ws.Cells[row, 5].Value = p.Precio;
            ws.Cells[row, 5].Style.Numberformat.Format = "$#,##0.00";
            ws.Cells[row, 6].Value = p.Stock;
            ws.Cells[row, 7].Value = p.Activo ? "Activo" : "Inactivo";
            ws.Cells[row, 8].Value = p.CreadoEn.ToString("yyyy-MM-dd HH:mm");

            for (int col = 1; col <= 8; col++)
            {
                ws.Cells[row, col].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.FromArgb(241, 245, 249));
            }

            row++;
        }

        // Autoajustar ancho de columnas al contenido
        ws.Cells[3, 1, Math.Max(3, row - 1), 8].AutoFitColumns();

        return await package.GetAsByteArrayAsync();
    }

    /// <summary>
    /// Consulta el histórico de ventas en PostgreSQL y genera un reporte tabular en Excel (.xlsx).
    /// ORIGEN DE DATOS: Tablas 'ventas' y 'clientes' en PostgreSQL -> Renderizado a byte[] con EPPlus.
    /// </summary>
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
        var ws = package.Workbook.Worksheets.Add("Ventas Registradas");

        // 1. Título del reporte
        ws.Cells["A1:H1"].Merge = true;
        ws.Cells["A1"].Value = "PLATAFORMA FIRMEZA - HISTÓRICO DE VENTAS";
        ws.Cells["A1"].Style.Font.Size = 16;
        ws.Cells["A1"].Style.Font.Bold = true;
        ws.Cells["A1"].Style.Font.Color.SetColor(Color.White);
        ws.Cells["A1"].Style.Fill.PatternType = ExcelFillStyle.Solid;
        ws.Cells["A1"].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(15, 118, 110)); // Verde azulado
        ws.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        ws.Row(1).Height = 35;

        // 2. Encabezados
        string[] headers = { "ID", "Nº Venta", "Fecha", "Cliente", "Identificación", "Subtotal", "IVA (19%)", "Total (COP)", "Estado" };
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = ws.Cells[3, i + 1];
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
            cell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(204, 251, 241));
            cell.Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.FromArgb(153, 246, 228));
        }

        // 3. Registros de ventas
        int row = 4;
        foreach (var v in ventas)
        {
            ws.Cells[row, 1].Value = v.Id;
            ws.Cells[row, 2].Value = v.NumeroVenta;
            ws.Cells[row, 3].Value = v.Fecha.ToString("yyyy-MM-dd HH:mm");
            ws.Cells[row, 4].Value = $"{v.Cliente.Nombres} {v.Cliente.Apellidos}".Trim();
            ws.Cells[row, 5].Value = v.Cliente.Identificacion;
            ws.Cells[row, 6].Value = v.Subtotal;
            ws.Cells[row, 6].Style.Numberformat.Format = "$#,##0.00";
            ws.Cells[row, 7].Value = v.Iva;
            ws.Cells[row, 7].Style.Numberformat.Format = "$#,##0.00";
            ws.Cells[row, 8].Value = v.Total;
            ws.Cells[row, 8].Style.Numberformat.Format = "$#,##0.00";
            ws.Cells[row, 9].Value = v.Estado;

            for (int col = 1; col <= 9; col++)
            {
                ws.Cells[row, col].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.FromArgb(241, 245, 249));
            }

            row++;
        }

        ws.Cells[3, 1, Math.Max(3, row - 1), 9].AutoFitColumns();

        return await package.GetAsByteArrayAsync();
    }

    /// <summary>
    /// Genera la plantilla oficial descargable en Excel con columnas y ejemplos guiados.
    /// ORIGEN DE DATOS: 100% en memoria RAM (no consulta base de datos ni archivos físicos).
    /// </summary>
    public byte[] GenerateProductsTemplate()
    {
        using var package = new ExcelPackage();
        var ws = package.Workbook.Worksheets.Add("Plantilla Carga Productos");

        string[] headers = { "Codigo", "Nombre", "Descripcion", "Precio", "Stock" };
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = ws.Cells[1, i + 1];
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
            cell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(226, 232, 240));
        }

        // Filas de ejemplo para orientar al usuario sobre los formatos admitidos
        ws.Cells[2, 1].Value = "PROD-100";
        ws.Cells[2, 2].Value = "Pala Redonda con Mango de Madera";
        ws.Cells[2, 3].Value = "Herramienta agrícola y de construcción forjada";
        ws.Cells[2, 4].Value = 48500.00;
        ws.Cells[2, 5].Value = 40;

        ws.Cells[3, 1].Value = "PROD-101";
        ws.Cells[3, 2].Value = "Disco de Corte Metal 4.5 pulg";
        ws.Cells[3, 3].Value = "Disco abrasivo para esmeriladora angular";
        ws.Cells[3, 4].Value = 6200.00;
        ws.Cells[3, 5].Value = 100;

        ws.Cells[1, 1, 3, 5].AutoFitColumns();

        return package.GetAsByteArray();
    }

    /// <summary>
    /// Genera la plantilla de Excel para carga desnormalizada de clientes y productos en una sola hoja.
    /// </summary>
    public byte[] GenerateDesnormalizedTemplate()
    {
        using var package = new ExcelPackage();
        var ws = package.Workbook.Worksheets.Add("Carga Desnormalizada");

        string[] headers = {
            "Codigo_Producto", "Nombre_Producto", "Descripcion_Producto", "Precio", "Stock",
            "Identificacion_Cliente", "Nombres_Cliente", "Apellidos_Cliente", "Email_Cliente", "Telefono_Cliente", "Direccion_Cliente", "Edad_Cliente"
        };

        for (int i = 0; i < headers.Length; i++)
        {
            var cell = ws.Cells[1, i + 1];
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
            cell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(203, 213, 225));
        }

        // Fila 1 de ejemplo
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

        ws.Cells[1, 1, 3, headers.Length].AutoFitColumns();
        return package.GetAsByteArray();
    }

    /// <summary>
    /// Importa un archivo Excel desnormalizado (productos y clientes combinados),
    /// normaliza los datos en memoria e inserta o actualiza en PostgreSQL.
    /// </summary>
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

        // Detección dinámica de columnas
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

        // Cache de entidades existentes en PostgreSQL para evitar queries N+1
        var productosExistentes = await _context.Productos.ToDictionaryAsync(p => p.Codigo, StringComparer.OrdinalIgnoreCase);
        var clientesPorIdentificacion = await _context.Clientes.ToDictionaryAsync(c => c.Identificacion, StringComparer.OrdinalIgnoreCase);

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
                        if (clientesPorIdentificacion.TryGetValue(key, out var existCli))
                        {
                            existCli.Nombres = cliNombres.Trim();
                            existCli.Apellidos = string.IsNullOrWhiteSpace(cliApellidos) ? existCli.Apellidos : cliApellidos.Trim();
                            existCli.Email = string.IsNullOrWhiteSpace(cliEmail) ? existCli.Email : cliEmail.Trim();
                            existCli.Telefono = string.IsNullOrWhiteSpace(cliTel) ? existCli.Telefono : cliTel.Trim();
                            existCli.Direccion = string.IsNullOrWhiteSpace(cliDir) ? existCli.Direccion : cliDir.Trim();
                            if (edad.HasValue) existCli.Edad = edad.Value;
                            existCli.Activo = true;
                            clientesActualizados++;
                        }
                        else
                        {
                            var nuevoCli = new Cliente
                            {
                                Identificacion = key,
                                Nombres = cliNombres.Trim(),
                                Apellidos = string.IsNullOrWhiteSpace(cliApellidos) ? "Consumidor Final" : cliApellidos.Trim(),
                                Email = string.IsNullOrWhiteSpace(cliEmail) ? $"cliente_{key}@firmeza.com" : cliEmail.Trim(),
                                Telefono = string.IsNullOrWhiteSpace(cliTel) ? null : cliTel.Trim(),
                                Direccion = string.IsNullOrWhiteSpace(cliDir) ? null : cliDir.Trim(),
                                Edad = edad,
                                Activo = true,
                                CreadoEn = DateTime.UtcNow
                            };
                            _context.Clientes.Add(nuevoCli);
                            clientesPorIdentificacion[key] = nuevoCli;
                            clientesCreados++;
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
