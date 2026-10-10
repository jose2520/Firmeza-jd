using System;

namespace Firmeza.Domain.Exceptions;

/// <summary>
/// Excepción base para todas las infracciones de reglas de negocio en la capa de Dominio.
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message)
    {
    }

    public DomainException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
