// Archivo: ForbiddenAccessException.cs
// Qué es: Clase de error para avisar que un usuario no tiene permiso para realizar una acción.
// Para qué sirve: Bloquea la operación cuando alguien intenta entrar a una función o modificar información sin tener el cargo o permiso necesario.

namespace Firmeza.Application.Exceptions;

// Error personalizado que se lanza cuando al usuario actual no se le permite hacer una operación.
public class ForbiddenAccessException : Exception
{
    // Crea el error con un mensaje estándar indicando falta de permisos.
    public ForbiddenAccessException()
        : base("No tiene los permisos requeridos para acceder o modificar este recurso.")
    {
    }

    // Crea el error con un mensaje personalizado explicando por qué se negó el acceso.
    public ForbiddenAccessException(string message)
        : base(message)
    {
    }
}
