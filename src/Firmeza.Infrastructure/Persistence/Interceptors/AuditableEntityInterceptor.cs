using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Firmeza.Infrastructure.Persistence.Interceptors;

/// <summary>
/// Interceptor de EF Core para auditar entidades automáticamente en operaciones de guardado.
/// Mantiene actualizadas las fechas de creación y modificación de las entidades de forma homogénea.
/// </summary>
public class AuditableEntityInterceptor : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        UpdateEntities(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        UpdateEntities(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void UpdateEntities(DbContext? context)
    {
        if (context == null) return;

        var utcNow = DateTime.UtcNow;

        foreach (var entry in context.ChangeTracker.Entries())
        {
            if (entry.State == EntityState.Added)
            {
                var creadoEnProp = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "CreadoEn");
                if (creadoEnProp != null && (creadoEnProp.CurrentValue == null || (DateTime)creadoEnProp.CurrentValue == default))
                {
                    creadoEnProp.CurrentValue = utcNow;
                }
            }
        }
    }
}
