using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Firmeza.Domain.Abstractions;
using Firmeza.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.Infrastructure.Persistence.Repositories;

/// <summary>
/// Implementación de repositorio para la entidad Venta.
/// </summary>
public class VentaRepository : GenericRepository<Venta>, IVentaRepository
{
    public VentaRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Venta?> GetByNumeroVentaAsync(string numeroVenta, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(v => v.Cliente)
            .Include(v => v.MetodoPago)
            .Include(v => v.DetallesVenta)
                .ThenInclude(d => d.Producto)
            .Include(v => v.Despacho)
            .FirstOrDefaultAsync(v => v.NumeroVenta == numeroVenta, cancellationToken);
    }

    public async Task<Venta?> GetVentaConDetallesAsync(int ventaId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(v => v.Cliente)
            .Include(v => v.MetodoPago)
            .Include(v => v.DetallesVenta)
                .ThenInclude(d => d.Producto)
            .Include(v => v.Despacho)
            .FirstOrDefaultAsync(v => v.Id == ventaId, cancellationToken);
    }

    public async Task<IReadOnlyList<Venta>> GetVentasByClienteAsync(int clienteId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(v => v.MetodoPago)
            .Where(v => v.ClienteId == clienteId)
            .OrderByDescending(v => v.Fecha)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Venta>> GetVentasByRangoFechasAsync(
        DateTime fechaInicio,
        DateTime fechaFin,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(v => v.Cliente)
            .Include(v => v.MetodoPago)
            .Where(v => v.Fecha >= fechaInicio && v.Fecha <= fechaFin)
            .OrderByDescending(v => v.Fecha)
            .ToListAsync(cancellationToken);
    }
}
