namespace Firmeza.Application.Interfaces;

/// <summary>
/// Contrato de servicio para la generación de documentos oficiales y facturas en formato PDF.
/// 
/// PARA QUÉ SIRVE:
///   Permite generar en tiempo de ejecución facturas comerciales de venta en PDF con diseño empresarial,
///   tablas estructuradas, desglose de impuestos y datos de cliente, listas para descarga o impresión.
/// 
/// ORIGEN DE LOS DATOS:
///   Consulta las tablas 'ventas', 'clientes', 'detalles_venta' y 'productos' en PostgreSQL ('firmeza_db'),
///   y renderiza los datos en memoria utilizando la biblioteca iText 7/9.
/// </summary>
public interface IPdfService
{
    /// <summary>
    /// Genera la factura comercial de una venta en formato binario PDF.
    /// Origen de datos: Registro de la venta (id: @saleId) en PostgreSQL -> Flujo MemoryStream con iText.
    /// </summary>
    /// <param name="saleId">ID de la venta a facturar.</param>
    /// <returns>Arreglo de bytes del documento PDF generado (application/pdf).</returns>
    Task<byte[]> GenerateSaleInvoicePdfAsync(int saleId);
}
