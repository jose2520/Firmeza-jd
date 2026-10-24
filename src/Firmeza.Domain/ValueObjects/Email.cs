// Archivo: Email.cs
// Representa y valida una dirección de correo electrónico de un cliente o usuario.
// Sirve para asegurar que los correos guardados estén bien escritos y en minúsculas.

using System.Text.RegularExpressions;
using Firmeza.Domain.Common;
using Firmeza.Domain.Exceptions;

namespace Firmeza.Domain.ValueObjects;

// Clase que guarda un correo electrónico después de comprobar que tiene arroba, dominio y longitud adecuada.
public sealed class Email : ValueObject
{
    // Regla que revisa que el texto tenga estructura de correo (usuario@dominio.com) sin espacios.
    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    // Dirección de correo electrónico limpia y en minúsculas.
    public string Valor { get; }

    // Revisa que el correo no esté vacío, lo pasa a minúsculas y valida que su formato sea real.
    public Email(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            throw new ValorInvalidoException(nameof(Email), "El correo electrónico no puede ser nulo ni estar vacío.");

        var normalizado = valor.Trim().ToLowerInvariant();

        if (normalizado.Length > 150)
            throw new ValorInvalidoException(nameof(Email), "El correo electrónico excede la longitud máxima permitida de 150 caracteres.");

        if (!EmailRegex.IsMatch(normalizado))
            throw new ValorInvalidoException(nameof(Email), $"El formato '{valor}' no corresponde a una dirección de correo válida.");

        Valor = normalizado;
    }

    // Devuelve la dirección de correo para comparar si dos correos son iguales.
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Valor;
    }

    // Devuelve el correo electrónico como texto simple.
    public override string ToString() => Valor;

    // Permite usar este objeto directamente como un texto normal.
    public static implicit operator string(Email email) => email.Valor;

    // Permite crear un correo validado a partir de un texto normal.
    public static explicit operator Email(string valor) => new(valor);
}
