using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Firmeza.Domain.Common;
using Firmeza.Domain.Exceptions;

namespace Firmeza.Domain.ValueObjects;

/// <summary>
/// Objeto de Valor que representa y valida un Documento de Identificación o NIT colombiano (RUT).
/// </summary>
public sealed class Nit : ValueObject
{
    private static readonly Regex NitRegex = new(
        @"^[0-9]{5,12}(-[0-9kK])?$",
        RegexOptions.Compiled);

    public string Valor { get; }

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

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Valor;
    }

    public override string ToString() => Valor;

    public static implicit operator string(Nit nit) => nit.Valor;
    public static explicit operator Nit(string valor) => new(valor);
}
