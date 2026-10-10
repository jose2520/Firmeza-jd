namespace Firmeza.Domain.Enums;

/// <summary>
/// Representa el ciclo de vigencia de un contrato de alquiler de maquinaria o vehículos.
/// </summary>
public enum EstadoAlquiler
{
    /// <summary>
    /// Contrato en ejecución; maquinaria operando en obra con el cliente.
    /// </summary>
    Activo = 1,

    /// <summary>
    /// Contrato cumplido, maquinaria devuelta a patio e inspeccionada.
    /// </summary>
    Finalizado = 2,

    /// <summary>
    /// Contrato cancelado antes de iniciarse o por incumplimiento contractual.
    /// </summary>
    Cancelado = 3
}
