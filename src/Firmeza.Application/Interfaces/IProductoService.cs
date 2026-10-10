using Firmeza.Application.DTOs;

namespace Firmeza.Application.Interfaces;

/// <summary>
/// Contrato de servicio para la gestión del catálogo de productos y control de existencias.
/// 
/// PARA QUÉ SIRVE:
///   Define las operaciones de negocio para consultar catálogo de productos,
///   buscar por identificador primario, crear artículos, modificar precios o stock,
///   y aplicar bajas lógicas sin alterar historiales de venta.
/// 
/// ORIGEN DE LOS DATOS:
///   Las operaciones interactúan directamente con la tabla 'productos' en PostgreSQL ('firmeza_db').
/// </summary>
public interface IProductoService
{
    /// <summary>
    /// Lista todos los productos activos en el catálogo.
    /// Origen de datos: Consulta SQL 'SELECT * FROM productos WHERE activo = TRUE' en PostgreSQL.
    /// </summary>
    /// <returns>Colección de productos proyectados a ProductoDto.</returns>
    Task<IEnumerable<ProductoDto>> GetAllAsync();

    /// <summary>
    /// Busca un producto específico por su ID único.
    /// Origen de datos: Consulta SQL 'SELECT * FROM productos WHERE id = @id AND activo = TRUE' en PostgreSQL.
    /// </summary>
    /// <param name="id">Clave primaria del producto.</param>
    /// <returns>El producto localizado o null si no existe o fue desactivado.</returns>
    Task<ProductoDto?> GetByIdAsync(int id);

    /// <summary>
    /// Registra un nuevo producto en el catálogo verificando código único.
    /// Origen de datos: Inserción en la tabla 'productos' con datos recibidos en el cuerpo HTTP (JSON).
    /// </summary>
    /// <param name="dto">Datos del producto a registrar (código, nombre, precio, stock).</param>
    /// <returns>El producto persistido con su ID asignado.</returns>
    Task<ProductoDto> CreateAsync(CreateProductoDto dto);

    /// <summary>
    /// Actualiza la información técnica, comercial o existencias de un producto.
    /// Origen de datos: Actualización en la tabla 'productos' a partir de datos del cuerpo HTTP (JSON).
    /// </summary>
    /// <param name="id">ID del producto a actualizar.</param>
    /// <param name="dto">Nuevos valores para el producto.</param>
    /// <returns>El producto actualizado o null si no existe.</returns>
    Task<ProductoDto?> UpdateAsync(int id, UpdateProductoDto dto);

    /// <summary>
    /// Realiza la baja lógica (Soft Delete) del producto en el catálogo.
    /// Origen de datos: Actualización de la columna 'activo = FALSE' en la tabla 'productos' de PostgreSQL.
    /// </summary>
    /// <param name="id">ID del producto a desactivar.</param>
    /// <returns>True si se desactivó exitosamente, False si no se encontró.</returns>
    Task<bool> DeleteAsync(int id);
}
