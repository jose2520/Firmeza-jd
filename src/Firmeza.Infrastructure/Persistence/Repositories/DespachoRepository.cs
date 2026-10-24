// Repositorio encargado de consultar y guardar envíos en la tabla "despachos".
// También carga la información relacionada de la tabla "ventas" para cada entrega.
// Permite buscar el despacho de una factura o listar los envíos pendientes y en camino.
using Firmeza.Domain.Abstractions;
using Firmeza.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.Infrastructure.Persistence.Repositories;

public class DespachoRepository : GenericRepository<Despacho>, IDespachoRepository
{
    // Inicializa el repositorio de despachos con el contexto de la base de datos.
    public DespachoRepository(ApplicationDbContext context) : base(context)
    {
    }

    // Busca los datos de envío asociados a una venta específica por el ID de la venta.
    public async Task<Despacho?> GetByVentaIdAsync(int ventaId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(d => d.Venta)
            .FirstOrDefaultAsync(d => d.VentaId == ventaId, cancellationToken);
    }

    // Obtiene la lista de todos los despachos que están en estado "Pendiente" de salir.
    public async Task<IReadOnlyList<Despacho>> GetDespachosPendientesAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Include(d => d.Venta)
            .Where(d => d.Estado == "Pendiente")
            .ToListAsync(cancellationToken);
    }

    // Obtiene la lista de todos los despachos que ya están marcados como "En Ruta" hacia el cliente.
    public async Task<IReadOnlyList<Despacho>> GetDespachosEnRutaAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Include(d => d.Venta)
            .Where(d => d.Estado == "En Ruta")
            .ToListAsync(cancellationToken);
    }
}
