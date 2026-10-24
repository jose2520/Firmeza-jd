// Repositorio encargado de consultar y registrar cambios de stock en la tabla "movimientos_inventario".
// También se conecta con la tabla "productos" para mostrar el nombre del producto en los reportes.
// Permite consultar el historial de entradas y salidas de un producto o ver los movimientos más recientes.
using Firmeza.Domain.Abstractions;
using Firmeza.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.Infrastructure.Persistence.Repositories;

public class MovimientoInventarioRepository : GenericRepository<MovimientoInventario>, IMovimientoInventarioRepository
{
    // Inicializa el repositorio de movimientos de inventario con la conexión a la base de datos.
    public MovimientoInventarioRepository(ApplicationDbContext context) : base(context)
    {
    }

    // Obtiene todos los movimientos de inventario de un producto específico, ordenados del más reciente al más antiguo.
    public async Task<IReadOnlyList<MovimientoInventario>> GetByProductoIdAsync(
        int productoId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Where(m => m.ProductoId == productoId)
            .OrderByDescending(m => m.Fecha)
            .ToListAsync(cancellationToken);
    }

    // Obtiene los últimos movimientos de inventario registrados en el sistema incluyendo los datos del producto.
    public async Task<IReadOnlyList<MovimientoInventario>> GetUltimosMovimientosAsync(
        int cantidad = 50,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Include(m => m.Producto)
            .OrderByDescending(m => m.Fecha)
            .Take(cantidad)
            .ToListAsync(cancellationToken);
    }
}
