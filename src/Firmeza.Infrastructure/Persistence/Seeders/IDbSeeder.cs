// Interfaz que define el método para cargar los datos iniciales en la base de datos.
// Es implementada por el archivo DbSeeder.cs para preparar las tablas de usuarios, roles y métodos de pago.
// Asegura que el sistema tenga la información mínima necesaria al arrancar.

namespace Firmeza.Infrastructure.Persistence.Seeders;

public interface IDbSeeder
{
    // Ejecuta la carga inicial de roles de seguridad, usuario administrador y métodos de pago si no existen.
    Task SeedAsync(CancellationToken cancellationToken = default);
}
