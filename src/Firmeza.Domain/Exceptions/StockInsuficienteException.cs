// Archivo: StockInsuficienteException.cs
// Representa el error que ocurre cuando se intenta despachar o vender más material del que hay en bodega.
// Sirve para frenar la salida de mercancía y mostrar cuánto hay disponible frente a lo pedido.

namespace Firmeza.Domain.Exceptions;

// Error detallado que informa cuál producto no tiene suficientes unidades en el almacén.
public class StockInsuficienteException : DomainException
{
    // Código del producto que presenta el faltante en bodega.
    public string CodigoProducto { get; }

    // Cantidad real de unidades que hay guardadas actualmente.
    public int StockDisponible { get; }

    // Cantidad de unidades que se intentaron vender o retirar.
    public int CantidadSolicitada { get; }

    // Crea el error armando un mensaje claro con el producto, lo que hay en bodega y lo que se pidió.
    public StockInsuficienteException(string codigoProducto, int stockDisponible, int cantidadSolicitada)
        : base($"Stock insuficiente para el producto '{codigoProducto}'. Existencias disponibles: {stockDisponible}, Cantidad solicitada: {cantidadSolicitada}.")
    {
        CodigoProducto = codigoProducto;
        StockDisponible = stockDisponible;
        CantidadSolicitada = cantidadSolicitada;
    }
}
