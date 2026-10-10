using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Firmeza.Domain.Abstractions;
using Firmeza.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.Infrastructure.Persistence.Repositories;

/// <summary>
/// Implementación de repositorio para la logística de Despachos y entregas en obra.
/// </summary>
public class DespachoRepository : GenericRepository<Despacho>, IDespachoRepository
{
    public DespachoRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Despacho?> GetByVentaIdAsync(int ventaId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(d => d.Venta)
            .FirstOrDefaultAsync(d => d.VentaId == ventaId, cancellationToken);
    }

    public async Task<IReadOnlyList<Despacho>> GetDespachosPendientesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(d => d.Venta)
            .Where(d => d.Estado == "Pendiente")
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Despacho>> GetDespachosEnRutaAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(d => d.Venta)
            .Where(d => d.Estado == "En Ruta")
            .ToListAsync(cancellationToken);
    }
}
