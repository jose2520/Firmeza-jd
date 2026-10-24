// Clase base que contiene las operaciones comunes de lectura, creación, edición y borrado.
// Trabaja con ApplicationDbContext y sirve de plantilla para todos los demás repositorios del sistema.
// Evita repetir el mismo código para buscar por ID, listar todo, agregar o eliminar registros en las tablas.
using System.Linq.Expressions;
using Firmeza.Domain.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.Infrastructure.Persistence.Repositories;

public class GenericRepository<TEntity> : IRepository<TEntity> where TEntity : class
{
    protected readonly ApplicationDbContext Context;
    protected readonly DbSet<TEntity> DbSet;

    // Recibe la conexión a la base de datos y prepara la tabla correspondiente a la entidad.
    public GenericRepository(ApplicationDbContext context)
    {
        Context = context ?? throw new ArgumentNullException(nameof(context));
        DbSet = Context.Set<TEntity>();
    }

    // Busca y devuelve un registro específico usando su número de identificación (ID).
    public virtual async Task<TEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await DbSet.FindAsync(new object[] { id }, cancellationToken);
    }

    // Obtiene la lista completa de todos los registros guardados en la tabla.
    public virtual async Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet.AsNoTracking().ToListAsync(cancellationToken);
    }

    // Busca y devuelve todos los registros de la tabla que cumplan con una condición específica.
    public virtual async Task<IReadOnlyList<TEntity>> FindAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await DbSet.AsNoTracking().Where(predicate).ToListAsync(cancellationToken);
    }

    // Prepara un nuevo registro para ser guardado en la tabla correspondiente.
    public virtual async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await DbSet.AddAsync(entity, cancellationToken);
    }

    // Prepara una lista de varios registros nuevos para guardarlos juntos en la tabla.
    public virtual async Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        await DbSet.AddRangeAsync(entities, cancellationToken);
    }

    // Marca un registro existente como modificado para actualizar sus datos al guardar.
    public virtual void Update(TEntity entity)
    {
        DbSet.Update(entity);
    }

    // Marca un registro para ser eliminado de la tabla al guardar los cambios.
    public virtual void Remove(TEntity entity)
    {
        DbSet.Remove(entity);
    }

    // Comprueba si existe al menos un registro en la tabla que cumpla con la condición indicada.
    public virtual async Task<bool> ExistsAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(predicate, cancellationToken);
    }
}
