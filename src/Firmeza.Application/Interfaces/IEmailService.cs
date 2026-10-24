// Archivo: IEmailService.cs
// Qué es: Plantilla para el servicio de envío de correos electrónicos.
// Para qué sirve: Define las funciones para mandar mensajes por correo a los clientes, como bienvenidas al registrarse o facturas en PDF tras una compra.

namespace Firmeza.Application.Interfaces;

// Define las acciones disponibles para enviar correos electrónicos desde el sistema.
public interface IEmailService
{
    // Envía un correo electrónico con diseño visual y permite adjuntar un archivo si hace falta.
    Task<bool> SendEmailAsync(string toEmail, string subject, string htmlBody, byte[]? attachmentBytes = null, string? attachmentFileName = null);

    // Envía un correo de bienvenida cuando un usuario nuevo crea su cuenta en la plataforma.
    Task<bool> SendWelcomeEmailAsync(string toEmail, string username);

    // Envía al cliente un correo confirmando su compra junto con el archivo PDF de su factura adjunto.
    Task<bool> SendInvoiceEmailAsync(string toEmail, string clientName, string saleNumber, decimal total, byte[] invoicePdfBytes);
}
