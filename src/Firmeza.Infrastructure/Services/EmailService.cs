// Servicio encargado de enviar correos electrónicos a los clientes de Firmeza.
// Lee la configuración del servidor de correo desde el archivo appsettings.json y recibe archivos PDF generados por PdfService.cs.
// Permite enviar correos de bienvenida a nuevos usuarios y comprobantes de compra con la factura adjunta.
using System.Globalization;
using System.Net;
using System.Net.Mail;
using Firmeza.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Firmeza.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    // Inicializa el servicio de correo con los ajustes del sistema y el registro de mensajes.
    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    // Envía un correo electrónico con diseño HTML y permite adjuntar un archivo PDF si se necesita.
    public async Task<bool> SendEmailAsync(
        string toEmail,
        string subject,
        string htmlBody,
        byte[]? attachmentBytes = null,
        string? attachmentFileName = null)
    {
        if (string.IsNullOrWhiteSpace(toEmail))
        {
            _logger.LogWarning("No se especificó destinatario para el correo: {Subject}", subject);
            return false;
        }

        var smtpSection = _configuration.GetSection("Smtp");
        var host = smtpSection["Host"] ?? "smtp.gmail.com";
        var port = int.TryParse(smtpSection["Port"], out var p) ? p : 587;
        var enableSsl = bool.TryParse(smtpSection["EnableSsl"], out var ssl) ? ssl : true;
        var userName = smtpSection["UserName"] ?? string.Empty;
        var password = smtpSection["Password"] ?? string.Empty;
        var fromEmail = smtpSection["FromEmail"] ?? (string.IsNullOrWhiteSpace(userName) ? "notificaciones@firmeza.com" : userName);
        var fromName = smtpSection["FromName"] ?? "Firmeza - Materiales de Construcción";

        // Si no hay credenciales configuradas en desarrollo, simulamos el envío y advertimos en consola
        if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password))
        {
            _logger.LogInformation("SMTP Simulado (Sin credenciales en appsettings): Correo '{Subject}' simulado para '{ToEmail}'", subject, toEmail);
            return true;
        }

        MemoryStream? attachmentStream = null;
        try
        {
            using var mailMessage = new MailMessage();
            mailMessage.From = new MailAddress(fromEmail, fromName);
            mailMessage.Subject = subject;
            mailMessage.Body = htmlBody;
            mailMessage.IsBodyHtml = true;

            mailMessage.To.Add(toEmail);

            if (attachmentBytes != null && attachmentBytes.Length > 0 && !string.IsNullOrWhiteSpace(attachmentFileName))
            {
                attachmentStream = new MemoryStream(attachmentBytes);
                var attachment = new Attachment(attachmentStream, attachmentFileName, "application/pdf");
                mailMessage.Attachments.Add(attachment);
            }

            using var smtpClient = new SmtpClient(host, port);
            smtpClient.EnableSsl = enableSsl;
            smtpClient.Credentials = new NetworkCredential(userName, password);
            smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtpClient.Timeout = 15000; // 15 segundos de espera máxima

            await smtpClient.SendMailAsync(mailMessage);
            _logger.LogInformation("Correo enviado exitosamente a {ToEmail}: {Subject}", toEmail, subject);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar correo SMTP a {ToEmail} para {Subject}: {Message}", toEmail, subject, ex.Message);
            // Si falla el envío, devolvemos falso sin detener la aplicación
            return false;
        }
        finally
        {
            attachmentStream?.Dispose();
        }
    }

    // Arma y envía un correo de bienvenida cuando un cliente nuevo se registra en la plataforma.
    public async Task<bool> SendWelcomeEmailAsync(string toEmail, string username)
    {
        var safeUsername = WebUtility.HtmlEncode(username);
        var subject = "¡Bienvenido a Plataforma Firmeza!";
        var html = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: 'Segoe UI', Arial, sans-serif; background-color: #f8fafc; color: #1e293b; padding: 20px; }}
        .container {{ max-width: 600px; margin: 0 auto; background: #ffffff; border-radius: 8px; overflow: hidden; border: 1px solid #e2e8f0; }}
        .header {{ background-color: #1e3a8a; color: #ffffff; padding: 30px; text-align: center; }}
        .content {{ padding: 30px; line-height: 1.6; }}
        .footer {{ background-color: #f1f5f9; text-align: center; padding: 15px; font-size: 12px; color: #64748b; }}
        .btn {{ display: inline-block; background-color: #0f766e; color: #ffffff; padding: 12px 24px; border-radius: 6px; text-decoration: none; font-weight: bold; margin-top: 20px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>FIRMEZA S.A.S.</h1>
            <p>Materiales para Construcción y Ferretería Industrial</p>
        </div>
        <div class='content'>
            <h2>¡Hola, {safeUsername}!</h2>
            <p>Tu cuenta ha sido creada exitosamente en la plataforma digital de <strong>Firmeza</strong>.</p>
            <p>A partir de ahora podrás explorar nuestro catálogo completo de materiales, cotizar en línea y realizar pedidos directos con total seguridad y rapidez.</p>
            <p style='margin-top: 25px;'>
                Si tienes preguntas sobre tus pedidos o necesitas asistencia técnica, puedes responder a este correo o contactar a nuestra línea de atención comercial.
            </p>
        </div>
        <div class='footer'>
            <p>© {DateTime.UtcNow.Year} Firmeza S.A.S. - Calle 100 # 15-20, Bogotá D.C.</p>
            <p>Este correo fue generado automáticamente por el sistema.</p>
        </div>
    </div>
</body>
</html>";

        return await SendEmailAsync(toEmail, subject, html);
    }

    // Arma y envía al cliente el correo de confirmación de su compra junto con el archivo PDF de su factura.
    public async Task<bool> SendInvoiceEmailAsync(
        string toEmail,
        string clientName,
        string saleNumber,
        decimal total,
        byte[] invoicePdfBytes)
    {
        var culture = new CultureInfo("es-CO");
        var formattedTotal = total.ToString("C2", culture);
        var safeClientName = WebUtility.HtmlEncode(clientName);
        var safeSaleNumber = WebUtility.HtmlEncode(saleNumber);
        var subject = $"Comprobante de Compra y Factura - {saleNumber} | Firmeza";
        var fileName = $"Factura_{saleNumber}.pdf";

        var html = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: 'Segoe UI', Arial, sans-serif; background-color: #f8fafc; color: #1e293b; padding: 20px; }}
        .container {{ max-width: 600px; margin: 0 auto; background: #ffffff; border-radius: 8px; overflow: hidden; border: 1px solid #e2e8f0; }}
        .header {{ background-color: #1e3a8a; color: #ffffff; padding: 25px; text-align: center; }}
        .content {{ padding: 30px; line-height: 1.6; }}
        .card {{ background-color: #f8fafc; border: 1px solid #cbd5e1; border-radius: 6px; padding: 20px; margin: 20px 0; }}
        .footer {{ background-color: #f1f5f9; text-align: center; padding: 15px; font-size: 12px; color: #64748b; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>FIRMEZA S.A.S.</h1>
            <p>Confirmación de Orden y Facturación Electrónica</p>
        </div>
        <div class='content'>
            <h2>Estimado/a {safeClientName},</h2>
            <p>¡Gracias por tu compra! Tu pedido ha sido procesado y registrado exitosamente en nuestro sistema.</p>
            <div class='card'>
                <p><strong>Número de Venta:</strong> {safeSaleNumber}</p>
                <p><strong>Fecha:</strong> {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC</p>
                <p><strong>Total Pagado (IVA 19% Incluido):</strong> <span style='font-size: 18px; color: #0f766e; font-weight: bold;'>{formattedTotal}</span></p>
            </div>
            <p>Adjunto a este correo encontrarás tu factura comercial oficial en formato <strong>PDF</strong> con el detalle completo de tus productos adquiridos e impuestos calculados.</p>
        </div>
        <div class='footer'>
            <p>© {DateTime.UtcNow.Year} Firmeza S.A.S. - Calle 100 # 15-20, Bogotá D.C.</p>
            <p>Facturación Electrónica Oficial y Garantía Directa de Fábrica.</p>
        </div>
    </div>
</body>
</html>";

        return await SendEmailAsync(toEmail, subject, html, invoicePdfBytes, fileName);
    }
}
