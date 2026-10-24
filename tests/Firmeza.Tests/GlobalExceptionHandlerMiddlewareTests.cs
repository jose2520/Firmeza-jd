// Pruebas para verificar el funcionamiento del capturador central de errores del servidor.
// Comprueba que cada tipo de fallo devuelva el código numérico correcto (400, 404, 409, 422 o 500)
// y que los errores graves del sistema no muestren detalles técnicos delicados al usuario.

using System.Text.Json;
using Firmeza.API.Middleware;
using Firmeza.Application.Exceptions;
using Firmeza.Domain.Exceptions;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;

namespace Firmeza.Tests;

public class GlobalExceptionHandlerMiddlewareTests
{
    private readonly Mock<ILogger<GlobalExceptionHandlerMiddleware>> _loggerMock = new();

    // Crea un contexto de petición de prueba en memoria para leer la respuesta generada por el servidor.
    private (DefaultHttpContext context, MemoryStream responseBody) CreateHttpContext()
    {
        var context = new DefaultHttpContext();
        var responseBody = new MemoryStream();
        context.Response.Body = responseBody;
        return (context, responseBody);
    }

    // Verifica que cuando no se encuentra un registro se devuelva el código 404 y el mensaje de recurso no encontrado.
    [Fact]
    public async Task InvokeAsync_WhenNotFoundException_ShouldReturn404()
    {
        // Preparar el capturador simulando que un cliente no fue encontrado
        var middleware = new GlobalExceptionHandlerMiddleware(
            _ => throw new NotFoundException("Cliente", 123),
            _loggerMock.Object);

        var (context, stream) = CreateHttpContext();

        // Ejecutar la petición
        await middleware.InvokeAsync(context);

        // Comprobar que responda 404 con el detalle esperado
        context.Response.StatusCode.Should().Be(404);
        stream.Seek(0, SeekOrigin.Begin);
        var json = await new StreamReader(stream).ReadToEndAsync();
        json.Should().Contain("Recurso no encontrado");
        json.Should().Contain("123");
    }

    // Comprueba que cuando se intenta guardar un dato repetido se devuelva el código 409 de conflicto.
    [Fact]
    public async Task InvokeAsync_WhenConflictException_ShouldReturn409()
    {
        // Preparar el capturador simulando un correo duplicado
        var middleware = new GlobalExceptionHandlerMiddleware(
            _ => throw new ConflictException("Cliente", "Email", "test@firmeza.com"),
            _loggerMock.Object);

        var (context, stream) = CreateHttpContext();

        // Ejecutar la petición
        await middleware.InvokeAsync(context);

        // Comprobar que responda 409 indicando conflicto de datos
        context.Response.StatusCode.Should().Be(409);
        stream.Seek(0, SeekOrigin.Begin);
        var json = await new StreamReader(stream).ReadToEndAsync();
        json.Should().Contain("Conflicto de integridad de datos");
    }

    // Verifica que cuando no alcanzan las existencias en bodega se devuelva el código 422 indicando el producto afectado.
    [Fact]
    public async Task InvokeAsync_WhenStockInsuficienteException_ShouldReturn422()
    {
        // Preparar el capturador simulando falta de existencias
        var middleware = new GlobalExceptionHandlerMiddleware(
            _ => throw new StockInsuficienteException("PROD-001", 5, 20),
            _loggerMock.Object);

        var (context, stream) = CreateHttpContext();

        // Ejecutar la petición
        await middleware.InvokeAsync(context);

        // Comprobar que responda 422 con los datos del producto sin inventario suficiente
        context.Response.StatusCode.Should().Be(422);
        stream.Seek(0, SeekOrigin.Begin);
        var json = await new StreamReader(stream).ReadToEndAsync();
        json.Should().Contain("Stock insuficiente en bodega");
        json.Should().Contain("PROD-001");
    }

    // Comprueba que ante un error de datos mal ingresados se devuelva el código 400 con el título de error de validación.
    [Fact]
    public async Task InvokeAsync_WhenValidationException_ShouldReturn400()
    {
        // Preparar el capturador simulando un precio inválido
        var middleware = new GlobalExceptionHandlerMiddleware(
            _ => throw new ValidationException("Precio", "No puede ser cero"),
            _loggerMock.Object);

        var (context, stream) = CreateHttpContext();

        // Ejecutar la petición
        await middleware.InvokeAsync(context);

        // Comprobar que responda 400
        context.Response.StatusCode.Should().Be(400);
        stream.Seek(0, SeekOrigin.Begin);
        var json = await new StreamReader(stream).ReadToEndAsync();
        json.Should().Contain("Error de validación");
    }

    // Verifica que una operación no permitida devuelva el código 400 de solicitud incorrecta.
    [Fact]
    public async Task InvokeAsync_WhenInvalidOperationException_ShouldReturn400()
    {
        // Preparar el capturador simulando una operación inválida
        var middleware = new GlobalExceptionHandlerMiddleware(
            _ => throw new InvalidOperationException("Operación no permitida"),
            _loggerMock.Object);

        var (context, _) = CreateHttpContext();

        // Ejecutar la petición
        await middleware.InvokeAsync(context);

        // Comprobar que responda 400
        context.Response.StatusCode.Should().Be(400);
    }

    // Comprueba que ante un fallo inesperado del servidor se responda 500 con un mensaje seguro sin filtrar detalles internos.
    [Fact]
    public async Task InvokeAsync_WhenUnhandledGenericException_ShouldReturn500()
    {
        // Preparar el capturador simulando un fallo grave inesperado
        var middleware = new GlobalExceptionHandlerMiddleware(
            _ => throw new Exception("Fallo catastrófico de hardware o red"),
            _loggerMock.Object);

        var (context, stream) = CreateHttpContext();

        // Ejecutar la petición
        await middleware.InvokeAsync(context);

        // Comprobar que responda 500 y oculte el mensaje técnico original
        context.Response.StatusCode.Should().Be(500);
        stream.Seek(0, SeekOrigin.Begin);
        var json = await new StreamReader(stream).ReadToEndAsync();
        json.Should().Contain("Error interno del servidor");
        json.Should().NotContain("Fallo catastrófico de hardware o red");
    }
}
