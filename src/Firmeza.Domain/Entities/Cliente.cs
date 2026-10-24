// Este archivo representa a un cliente registrado en la plataforma comercial de Firmeza.
// Sirve para guardar los datos personales, de contacto y ubicación de las personas o empresas
// que compran materiales de construcción o alquilan maquinaria y vehículos en la ferretería.

namespace Firmeza.Domain.Entities;

// Representa la información de un cliente guardada en la tabla 'clientes' de la base de datos.
public class Cliente
{
    // Número identificador único del cliente en la base de datos.
    public int Id { get; set; }

    // Número de documento de identidad del cliente (cédula de ciudadanía, NIT o pasaporte).
    public string Identificacion { get; set; } = null!;

    // Nombres de pila del cliente o nombre del representante.
    public string Nombres { get; set; } = null!;

    // Apellidos del cliente.
    public string Apellidos { get; set; } = null!;

    // Dirección de correo electrónico única para enviar facturas y notificaciones.
    public string Email { get; set; } = null!;

    // Número de teléfono fijo o celular para contactar al cliente.
    public string? Telefono { get; set; }

    // Dirección física de residencia o de la obra principal del cliente.
    public string? Direccion { get; set; }

    // Identificador de la cuenta de usuario del sistema, en caso de que el cliente tenga acceso al portal web.
    public string? UserId { get; set; }

    // Edad del cliente expresada en años.
    public int? Edad { get; set; }

    // Indica si el cliente está habilitado para operar (true) o si fue desactivado sin borrar su historial (false).
    public bool Activo { get; set; } = true;

    // Fecha y hora en que el cliente fue registrado por primera vez en el sistema.
    public DateTime CreadoEn { get; set; }

    // Historial de todas las compras de materiales realizadas por este cliente.
    public virtual ICollection<Venta> Ventas { get; set; } = new List<Venta>();

    // Historial de todos los contratos de alquiler de vehículos o maquinaria tomados por este cliente.
    public virtual ICollection<Alquiler> Alquileres { get; set; } = new List<Alquiler>();
}
