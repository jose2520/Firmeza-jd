// Repositorio encargado de consultar y guardar artículos en la tabla "productos".
// También consulta las tablas "categorias" y "proveedores" para obtener la información completa del producto.
// Permite buscar por código, filtrar por categoría, detectar stock bajo y validar códigos repetidos.
using Firmeza.Domain.Abstractions;
using Firmeza.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.Infrastructure.Persistence.Repositories;

public class ProductoRepository : GenericRepository<Producto>, IProductoRepository
{
    // Inicializa el repositorio de productos con la conexión a la base de datos.
    public ProductoRepository(ApplicationDbContext context) : base(context)
    {
    }

    // Busca un producto por su código único incluyendo los datos de su categoría y proveedor.
    public async Task<Producto?> GetByCodigoAsync(string codigo, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(p => p.Categoria)
            .Include(p => p.Proveedor)
            .FirstOrDefaultAsync(p => p.Codigo == codigo, cancellationToken);
    }

    // Devuelve todos los productos que pertenecen a una categoría específica.
    public async Task<IReadOnlyList<Producto>> GetByCategoriaIdAsync(int categoriaId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Where(p => p.CategoriaId == categoriaId)
            .ToListAsync(cancellationToken);
    }

    // Obtiene la lista de productos que tienen 10 o menos unidades disponibles en el inventario.
    public async Task<IReadOnlyList<Producto>> GetProductosConStockBajoAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Where(p => p.Stock <= 10)
            .ToListAsync(cancellationToken);
    }

    // Verifica si ya existe un producto con el mismo código, permitiendo excluir un ID al editar.
    public async Task<bool> ExistsCodigoAsync(string codigo, int? excludeId = null, CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(
            p => p.Codigo == codigo && (!excludeId.HasValue || p.Id != excludeId.Value),
            cancellationToken);
    }
}
