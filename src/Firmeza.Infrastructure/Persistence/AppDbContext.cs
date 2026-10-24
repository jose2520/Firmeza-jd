// Clase que funciona como un nombre corto para ApplicationDbContext.
// Permite acceder a todas las tablas de la base de datos (clientes, productos, ventas, etc.)
// desde cualquier servicio o prueba del proyecto de forma más sencilla.
using Microsoft.EntityFrameworkCore;

namespace Firmeza.Infrastructure.Persistence;

public class AppDbContext : ApplicationDbContext
{
    // Crea una instancia vacía del contexto de base de datos.
    public AppDbContext()
    {
    }

    // Crea una instancia del contexto recibiendo las opciones de conexión a la base de datos.
    public AppDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
}
