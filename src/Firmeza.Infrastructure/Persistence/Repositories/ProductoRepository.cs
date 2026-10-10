using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Firmeza.Domain.Abstractions;
using Firmeza.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.Infrastructure.Persistence.Repositories;

/// <summary>
/// Implementación de repositorio para la entidad Producto.
/// </summary>
public class ProductoRepository : GenericRepository<Producto>, IProductoRepository
{
    public ProductoRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Producto?> GetByCodigoAsync(string codigo, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.Categoria)
            .Include(p => p.Proveedor)
            .FirstOrDefaultAsync(p => p.Codigo == codigo, cancellationToken);
    }

    public async Task<IReadOnlyList<Producto>> GetByCategoriaIdAsync(int categoriaId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(p => p.CategoriaId == categoriaId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Producto>> GetProductosConStockBajoAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(p => p.Stock <= 10)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsCodigoAsync(string codigo, int? excludeId = null, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(
            p => p.Codigo == codigo && (!excludeId.HasValue || p.Id != excludeId.Value),
            cancellationToken);
    }
}
