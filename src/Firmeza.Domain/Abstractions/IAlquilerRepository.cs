// Este archivo define las operaciones de consulta y guardado para los alquileres de maquinaria y vehículos.
// En el negocio de Firmeza, sirve para buscar contratos por su código, revisar qué equipos siguen
// prestados y consultar el historial de alquileres de cada cliente o vehículo.

using Firmeza.Domain.Entities;

namespace Firmeza.Domain.Abstractions;

// Define las funciones para consultar y guardar contratos de alquiler en la base de datos.
public interface IAlquilerRepository : IRepository<Alquiler>
{
    // Busca un contrato de alquiler específico usando su código único (por ejemplo, ALQ-2026-0001).
    Task<Alquiler?> GetByNumeroContratoAsync(string numeroContrato, CancellationToken cancellationToken = default);

    // Obtiene la lista de todos los alquileres que están actualmente en curso.
    Task<IReadOnlyList<Alquiler>> GetAlquileresActivosAsync(CancellationToken cancellationToken = default);

    // Devuelve todos los contratos de alquiler que ha realizado un cliente en particular.
    Task<IReadOnlyList<Alquiler>> GetAlquileresByClienteAsync(int clienteId, CancellationToken cancellationToken = default);

    // Devuelve el historial de contratos de alquiler en los que se ha usado un vehículo específico.
    Task<IReadOnlyList<Alquiler>> GetAlquileresByVehiculoAsync(int vehiculoId, CancellationToken cancellationToken = default);
}
