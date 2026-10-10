using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Firmeza.Domain.Common;
using Firmeza.Domain.Exceptions;

namespace Firmeza.Domain.ValueObjects;

/// <summary>
/// Objeto de Valor que encapsula y valida un número de teléfono fijo o móvil.
/// </summary>
public sealed class Telefono : ValueObject
{
    private static readonly Regex DigitsOnly = new(@"[^\d]", RegexOptions.Compiled);

    public string Valor { get; }

    public Telefono(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            throw new ValorInvalidoException(nameof(Telefono), "El número telefónico no puede estar vacío.");

        var soloDigitos = DigitsOnly.Replace(valor, "");

        if (soloDigitos.Length < 7 || soloDigitos.Length > 15)
            throw new ValorInvalidoException(nameof(Telefono), "El número telefónico debe contener entre 7 y 15 dígitos numéricos.");

        Valor = valor.Trim();
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Valor;
    }

    public override string ToString() => Valor;

    public static implicit operator string(Telefono tel) => tel.Valor;
    public static explicit operator Telefono(string valor) => new(valor);
}
