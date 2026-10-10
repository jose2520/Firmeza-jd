using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Firmeza.Domain.Common;
using Firmeza.Domain.Exceptions;

namespace Firmeza.Domain.ValueObjects;

/// <summary>
/// Objeto de Valor que representa y valida la placa vehicular o de maquinaria (ej. FIR-101, ABC-123).
/// </summary>
public sealed class Placa : ValueObject
{
    private static readonly Regex PlacaRegex = new(
        @"^[A-Za-z]{3}-?[0-9]{2}[0-9A-Za-z]?$",
        RegexOptions.Compiled);

    public string Valor { get; }

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

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Valor;
    }

    public override string ToString() => Valor;

    public static implicit operator string(Placa placa) => placa.Valor;
    public static explicit operator Placa(string valor) => new(valor);
}
