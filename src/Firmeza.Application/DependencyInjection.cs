// Archivo: DependencyInjection.cs
// Qué es: Configuración inicial de las herramientas de la capa de Aplicación.
// Para qué sirve: Conecta y deja listas las herramientas de conversión de datos para que el resto del programa pueda usarlas fácilmente.

using AutoMapper;
using Firmeza.Application.Mappings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace Firmeza.Application;

// Clase que agrupa el registro de los servicios de aplicación al iniciar el sistema.
public static class DependencyInjection
{
    // Registra las reglas de conversión automática de datos para que estén disponibles en toda la aplicación.
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Configurar y registrar AutoMapper para convertir datos entre tablas y objetos de envío
        var mapperConfig = new MapperConfiguration(
            cfg => cfg.AddProfile<MappingProfile>(),
            NullLoggerFactory.Instance);
        services.AddSingleton(mapperConfig.CreateMapper());

        return services;
    }
}
