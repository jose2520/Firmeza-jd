// Archivo: DomainError.cs
// Define la estructura básica para representar un problema o error esperado en el sistema.
// Sirve para devolver mensajes claros con su código y categoría cuando algo sale mal.

namespace Firmeza.Domain.Errors;

// Categorías generales para clasificar qué tipo de problema ocurrió en una operación.
public enum ErrorType
{
    // Fallo general en una operación del negocio.
    Failure = 0,

    // Datos ingresados incorrectos o que no cumplen el formato requerido.
    Validation = 1,

    // El registro buscado no existe en la base de datos.
    NotFound = 2,

    // Conflicto con el estado actual, como un dato duplicado o una acción repetida.
    Conflict = 3
}

// ReSharper disable NotAccessedPositionalProperty.Global

// Representa un error del negocio con un código identificador, un mensaje claro y su categoría.
[System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "NotAccessedPositionalProperty.Global")]
public sealed record DomainError(string Code, string Description, ErrorType Type)
{
    // Valor vacío que indica que no ocurrió ningún error en la operación.
    public static readonly DomainError None = new(string.Empty, string.Empty, ErrorType.Failure);

    // Crea un error para indicar que los datos enviados por el usuario no son válidos.
    public static DomainError Validation(string code, string description) =>
        new(code, description, ErrorType.Validation);

    // Crea un error para indicar que no se encontró el elemento solicitado.
    public static DomainError NotFound(string code, string description) =>
        new(code, description, ErrorType.NotFound);

    // Crea un error para indicar un choque o duplicidad con información ya existente.
    public static DomainError Conflict(string code, string description) =>
        new(code, description, ErrorType.Conflict);

    // Crea un error general cuando una regla del negocio impide continuar.
    public static DomainError Failure(string code, string description) =>
        new(code, description, ErrorType.Failure);
}

// ReSharper restore NotAccessedPositionalProperty.Global
