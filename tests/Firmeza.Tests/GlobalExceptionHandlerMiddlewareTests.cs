using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Firmeza.API.Middleware;
using Firmeza.Application.Exceptions;
using Firmeza.Domain.Exceptions;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Firmeza.Tests;

public class GlobalExceptionHandlerMiddlewareTests
{
    private readonly Mock<ILogger<GlobalExceptionHandlerMiddleware>> _loggerMock = new();

    private (DefaultHttpContext context, MemoryStream responseBody) CreateHttpContext()
    {
        var context = new DefaultHttpContext();
        var responseBody = new MemoryStream();
        context.Response.Body = responseBody;
        return (context, responseBody);
    }

    [Fact]
    public async Task InvokeAsync_WhenNotFoundException_ShouldReturn404()
    {
        // Arrange
        var middleware = new GlobalExceptionHandlerMiddleware(
            _ => throw new NotFoundException("Cliente", 123),
            _loggerMock.Object);

        var (context, stream) = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(404);
        stream.Seek(0, SeekOrigin.Begin);
        var json = await new StreamReader(stream).ReadToEndAsync();
        json.Should().Contain("Recurso no encontrado");
        json.Should().Contain("123");
    }

    [Fact]
    public async Task InvokeAsync_WhenConflictException_ShouldReturn409()
    {
        // Arrange
        var middleware = new GlobalExceptionHandlerMiddleware(
            _ => throw new ConflictException("Cliente", "Email", "test@firmeza.com"),
            _loggerMock.Object);

        var (context, stream) = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(409);
        stream.Seek(0, SeekOrigin.Begin);
        var json = await new StreamReader(stream).ReadToEndAsync();
        json.Should().Contain("Conflicto de integridad de datos");
    }

    [Fact]
    public async Task InvokeAsync_WhenStockInsuficienteException_ShouldReturn422()
    {
        // Arrange
        var middleware = new GlobalExceptionHandlerMiddleware(
            _ => throw new StockInsuficienteException("PROD-001", 5, 20),
            _loggerMock.Object);

        var (context, stream) = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(422);
        stream.Seek(0, SeekOrigin.Begin);
        var json = await new StreamReader(stream).ReadToEndAsync();
        json.Should().Contain("Stock insuficiente en bodega");
        json.Should().Contain("PROD-001");
    }

    [Fact]
    public async Task InvokeAsync_WhenValidationException_ShouldReturn400()
    {
        // Arrange
        var middleware = new GlobalExceptionHandlerMiddleware(
            _ => throw new ValidationException("Precio", "No puede ser cero"),
            _loggerMock.Object);

        var (context, stream) = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(400);
        stream.Seek(0, SeekOrigin.Begin);
        var json = await new StreamReader(stream).ReadToEndAsync();
        json.Should().Contain("Error de validación");
    }

    [Fact]
    public async Task InvokeAsync_WhenInvalidOperationException_ShouldReturn400()
    {
        // Arrange
        var middleware = new GlobalExceptionHandlerMiddleware(
            _ => throw new InvalidOperationException("Operación no permitida"),
            _loggerMock.Object);

        var (context, _) = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task InvokeAsync_WhenUnhandledGenericException_ShouldReturn500()
    {
        // Arrange
        var middleware = new GlobalExceptionHandlerMiddleware(
            _ => throw new Exception("Fallo catastrófico de hardware o red"),
            _loggerMock.Object);

        var (context, stream) = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(500);
        stream.Seek(0, SeekOrigin.Begin);
        var json = await new StreamReader(stream).ReadToEndAsync();
        json.Should().Contain("Error interno del servidor");
    }
}
