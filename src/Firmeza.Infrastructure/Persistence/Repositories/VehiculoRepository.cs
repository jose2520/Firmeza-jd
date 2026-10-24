// Repositorio encargado de consultar y guardar maquinaria y camiones en la tabla "vehiculos".
// Permite buscar un vehículo por su número de placa, ver cuáles están disponibles para alquilar
// o filtrar la flota según el tipo de vehículo o maquinaria.
using Firmeza.Domain.Abstractions;
using Firmeza.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.Infrastructure.Persistence.Repositories;

public class VehiculoRepository : GenericRepository<Vehiculo>, IVehiculoRepository
{
    // Inicializa el repositorio de vehículos con la conexión a la base de datos.
    public VehiculoRepository(ApplicationDbContext context) : base(context)
    {
    }

    // Busca un vehículo específico en la base de datos usando su número de placa.
    public async Task<Vehiculo?> GetByPlacaAsync(string placa, CancellationToken cancellationToken = default)
    {
        return await DbSet.FirstOrDefaultAsync(v => v.Placa == placa, cancellationToken);
    }

    // Obtiene todos los vehículos que están activos y tienen estado "Disponible" para ser alquilados.
    public async Task<IReadOnlyList<Vehiculo>> GetDisponiblesParaAlquilerAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Where(v => v.Estado == "Disponible" && v.Activo)
            .OrderBy(v => v.TipoVehiculo)
            .ToListAsync(cancellationToken);
    }

    // Devuelve la lista de vehículos que pertenecen a un tipo específico (por ejemplo, Volqueta o Retroexcavadora).
    public async Task<IReadOnlyList<Vehiculo>> GetByTipoAsync(string tipo, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Where(v => v.TipoVehiculo == tipo)
            .ToListAsync(cancellationToken);
    }
}
