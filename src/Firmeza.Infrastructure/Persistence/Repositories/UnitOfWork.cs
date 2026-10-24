// Clase encargada de confirmar o deshacer en bloque los cambios hechos en la base de datos.
// Trabaja directamente con ApplicationDbContext para asegurar que operaciones complejas (como una venta y su descuento de stock)
// se guarden completas sin errores o se cancelen por completo si algo falla a mitad de camino.
using Firmeza.Domain.Abstractions;
using Microsoft.EntityFrameworkCore.Storage;

namespace Firmeza.Infrastructure.Persistence.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction? _currentTransaction;
    private bool _disposed;

    // Recibe el contexto de la base de datos para coordinar el guardado de cambios.
    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    // Guarda en la base de datos todos los cambios pendientes realizados en los repositorios.
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    // Inicia un grupo de operaciones seguras en la base de datos para confirmarlas todas juntas al final.
    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction != null)
        {
            return;
        }

        // Si el proveedor de base de datos soporta operaciones agrupadas (como PostgreSQL)
        if (_context.Database.ProviderName != "Microsoft.EntityFrameworkCore.InMemory")
        {
            _currentTransaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        }
    }

    // Guarda los cambios y confirma definitivamente todas las operaciones del grupo actual; si falla, deshace todo.
    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _context.SaveChangesAsync(cancellationToken);

            if (_currentTransaction != null)
            {
                await _currentTransaction.CommitAsync(cancellationToken);
            }
        }
        catch
        {
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
        finally
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }
        }
    }

    // Cancela y deshace todos los cambios realizados desde que se inició el grupo de operaciones actual.
    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction != null)
        {
            await _currentTransaction.RollbackAsync(cancellationToken);
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }
    }

    // Libera los recursos de memoria y cierra cualquier operación pendiente al terminar de usar la clase.
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    // Limpia internamente la operación activa si aún estaba abierta al cerrar la clase.
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _currentTransaction?.Dispose();
                _currentTransaction = null;
            }

            _disposed = true;
        }
    }
}
