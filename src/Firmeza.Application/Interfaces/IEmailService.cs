// =================================================================================================
// INTERFAZ: IEmailService.cs
// CAPA: Firmeza.Application (Interfaces y Casos de Uso)
// PROPÓSITO:
//   Contrato para el servicio de envío de correos electrónicos transaccionales del negocio
//   (confirmaciones de compra con factura en PDF y bienvenida para nuevos clientes).
// =================================================================================================

namespace Firmeza.Application.Interfaces;

public interface IEmailService
{
    /// <summary>
    /// Envía un correo electrónico general en formato HTML con adjuntos opcionales (ej. PDF).
    /// </summary>
    Task<bool> SendEmailAsync(string toEmail, string subject, string htmlBody, byte[]? attachmentBytes = null, string? attachmentFileName = null);

    /// <summary>
    /// Envía un correo corporativo de bienvenida a un usuario registrado.
    /// </summary>
    Task<bool> SendWelcomeEmailAsync(string toEmail, string username);

    /// <summary>
    /// Envía la confirmación de compra y la factura comercial PDF adjunta al cliente.
    /// </summary>
    Task<bool> SendInvoiceEmailAsync(string toEmail, string clientName, string saleNumber, decimal total, byte[] invoicePdfBytes);
}
