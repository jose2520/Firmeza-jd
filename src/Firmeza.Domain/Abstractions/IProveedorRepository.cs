using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Firmeza.Domain.Entities;

namespace Firmeza.Domain.Abstractions;

/// <summary>
/// Contrato de repositorio de dominio para Proveedores y fabricantes aliados.
/// </summary>
public interface IProveedorRepository : IRepository<Proveedor>
{
    /// <summary>
    /// Busca un proveedor por su NIT tributario único.
    /// </summary>
    Task<Proveedor?> GetByNitAsync(string nit, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene los proveedores que se encuentran activos comercialmente.
    /// </summary>
    Task<IReadOnlyList<Proveedor>> GetActivosAsync(CancellationToken cancellationToken = default);
}
