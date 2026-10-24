// Este archivo representa cada línea o producto individual incluido dentro de una factura de venta.
// En el negocio de Firmeza, sirve para detallar qué material se vendió, cuántas unidades llevó el cliente,
// a qué precio unitario se le cobró en ese momento y cuánto suma ese renglón.

namespace Firmeza.Domain.Entities;

// Representa un renglón individual de productos comprados dentro de una venta en la tabla 'detalles_venta'.
public class DetalleVenta
{
    // Número identificador único de este renglón de venta en la base de datos.
    public int Id { get; set; }

    // Identificador de la factura o venta principal a la que pertenece este renglón.
    public int VentaId { get; set; }

    // Identificador del producto o material de construcción que fue vendido.
    public int ProductoId { get; set; }

    // Cantidad de unidades compradas de este producto.
    public int Cantidad { get; set; }

    // Precio por unidad que tenía el producto en el momento exacto de realizar la venta.
    public decimal PrecioUnitario { get; set; }

    // Valor total de este renglón, calculado al multiplicar la cantidad por el precio unitario.
    public decimal Subtotal { get; set; }

    // Información completa del producto o material que se vendió en este renglón.
    public virtual Producto Producto { get; set; } = null!;

    // Información completa de la factura de venta a la que pertenece este renglón.
    public virtual Venta Venta { get; set; } = null!;
}
