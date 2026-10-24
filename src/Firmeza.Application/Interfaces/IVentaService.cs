// Archivo: IVentaService.cs
// Qué es: Plantilla de acciones para el registro y control de ventas.
// Para qué sirve: Define las funciones para consultar el historial de facturas, ver el detalle de una compra, registrar una venta descontando unidades y anular ventas devolviendo la mercancía al inventario.

using Firmeza.Application.DTOs;

namespace Firmeza.Application.Interfaces;

// Define las operaciones principales para procesar y consultar las ventas del negocio.
public interface IVentaService
{
    // Obtiene la lista de todas las ventas realizadas con la información del cliente y los productos comprados.
    Task<IEnumerable<VentaDto>> GetAllAsync();

    // Busca una venta específica por su número identificador con todos sus detalles.
    Task<VentaDto?> GetByIdAsync(int id);

    // Registra una venta nueva, descuenta las unidades vendidas de la bodega y calcula el subtotal, el IVA y el total a pagar.
    Task<VentaDto> CreateAsync(CreateVentaDto dto);

    // Anula una venta registrada y devuelve automáticamente las unidades de los productos al inventario.
    Task<bool> CancelAsync(int id);
}
