// Archivo: ValidationException.cs
// Qué es: Clase de error para avisar que los datos enviados están incompletos o mal escritos.
// Para qué sirve: Reúne todos los problemas encontrados al revisar un formulario (como precios negativos o campos vacíos) para devolverlos juntos al usuario.

namespace Firmeza.Application.Exceptions;

// Error personalizado que guarda la lista de fallos cuando los datos enviados no cumplen las reglas del negocio.
public class ValidationException : Exception
{
    // Lista de errores organizados por el nombre de cada campo que falló.
    public IDictionary<string, string[]> Errors { get; }

    // Crea el error de validación con un mensaje general y una lista vacía de fallos.
    public ValidationException()
        : base("Se presentaron uno o más errores de validación en la solicitud.")
    {
        Errors = new Dictionary<string, string[]>();
    }

    // Crea el error de validación con un mensaje específico.
    public ValidationException(string message)
        : base(message)
    {
        Errors = new Dictionary<string, string[]>();
    }

    // Crea el error guardando un grupo de varios campos que tuvieron problemas y sus mensajes.
    public ValidationException(IDictionary<string, string[]> errors)
        : base("Se presentaron uno o más errores de validación en la solicitud.")
    {
        Errors = errors;
    }

    // Crea el error para un solo campo específico junto con la explicación del problema encontrado.
    public ValidationException(string propertyName, string errorMessage)
        : base("Se presentaron uno o más errores de validación en la solicitud.")
    {
        Errors = new Dictionary<string, string[]>
        {
            { propertyName, new[] { errorMessage } }
        };
    }
}
