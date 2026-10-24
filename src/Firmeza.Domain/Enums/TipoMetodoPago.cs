// Archivo: TipoMetodoPago.cs
// Define las diferentes formas de pago que pueden usar los clientes al comprar o alquilar.
// Sirve para llevar el control en caja y bancos de cómo ingresa el dinero a la empresa.

namespace Firmeza.Domain.Enums;

// Medios de pago aceptados en Firmeza para compras de materiales y alquiler de equipos.
public enum TipoMetodoPago
{
    // Pago en billetes y monedas directamente en la caja o al momento de recibir el pedido.
    Efectivo = 1,

    // Pago enviado de cuenta a cuenta a través de bancos como Bancolombia o Davivienda.
    TransferenciaBancaria = 2,

    // Pago con tarjeta débito pasando por datáfono o enlace de pago.
    TarjetaDebito = 3,

    // Pago con tarjeta de crédito bancaria.
    TarjetaCredito = 4,

    // Pago a plazo otorgado directamente a constructoras y clientes autorizados (30 o 60 días).
    CreditoComercial = 5,

    // Pago mediante cheque certificado emitido por una entidad bancaria.
    ChequeGerencia = 6,

    // Pago rápido desde el celular usando la billetera digital Nequi.
    Nequi = 7,

    // Pago rápido desde el celular usando la billetera digital Daviplata.
    Daviplata = 8
}
