using Firmeza.Application.DTOs;

namespace Firmeza.Application.Interfaces;

/// <summary>
/// Contrato de servicio para la gestión de clientes en el sistema.
/// 
/// PARA QUÉ SIRVE:
///   Define las operaciones de negocio para listar, consultar por identificador,
///   registrar nuevos clientes, actualizar datos de contacto y realizar bajas lógicas (Soft Delete).
/// 
/// ORIGEN DE LOS DATOS:
///   Las operaciones interactúan con la tabla 'clientes' de la base de datos PostgreSQL ('firmeza_db').
/// </summary>
public interface IClienteService
{
    /// <summary>
    /// Obtiene la lista de todos los clientes activos en el sistema.
    /// Origen de datos: Consulta SQL 'SELECT * FROM clientes WHERE activo = TRUE' en PostgreSQL.
    /// </summary>
    /// <returns>Colección de clientes proyectados a ClienteDto.</returns>
    Task<IEnumerable<ClienteDto>> GetAllAsync();

    /// <summary>
    /// Busca un cliente específico por su ID único de base de datos.
    /// Origen de datos: Consulta SQL 'SELECT * FROM clientes WHERE id = @id AND activo = TRUE' en PostgreSQL.
    /// </summary>
    /// <param name="id">Identificador numérico de la clave primaria.</param>
    /// <returns>El cliente encontrado o null si no existe o está inactivo.</returns>
    Task<ClienteDto?> GetByIdAsync(int id);

    /// <summary>
    /// Registra un nuevo cliente validando unicidad de identificación y correo.
    /// Origen de datos: Inserción en la tabla 'clientes' con datos provenientes del cuerpo HTTP (JSON).
    /// </summary>
    /// <param name="dto">Datos del nuevo cliente a crear.</param>
    /// <returns>El cliente persistido con su ID asignado.</returns>
    Task<ClienteDto> CreateAsync(CreateClienteDto dto);

    /// <summary>
    /// Actualiza los datos de un cliente existente.
    /// Origen de datos: Actualización en la tabla 'clientes' a partir de datos del cuerpo HTTP (JSON).
    /// </summary>
    /// <param name="id">ID del cliente a modificar.</param>
    /// <param name="dto">Nuevos datos a aplicar.</param>
    /// <returns>El cliente actualizado o null si no fue localizado.</returns>
    Task<ClienteDto?> UpdateAsync(int id, UpdateClienteDto dto);

    /// <summary>
    /// Realiza la baja lógica (Soft Delete) del cliente, marcando 'activo = FALSE'.
    /// Origen de datos: Actualización de la columna 'activo' en la tabla 'clientes' de PostgreSQL.
    /// </summary>
    /// <param name="id">ID del cliente a desactivar.</param>
    /// <returns>True si se desactivó con éxito, False si no se encontró.</returns>
    Task<bool> DeleteAsync(int id);
}
