// Este archivo define el encargado de confirmar o cancelar de forma segura los cambios en la base de datos.
// En el negocio de Firmeza, sirve para asegurar que operaciones compuestas (como registrar una venta y
// descontar el inventario al mismo tiempo) se guarden completas o se deshagan si ocurre algún error.

namespace Firmeza.Domain.Abstractions;

// Coordina el guardado definitivo de los cambios y el control de operaciones seguras en la base de datos.
public interface IUnitOfWork : IDisposable
{
    // Guarda de forma definitiva todos los cambios pendientes en la base de datos y devuelve cuántos registros se actualizaron.
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    // Inicia un bloque de operaciones agrupadas para que todas se ejecuten juntas de manera segura.
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    // Confirma y aplica de forma permanente todos los cambios realizados dentro del bloque actual.
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    // Deshace todos los cambios realizados en el bloque actual si ocurre algún problema, dejando todo como estaba antes.
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
