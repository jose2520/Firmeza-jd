// Pruebas para verificar que los errores de aplicación generen los mensajes adecuados.
// Comprueba los casos en que un registro no existe, cuando hay datos repetidos,
// cuando faltan campos obligatorios o cuando un usuario no tiene permiso para realizar una acción.

using Firmeza.Application.Exceptions;
using FluentAssertions;

namespace Firmeza.Tests;

public class ApplicationExceptionsTests
{
    // Verifica que el error de registro no encontrado arme el texto indicando el nombre del elemento y su número identificador.
    [Fact]
    public void NotFoundException_ShouldFormatMessageCorrectly_WithEntityNameAndKey()
    {
        // Ejecutar la prueba
        var ex = new NotFoundException("Cliente", 42);

        // Comprobar el resultado
        ex.Message.Should().Be("La entidad 'Cliente' con identificador '42' no fue encontrada.");
        ex.Should().BeAssignableTo<Exception>();
    }

    // Comprueba que el error de registro no encontrado conserve exactamente el mensaje personalizado que se le envíe.
    [Fact]
    public void NotFoundException_ShouldPreserveDirectMessage()
    {
        var ex = new NotFoundException("Producto no encontrado en inventario");
        ex.Message.Should().Be("Producto no encontrado en inventario");
    }

    // Verifica que el error de datos duplicados indique claramente qué campo y qué valor ya están registrados.
    [Fact]
    public void ConflictException_ShouldFormatMessageCorrectly_WithEntityAndProperty()
    {
        // Ejecutar la prueba
        var ex = new ConflictException("Cliente", "Identificacion", "10203040");

        // Comprobar el resultado
        ex.Message.Should().Be("Ya existe un registro de 'Cliente' con el valor '10203040' en el campo 'Identificacion'.");
        ex.Should().BeAssignableTo<Exception>();
    }

    // Comprueba que el error de conflicto conserve un mensaje directo cuando se indica manualmente.
    [Fact]
    public void ConflictException_ShouldPreserveDirectMessage()
    {
        var ex = new ConflictException("El código SKU ya está en uso");
        ex.Message.Should().Be("El código SKU ya está en uso");
    }

    // Verifica que el error de validación guarde correctamente la lista de varios campos con sus respectivos mensajes de fallo.
    [Fact]
    public void ValidationException_ShouldStoreErrorsDictionary()
    {
        // Preparar los datos de prueba
        var errors = new Dictionary<string, string[]>
        {
            { "Email", new[] { "Formato inválido", "El correo es requerido" } },
            { "Telefono", new[] { "Debe contener al menos 7 dígitos" } }
        };

        // Ejecutar la prueba
        var ex = new ValidationException(errors);

        // Comprobar el resultado
        ex.Errors.Should().HaveCount(2);
        ex.Errors["Email"].Should().Contain("Formato inválido");
        ex.Errors["Telefono"].Should().ContainSingle();
        ex.Message.Should().Contain("errores de validación");
    }

    // Comprueba que se pueda crear un error de validación indicando un único campo y su motivo de fallo.
    [Fact]
    public void ValidationException_SinglePropertyConstructor_ShouldInitializeProperly()
    {
        var ex = new ValidationException("Precio", "El precio debe ser mayor a cero");
        ex.Errors.Should().ContainKey("Precio");
        ex.Errors["Precio"].Should().Contain("El precio debe ser mayor a cero");
    }

    // Verifica que el error de acceso prohibido muestre un mensaje predeterminado claro cuando no se especifica otro.
    [Fact]
    public void ForbiddenAccessException_ShouldProvideDefaultMessage()
    {
        var ex = new ForbiddenAccessException();
        ex.Message.Should().Be("No tiene los permisos requeridos para acceder o modificar este recurso.");
        ex.Should().BeAssignableTo<Exception>();
    }

    // Comprueba que el error de acceso prohibido conserve el mensaje personalizado cuando se le pasa uno.
    [Fact]
    public void ForbiddenAccessException_ShouldPreserveCustomMessage()
    {
        var ex = new ForbiddenAccessException("Solo administradores pueden eliminar registros");
        ex.Message.Should().Be("Solo administradores pueden eliminar registros");
    }
}
