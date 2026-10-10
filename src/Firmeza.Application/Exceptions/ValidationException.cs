using System;
using System.Collections.Generic;

namespace Firmeza.Application.Exceptions;

/// <summary>
/// Excepción lanzada cuando los datos de entrada de una solicitud o DTO no cumplen con las reglas
/// de validación de formato, estructura o requisitos mínimos del caso de uso.
/// Se traduce automáticamente a HTTP 400 (Bad Request).
/// </summary>
public class ValidationException : Exception
{
    /// <summary>
    /// Diccionario con los errores de validación agrupados por nombre de propiedad/campo.
    /// </summary>
    public IDictionary<string, string[]> Errors { get; }

    public ValidationException()
        : base("Se presentaron uno o más errores de validación en la solicitud.")
    {
        Errors = new Dictionary<string, string[]>();
    }

    public ValidationException(string message)
        : base(message)
    {
        Errors = new Dictionary<string, string[]>();
    }

    public ValidationException(IDictionary<string, string[]> errors)
        : base("Se presentaron uno o más errores de validación en la solicitud.")
    {
        Errors = errors;
    }

    public ValidationException(string propertyName, string errorMessage)
        : base("Se presentaron uno o más errores de validación en la solicitud.")
    {
        Errors = new Dictionary<string, string[]>
        {
            { propertyName, new[] { errorMessage } }
        };
    }
}
