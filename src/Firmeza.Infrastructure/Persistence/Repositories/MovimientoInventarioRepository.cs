using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Firmeza.Domain.Abstractions;
using Firmeza.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.Infrastructure.Persistence.Repositories;

/// <summary>
/// Implementación de repositorio para movimientos de inventario y Kardex.
/// </summary>
public class MovimientoInventarioRepository : GenericRepository<MovimientoInventario>, IMovimientoInventarioRepository
{
    public MovimientoInventarioRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<MovimientoInventario>> GetByProductoIdAsync(
        int productoId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(m => m.ProductoId == productoId)
            .OrderByDescending(m => m.Fecha)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<MovimientoInventario>> GetUltimosMovimientosAsync(
        int cantidad = 50,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(m => m.Producto)
            .OrderByDescending(m => m.Fecha)
            .Take(cantidad)
            .ToListAsync(cancellationToken);
    }
}
