// Archivo: DomainException.cs
// Define el error base del cual parten todos los errores de reglas de negocio de la empresa.
// Sirve para identificar fácilmente cuando ocurre un problema propio de las operaciones de Firmeza.

namespace Firmeza.Domain.Exceptions;

// Clase principal para representar errores cuando no se cumple una condición importante del negocio.
public class DomainException : Exception
{
    // Crea el error con un mensaje claro explicando qué sucedió.
    public DomainException(string message) : base(message)
    {
    }

    // Crea el error con un mensaje y guarda también el detalle del error original que lo causó.
    public DomainException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
