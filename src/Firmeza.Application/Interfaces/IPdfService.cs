// Archivo: IPdfService.cs
// Qué es: Plantilla para el servicio que genera documentos en formato PDF.
// Para qué sirve: Define la función que arma el documento de la factura de una venta listo para descargar, imprimir o enviar por correo.

namespace Firmeza.Application.Interfaces;

// Define las operaciones para crear documentos PDF dentro del sistema.
public interface IPdfService
{
    // Genera el archivo PDF de la factura de una venta con los datos del cliente, productos comprados e impuestos.
    Task<byte[]> GenerateSaleInvoicePdfAsync(int saleId);
}
