// Archivo: UnidadMedida.cs
// Define las unidades en las que se pesan, miden y venden los materiales de construcción.
// Sirve para saber si un producto se factura por bulto, metro, kilo, galón o unidad.

namespace Firmeza.Domain.Enums;

// Unidades de medida utilizadas en el catálogo para controlar el inventario y las ventas.
public enum UnidadMedida
{
    // Pieza individual como herramientas, accesorios, ladrillos o bloques.
    Unidad = 1,

    // Saco o bolsa empacada como cemento de 50 kg, cal, yeso o pegante cerámico.
    Bulto = 2,

    // Peso en kilos para productos vendidos al destajo como alambre o puntillas.
    Kilogramo = 3,

    // Peso de mil kilos para compras grandes de acero o agregados.
    Tonelada = 4,

    // Medida de longitud para mangueras, perfiles o cables vendidos por metro lineal.
    Metro = 5,

    // Medida de superficie para pisos, enchapes, tejas o láminas de drywall.
    MetroCuadrado = 6,

    // Medida de volumen para arena lavada, grava triturada o concreto premezclado.
    MetroCubico = 7,

    // Envase de 3.78 litros usado en pinturas, impermeabilizantes y aditivos.
    Galon = 8,

    // Medida líquida para solventes, thinner o aditivos químicos pequeños.
    Litro = 9,

    // Barra completa de hierro o acero corrugado de 6 o 12 metros de largo.
    Varilla = 10,

    // Presentación enrollada para mallas electrosoldadas, plástico negro o cables.
    Rollo = 11,

    // Empaque cerrado que agrupa varias unidades como cajas de tornillos o baldosas.
    Caja = 12,

    // Tramo completo de tubería de 6 metros para agua, luz o alcantarillado.
    Tubo = 13
}
