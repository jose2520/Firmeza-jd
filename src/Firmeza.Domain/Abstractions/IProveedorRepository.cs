// Este archivo define las operaciones para consultar y administrar los proveedores de la ferretería.
// En el negocio de Firmeza, sirve para buscar empresas distribuidoras o fabricantes por su NIT
// y obtener el listado de proveedores con los que se trabaja actualmente.

using Firmeza.Domain.Entities;

namespace Firmeza.Domain.Abstractions;

// Define las funciones para consultar y guardar la información de los proveedores y fabricantes aliados.
public interface IProveedorRepository : IRepository<Proveedor>
{
    // Busca a un proveedor específico utilizando su número de identificación tributaria (NIT).
    Task<Proveedor?> GetByNitAsync(string nit, CancellationToken cancellationToken = default);

    // Obtiene la lista de todos los proveedores que se encuentran habilitados comercialmente.
    Task<IReadOnlyList<Proveedor>> GetActivosAsync(CancellationToken cancellationToken = default);
}
