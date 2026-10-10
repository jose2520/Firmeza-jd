using System;
using System.Collections.Generic;

namespace Firmeza.Domain.Entities;

/// <summary>
/// Representa la entidad de dominio para un Renglón o Partida individual dentro de una Venta.
/// 
/// PARA QUÉ SIRVE:
///   Almacena qué producto específico fue comprado, la cantidad de unidades,
///   el precio unitario fijado en el momento de la venta y el subtotal resultante.
/// 
/// ORIGEN DE LOS DATOS:
///   Mapea directamente contra la tabla "detalles_venta" de la base de datos PostgreSQL ('firmeza_db').
///   Gestionada a través de ApplicationDbContext en la capa de Infraestructura.
/// </summary>
public partial class DetalleVenta
{
    /// <summary>
    /// Identificador único autoincremental de la clave primaria.
    /// Origen: Columna 'id' (SERIAL PRIMARY KEY) de la tabla 'detalles_venta'.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Clave foránea que asocia este renglón con la venta principal.
    /// Origen: Columna 'venta_id' (INT NOT NULL) con FK hacia ventas(id) ON DELETE CASCADE.
    /// </summary>
    public int VentaId { get; set; }

    /// <summary>
    /// Clave foránea que referencia al producto adquirido.
    /// Origen: Columna 'producto_id' (INT NOT NULL) con FK hacia productos(id) ON DELETE RESTRICT.
    /// </summary>
    public int ProductoId { get; set; }

    /// <summary>
    /// Número de unidades adquiridas del producto.
    /// Origen: Columna 'cantidad' (INT NOT NULL CHECK > 0) en PostgreSQL.
    /// </summary>
    public int Cantidad { get; set; }

    /// <summary>
    /// Precio unitario pactado al momento de la venta (congela el precio histórico).
    /// Origen: Columna 'precio_unitario' (NUMERIC(12,2) CHECK >= 0) en PostgreSQL.
    /// </summary>
    public decimal PrecioUnitario { get; set; }

    /// <summary>
    /// Subtotal del renglón (Cantidad * PrecioUnitario).
    /// Origen: Columna 'subtotal' (NUMERIC(12,2) CHECK >= 0) en PostgreSQL.
    /// </summary>
    public decimal Subtotal { get; set; }

    /// <summary>
    /// Propiedad de navegación hacia el producto adquirido (Relación N a 1).
    /// </summary>
    public virtual Producto Producto { get; set; } = null!;

    /// <summary>
    /// Propiedad de navegación hacia la cabecera de la venta (Relación N a 1).
    /// </summary>
    public virtual Venta Venta { get; set; } = null!;
}
