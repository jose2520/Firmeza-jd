// Repositorio encargado de consultar y guardar empresas en la tabla "proveedores".
// Permite buscar un proveedor por su número de NIT o listar solo los proveedores activos.
// Facilita la gestión de quiénes suministran materiales y herramientas a Firmeza.
using Firmeza.Domain.Abstractions;
using Firmeza.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.Infrastructure.Persistence.Repositories;

public class ProveedorRepository : GenericRepository<Proveedor>, IProveedorRepository
{
    // Inicializa el repositorio de proveedores con la conexión a la base de datos.
    public ProveedorRepository(ApplicationDbContext context) : base(context)
    {
    }

    // Busca un proveedor en la base de datos usando su número de NIT.
    public async Task<Proveedor?> GetByNitAsync(string nit, CancellationToken cancellationToken = default)
    {
        return await DbSet.FirstOrDefaultAsync(p => p.Nit == nit, cancellationToken);
    }

    // Obtiene la lista de proveedores que están activos, ordenados alfabéticamente por su razón social.
    public async Task<IReadOnlyList<Proveedor>> GetActivosAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Where(p => p.Activo)
            .OrderBy(p => p.RazonSocial)
            .ToListAsync(cancellationToken);
    }
}
