using System;
using Firmeza.Domain.Common;
using Firmeza.Domain.Events;
using Firmeza.Domain.Exceptions;

namespace Firmeza.Domain.Entities;

/// <summary>
/// Representa un contrato de alquiler de vehículos o maquinaria de construcción suscrito por un cliente.
/// Responde al requerimiento del servicio de alquiler (US 4: "adquirir materiales de construcción o alquilar vehículos").
/// Mapea contra la tabla 'alquileres' en PostgreSQL ('firmeza_db').
/// </summary>
public partial class Alquiler : AggregateRoot
{
    public int Id { get; set; }
    public string NumeroContrato { get; set; } = null!;
    public int ClienteId { get; set; }
    public int VehiculoId { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public int DiasAlquiler { get; set; }
    public decimal ValorDiario { get; set; }
    public decimal Total { get; set; }
    public string Estado { get; set; } = "Activo"; // Activo, Finalizado, Cancelado
    public DateTime CreadoEn { get; set; }

    public virtual Cliente Cliente { get; set; } = null!;
    public virtual Vehiculo Vehiculo { get; set; } = null!;

    // =========================================================================
    // MÉTODOS DE DOMINIO Y REGLAS DE NEGOCIO (DDD)
    // =========================================================================

    /// <summary>
    /// Inicia el contrato de alquiler validando días y fechas, y emite VehiculoAlquiladoDomainEvent.
    /// </summary>
    public void IniciarAlquiler()
    {
        if (DiasAlquiler <= 0)
            throw new ReglaNegocioException("Alquiler.DiasInvalidos", "La duración del alquiler debe ser de al menos 1 día.");

        if (FechaFin <= FechaInicio)
            throw new ReglaNegocioException("Alquiler.FechasInconsistentes", "La fecha fin del alquiler debe ser estrictamente posterior a la fecha de inicio.");

        Total = DiasAlquiler * ValorDiario;
        Estado = "Activo";

        AddDomainEvent(new VehiculoAlquiladoDomainEvent(
            Id, NumeroContrato, VehiculoId, Vehiculo?.Placa ?? string.Empty, ClienteId, DiasAlquiler, Total, DateTime.UtcNow));
    }

    /// <summary>
    /// Finaliza el contrato de alquiler y emite VehiculoLiberadoDomainEvent para liberar el vehículo a Disponible.
    /// </summary>
    public void FinalizarAlquiler()
    {
        if (Estado == "Finalizado")
            throw new ReglaNegocioException("Alquiler.YaFinalizado", "El contrato de alquiler ya se encuentra finalizado.");

        Estado = "Finalizado";

        AddDomainEvent(new VehiculoLiberadoDomainEvent(
            VehiculoId, Vehiculo?.Placa ?? string.Empty, NumeroContrato, DateTime.UtcNow));
    }
}
