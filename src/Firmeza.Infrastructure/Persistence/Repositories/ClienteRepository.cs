using System.Threading;
using System.Threading.Tasks;
using Firmeza.Domain.Abstractions;
using Firmeza.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.Infrastructure.Persistence.Repositories;

/// <summary>
/// Implementación de repositorio para la entidad Cliente.
/// </summary>
public class ClienteRepository : GenericRepository<Cliente>, IClienteRepository
{
    public ClienteRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Cliente?> GetByIdentificacionAsync(string identificacion, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(c => c.Identificacion == identificacion, cancellationToken);
    }

    public async Task<Cliente?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(c => c.Email == email, cancellationToken);
    }

    public async Task<bool> ExistsIdentificacionAsync(string identificacion, int? excludeId = null, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(
            c => c.Identificacion == identificacion && (!excludeId.HasValue || c.Id != excludeId.Value),
            cancellationToken);
    }

    public async Task<bool> ExistsEmailAsync(string email, int? excludeId = null, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(
            c => c.Email == email && (!excludeId.HasValue || c.Id != excludeId.Value),
            cancellationToken);
    }
}
