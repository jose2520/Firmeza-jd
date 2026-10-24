// Archivo: ConflictException.cs
// Qué es: Clase de error para avisar cuando un dato choca con otro que ya existe.
// Para qué sirve: Detiene el proceso y avisa de forma clara cuando se intenta guardar algo repetido, como una cédula, NIT, código o correo que ya está registrado.

namespace Firmeza.Application.Exceptions;

// Error personalizado que se lanza cuando se detecta información duplicada o en conflicto en el sistema.
public class ConflictException : Exception
{
    // Crea el error con un mensaje explicativo directo.
    public ConflictException(string message) : base(message)
    {
    }

    // Crea el error indicando exactamente qué tipo de registro, campo y valor ya existen en el sistema.
    public ConflictException(string entityName, string propertyName, object value)
        : base($"Ya existe un registro de '{entityName}' con el valor '{value}' en el campo '{propertyName}'.")
    {
    }
}
