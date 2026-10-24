// Repositorio encargado de consultar y guardar facturas en la tabla "ventas".
// También une los datos de las tablas "clientes", "metodos_pago", "detalles_venta", "productos" y "despachos".
// Permite buscar ventas con todos sus detalles por número o ID, por cliente o entre dos fechas.
using Firmeza.Domain.Abstractions;
using Firmeza.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.Infrastructure.Persistence.Repositories;

public class VentaRepository : GenericRepository<Venta>, IVentaRepository
{
    // Inicializa el repositorio de ventas con la conexión a la base de datos.
    public VentaRepository(ApplicationDbContext context) : base(context)
    {
    }

    // Busca una venta por su número de factura incluyendo cliente, método de pago, productos vendidos y despacho.
    public async Task<Venta?> GetByNumeroVentaAsync(string numeroVenta, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(v => v.Cliente)
            .Include(v => v.MetodoPago)
            .Include(v => v.DetallesVenta)
                .ThenInclude(d => d.Producto)
            .Include(v => v.Despacho)
            .FirstOrDefaultAsync(v => v.NumeroVenta == numeroVenta, cancellationToken);
    }

    // Busca una venta por su ID incluyendo toda la información del cliente, pago, productos y despacho.
    public async Task<Venta?> GetVentaConDetallesAsync(int ventaId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(v => v.Cliente)
            .Include(v => v.MetodoPago)
            .Include(v => v.DetallesVenta)
                .ThenInclude(d => d.Producto)
            .Include(v => v.Despacho)
            .FirstOrDefaultAsync(v => v.Id == ventaId, cancellationToken);
    }

    // Obtiene todas las compras realizadas por un cliente específico, ordenadas desde la más reciente.
    public async Task<IReadOnlyList<Venta>> GetVentasByClienteAsync(int clienteId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Include(v => v.MetodoPago)
            .Where(v => v.ClienteId == clienteId)
            .OrderByDescending(v => v.Fecha)
            .ToListAsync(cancellationToken);
    }

    // Obtiene las ventas registradas dentro de un rango de fechas incluyendo el cliente y el método de pago.
    public async Task<IReadOnlyList<Venta>> GetVentasByRangoFechasAsync(
        DateTime fechaInicio,
        DateTime fechaFin,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Include(v => v.Cliente)
            .Include(v => v.MetodoPago)
            .Where(v => v.Fecha >= fechaInicio && v.Fecha <= fechaFin)
            .OrderByDescending(v => v.Fecha)
            .ToListAsync(cancellationToken);
    }
}
