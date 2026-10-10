using System;

namespace Firmeza.Application.Exceptions;

/// <summary>
/// Excepción lanzada cuando una operación entra en conflicto con el estado actual del sistema,
/// como la duplicidad de llaves únicas (cédula, NIT, código de producto o email existente).
/// Se traduce automáticamente a HTTP 409 (Conflict).
/// </summary>
public class ConflictException : Exception
{
    public ConflictException(string message) : base(message)
    {
    }

    public ConflictException(string entityName, string propertyName, object value)
        : base($"Ya existe un registro de '{entityName}' con el valor '{value}' en el campo '{propertyName}'.")
    {
    }
}
