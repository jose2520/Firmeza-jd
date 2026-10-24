// Capturador central de errores para todas las peticiones que llegan al servidor.
// Se encarga de interceptar cualquier fallo del sistema o de las reglas del negocio
// y devolver una respuesta clara y ordenada al usuario sin exponer datos internos.

using System.Net;
using System.Text.Json;
using Firmeza.Application.Exceptions;
using Firmeza.Domain.Exceptions;

namespace Firmeza.API.Middleware;

public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    // Recibe el siguiente paso de la petición y el registrador de eventos del servidor.
    public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    // Ejecuta la petición actual y, si ocurre algún error inesperado, lo envía al método que genera la respuesta de error.
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

    // Identifica el tipo de error ocurrido, asigna el código de estado adecuado y devuelve un mensaje claro al cliente.
    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        (HttpStatusCode StatusCode, string Title, object? Errors) errorInfo = exception switch
        {
            NotFoundException => (HttpStatusCode.NotFound, "Recurso no encontrado", null),
            ConflictException => (HttpStatusCode.Conflict, "Conflicto de integridad de datos", null),
            ForbiddenAccessException => (HttpStatusCode.Forbidden, "Acceso no autorizado al recurso", null),
            ValidationException valEx => (HttpStatusCode.BadRequest, "Error de validación", valEx.Errors),
            ValorInvalidoException valInvalido => (HttpStatusCode.BadRequest, "Valor de entrada inválido", new { campo = valInvalido.NombreCampo }),
            StockInsuficienteException stock => (HttpStatusCode.UnprocessableEntity, "Stock insuficiente en bodega", new
            {
                codigoProducto = stock.CodigoProducto,
                disponible = stock.StockDisponible,
                solicitado = stock.CantidadSolicitada
            }),
            ReglaNegocioException regla => (HttpStatusCode.UnprocessableEntity, "Violación de regla de negocio", new
            {
                codigoRegla = regla.CodigoRegla
            }),
            DomainException => (HttpStatusCode.BadRequest, "Error de dominio", null),
            InvalidOperationException => (HttpStatusCode.BadRequest, "Operación inválida", null),
            _ => (HttpStatusCode.InternalServerError, "Error interno del servidor", null)
        };

        var (statusCode, title, errors) = errorInfo;

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

        var safeMessage = statusCode == HttpStatusCode.InternalServerError
            ? "Ha ocurrido un error interno en el servidor. Por favor contacte al soporte técnico."
            : exception.Message;

        var response = new
        {
            status = (int)statusCode,
            title,
            message = safeMessage,
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
