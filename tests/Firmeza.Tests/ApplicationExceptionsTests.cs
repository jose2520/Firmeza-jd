using System;
using System.Collections.Generic;
using Firmeza.Application.Exceptions;
using FluentAssertions;
using Xunit;

namespace Firmeza.Tests;

public class ApplicationExceptionsTests
{
    [Fact]
    public void NotFoundException_ShouldFormatMessageCorrectly_WithEntityNameAndKey()
    {
        // Act
        var ex = new NotFoundException("Cliente", 42);

        // Assert
        ex.Message.Should().Be("La entidad 'Cliente' con identificador '42' no fue encontrada.");
        ex.Should().BeAssignableTo<Exception>();
    }

    [Fact]
    public void NotFoundException_ShouldPreserveDirectMessage()
    {
        var ex = new NotFoundException("Producto no encontrado en inventario");
        ex.Message.Should().Be("Producto no encontrado en inventario");
    }

    [Fact]
    public void ConflictException_ShouldFormatMessageCorrectly_WithEntityAndProperty()
    {
        // Act
        var ex = new ConflictException("Cliente", "Identificacion", "10203040");

        // Assert
        ex.Message.Should().Be("Ya existe un registro de 'Cliente' con el valor '10203040' en el campo 'Identificacion'.");
        ex.Should().BeAssignableTo<Exception>();
    }

    [Fact]
    public void ConflictException_ShouldPreserveDirectMessage()
    {
        var ex = new ConflictException("El código SKU ya está en uso");
        ex.Message.Should().Be("El código SKU ya está en uso");
    }

    [Fact]
    public void ValidationException_ShouldStoreErrorsDictionary()
    {
        // Arrange
        var errors = new Dictionary<string, string[]>
        {
            { "Email", new[] { "Formato inválido", "El correo es requerido" } },
            { "Telefono", new[] { "Debe contener al menos 7 dígitos" } }
        };

        // Act
        var ex = new ValidationException(errors);

        // Assert
        ex.Errors.Should().HaveCount(2);
        ex.Errors["Email"].Should().Contain("Formato inválido");
        ex.Errors["Telefono"].Should().ContainSingle();
        ex.Message.Should().Contain("errores de validación");
    }

    [Fact]
    public void ValidationException_SinglePropertyConstructor_ShouldInitializeProperly()
    {
        var ex = new ValidationException("Precio", "El precio debe ser mayor a cero");
        ex.Errors.Should().ContainKey("Precio");
        ex.Errors["Precio"].Should().Contain("El precio debe ser mayor a cero");
    }

    [Fact]
    public void ForbiddenAccessException_ShouldProvideDefaultMessage()
    {
        var ex = new ForbiddenAccessException();
        ex.Message.Should().Be("No tiene los permisos requeridos para acceder o modificar este recurso.");
        ex.Should().BeAssignableTo<Exception>();
    }

    [Fact]
    public void ForbiddenAccessException_ShouldPreserveCustomMessage()
    {
        var ex = new ForbiddenAccessException("Solo administradores pueden eliminar registros");
        ex.Message.Should().Be("Solo administradores pueden eliminar registros");
    }
}
