namespace Firmeza.Domain.Enums;

/// <summary>
/// Unidades de medida estándar para inventario, pesaje y comercialización de materiales de construcción.
/// </summary>
public enum UnidadMedida
{
    /// <summary>
    /// Unidad individual (herramientas, accesorios, bloques).
    /// </summary>
    Unidad = 1,

    /// <summary>
    /// Bulto (cemento 50kg, cal, yeso, pegantes).
    /// </summary>
    Bulto = 2,

    /// <summary>
    /// Kilogramo (alambre, clavos a granel).
    /// </summary>
    Kilogramo = 3,

    /// <summary>
    /// Tonelada métrica (acero figurado, agregados en gran volumen).
    /// </summary>
    Tonelada = 4,

    /// <summary>
    /// Metro lineal (tuberías, mangueras, perfiles).
    /// </summary>
    Metro = 5,

    /// <summary>
    /// Metro cuadrado (drywall, tejas, enchapes, pisos).
    /// </summary>
    MetroCuadrado = 6,

    /// <summary>
    /// Metro cúbico (arena lavada, triturado, concreto premezclado).
    /// </summary>
    MetroCubico = 7,

    /// <summary>
    /// Galón (pinturas, impermeabilizantes, aditivos).
    /// </summary>
    Galon = 8,

    /// <summary>
    /// Litro (solventes, desmoldantes, aditivos líquidos).
    /// </summary>
    Litro = 9,

    /// <summary>
    /// Varilla de 6 o 12 metros (varilla corrugada de hierro/acero).
    /// </summary>
    Varilla = 10,

    /// <summary>
    /// Rollo (malla electrosoldada, geomembrana, cable eléctrico).
    /// </summary>
    Rollo = 11,

    /// <summary>
    /// Caja (tornillería, cerraduras, baldosas).
    /// </summary>
    Caja = 12,

    /// <summary>
    /// Tubo de 6 metros (PVC sanitario, conduit, estructural).
    /// </summary>
    Tubo = 13
}
