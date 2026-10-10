using System;
using System.Collections.Generic;
using Firmeza.Domain.Common;
using Firmeza.Domain.Events;
using Firmeza.Domain.Exceptions;

namespace Firmeza.Domain.Entities;

/// <summary>
/// Representa la entidad de dominio y Agregado Raíz para un Producto del catálogo ferretero y de construcción.
/// 
/// PARA QUÉ SIRVE:
///   Gestiona el catálogo de artículos, su precio unitario de venta, el stock disponible
///   en inventario, categorización, proveedor asociado y el control de bajas lógicas (Soft Delete).
/// 
/// ORIGEN DE LOS DATOS:
///   Mapea directamente contra la tabla "productos" de la base de datos PostgreSQL ('firmeza_db').
///   Gestionada a través de ApplicationDbContext en la capa de Infraestructura.
/// </summary>
public partial class Producto : AggregateRoot
{
    /// <summary>
    /// Identificador único autoincremental de la clave primaria.
    /// Origen: Columna 'id' (SERIAL PRIMARY KEY) de la tabla 'productos'.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Código alfanumérico único del producto (SKU / Código de barras).
    /// Origen: Columna 'codigo' (VARCHAR(50) UNIQUE) en PostgreSQL.
    /// </summary>
    public string Codigo { get; set; } = null!;

    /// <summary>
    /// Nombre descriptivo del producto (ej. Cemento Gris, Tornillo de Acero).
    /// Origen: Columna 'nombre' (VARCHAR(150)) en PostgreSQL.
    /// </summary>
    public string Nombre { get; set; } = null!;

    /// <summary>
    /// Descripción técnica o comercial detallada del producto.
    /// Origen: Columna 'descripcion' (TEXT, opcional) en PostgreSQL.
    /// </summary>
    public string? Descripcion { get; set; }

    /// <summary>
    /// Precio unitario de venta antes de impuestos.
    /// Origen: Columna 'precio' (NUMERIC(12,2) CHECK >= 0) en PostgreSQL.
    /// </summary>
    public decimal Precio { get; set; }

    /// <summary>
    /// Unidades disponibles actualmente en inventario físico/bodega.
    /// Origen: Columna 'stock' (INT DEFAULT 0 CHECK >= 0) en PostgreSQL.
    /// </summary>
    public int Stock { get; set; }

    /// <summary>
    /// Clave foránea opcional hacia la categoría del producto.
    /// Origen: Columna 'categoria_id' (INT) con FK hacia categorias(id).
    /// </summary>
    public int? CategoriaId { get; set; }

    /// <summary>
    /// Clave foránea opcional hacia el proveedor o fabricante.
    /// Origen: Columna 'proveedor_id' (INT) con FK hacia proveedores(id).
    /// </summary>
    public int? ProveedorId { get; set; }

    /// <summary>
    /// Indicador de visibilidad y estado activo del producto (Soft Delete).
    /// Origen: Columna 'activo' (BOOLEAN DEFAULT TRUE) en PostgreSQL.
    /// </summary>
    public bool Activo { get; set; } = true;

    /// <summary>
    /// Fecha y hora de registro del producto en el catálogo.
    /// Origen: Columna 'creado_en' (TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP) en PostgreSQL.
    /// </summary>
    public DateTime CreadoEn { get; set; }

    /// <summary>
    /// Navegación a la categoría.
    /// </summary>
    public virtual Categoria? Categoria { get; set; }

    /// <summary>
    /// Navegación al proveedor.
    /// </summary>
    public virtual Proveedor? Proveedor { get; set; }

    /// <summary>
    /// Renglones de venta en los que este producto ha sido incluido (Relación 1 a N).
    /// </summary>
    public virtual ICollection<DetalleVenta> DetallesVenta { get; set; } = new List<DetalleVenta>();

    /// <summary>
    /// Historial de movimientos de inventario Kardex (Relación 1 a N).
    /// </summary>
    public virtual ICollection<MovimientoInventario> MovimientosInventario { get; set; } = new List<MovimientoInventario>();

    // =========================================================================
    // MÉTODOS DE DOMINIO Y REGLAS DE NEGOCIO (DDD)
    // =========================================================================

    /// <summary>
    /// Descuenta existencias del inventario garantizando que no se rebase el stock disponible.
    /// Si el stock resultante es inferior o igual a 10 unidades, emite StockBajoDomainEvent.
    /// </summary>
    public void DescontarStock(int cantidad)
    {
        if (cantidad <= 0)
            throw new ReglaNegocioException("Producto.CantidadInvalida", "La cantidad a descontar debe ser estrictamente mayor a cero.");

        if (Stock < cantidad)
            throw new StockInsuficienteException(Codigo, Stock, cantidad);

        Stock -= cantidad;

        if (Stock <= 10)
        {
            AddDomainEvent(new StockBajoDomainEvent(Id, Codigo, Nombre, Stock, DateTime.UtcNow));
        }
    }

    /// <summary>
    /// Incrementa las existencias del inventario por compras o devoluciones autorizadas.
    /// </summary>
    public void IncrementarStock(int cantidad)
    {
        if (cantidad <= 0)
            throw new ReglaNegocioException("Producto.CantidadInvalida", "La cantidad a ingresar debe ser estrictamente mayor a cero.");

        Stock += cantidad;
    }

    /// <summary>
    /// Actualiza el precio del producto validando que no sea negativo.
    /// </summary>
    public void ActualizarPrecio(decimal nuevoPrecio)
    {
        if (nuevoPrecio < 0)
            throw new ValorInvalidoException(nameof(Precio), "El precio del producto no puede ser inferior a cero.");

        Precio = nuevoPrecio;
    }
}
