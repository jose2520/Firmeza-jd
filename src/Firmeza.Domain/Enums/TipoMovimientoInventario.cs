namespace Firmeza.Domain.Enums;

/// <summary>
/// Clasificación de transacciones de inventario en el Kardex.
/// </summary>
public enum TipoMovimientoInventario
{
    /// <summary>
    /// Ingreso de mercancía por compras a fabricantes o proveedores.
    /// </summary>
    Entrada = 1,

    /// <summary>
    /// Retiro de existencias por facturación y entrega a clientes.
    /// </summary>
    Salida = 2,

    /// <summary>
    /// Corrección de existencias por inventario físico o mermas justificadas.
    /// </summary>
    Ajuste = 3
}
