// Archivo: Placa.cs
// Representa y valida la placa de identificación de un vehículo o maquinaria (como ABC-123).
// Sirve para garantizar que todas las placas registradas tengan letras y números válidos en mayúsculas.

using System.Text.RegularExpressions;
using Firmeza.Domain.Common;
using Firmeza.Domain.Exceptions;

namespace Firmeza.Domain.ValueObjects;

// Clase que guarda la placa de un vehículo comprobando que tenga tres letras seguidas de números o letra final.
public sealed class Placa : ValueObject
{
    // Regla que valida tres letras iniciales, un guion opcional y la combinación numérica de la placa.
    private static readonly Regex PlacaRegex = new(
        @"^[A-Za-z]{3}-?[0-9]{2}[0-9A-Za-z]?$",
        RegexOptions.Compiled);

    // Texto de la placa limpio y convertido a mayúsculas.
    public string Valor { get; }

    // Revisa que la placa no esté vacía, la convierte a mayúsculas y verifica su estructura.
    public Placa(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            throw new ValorInvalidoException(nameof(Placa), "La placa del vehículo no puede estar vacía.");

        var limpia = valor.Trim().ToUpperInvariant();

        if (limpia.Length < 5 || limpia.Length > 10)
            throw new ValorInvalidoException(nameof(Placa), "La placa vehicular debe tener entre 5 y 10 caracteres.");

        if (!PlacaRegex.IsMatch(limpia))
            throw new ValorInvalidoException(nameof(Placa), $"El formato '{valor}' no corresponde a una placa vehicular válida (ej. FIR-101, ABC-123).");

        Valor = limpia;
    }

    // Devuelve el texto de la placa para comparar si dos placas son iguales.
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Valor;
    }

    // Devuelve la placa como texto simple.
    public override string ToString() => Valor;

    // Permite usar este objeto directamente como un texto normal.
    public static implicit operator string(Placa placa) => placa.Valor;

    // Permite convertir un texto normal en una placa validada.
    public static explicit operator Placa(string valor) => new(valor);
}
