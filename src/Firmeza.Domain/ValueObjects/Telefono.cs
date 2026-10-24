// Archivo: Telefono.cs
// Representa y valida un número telefónico fijo o celular de contacto.
// Sirve para asegurar que los teléfonos registrados tengan entre 7 y 15 dígitos reales.

using System.Text.RegularExpressions;
using Firmeza.Domain.Common;
using Firmeza.Domain.Exceptions;

namespace Firmeza.Domain.ValueObjects;

// Clase que guarda un número de teléfono verificando que contenga una cantidad válida de números.
public sealed class Telefono : ValueObject
{
    // Regla utilizada para ignorar espacios o guiones y contar únicamente los números del teléfono.
    private static readonly Regex DigitsOnly = new(@"[^\d]", RegexOptions.Compiled);

    // Número telefónico validado y sin espacios innecesarios en los extremos.
    public string Valor { get; }

    // Revisa que el teléfono no esté vacío y que al contar sus números tenga entre 7 y 15 dígitos.
    public Telefono(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            throw new ValorInvalidoException(nameof(Telefono), "El número telefónico no puede estar vacío.");

        var soloDigitos = DigitsOnly.Replace(valor, "");

        if (soloDigitos.Length < 7 || soloDigitos.Length > 15)
            throw new ValorInvalidoException(nameof(Telefono), "El número telefónico debe contener entre 7 y 15 dígitos numéricos.");

        Valor = valor.Trim();
    }

    // Devuelve el número telefónico para comparar si dos teléfonos son iguales.
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Valor;
    }

    // Devuelve el teléfono como texto simple.
    public override string ToString() => Valor;

    // Permite usar este objeto directamente como un texto normal.
    public static implicit operator string(Telefono tel) => tel.Valor;

    // Permite convertir un texto normal en un teléfono validado.
    public static explicit operator Telefono(string valor) => new(valor);
}
