using System;
using System.Collections.Generic;
using Firmeza.Domain.Common;
using Firmeza.Domain.Events;
using Firmeza.Domain.Exceptions;

namespace Firmeza.Domain.Entities;

/// <summary>
/// Representa la entidad de dominio y Agregado Raíz para el Encabezado de una Transacción de Venta.
/// 
/// PARA QUÉ SIRVE:
///   Almacena los datos consolidados de una factura o recibo de venta: número consecutivo,
///   cliente asociado, método de pago, fecha, subtotal, impuesto (IVA), total a pagar y estado.
/// 
/// ORIGEN DE LOS DATOS:
///   Mapea directamente contra la tabla "ventas" de la base de datos PostgreSQL ('firmeza_db').
///   Gestionada a través de ApplicationDbContext en la capa de Infraestructura.
/// </summary>
public partial class Venta : AggregateRoot
{
    /// <summary>
    /// Identificador único autoincremental de la clave primaria.
    /// Origen: Columna 'id' (SERIAL PRIMARY KEY) de la tabla 'ventas'.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Código consecutivo único de la venta (ej. VENTA-2026-0001).
    /// Origen: Columna 'numero_venta' (VARCHAR(50) UNIQUE) en PostgreSQL.
    /// </summary>
    public string NumeroVenta { get; set; } = null!;

    /// <summary>
    /// Clave foránea que referencia al cliente que realizó la compra.
    /// Origen: Columna 'cliente_id' (INT NOT NULL) con FK hacia clientes(id) en PostgreSQL.
    /// </summary>
    public int ClienteId { get; set; }

    /// <summary>
    /// Clave foránea opcional que referencia el método de pago utilizado.
    /// Origen: Columna 'metodo_pago_id' (INT) con FK hacia metodos_pago(id) en PostgreSQL.
    /// </summary>
    public int? MetodoPagoId { get; set; }

    /// <summary>
    /// Fecha y hora exacta de emisión de la venta.
    /// Origen: Columna 'fecha' (TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP) en PostgreSQL.
    /// </summary>
    public DateTime Fecha { get; set; }

    /// <summary>
    /// Valor monetario antes de impuestos calculado sumando los subtotales de sus renglones.
    /// Origen: Columna 'subtotal' (NUMERIC(12,2) CHECK >= 0) en PostgreSQL.
    /// </summary>
    public decimal Subtotal { get; set; }

    /// <summary>
    /// Valor monetario del impuesto al valor agregado (IVA 19%).
    /// Origen: Columna 'iva' (NUMERIC(12,2) CHECK >= 0) en PostgreSQL.
    /// </summary>
    public decimal Iva { get; set; }

    /// <summary>
    /// Importe total de la venta (Subtotal + IVA).
    /// Origen: Columna 'total' (NUMERIC(12,2) CHECK >= 0) en PostgreSQL.
    /// </summary>
    public decimal Total { get; set; }

    /// <summary>
    /// Estado del ciclo de vida de la transacción ('Completada', 'Pendiente', 'Anulada').
    /// Origen: Columna 'estado' (VARCHAR(20) DEFAULT 'Completada') en PostgreSQL.
    /// </summary>
    public string Estado { get; set; } = null!;

    /// <summary>
    /// Propiedad de navegación hacia el cliente comprador (Relación N a 1).
    /// </summary>
    public virtual Cliente Cliente { get; set; } = null!;

    /// <summary>
    /// Propiedad de navegación hacia el método de pago utilizado (Relación N a 1).
    /// </summary>
    public virtual MetodoPago? MetodoPago { get; set; }

    /// <summary>
    /// Información logística del despacho a obra, si aplica (Relación 1 a 1).
    /// </summary>
    public virtual Despacho? Despacho { get; set; }

    /// <summary>
    /// Colección de renglones o partidas individuales adquiridas (Relación 1 a N).
    /// </summary>
    public virtual ICollection<DetalleVenta> DetallesVenta { get; set; } = new List<DetalleVenta>();

    // =========================================================================
    // MÉTODOS DE DOMINIO Y REGLAS DE NEGOCIO (DDD)
    // =========================================================================

    /// <summary>
    /// Valida y confirma la venta emitiendo el evento VentaRegistradaDomainEvent.
    /// </summary>
    public void RegistrarVenta()
    {
        if (Total < 0)
            throw new ValorInvalidoException(nameof(Total), "El total de la venta no puede ser negativo.");

        Estado = "Completada";
        AddDomainEvent(new VentaRegistradaDomainEvent(Id, NumeroVenta, ClienteId, Total, DateTime.UtcNow));
    }

    /// <summary>
    /// Anula la transacción comercial y emite VentaCanceladaDomainEvent.
    /// </summary>
    public void CancelarVenta(string motivo)
    {
        if (Estado == "Anulada")
            throw new ReglaNegocioException("Venta.YaAnulada", "La venta ya se encuentra en estado anulada.");

        Estado = "Anulada";
        AddDomainEvent(new VentaCanceladaDomainEvent(Id, NumeroVenta, motivo, DateTime.UtcNow));
    }
}
