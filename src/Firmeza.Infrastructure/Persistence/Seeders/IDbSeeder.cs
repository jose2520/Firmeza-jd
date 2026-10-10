using System.Threading;
using System.Threading.Tasks;

namespace Firmeza.Infrastructure.Persistence.Seeders;

/// <summary>
/// Contrato de servicio para el sembrado programático e inicialización de la base de datos.
/// </summary>
public interface IDbSeeder
{
    /// <summary>
    /// Ejecuta la siembra idempotente de roles de seguridad, usuario administrador y catálogos maestros.
    /// </summary>
    Task SeedAsync(CancellationToken cancellationToken = default);
}
