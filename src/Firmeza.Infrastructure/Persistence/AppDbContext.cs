using Microsoft.EntityFrameworkCore;

namespace Firmeza.Infrastructure.Persistence;

/// <summary>
/// Alias conciso y conveniente para ApplicationDbContext.
/// Permite inyectar AppDbContext en servicios, controladores o tests de manera transparente.
/// </summary>
public class AppDbContext : ApplicationDbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
}
