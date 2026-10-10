using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Firmeza.Domain.Entities;

namespace Firmeza.Domain.Abstractions;

/// <summary>
/// Contrato de repositorio de dominio para la flota de Vehículos de transporte y Maquinaria pesada.
/// </summary>
public interface IVehiculoRepository : IRepository<Vehiculo>
{
    /// <summary>
    /// Busca un vehículo o maquinaria por su número de placa o serial único.
    /// </summary>
    Task<Vehiculo?> GetByPlacaAsync(string placa, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene los vehículos que se encuentran en estado 'Disponible' para ser asignados en alquiler.
    /// </summary>
    Task<IReadOnlyList<Vehiculo>> GetDisponiblesParaAlquilerAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene los vehículos filtrados por tipo (ej. Volqueta, Retroexcavadora, Mezcladora).
    /// </summary>
    Task<IReadOnlyList<Vehiculo>> GetByTipoAsync(string tipo, CancellationToken cancellationToken = default);
}
