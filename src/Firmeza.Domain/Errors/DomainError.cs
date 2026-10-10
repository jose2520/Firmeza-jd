namespace Firmeza.Domain.Errors;

public enum ErrorType
{
    Failure = 0,
    Validation = 1,
    NotFound = 2,
    Conflict = 3
}

/// <summary>
/// Representa un error de negocio fuertemente tipado para flujos de validación (Result Pattern).
/// Evita el uso costoso de excepciones para casos de error esperados del dominio.
/// </summary>
public sealed record DomainError(string Code, string Description, ErrorType Type = ErrorType.Failure)
{
    public static readonly DomainError None = new(string.Empty, string.Empty);

    public static DomainError Validation(string code, string description) =>
        new(code, description, ErrorType.Validation);

    public static DomainError NotFound(string code, string description) =>
        new(code, description, ErrorType.NotFound);

    public static DomainError Conflict(string code, string description) =>
        new(code, description, ErrorType.Conflict);

    public static DomainError Failure(string code, string description) =>
        new(code, description, ErrorType.Failure);
}
