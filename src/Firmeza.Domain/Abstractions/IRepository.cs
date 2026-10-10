using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Firmeza.Domain.Abstractions;

/// <summary>
/// Contrato genérico base para repositorios de entidades de dominio en Firmeza.
/// Define las operaciones estándar de lectura, consulta y persistencia.
/// </summary>
/// <typeparam name="TEntity">Tipo de la entidad de dominio.</typeparam>
public interface IRepository<TEntity> where TEntity : class
{
    /// <summary>
    /// Obtiene una entidad por su identificador primario.
    /// </summary>
    Task<TEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene la colección completa de entidades de este tipo.
    /// </summary>
    Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca entidades que cumplan con la expresión o predicado especificado.
    /// </summary>
    Task<IReadOnlyList<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Agrega una nueva entidad al repositorio.
    /// </summary>
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Agrega una colección de entidades al repositorio de forma masiva.
    /// </summary>
    Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marca una entidad existente como modificada.
    /// </summary>
    void Update(TEntity entity);

    /// <summary>
    /// Elimina una entidad del repositorio.
    /// </summary>
    void Remove(TEntity entity);

    /// <summary>
    /// Determina si existe al menos una entidad que satisfaga la condición especificada.
    /// </summary>
    Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
}
