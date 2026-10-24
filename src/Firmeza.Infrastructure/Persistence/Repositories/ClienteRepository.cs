// Repositorio encargado de consultar y guardar información en la tabla "clientes".
// Permite buscar clientes por su documento de identificación o por su correo electrónico.
// Incluye validaciones para verificar si un documento o correo ya pertenece a otro cliente.
using Firmeza.Domain.Abstractions;
using Firmeza.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.Infrastructure.Persistence.Repositories;

public class ClienteRepository : GenericRepository<Cliente>, IClienteRepository
{
    // Inicializa el repositorio de clientes con el contexto de la base de datos.
    public ClienteRepository(ApplicationDbContext context) : base(context)
    {
    }

    // Busca un cliente por su número de documento de identificación.
    public async Task<Cliente?> GetByIdentificacionAsync(string identificacion, CancellationToken cancellationToken = default)
    {
        return await DbSet.FirstOrDefaultAsync(c => c.Identificacion == identificacion, cancellationToken);
    }

    // Busca un cliente por su dirección de correo electrónico.
    public async Task<Cliente?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await DbSet.FirstOrDefaultAsync(c => c.Email == email, cancellationToken);
    }

    // Comprueba si ya existe un cliente con esa identificación, permitiendo ignorar un ID al editar.
    public async Task<bool> ExistsIdentificacionAsync(string identificacion, int? excludeId = null, CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(
            c => c.Identificacion == identificacion && (!excludeId.HasValue || c.Id != excludeId.Value),
            cancellationToken);
    }

    // Comprueba si ya existe un cliente con ese correo electrónico, permitiendo ignorar un ID al editar.
    public async Task<bool> ExistsEmailAsync(string email, int? excludeId = null, CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(
            c => c.Email == email && (!excludeId.HasValue || c.Id != excludeId.Value),
            cancellationToken);
    }
}
