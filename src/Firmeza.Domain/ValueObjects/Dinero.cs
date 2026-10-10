using System;
using System.Collections.Generic;
using System.Globalization;
using Firmeza.Domain.Common;
using Firmeza.Domain.Exceptions;

namespace Firmeza.Domain.ValueObjects;

/// <summary>
/// Objeto de Valor que encapsula un importe monetario expresado en Pesos Colombianos (COP).
/// Garantiza que los valores comerciales no sean negativos y soporta operaciones aritméticas seguras.
/// </summary>
public sealed class Dinero : ValueObject, IComparable<Dinero>
{
    private static readonly CultureInfo CulturaCo = new("es-CO");

    public decimal Monto { get; }

    public Dinero(decimal monto)
    {
        if (monto < 0)
            throw new ValorInvalidoException(nameof(Dinero), $"El valor monetario no puede ser negativo (recibido: {monto}).");

        Monto = Math.Round(monto, 2);
    }

    public static Dinero Cero => new(0m);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Monto;
    }

    public int CompareTo(Dinero? other)
    {
        if (other is null) return 1;
        return Monto.CompareTo(other.Monto);
    }

    public static Dinero operator +(Dinero a, Dinero b) => new(a.Monto + b.Monto);
    public static Dinero operator -(Dinero a, Dinero b)
    {
        if (a.Monto < b.Monto)
            throw new ReglaNegocioException("Dinero.RestaInvalida", "La resta monetaria resultaría en un valor negativo.");
        return new(a.Monto - b.Monto);
    }
    public static Dinero operator *(Dinero a, decimal factor)
    {
        if (factor < 0)
            throw new ReglaNegocioException("Dinero.FactorNegativo", "No es posible multiplicar dinero por un factor negativo.");
        return new(a.Monto * factor);
    }

    public static bool operator <(Dinero left, Dinero right) => left.Monto < right.Monto;
    public static bool operator <=(Dinero left, Dinero right) => left.Monto <= right.Monto;
    public static bool operator >(Dinero left, Dinero right) => left.Monto > right.Monto;
    public static bool operator >=(Dinero left, Dinero right) => left.Monto >= right.Monto;

    public override string ToString() => Monto.ToString("C", CulturaCo);

    public static implicit operator decimal(Dinero dinero) => dinero.Monto;
    public static explicit operator Dinero(decimal monto) => new(monto);
}
