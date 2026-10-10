namespace Firmeza.Domain.Enums;

/// <summary>
/// Tipos de medios de pago aceptados para compras y alquileres en Firmeza.
/// </summary>
public enum TipoMetodoPago
{
    /// <summary>
    /// Pago en efectivo en caja o contra entrega.
    /// </summary>
    Efectivo = 1,

    /// <summary>
    /// Transferencia bancaria directa (Bancolombia, Davivienda, etc.).
    /// </summary>
    TransferenciaBancaria = 2,

    /// <summary>
    /// Tarjeta débito mediante datáfono o pasarela.
    /// </summary>
    TarjetaDebito = 3,

    /// <summary>
    /// Tarjeta crédito.
    /// </summary>
    TarjetaCredito = 4,

    /// <summary>
    /// Crédito comercial directo acordado con constructoras y contratistas (plazo 30/60 días).
    /// </summary>
    CreditoComercial = 5,

    /// <summary>
    /// Cheque de gerencia certificado.
    /// </summary>
    ChequeGerencia = 6,

    /// <summary>
    /// Billetera digital Nequi.
    /// </summary>
    Nequi = 7,

    /// <summary>
    /// Billetera digital Daviplata.
    /// </summary>
    Daviplata = 8
}
