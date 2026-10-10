using System;
using System.Collections.Generic;

namespace Firmeza.Domain.Entities;

/// <summary>
/// Representa un vehículo de carga o maquinaria de construcción (volqueta, camión, mezcladora, etc.)
/// destinado a servicios de transporte/flete y alquiler a constructores y clientes.
/// Mapea contra la tabla 'vehiculos' en PostgreSQL ('firmeza_db').
/// </summary>
public partial class Vehiculo
{
    public int Id { get; set; }
    public string Placa { get; set; } = null!;
    public string Marca { get; set; } = null!;
    public string Modelo { get; set; } = null!;
    public string TipoVehiculo { get; set; } = null!;
    public decimal? CapacidadCargaTon { get; set; }
    public decimal TarifaDiaria { get; set; }
    public string Estado { get; set; } = "Disponible"; // Disponible, Alquilado, En Mantenimiento
    public bool Activo { get; set; } = true;
    public DateTime CreadoEn { get; set; }

    public virtual ICollection<Alquiler> Alquileres { get; set; } = new List<Alquiler>();
}
