namespace Firmeza.Domain.Enums;

/// <summary>
/// Tipos de documento de identificación para clientes, proveedores y contratistas en Colombia.
/// </summary>
public enum TipoIdentificacion
{
    /// <summary>
    /// Cédula de Ciudadanía (persona natural colombiana).
    /// </summary>
    CedulaCiudadania = 1,

    /// <summary>
    /// Número de Identificación Tributaria (persona jurídica o comerciante con DIAN).
    /// </summary>
    NIT = 2,

    /// <summary>
    /// Cédula de Extranjería.
    /// </summary>
    CedulaExtranjeria = 3,

    /// <summary>
    /// Pasaporte para clientes extranjeros.
    /// </summary>
    Pasaporte = 4,

    /// <summary>
    /// Registro Único Tributario.
    /// </summary>
    RUT = 5,

    /// <summary>
    /// Tarjeta de Identidad.
    /// </summary>
    TarjetaIdentidad = 6
}
