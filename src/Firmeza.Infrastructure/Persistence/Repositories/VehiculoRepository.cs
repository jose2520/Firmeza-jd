using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Firmeza.Domain.Abstractions;
using Firmeza.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.Infrastructure.Persistence.Repositories;

/// <summary>
/// Implementación de repositorio para la flota de Vehículos y maquinaria de construcción.
/// </summary>
public class VehiculoRepository : GenericRepository<Vehiculo>, IVehiculoRepository
{
    public VehiculoRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Vehiculo?> GetByPlacaAsync(string placa, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(v => v.Placa == placa, cancellationToken);
    }

    public async Task<IReadOnlyList<Vehiculo>> GetDisponiblesParaAlquilerAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(v => v.Estado == "Disponible" && v.Activo)
            .OrderBy(v => v.TipoVehiculo)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Vehiculo>> GetByTipoAsync(string tipo, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(v => v.TipoVehiculo == tipo)
            .ToListAsync(cancellationToken);
    }
}
