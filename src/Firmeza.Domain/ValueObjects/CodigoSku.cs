// Archivo: CodigoSku.cs
// Representa y valida el código de referencia único de cada producto en el catálogo (como PROD-001).
// Sirve para asegurar que ningún material tenga códigos vacíos, muy cortos o con símbolos raros.

using System.Text.RegularExpressions;
using Firmeza.Domain.Common;
using Firmeza.Domain.Exceptions;

namespace Firmeza.Domain.ValueObjects;

// Clase que guarda el código de un producto garantizando que esté limpio, en mayúsculas y bien escrito.
public sealed class CodigoSku : ValueObject
{
    // Regla de formato que solo permite letras, números, guiones y entre 3 y 50 caracteres.
    private static readonly Regex SkuRegex = new(
        @"^[A-Za-z0-9\-_]{3,50}$",
        RegexOptions.Compiled);

    // Texto final del código del producto ya validado y en mayúsculas.
    public string Valor { get; }

    // Revisa que el código no esté vacío, lo pasa a mayúsculas y comprueba que tenga el formato correcto.
    public CodigoSku(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            throw new ValorInvalidoException(nameof(CodigoSku), "El código de referencia (SKU) no puede estar vacío.");

        var normalizado = valor.Trim().ToUpperInvariant();

        if (!SkuRegex.IsMatch(normalizado))
            throw new ValorInvalidoException(nameof(CodigoSku), $"El código '{valor}' contiene caracteres no permitidos o no cumple la longitud de 3 a 50 caracteres.");

        Valor = normalizado;
    }

    // Devuelve el valor del código para poder comparar si dos códigos de producto son iguales.
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Valor;
    }

    // Devuelve el código en formato de texto simple.
    public override string ToString() => Valor;

    // Permite usar este objeto directamente como si fuera un texto normal.
    public static implicit operator string(CodigoSku sku) => sku.Valor;

    // Permite convertir un texto normal en un código de producto validado.
    public static explicit operator CodigoSku(string valor) => new(valor);
}
