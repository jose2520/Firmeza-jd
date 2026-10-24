// Este archivo representa un contrato de alquiler de maquinaria pesada o vehículos de transporte.
// En el negocio de Firmeza, sirve para registrar qué cliente alquila un equipo, por cuántos días,
// cuál es el costo diario y total, y controlar cuándo inicia y finaliza el servicio.

using Firmeza.Domain.Common;
using Firmeza.Domain.Events;
using Firmeza.Domain.Exceptions;

namespace Firmeza.Domain.Entities;

// Representa los datos y reglas de un contrato de alquiler de vehículos o maquinaria en la tabla 'alquileres'.
public class Alquiler : AggregateRoot
{
    // Número identificador único del registro de alquiler en la base de datos.
    public int Id { get; set; }

    // Código consecutivo único del contrato de alquiler (por ejemplo, ALQ-2026-0001).
    public string NumeroContrato { get; set; } = null!;

    // Identificador del cliente que toma en alquiler el vehículo o maquinaria.
    public int ClienteId { get; set; }

    // Identificador del vehículo o máquina que se entrega en alquiler.
    public int VehiculoId { get; set; }

    // Fecha y hora en que comienza el alquiler del equipo.
    public DateTime FechaInicio { get; set; }

    // Fecha y hora acordada para la devolución del equipo.
    public DateTime FechaFin { get; set; }

    // Cantidad total de días que durará el alquiler.
    public int DiasAlquiler { get; set; }

    // Precio cobrado por cada día de uso del vehículo o maquinaria.
    public decimal ValorDiario { get; set; }

    // Valor total a pagar por todo el contrato (días de alquiler multiplicados por el valor diario).
    public decimal Total { get; set; }

    // Estado actual del contrato de alquiler (puede ser Activo, Finalizado o Cancelado).
    public string Estado { get; set; } = "Activo"; // Activo, Finalizado, Cancelado

    // Fecha y hora exacta en que se creó el contrato en el sistema.
    public DateTime CreadoEn { get; set; }

    // Datos completos del cliente asociado a este contrato de alquiler.
    public virtual Cliente Cliente { get; set; } = null!;

    // Datos completos del vehículo o maquinaria alquilada en este contrato.
    public virtual Vehiculo Vehiculo { get; set; } = new();

    // Revisa que los días y fechas sean válidos, calcula el total a pagar, activa el contrato y avisa que el vehículo fue alquilado.
    public void IniciarAlquiler()
    {
        if (DiasAlquiler <= 0)
            throw new ReglaNegocioException("Alquiler.DiasInvalidos", "La duración del alquiler debe ser de al menos 1 día.");

        if (FechaFin <= FechaInicio)
            throw new ReglaNegocioException("Alquiler.FechasInconsistentes", "La fecha fin del alquiler debe ser estrictamente posterior a la fecha de inicio.");

        Total = DiasAlquiler * ValorDiario;
        Estado = "Activo";

        AddDomainEvent(new VehiculoAlquiladoDomainEvent(
            Id, NumeroContrato, VehiculoId, Vehiculo.Placa, ClienteId, DiasAlquiler, Total, DateTime.UtcNow));
    }

    // Marca el contrato como terminado cuando el cliente devuelve el equipo y envía el aviso para dejar el vehículo disponible otra vez.
    public void FinalizarAlquiler()
    {
        if (Estado == "Finalizado")
            throw new ReglaNegocioException("Alquiler.YaFinalizado", "El contrato de alquiler ya se encuentra finalizado.");

        Estado = "Finalizado";

        AddDomainEvent(new VehiculoLiberadoDomainEvent(
            VehiculoId, Vehiculo.Placa, NumeroContrato, DateTime.UtcNow));
    }
}
