// Archivo: EstadoVehiculo.cs
// Define la situación actual de cada vehículo o máquina pesada de la empresa.
// Sirve para saber rápidamente qué equipos se pueden alquilar y cuáles están ocupados o en taller.

namespace Firmeza.Domain.Enums;

// Estados de disponibilidad operativa de los vehículos y maquinaria de construcción.
public enum EstadoVehiculo
{
    // El vehículo o máquina está libre en el patio y listo para ser alquilado o usado.
    Disponible = 1,

    // El equipo está actualmente entregado a un cliente bajo un contrato de alquiler.
    Alquilado = 2,

    // El equipo se encuentra en el taller recibiendo revisión o reparación mecánica.
    EnMantenimiento = 3
}
