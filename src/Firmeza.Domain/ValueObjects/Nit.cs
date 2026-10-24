// Archivo: Nit.cs
// Representa y valida el número de documento de identidad o NIT tributario en Colombia.
// Sirve para asegurar que las cédulas y NITs de clientes tengan dígitos y formato válidos.

using System.Text.RegularExpressions;
using Firmeza.Domain.Common;
using Firmeza.Domain.Exceptions;

namespace Firmeza.Domain.ValueObjects;

// Clase que guarda un documento o NIT verificando que tenga la cantidad correcta de números y dígito de verificación opcional.
public sealed class Nit : ValueObject
{
    // Regla que acepta de 5 a 12 números seguidos opcionalmente por un guion y el dígito de verificación.
    private static readonly Regex NitRegex = new(
        @"^[0-9]{5,12}(-[0-9kK])?$",
        RegexOptions.Compiled);

    // Número de documento o NIT validado y sin espacios a los lados.
    public string Valor { get; }

    // Comprueba que el documento no esté vacío y cumpla con la estructura de cédula o NIT colombiano.
    public Nit(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            throw new ValorInvalidoException(nameof(Nit), "El número de identificación o NIT no puede ser nulo ni estar vacío.");

        var normalizado = valor.Trim();

        if (normalizado.Length < 5 || normalizado.Length > 20)
            throw new ValorInvalidoException(nameof(Nit), "El documento o NIT debe contener entre 5 y 20 caracteres.");

        if (!NitRegex.IsMatch(normalizado))
            throw new ValorInvalidoException(nameof(Nit), $"El documento o NIT '{valor}' no posee una estructura válida (ej. 860002130-1 o 1020304050).");

        Valor = normalizado;
    }

    // Devuelve el número de documento para comparar si dos identificaciones son iguales.
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Valor;
    }

    // Devuelve el NIT o documento como texto simple.
    public override string ToString() => Valor;

    // Permite usar este objeto directamente como un texto normal.
    public static implicit operator string(Nit nit) => nit.Valor;

    // Permite convertir un texto normal en un documento o NIT validado.
    public static explicit operator Nit(string valor) => new(valor);
}
