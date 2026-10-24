// Clase que revisa automáticamente los registros nuevos antes de guardarlos en la base de datos.
// Trabaja junto con ApplicationDbContext sobre todas las tablas que tienen la columna "creado_en".
// Si un registro nuevo no tiene fecha de creación, le asigna la fecha y hora actual automáticamente.
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Firmeza.Infrastructure.Persistence.Interceptors;

public class AuditableEntityInterceptor : SaveChangesInterceptor
{
    // Se ejecuta justo antes de guardar cambios de forma normal para asignar las fechas de creación.
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        UpdateEntities(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    // Se ejecuta justo antes de guardar cambios en segundo plano para asignar las fechas de creación.
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        UpdateEntities(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    // Recorre las entidades recién agregadas y completa el campo CreadoEn si está vacío.
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
