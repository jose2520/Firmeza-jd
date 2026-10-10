using System;
using System.Threading;
using System.Threading.Tasks;

namespace Firmeza.Domain.Abstractions;

/// <summary>
/// Contrato para el patrón Unit of Work (Unidad de Trabajo).
/// Coordina la persistencia atómica y el manejo de transacciones entre múltiples repositorios.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// Guarda todos los cambios pendientes en el almacén de datos de forma asíncrona.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Número de entidades afectadas o guardadas.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Inicia una transacción de base de datos atómica.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación.</param>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Confirma la transacción activa actual.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación.</param>
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Revierte la transacción activa actual descartando cualquier modificación.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación.</param>
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
