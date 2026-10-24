// Este archivo representa un registro en el historial de entradas, salidas o ajustes de bodega.
// En el negocio de Firmeza, sirve para saber exactamente cuándo entró o salió mercancía, quién hizo
// el movimiento, cuántas unidades había antes y cuántas quedaron después en el inventario.

namespace Firmeza.Domain.Entities;

// Representa un movimiento de entrada, salida o ajuste de mercancía en la tabla 'movimientos_inventario'.
public class MovimientoInventario
{
    // Número identificador único del movimiento de bodega en la base de datos.
    public int Id { get; set; }

    // Identificador del producto o material cuyas existencias cambiaron.
    public int ProductoId { get; set; }

    // Identificador del empleado o usuario que realizó el movimiento en el sistema.
    public string? UsuarioId { get; set; }

    // Tipo de operación realizada en la bodega (puede ser Entrada, Salida o Ajuste).
    public string TipoMovimiento { get; set; } = null!; // Entrada, Salida, Ajuste

    // Cantidad de unidades que entraron, salieron o se ajustaron en esta operación.
    public int Cantidad { get; set; }

    // Cantidad de unidades que había disponibles en bodega justo antes de este movimiento.
    public int StockPrevio { get; set; }

    // Cantidad de unidades que quedaron disponibles en bodega después de aplicar este movimiento.
    public int StockPosterior { get; set; }

    // Código del documento relacionado con el movimiento (por ejemplo, el número de factura o de orden de compra).
    public string? Referencia { get; set; }

    // Explicación breve de por qué se hizo el movimiento (por ejemplo: venta a cliente, llegada de proveedor o merma).
    public string? Motivo { get; set; }

    // Fecha y hora exacta en que se registró el movimiento en la bodega.
    public DateTime Fecha { get; set; }

    // Información completa del producto al que corresponde este movimiento de inventario.
    public virtual Producto Producto { get; set; } = null!;
}
