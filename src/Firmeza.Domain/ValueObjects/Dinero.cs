// Archivo: Dinero.cs
// Representa una cantidad de dinero en Pesos Colombianos (COP) para precios, fletes y totales.
// Sirve para hacer sumas, restas y multiplicaciones seguras evitando valores negativos por error.

using System.Globalization;
using Firmeza.Domain.Common;
using Firmeza.Domain.Exceptions;

namespace Firmeza.Domain.ValueObjects;

// Clase que maneja valores en dinero asegurando que nunca sean menores a cero y tengan máximo dos decimales.
public sealed class Dinero : ValueObject, IComparable<Dinero>
{
    // Configuración regional de Colombia para mostrar el signo pesos y separadores correctos.
    private static readonly CultureInfo CulturaCo = new("es-CO");

    // Cantidad numérica del dinero redondeada a dos decimales.
    public decimal Monto { get; }

    // Verifica que el monto no sea negativo y lo redondea a dos decimales antes de guardarlo.
    public Dinero(decimal monto)
    {
        if (monto < 0)
            throw new ValorInvalidoException(nameof(Dinero), $"El valor monetario no puede ser negativo (recibido: {monto}).");

        Monto = Math.Round(monto, 2);
    }

    // Representa un valor de cero pesos listo para usar al iniciar cuentas.
    public static Dinero Cero => new(0m);

    // Devuelve el monto para poder comparar si dos cantidades de dinero son exactamente iguales.
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Monto;
    }

    // Compara este monto con otro para saber cuál de los dos es mayor o menor.
    public int CompareTo(Dinero? other)
    {
        if (other is null) return 1;
        return Monto.CompareTo(other.Monto);
    }

    // Suma dos cantidades de dinero y devuelve el nuevo total.
    public static Dinero operator +(Dinero a, Dinero b) => new(a.Monto + b.Monto);

    // Resta dos cantidades de dinero asegurando que el resultado no quede en negativo.
    public static Dinero operator -(Dinero a, Dinero b)
    {
        if (a.Monto < b.Monto)
            throw new ReglaNegocioException("Dinero.RestaInvalida", "La resta monetaria resultaría en un valor negativo.");
        return new(a.Monto - b.Monto);
    }

    // Multiplica el dinero por una cantidad o porcentaje verificando que no sea negativo.
    public static Dinero operator *(Dinero a, decimal factor)
    {
        if (factor < 0)
            throw new ReglaNegocioException("Dinero.FactorNegativo", "No es posible multiplicar dinero por un factor negativo.");
        return new(a.Monto * factor);
    }

    // Verifica si la primera cantidad de dinero es menor que la segunda.
    public static bool operator <(Dinero left, Dinero right) => left.Monto < right.Monto;

    // Verifica si la primera cantidad de dinero es menor o igual que la segunda.
    public static bool operator <=(Dinero left, Dinero right) => left.Monto <= right.Monto;

    // Verifica si la primera cantidad de dinero es mayor que la segunda.
    public static bool operator >(Dinero left, Dinero right) => left.Monto > right.Monto;

    // Verifica si la primera cantidad de dinero es mayor o igual que la segunda.
    public static bool operator >=(Dinero left, Dinero right) => left.Monto >= right.Monto;

    // Muestra el monto formateado como moneda colombiana (por ejemplo: $ 50.000).
    public override string ToString() => Monto.ToString("C", CulturaCo);

    // Permite obtener el número decimal directamente cuando se necesite hacer cálculos.
    public static implicit operator decimal(Dinero dinero) => dinero.Monto;

    // Permite convertir un número decimal en un objeto de dinero validado.
    public static explicit operator Dinero(decimal monto) => new(monto);
}
