namespace Firmeza.Domain.Exceptions;

/// <summary>
/// Excepción lanzada cuando una operación de salida de inventario supera el stock físico disponible.
/// </summary>
public class StockInsuficienteException : DomainException
{
    public string CodigoProducto { get; }
    public int StockDisponible { get; }
    public int CantidadSolicitada { get; }

    public StockInsuficienteException(string codigoProducto, int stockDisponible, int cantidadSolicitada)
        : base($"Stock insuficiente para el producto '{codigoProducto}'. Existencias disponibles: {stockDisponible}, Cantidad solicitada: {cantidadSolicitada}.")
    {
        CodigoProducto = codigoProducto;
        StockDisponible = stockDisponible;
        CantidadSolicitada = cantidadSolicitada;
    }
}
