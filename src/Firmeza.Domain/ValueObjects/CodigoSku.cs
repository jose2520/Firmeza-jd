using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Firmeza.Domain.Common;
using Firmeza.Domain.Exceptions;

namespace Firmeza.Domain.ValueObjects;

/// <summary>
/// Objeto de Valor que representa y valida un código SKU o referencia única de producto (ej. PROD-001).
/// </summary>
public sealed class CodigoSku : ValueObject
{
    private static readonly Regex SkuRegex = new(
        @"^[A-Za-z0-9\-_]{3,50}$",
        RegexOptions.Compiled);

    public string Valor { get; }

    public CodigoSku(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            throw new ValorInvalidoException(nameof(CodigoSku), "El código de referencia (SKU) no puede estar vacío.");

        var normalizado = valor.Trim().ToUpperInvariant();

        if (!SkuRegex.IsMatch(normalizado))
            throw new ValorInvalidoException(nameof(CodigoSku), $"El código '{valor}' contiene caracteres no permitidos o no cumple la longitud de 3 a 50 caracteres.");

        Valor = normalizado;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Valor;
    }

    public override string ToString() => Valor;

    public static implicit operator string(CodigoSku sku) => sku.Valor;
    public static explicit operator CodigoSku(string valor) => new(valor);
}
