// Archivo: TipoMovimientoInventario.cs
// Define las razones por las cuales cambia la cantidad de un producto en la bodega.
// Sirve para llevar el historial claro de entradas, salidas y correcciones de mercancía.

namespace Firmeza.Domain.Enums;

// Tipos de operaciones que suman, restan o corrigen las existencias en el almacén.
public enum TipoMovimientoInventario
{
    // Aumento de productos en bodega por compras a proveedores o devoluciones.
    Entrada = 1,

    // Disminución de productos en bodega por ventas entregadas a los clientes.
    Salida = 2,

    // Corrección manual de cantidades después de un conteo físico o por material dañado.
    Ajuste = 3
}
