// Este archivo define las operaciones para consultar y administrar la flota de vehículos y maquinaria.
// En el negocio de Firmeza, sirve para buscar equipos por su placa, saber cuáles están libres para
// alquilar a los clientes y filtrarlos según su tipo (como volquetas, mezcladoras o retroexcavadoras).

using Firmeza.Domain.Entities;

namespace Firmeza.Domain.Abstractions;

// Define las funciones para consultar y gestionar los vehículos de transporte y maquinaria pesada.
public interface IVehiculoRepository : IRepository<Vehiculo>
{
    // Busca un vehículo o máquina específica usando su número de placa o código de serie.
    Task<Vehiculo?> GetByPlacaAsync(string placa, CancellationToken cancellationToken = default);

    // Obtiene la lista de vehículos que están libres y listos para ser entregados en alquiler.
    Task<IReadOnlyList<Vehiculo>> GetDisponiblesParaAlquilerAsync(CancellationToken cancellationToken = default);

    // Devuelve los vehículos filtrados según su categoría o tipo (por ejemplo, Volqueta o Mezcladora).
    Task<IReadOnlyList<Vehiculo>> GetByTipoAsync(string tipo, CancellationToken cancellationToken = default);
}
