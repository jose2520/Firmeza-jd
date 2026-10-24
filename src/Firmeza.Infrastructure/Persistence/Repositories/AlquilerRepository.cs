// Repositorio encargado de consultar y guardar contratos de alquiler en la tabla "alquileres".
// También consulta información relacionada de las tablas "clientes" y "vehiculos".
// Permite buscar contratos por su número, ver los que están activos o filtrar por cliente y vehículo.
using Firmeza.Domain.Abstractions;
using Firmeza.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.Infrastructure.Persistence.Repositories;

public class AlquilerRepository : GenericRepository<Alquiler>, IAlquilerRepository
{
    // Inicializa el repositorio con la conexión a la base de datos principal.
    public AlquilerRepository(ApplicationDbContext context) : base(context)
    {
    }

    // Busca un contrato de alquiler por su número incluyendo los datos del cliente y del vehículo.
    public async Task<Alquiler?> GetByNumeroContratoAsync(string numeroContrato, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(a => a.Cliente)
            .Include(a => a.Vehiculo)
            .FirstOrDefaultAsync(a => a.NumeroContrato == numeroContrato, cancellationToken);
    }

    // Obtiene la lista de todos los alquileres que tienen estado "Activo" junto con su cliente y vehículo.
    public async Task<IReadOnlyList<Alquiler>> GetAlquileresActivosAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Include(a => a.Cliente)
            .Include(a => a.Vehiculo)
            .Where(a => a.Estado == "Activo")
            .ToListAsync(cancellationToken);
    }

    // Devuelve el historial de alquileres de un cliente específico ordenado desde el más reciente.
    public async Task<IReadOnlyList<Alquiler>> GetAlquileresByClienteAsync(int clienteId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Include(a => a.Vehiculo)
            .Where(a => a.ClienteId == clienteId)
            .OrderByDescending(a => a.FechaInicio)
            .ToListAsync(cancellationToken);
    }

    // Devuelve el historial de alquileres de un vehículo específico ordenado desde el más reciente.
    public async Task<IReadOnlyList<Alquiler>> GetAlquileresByVehiculoAsync(int vehiculoId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Include(a => a.Cliente)
            .Where(a => a.VehiculoId == vehiculoId)
            .OrderByDescending(a => a.FechaInicio)
            .ToListAsync(cancellationToken);
    }
}
