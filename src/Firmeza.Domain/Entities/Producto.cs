// Este archivo representa un producto o material de construcción dentro del catálogo de Firmeza.
// Sirve para administrar toda la información de cada artículo en venta: su código, nombre, precio,
// cantidad disponible en bodega, proveedor y las reglas para sumar o descontar existencias sin errores.

using Firmeza.Domain.Common;
using Firmeza.Domain.Events;
using Firmeza.Domain.Exceptions;

namespace Firmeza.Domain.Entities;

// Representa un artículo o material de construcción guardado en la tabla 'productos' de la base de datos.
public class Producto : AggregateRoot
{
    // Número identificador único del producto en la base de datos.
    public int Id { get; set; }

    // Código único de referencia o código de barras del producto en el catálogo.
    public string Codigo { get; set; } = null!;

    // Nombre comercial del producto (por ejemplo: Cemento Gris 50 kg o Varilla Corrugada).
    public string Nombre { get; set; } = null!;

    // Detalle adicional sobre las características, medidas o usos del material.
    public string? Descripcion { get; set; }

    // Precio de venta por cada unidad del producto antes de aplicar impuestos.
    public decimal Precio { get; set; }

    // Cantidad de unidades físicas disponibles actualmente en la bodega para la venta.
    public int Stock { get; set; }

    // Identificador de la categoría o familia a la que pertenece este producto.
    public int? CategoriaId { get; set; }

    // Identificador de la empresa proveedora o fabricante que suministra este producto.
    public int? ProveedorId { get; set; }

    // Indica si el producto está disponible en el catálogo (true) o si fue ocultado sin borrar su historial (false).
    public bool Activo { get; set; } = true;

    // Fecha y hora en que el producto fue dado de alta en el catálogo.
    public DateTime CreadoEn { get; set; }

    // Datos de la categoría a la que pertenece el producto.
    public virtual Categoria? Categoria { get; set; }

    // Datos del proveedor que suministra el producto.
    public virtual Proveedor? Proveedor { get; set; }

    // Lista de todos los renglones de facturas en los que se ha vendido este producto.
    public virtual ICollection<DetalleVenta> DetallesVenta { get; set; } = new List<DetalleVenta>();

    // Historial completo de entradas, salidas y ajustes de bodega de este producto.
    public virtual ICollection<MovimientoInventario> MovimientosInventario { get; set; } = new List<MovimientoInventario>();

    // Resta unidades de la bodega al realizar una venta, verificando que alcance la mercancía y avisando si quedan 10 o menos unidades.
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

    // Suma nuevas unidades al inventario cuando llega mercancía de un proveedor o se hace una devolución.
    public void IncrementarStock(int cantidad)
    {
        if (cantidad <= 0)
            throw new ReglaNegocioException("Producto.CantidadInvalida", "La cantidad a ingresar debe ser estrictamente mayor a cero.");

        Stock += cantidad;
    }

    // Cambia el precio unitario del producto asegurando que nunca se guarde un valor negativo.
    public void ActualizarPrecio(decimal nuevoPrecio)
    {
        if (nuevoPrecio < 0)
            throw new ValorInvalidoException(nameof(Precio), "El precio del producto no puede ser inferior a cero.");

        Precio = nuevoPrecio;
    }
}
