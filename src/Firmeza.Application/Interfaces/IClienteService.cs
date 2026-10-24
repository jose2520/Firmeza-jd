// Archivo: IClienteService.cs
// Qué es: Plantilla de acciones para la administración de clientes.
// Para qué sirve: Define las funciones obligatorias para listar, buscar, crear, actualizar y desactivar clientes en el sistema.

using Firmeza.Application.DTOs;

namespace Firmeza.Application.Interfaces;

// Define las operaciones disponibles para manejar la información de los clientes.
public interface IClienteService
{
    // Obtiene la lista de todos los clientes que están activos en el sistema.
    Task<IEnumerable<ClienteDto>> GetAllAsync();

    // Busca y devuelve los datos de un cliente usando su número identificador único.
    Task<ClienteDto?> GetByIdAsync(int id);

    // Registra un cliente nuevo revisando antes que su documento o correo no estén repetidos.
    Task<ClienteDto> CreateAsync(CreateClienteDto dto);

    // Modifica los datos personales o de contacto de un cliente que ya existe.
    Task<ClienteDto?> UpdateAsync(int id, UpdateClienteDto dto);

    // Desactiva un cliente para que no aparezca disponible sin borrar su historial de compras.
    Task<bool> DeleteAsync(int id);
}
