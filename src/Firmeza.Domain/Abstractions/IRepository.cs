// Este archivo define las operaciones básicas de lectura, búsqueda, registro, actualización y borrado.
// En el negocio de Firmeza, sirve como plantilla común para que todos los módulos (clientes, productos,
// ventas, vehículos) manejen sus datos de forma ordenada y uniforme.

using System.Linq.Expressions;

namespace Firmeza.Domain.Abstractions;

// Define las operaciones básicas para consultar, agregar, modificar y eliminar registros de cualquier tabla.
public interface IRepository<TEntity> where TEntity : class
{
    // Busca y devuelve un registro específico usando su número identificador único.
    Task<TEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    // Obtiene la lista completa de todos los registros guardados de este tipo.
    Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);

    // Busca y devuelve todos los registros que cumplan con una condición de búsqueda específica.
    Task<IReadOnlyList<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

    // Agrega un nuevo registro para que sea guardado en la base de datos.
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    // Agrega varios registros al mismo tiempo de forma masiva.
    Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    // Marca un registro existente como modificado para actualizar sus datos.
    void Update(TEntity entity);

    // Elimina un registro existente del sistema.
    void Remove(TEntity entity);

    // Verifica si existe al menos un registro que cumpla con la condición indicada.
    Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
}
