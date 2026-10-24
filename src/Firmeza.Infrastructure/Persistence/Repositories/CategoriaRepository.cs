// Repositorio encargado de consultar y gestionar registros de la tabla "categorias".
// Permite buscar categorías por su nombre exacto y listar únicamente las categorías activas.
// Ayuda a organizar el catálogo de productos de construcción y herramientas.
using Firmeza.Domain.Abstractions;
using Firmeza.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.Infrastructure.Persistence.Repositories;

public class CategoriaRepository : GenericRepository<Categoria>, ICategoriaRepository
{
    // Inicializa el repositorio con la conexión a la base de datos.
    public CategoriaRepository(ApplicationDbContext context) : base(context)
    {
    }

    // Busca una categoría en la base de datos que coincida exactamente con el nombre indicado.
    public async Task<Categoria?> GetByNombreAsync(string nombre, CancellationToken cancellationToken = default)
    {
        return await DbSet.FirstOrDefaultAsync(c => c.Nombre == nombre, cancellationToken);
    }

    // Obtiene todas las categorías que están marcadas como activas, ordenadas alfabéticamente por nombre.
    public async Task<IReadOnlyList<Categoria>> GetActivasAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Where(c => c.Activo)
            .OrderBy(c => c.Nombre)
            .ToListAsync(cancellationToken);
    }
}
