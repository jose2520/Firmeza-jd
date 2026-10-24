// Este archivo representa a una empresa proveedora o fabricante que abastece a la ferretería.
// En el negocio de Firmeza, sirve para guardar los datos tributarios, el nombre del contacto comercial,
// el teléfono y la ubicación de los aliados que suministran materiales de construcción.

namespace Firmeza.Domain.Entities;

// Representa la información de un proveedor o fabricante aliado en la tabla 'proveedores'.
public class Proveedor
{
    // Número identificador único del proveedor en la base de datos.
    public int Id { get; set; }

    // Número de identificación tributaria (NIT) único de la empresa proveedora.
    public string Nit { get; set; } = null!;

    // Nombre legal o comercial de la empresa proveedora (por ejemplo: Cementos Argos o Pavco).
    public string RazonSocial { get; set; } = null!;

    // Nombre completo del asesor o persona de contacto dentro del proveedor.
    public string? ContactoNombre { get; set; }

    // Número de teléfono o celular para comunicarse con el proveedor.
    public string? Telefono { get; set; }

    // Dirección de correo electrónico para enviar pedidos o solicitudes de cotización.
    public string? Email { get; set; }

    // Dirección física de las oficinas o bodega principal del proveedor.
    public string? Direccion { get; set; }

    // Ciudad donde se encuentra ubicado el proveedor.
    public string? Ciudad { get; set; }

    // Indica si el proveedor sigue trabajando activamente con la ferretería (true) o si está inactivo (false).
    public bool Activo { get; set; } = true;

    // Fecha y hora en que se registró al proveedor en el sistema.
    public DateTime CreadoEn { get; set; }

    // Lista de todos los productos del catálogo que son suministrados por este proveedor.
    public virtual ICollection<Producto> Productos { get; set; } = new List<Producto>();
}
