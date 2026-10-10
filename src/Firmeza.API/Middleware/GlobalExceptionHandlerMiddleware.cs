using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Firmeza.Application.Exceptions;
using Firmeza.Domain.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Firmeza.API.Middleware;

/// <summary>
/// Middleware global para el manejo centralizado de excepciones en la API de Firmeza.
/// Captura tanto excepciones de Dominio como de Aplicación y las transforma en respuestas
/// HTTP estandarizadas siguiendo el formato RFC 7807 (ProblemDetails).
/// </summary>
public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, title, errors) = exception switch
        {
            NotFoundException notFound => (HttpStatusCode.NotFound, "Recurso no encontrado", (object?)null),
            ConflictException conflict => (HttpStatusCode.Conflict, "Conflicto de integridad de datos", (object?)null),
            ForbiddenAccessException forbidden => (HttpStatusCode.Forbidden, "Acceso no autorizado al recurso", (object?)null),
            ValidationException valEx => (HttpStatusCode.BadRequest, "Error de validación", (object?)valEx.Errors),
            ValorInvalidoException valInvalido => (HttpStatusCode.BadRequest, "Valor de entrada inválido", (object?)new { campo = valInvalido.NombreCampo }),
            StockInsuficienteException stock => (HttpStatusCode.UnprocessableEntity, "Stock insuficiente en bodega", (object?)new
            {
                codigoProducto = stock.CodigoProducto,
                disponible = stock.StockDisponible,
                solicitado = stock.CantidadSolicitada
            }),
            ReglaNegocioException regla => (HttpStatusCode.UnprocessableEntity, "Violación de regla de negocio", (object?)new
            {
                codigoRegla = regla.CodigoRegla
            }),
            DomainException domain => (HttpStatusCode.BadRequest, "Error de dominio", (object?)null),
            InvalidOperationException invalidOp => (HttpStatusCode.BadRequest, "Operación inválida", (object?)null),
            _ => (HttpStatusCode.InternalServerError, "Error interno del servidor", (object?)null)
        };

        if (statusCode == HttpStatusCode.InternalServerError)
        {
            _logger.LogError(exception, "Excepción no controlada en el servidor: {Message}", exception.Message);
        }
        else
        {
            _logger.LogWarning("Excepción de negocio o aplicación capturada ({StatusCode}): {Message}", statusCode, exception.Message);
        }

        context.Response.ContentType = "application/json; charset=utf-8";
        context.Response.StatusCode = (int)statusCode;

        var response = new
        {
            status = (int)statusCode,
            title,
            message = exception.Message,
            errors,
            timestamp = DateTime.UtcNow
        };

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, jsonOptions));
    }
}
