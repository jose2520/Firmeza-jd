// Pruebas para verificar el funcionamiento del servicio de envío de correos electrónicos.
// Comprueba el envío de mensajes generales, correos de bienvenida a nuevos clientes,
// envío de facturas con archivo PDF adjunto y el rechazo de destinatarios vacíos.

using Firmeza.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace Firmeza.Tests;

public class EmailServiceTests
{
    private readonly IConfiguration _configuration;
    private readonly Mock<ILogger<EmailService>> _loggerMock;

    // Configura los datos del servidor de correo en modo de prueba sin usar contraseñas reales.
    public EmailServiceTests()
    {
        var inMemorySettings = new Dictionary<string, string?>
        {
            {"Smtp:Host", "smtp.gmail.com"},
            {"Smtp:Port", "587"},
            {"Smtp:EnableSsl", "true"},
            {"Smtp:UserName", ""}, // Sin credenciales reales para simular el envío durante las pruebas
            {"Smtp:Password", ""},
            {"Smtp:FromEmail", "notificaciones@firmeza.com"},
            {"Smtp:FromName", "Firmeza S.A.S."}
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        _loggerMock = new Mock<ILogger<EmailService>>();
    }

    // Verifica que cuando no hay contraseña configurada, el servicio simule el envío del correo y responda con éxito.
    [Fact]
    public async Task SendEmailAsync_WhenNoCredentials_ShouldSimulateAndReturnTrue()
    {
        // Preparar el servicio de correo
        var service = new EmailService(_configuration, _loggerMock.Object);

        // Enviar un correo de prueba
        var result = await service.SendEmailAsync("cliente@example.com", "Asunto de prueba", "<p>Hola</p>");

        // Comprobar que el resultado fue exitoso
        result.Should().BeTrue();
    }

    // Comprueba que el correo de bienvenida para un usuario recién registrado se procese sin errores.
    [Fact]
    public async Task SendWelcomeEmailAsync_ShouldSucceed()
    {
        // Preparar el servicio de correo
        var service = new EmailService(_configuration, _loggerMock.Object);

        // Enviar correo de bienvenida
        var result = await service.SendWelcomeEmailAsync("nuevo.usuario@example.com", "CarlosG");

        // Comprobar que el envío fue exitoso
        result.Should().BeTrue();
    }

    // Verifica que el envío del correo con la factura de compra y su archivo PDF adjunto funcione correctamente.
    [Fact]
    public async Task SendInvoiceEmailAsync_WithPdfAttachment_ShouldSucceed()
    {
        // Preparar el servicio y un archivo PDF de ejemplo
        var service = new EmailService(_configuration, _loggerMock.Object);
        var dummyPdfBytes = new byte[] { 0x25, 0x50, 0x44, 0x46 }; // %PDF

        // Enviar la factura por correo
        var result = await service.SendInvoiceEmailAsync("comprador@example.com", "Carlos Gómez", "VENTA-2026-0001", 119000m, dummyPdfBytes);

        // Comprobar que el resultado fue exitoso
        result.Should().BeTrue();
    }

    // Comprueba que si no se indica la dirección de correo del destinatario, el servicio devuelva falso sin fallar.
    [Fact]
    public async Task SendEmailAsync_WhenRecipientIsEmpty_ShouldReturnFalse()
    {
        // Preparar el servicio de correo
        var service = new EmailService(_configuration, _loggerMock.Object);

        // Intentar enviar un correo con el destinatario vacío
        var result = await service.SendEmailAsync("", "Asunto", "<p>Contenido</p>");

        // Comprobar que indique que no se pudo enviar
        result.Should().BeFalse();
    }
}
