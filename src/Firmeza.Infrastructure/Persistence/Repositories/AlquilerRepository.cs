using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Firmeza.Domain.Abstractions;
using Firmeza.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.Infrastructure.Persistence.Repositories;

/// <summary>
/// Implementación de repositorio para contratos de Alquiler de maquinaria y vehículos.
/// </summary>
public class AlquilerRepository : GenericRepository<Alquiler>, IAlquilerRepository
{
    public AlquilerRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Alquiler?> GetByNumeroContratoAsync(string numeroContrato, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(a => a.Cliente)
            .Include(a => a.Vehiculo)
            .FirstOrDefaultAsync(a => a.NumeroContrato == numeroContrato, cancellationToken);
    }

    public async Task<IReadOnlyList<Alquiler>> GetAlquileresActivosAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(a => a.Cliente)
            .Include(a => a.Vehiculo)
            .Where(a => a.Estado == "Activo")
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Alquiler>> GetAlquileresByClienteAsync(int clienteId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(a => a.Vehiculo)
            .Where(a => a.ClienteId == clienteId)
            .OrderByDescending(a => a.FechaInicio)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Alquiler>> GetAlquileresByVehiculoAsync(int vehiculoId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(a => a.Cliente)
            .Where(a => a.VehiculoId == vehiculoId)
            .OrderByDescending(a => a.FechaInicio)
            .ToListAsync(cancellationToken);
    }
}
