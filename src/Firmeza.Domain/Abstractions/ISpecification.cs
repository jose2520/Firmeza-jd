using System;
using System.Linq.Expressions;

namespace Firmeza.Domain.Abstractions;

/// <summary>
/// Contrato base del patrón Specification para encapsular criterios de selección y reglas de negocio reusables.
/// </summary>
/// <typeparam name="T">Tipo de la entidad a evaluar.</typeparam>
public interface ISpecification<T>
{
    /// <summary>
    /// Convierte la especificación en una expresión lambda evaluable por LINQ / EF Core.
    /// </summary>
    Expression<Func<T, bool>> ToExpression();

    /// <summary>
    /// Determina si una instancia de entidad en memoria satisface la especificación.
    /// </summary>
    bool IsSatisfiedBy(T entity);
}
