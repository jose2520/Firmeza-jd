using System.Threading;
using System.Threading.Tasks;
using Firmeza.Domain.Entities;

namespace Firmeza.Domain.Abstractions;

/// <summary>
/// Contrato de repositorio de dominio para la gestión y consulta de Clientes.
/// </summary>
public interface IClienteRepository : IRepository<Cliente>
{
    /// <summary>
    /// Busca un cliente por su número de identificación fiscal o cédula.
    /// </summary>
    Task<Cliente?> GetByIdentificacionAsync(string identificacion, CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca un cliente por su dirección de correo electrónico registrada.
    /// </summary>
    Task<Cliente?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Comprueba si ya existe un cliente con la identificación dada, opcionalmente excluyendo un ID específico.
    /// </summary>
    Task<bool> ExistsIdentificacionAsync(string identificacion, int? excludeId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Comprueba si ya existe un cliente con el correo electrónico dado, opcionalmente excluyendo un ID específico.
    /// </summary>
    Task<bool> ExistsEmailAsync(string email, int? excludeId = null, CancellationToken cancellationToken = default);
}
