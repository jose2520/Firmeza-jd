using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace Firmeza.Application;

/// <summary>
/// Clase de extensión para registrar servicios de la capa de Aplicación en el contenedor IoC.
/// 
/// PARA QUÉ SIRVE:
///   Permite que la capa API registre de forma modular y desacoplada todas las dependencias
///   propias de la capa de Aplicación (validadores, casos de uso, mediadores).
/// 
/// ORIGEN DE LOS DATOS:
///   Recibe la colección de servicios en memoria del contenedor de dependencias (IServiceCollection).
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Agrega los servicios de la capa de Aplicación al contenedor de inyección de dependencias.
    /// </summary>
    /// <param name="services">Contenedor IServiceCollection de ASP.NET Core.</param>
    /// <returns>La misma colección de servicios para encadenamiento fluido.</returns>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // 1. Configurar y registrar AutoMapper para transformaciones entre entidades y DTOs
        var mapperConfig = new AutoMapper.MapperConfiguration(
            cfg => cfg.AddProfile<Mappings.MappingProfile>(),
            Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance);
        services.AddSingleton<AutoMapper.IMapper>(mapperConfig.CreateMapper());

        return services;
    }
}
