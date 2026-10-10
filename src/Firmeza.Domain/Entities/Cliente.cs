using System;
using System.Collections.Generic;

namespace Firmeza.Domain.Entities;

/// <summary>
/// Representa la entidad de dominio para un Cliente comercial en la plataforma Firmeza.
/// 
/// PARA QUÉ SIRVE:
///   Almacena y gestiona la información de identificación, contacto y estado de los clientes
///   que realizan compras de materiales y alquileres de maquinaria/vehículos en el sistema.
/// 
/// ORIGEN DE LOS DATOS:
///   Mapea directamente contra la tabla "clientes" de la base de datos PostgreSQL ('firmeza_db').
///   Gestionada a través de ApplicationDbContext en la capa de Infraestructura.
/// </summary>
public partial class Cliente
{
    /// <summary>
    /// Identificador único autoincremental de la clave primaria.
    /// Origen: Columna 'id' (SERIAL PRIMARY KEY) de la tabla 'clientes'.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Documento de identidad único (Cédula de Ciudadanía, NIT, etc.).
    /// Origen: Columna 'identificacion' (VARCHAR(20) UNIQUE) en PostgreSQL.
    /// </summary>
    public string Identificacion { get; set; } = null!;

    /// <summary>
    /// Nombres del cliente.
    /// Origen: Columna 'nombres' (VARCHAR(100)) en PostgreSQL.
    /// </summary>
    public string Nombres { get; set; } = null!;

    /// <summary>
    /// Apellidos del cliente.
    /// Origen: Columna 'apellidos' (VARCHAR(100)) en PostgreSQL.
    /// </summary>
    public string Apellidos { get; set; } = null!;

    /// <summary>
    /// Correo electrónico único para notificaciones y facturación.
    /// Origen: Columna 'email' (VARCHAR(150) UNIQUE) en PostgreSQL.
    /// </summary>
    public string Email { get; set; } = null!;

    /// <summary>
    /// Número de teléfono o móvil de contacto.
    /// Origen: Columna 'telefono' (VARCHAR(20), opcional) en PostgreSQL.
    /// </summary>
    public string? Telefono { get; set; }

    /// <summary>
    /// Dirección física o de entrega.
    /// Origen: Columna 'direccion' (VARCHAR(200), opcional) en PostgreSQL.
    /// </summary>
    public string? Direccion { get; set; }

    /// <summary>
    /// Identificador del usuario en Identity (AspNetUsers), si el cliente tiene cuenta de acceso.
    /// Origen: Columna 'user_id' (TEXT, opcional) en PostgreSQL.
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// Edad del cliente en años.
    /// Origen: Columna 'edad' (INT CHECK (edad >= 0), opcional) en PostgreSQL.
    /// </summary>
    public int? Edad { get; set; }

    /// <summary>
    /// Estado lógico del registro para borrado suave (Soft Delete).
    /// Si es false, el cliente está deshabilitado sin ser eliminado físicamente.
    /// Origen: Columna 'activo' (BOOLEAN DEFAULT TRUE) en PostgreSQL.
    /// </summary>
    public bool Activo { get; set; } = true;

    /// <summary>
    /// Marca temporal de creación del registro en el sistema.
    /// Origen: Columna 'creado_en' (TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP) en PostgreSQL.
    /// </summary>
    public DateTime CreadoEn { get; set; }

    /// <summary>
    /// Colección de ventas asociadas a este cliente (Relación 1 a N).
    /// Origen: Clave foránea 'cliente_id' en la tabla 'ventas' de PostgreSQL.
    /// </summary>
    public virtual ICollection<Venta> Ventas { get; set; } = new List<Venta>();

    /// <summary>
    /// Colección de contratos de alquiler suscritos por este cliente (Relación 1 a N).
    /// Origen: Clave foránea 'cliente_id' en la tabla 'alquileres' de PostgreSQL.
    /// </summary>
    public virtual ICollection<Alquiler> Alquileres { get; set; } = new List<Alquiler>();
}
