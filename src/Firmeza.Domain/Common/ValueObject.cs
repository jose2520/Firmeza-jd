// Este archivo define una base para representar datos del negocio que se comparan por su contenido y no por un número de ID.
// En Firmeza, sirve para manejar valores como correos electrónicos, documentos de identidad, placas o montos de dinero,
// asegurando que dos datos sean considerados iguales si tienen exactamente la misma información escrita.

namespace Firmeza.Domain.Common;

// Clase base para datos del negocio que no usan un identificador numérico, sino que se definen por sus propios valores.
public abstract class ValueObject : IEquatable<ValueObject>
{
    // Obtiene la lista de valores internos que se deben revisar para saber si dos datos son iguales.
    protected abstract IEnumerable<object?> GetEqualityComponents();

    // Compara este objeto con cualquier otro para verificar si son del mismo tipo y tienen el mismo contenido.
    public override bool Equals(object? obj)
    {
        if (obj is null || obj.GetType() != GetType())
            return false;

        return Equals((ValueObject)obj);
    }

    // Comprueba si otro dato del mismo tipo tiene exactamente los mismos valores internos que este.
    public bool Equals(ValueObject? other)
    {
        if (other is null)
            return false;

        return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }

    // Calcula un código numérico resumido a partir de sus valores internos para facilitar búsquedas rápidas.
    public override int GetHashCode()
    {
        return GetEqualityComponents()
            .Select(x => x?.GetHashCode() ?? 0)
            .Aggregate((x, y) => x ^ y);
    }

    // Permite usar el símbolo de igualdad (==) para comparar directamente si dos datos tienen el mismo valor.
    public static bool operator ==(ValueObject? left, ValueObject? right)
    {
        if (left is null && right is null)
            return true;

        if (left is null || right is null)
            return false;

        return left.Equals(right);
    }

    // Permite usar el símbolo de diferencia (!=) para comprobar si dos datos tienen valores distintos.
    public static bool operator !=(ValueObject? left, ValueObject? right)
    {
        return !(left == right);
    }
}
