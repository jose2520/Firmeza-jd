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

    public EmailServiceTests()
    {
        var inMemorySettings = new Dictionary<string, string?>
        {
            {"Smtp:Host", "smtp.gmail.com"},
            {"Smtp:Port", "587"},
            {"Smtp:EnableSsl", "true"},
            {"Smtp:UserName", ""}, // Simulado sin credenciales para pruebas unitarias
            {"Smtp:Password", ""},
            {"Smtp:FromEmail", "notificaciones@firmeza.com"},
            {"Smtp:FromName", "Firmeza S.A.S."}
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        _loggerMock = new Mock<ILogger<EmailService>>();
    }

    [Fact]
    public async Task SendEmailAsync_WhenNoCredentials_ShouldSimulateAndReturnTrue()
    {
        // Arrange
        var service = new EmailService(_configuration, _loggerMock.Object);

        // Act
        var result = await service.SendEmailAsync("cliente@example.com", "Asunto de prueba", "<p>Hola</p>");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task SendWelcomeEmailAsync_ShouldSucceed()
    {
        // Arrange
        var service = new EmailService(_configuration, _loggerMock.Object);

        // Act
        var result = await service.SendWelcomeEmailAsync("nuevo.usuario@example.com", "CarlosG");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task SendInvoiceEmailAsync_WithPdfAttachment_ShouldSucceed()
    {
        // Arrange
        var service = new EmailService(_configuration, _loggerMock.Object);
        var dummyPdfBytes = new byte[] { 0x25, 0x50, 0x44, 0x46 }; // %PDF

        // Act
        var result = await service.SendInvoiceEmailAsync("comprador@example.com", "Carlos Gómez", "VENTA-2026-0001", 119000m, dummyPdfBytes);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task SendEmailAsync_WhenRecipientIsEmpty_ShouldReturnFalse()
    {
        // Arrange
        var service = new EmailService(_configuration, _loggerMock.Object);

        // Act
        var result = await service.SendEmailAsync("", "Asunto", "<p>Contenido</p>");

        // Assert
        result.Should().BeFalse();
    }
}
