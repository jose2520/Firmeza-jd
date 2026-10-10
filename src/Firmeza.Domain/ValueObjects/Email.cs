using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Firmeza.Domain.Common;
using Firmeza.Domain.Exceptions;

namespace Firmeza.Domain.ValueObjects;

/// <summary>
/// Objeto de Valor que representa y valida una dirección de correo electrónico corporativa o de cliente.
/// Inmutable y normalizado a minúsculas.
/// </summary>
public sealed class Email : ValueObject
{
    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public string Valor { get; }

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

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Valor;
    }

    public override string ToString() => Valor;

    public static implicit operator string(Email email) => email.Valor;
    public static explicit operator Email(string valor) => new(valor);
}
