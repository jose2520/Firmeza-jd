// Este archivo define las operaciones para buscar, validar y guardar la información de los clientes.
// En el negocio de Firmeza, sirve para encontrar compradores por su documento o correo electrónico
// y evitar que se registren clientes duplicados en el sistema.

using Firmeza.Domain.Entities;

namespace Firmeza.Domain.Abstractions;

// Define las funciones para consultar y administrar el directorio de clientes de la ferretería.
public interface IClienteRepository : IRepository<Cliente>
{
    // Busca a un cliente por su número de cédula, NIT o documento de identidad.
    Task<Cliente?> GetByIdentificacionAsync(string identificacion, CancellationToken cancellationToken = default);

    // Busca a un cliente utilizando su dirección de correo electrónico registrada.
    Task<Cliente?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    // Revisa si ya hay otro cliente registrado con ese mismo documento de identidad.
    Task<bool> ExistsIdentificacionAsync(string identificacion, int? excludeId = null, CancellationToken cancellationToken = default);

    // Revisa si ya hay otro cliente registrado con ese mismo correo electrónico.
    Task<bool> ExistsEmailAsync(string email, int? excludeId = null, CancellationToken cancellationToken = default);
}
