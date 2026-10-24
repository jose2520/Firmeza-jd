// Este archivo representa un vehículo de transporte o una máquina pesada perteneciente a la flota de Firmeza.
// En el negocio, sirve para administrar las volquetas, camiones, mezcladoras y retroexcavadoras,
// controlando su placa, capacidad de carga, tarifa de alquiler por día y si están libres o en uso.

namespace Firmeza.Domain.Entities;

// Representa un vehículo de carga o maquinaria de construcción registrado en la tabla 'vehiculos'.
public class Vehiculo
{
    // Número identificador único del vehículo o máquina en la base de datos.
    public int Id { get; set; }

    // Número de placa de tránsito o serial único que identifica al vehículo o equipo.
    public string Placa { get; set; } = null!;

    // Marca del fabricante del vehículo o maquinaria (por ejemplo: Caterpillar, Chevrolet, Kenworth).
    public string Marca { get; set; } = null!;

    // Línea o año del modelo del vehículo o equipo.
    public string Modelo { get; set; } = null!;

    // Clasificación del equipo (por ejemplo: Volqueta, Camión, Mezcladora o Retroexcavadora).
    public string TipoVehiculo { get; set; } = null!;

    // Capacidad máxima de peso que puede transportar o soportar el equipo, medida en toneladas.
    public decimal? CapacidadCargaTon { get; set; }

    // Precio que se cobra al cliente por cada día de alquiler de este vehículo o maquinaria.
    public decimal TarifaDiaria { get; set; }

    // Situación operativa actual del equipo (puede ser Disponible, Alquilado o En Mantenimiento).
    public string Estado { get; set; } = "Disponible"; // Disponible, Alquilado, En Mantenimiento

    // Indica si el vehículo sigue formando parte activa de la flota (true) o si fue dado de baja (false).
    public bool Activo { get; set; } = true;

    // Fecha y hora en que el vehículo fue registrado en el sistema.
    public DateTime CreadoEn { get; set; }

    // Historial de todos los contratos de alquiler en los que ha participado este vehículo.
    public virtual ICollection<Alquiler> Alquileres { get; set; } = new List<Alquiler>();
}
