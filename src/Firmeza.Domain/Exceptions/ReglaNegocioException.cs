// Archivo: ReglaNegocioException.cs
// Representa un error que se detona cuando una acción incumple una norma comercial de la empresa.
// Sirve para detener la operación e informar exactamente qué regla fue la que no se respetó.

namespace Firmeza.Domain.Exceptions;

// Error específico que guarda el código de la regla comercial incumplida junto con su explicación.
public class ReglaNegocioException : DomainException
{
    // Identificador corto de la regla del negocio que falló.
    public string CodigoRegla { get; }

    // Crea el error indicando el código de la regla y el mensaje explicativo para el usuario.
    public ReglaNegocioException(string codigoRegla, string message) : base(message)
    {
        CodigoRegla = codigoRegla;
    }
}
