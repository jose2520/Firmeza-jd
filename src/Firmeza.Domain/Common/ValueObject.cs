using System;
using System.Collections.Generic;
using System.Linq;

namespace Firmeza.Domain.Common;

/// <summary>
/// Clase base para todos los Objetos de Valor (Value Objects) en Domain-Driven Design (DDD).
/// 
/// CARACTERÍSTICAS:
///   - Carecen de identidad propia (no tienen 'Id').
///   - Son completamente inmutables.
///   - Dos Value Objects son iguales si todos sus atributos/componentes son iguales (igualdad estructural).
/// </summary>
public abstract class ValueObject : IEquatable<ValueObject>
{
    /// <summary>
    /// Retorna la secuencia de componentes atómicos que determinan la igualdad del objeto de valor.
    /// </summary>
    protected abstract IEnumerable<object?> GetEqualityComponents();

    public override bool Equals(object? obj)
    {
        if (obj is null || obj.GetType() != GetType())
            return false;

        return Equals((ValueObject)obj);
    }

    public bool Equals(ValueObject? other)
    {
        if (other is null)
            return false;

        return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }

    public override int GetHashCode()
    {
        return GetEqualityComponents()
            .Select(x => x?.GetHashCode() ?? 0)
            .Aggregate((x, y) => x ^ y);
    }

    public static bool operator ==(ValueObject? left, ValueObject? right)
    {
        if (left is null && right is null)
            return true;

        if (left is null || right is null)
            return false;

        return left.Equals(right);
    }

    public static bool operator !=(ValueObject? left, ValueObject? right)
    {
        return !(left == right);
    }
}
