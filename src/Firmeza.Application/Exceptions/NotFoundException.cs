// Archivo: NotFoundException.cs
// Qué es: Clase de error para avisar cuando no se encuentra lo que se está buscando.
// Para qué sirve: Informa de manera clara cuando se intenta consultar, editar o borrar un cliente, producto o venta que no existe en la base de datos.

namespace Firmeza.Application.Exceptions;

// Error personalizado que se lanza cuando un registro buscado no existe.
public class NotFoundException : Exception
{
    // Crea el error con un mensaje personalizado sobre lo que no se encontró.
    public NotFoundException(string message) : base(message)
    {
    }

    // Crea el error indicando el nombre del elemento buscado y el número o código con el que se intentó encontrar.
    public NotFoundException(string entityName, object key)
        : base($"La entidad '{entityName}' con identificador '{key}' no fue encontrada.")
    {
    }
}
