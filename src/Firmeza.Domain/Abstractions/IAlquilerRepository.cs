using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Firmeza.Domain.Entities;

namespace Firmeza.Domain.Abstractions;

/// <summary>
/// Contrato de repositorio de dominio para la gestión de contratos de Alquiler de maquinaria y vehículos.
/// </summary>
public interface IAlquilerRepository : IRepository<Alquiler>
{
    /// <summary>
    /// Busca un contrato de alquiler por su número consecutivo (ej. ALQ-2026-0001).
    /// </summary>
    Task<Alquiler?> GetByNumeroContratoAsync(string numeroContrato, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene todos los alquileres en estado 'Activo' en curso.
    /// </summary>
    Task<IReadOnlyList<Alquiler>> GetAlquileresActivosAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene los contratos de alquiler asociados a un cliente específico.
    /// </summary>
    Task<IReadOnlyList<Alquiler>> GetAlquileresByClienteAsync(int clienteId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene los contratos de alquiler en los que ha participado un vehículo determinado.
    /// </summary>
    Task<IReadOnlyList<Alquiler>> GetAlquileresByVehiculoAsync(int vehiculoId, CancellationToken cancellationToken = default);
}
