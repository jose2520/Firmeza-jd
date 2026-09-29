using Microsoft.Extensions.DependencyInjection;

namespace Firmeza.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Registrar servicios de la capa de Aplicación
        return services;
    }
}
