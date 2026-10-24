// Archivo: TipoIdentificacion.cs
// Define los tipos de documentos de identidad aceptados para registrar clientes y proveedores.
// Sirve para cumplir con los requisitos legales y de facturación en Colombia.

namespace Firmeza.Domain.Enums;

// Documentos de identificación válidos para personas naturales y empresas en Colombia.
public enum TipoIdentificacion
{
    // Cédula de Ciudadanía para personas naturales colombianas mayores de edad.
    CedulaCiudadania = 1,

    // Número de Identificación Tributaria para empresas o comerciantes registrados en la DIAN.
    Nit = 2,

    // Cédula de Extranjería para residentes extranjeros en el país.
    CedulaExtranjeria = 3,

    // Pasaporte oficial para ciudadanos extranjeros no residentes.
    Pasaporte = 4,

    // Registro Único Tributario para identificar contribuyentes y actividades económicas.
    Rut = 5,

    // Tarjeta de Identidad para menores de edad cuando aplique su registro.
    TarjetaIdentidad = 6
}
