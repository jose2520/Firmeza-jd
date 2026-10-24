// Archivo: IProductoService.cs
// Qué es: Plantilla de acciones para administrar el catálogo de productos y el inventario.
// Para qué sirve: Define las funciones obligatorias para consultar artículos, buscarlos por su número, registrar nuevos, cambiar precios o cantidades y desactivarlos.

using Firmeza.Application.DTOs;

namespace Firmeza.Application.Interfaces;

// Define las operaciones disponibles para manejar los productos de la ferretería.
public interface IProductoService
{
    // Devuelve la lista completa de todos los productos que están activos en el catálogo.
    Task<IEnumerable<ProductoDto>> GetAllAsync();

    // Busca y devuelve un producto específico usando su número identificador.
    Task<ProductoDto?> GetByIdAsync(int id);

    // Registra un producto nuevo en el inventario revisando que su código no esté repetido.
    Task<ProductoDto> CreateAsync(CreateProductoDto dto);

    // Actualiza el nombre, descripción, precio o cantidad disponible de un producto existente.
    Task<ProductoDto?> UpdateAsync(int id, UpdateProductoDto dto);

    // Desactiva un producto para que no siga a la venta sin perder el historial de facturas anteriores.
    Task<bool> DeleteAsync(int id);
}
