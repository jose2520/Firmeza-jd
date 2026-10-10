using Firmeza.Application.DTOs;

namespace Firmeza.Application.Interfaces;

/// <summary>
/// Contrato de servicio para el procesamiento y control transaccional de ventas.
/// 
/// PARA QUÉ SIRVE:
///   Gestiona el registro de compras con validación de stock disponible,
///   deducción atómica de existencias, cálculo de impuestos (IVA 19%),
///   generación de consecutivos únicos (VENTA-YYYY-XXXX) y anulación de órdenes con restitución de inventario.
/// 
/// ORIGEN DE LOS DATOS:
///   Interactúa transaccionalmente con las tablas 'ventas', 'detalles_venta' y 'productos'
///   en la base de datos PostgreSQL ('firmeza_db').
/// </summary>
public interface IVentaService
{
    /// <summary>
    /// Consulta el listado histórico de todas las ventas con su cliente y partidas.
    /// Origen de datos: Consulta SQL 'SELECT * FROM ventas' con JOIN a 'clientes' y 'detalles_venta' en PostgreSQL.
    /// </summary>
    /// <returns>Colección de ventas proyectadas a VentaDto.</returns>
    Task<IEnumerable<VentaDto>> GetAllAsync();

    /// <summary>
    /// Obtiene una venta específica con el detalle completo de productos facturados.
    /// Origen de datos: Consulta SQL sobre 'ventas' por ID con includes a Cliente y Detalles en PostgreSQL.
    /// </summary>
    /// <param name="id">ID primario de la venta.</param>
    /// <returns>La venta encontrada o null si no existe.</returns>
    Task<VentaDto?> GetByIdAsync(int id);

    /// <summary>
    /// Registra una nueva venta de forma transaccional (ACID):
    /// - Valida que el cliente exista y esté activo.
    /// - Verifica y descuenta el stock de cada producto en 'productos'.
    /// - Calcula Subtotal, IVA (19%) y Total.
    /// - Inserta cabecera en 'ventas' y partidas en 'detalles_venta'.
    /// Origen de datos: Datos recibidos del cuerpo HTTP (CreateVentaDto) + actualización de tablas en PostgreSQL.
    /// </summary>
    /// <param name="dto">Datos de la venta (cliente, lista de productos y cantidades).</param>
    /// <returns>La venta confirmada y registrada.</returns>
    Task<VentaDto> CreateAsync(CreateVentaDto dto);

    /// <summary>
    /// Anula una venta previamente registrada, cambia su estado a 'Anulada'
    /// y reintegra las cantidades adquiridas al stock de cada producto.
    /// Origen de datos: Modificación de 'estado' en 'ventas' e incremento de 'stock' en 'productos' en PostgreSQL.
    /// </summary>
    /// <param name="id">ID de la venta a anular.</param>
    /// <returns>True si la anulación fue exitosa, False si no se encontró o ya estaba anulada.</returns>
    Task<bool> CancelAsync(int id);
}
