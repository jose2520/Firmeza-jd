namespace Firmeza.Domain.Enums;

/// <summary>
/// Representa el estado de operatividad y disponibilidad de un vehículo o maquinaria de construcción.
/// </summary>
public enum EstadoVehiculo
{
    /// <summary>
    /// Vehículo o máquina lista en patio para servicios de flete o alquiler.
    /// </summary>
    Disponible = 1,

    /// <summary>
    /// Vehículo actualmente en servicio activo bajo contrato de alquiler a un cliente.
    /// </summary>
    Alquilado = 2,

    /// <summary>
    /// Vehículo en taller para revisión técnica, preventiva o correctiva.
    /// </summary>
    EnMantenimiento = 3
}
