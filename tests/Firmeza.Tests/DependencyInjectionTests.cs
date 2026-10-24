// Pruebas para comprobar que el registro inicial de servicios de la aplicación funcione sin fallos.
// También verifica que las herramientas de simulación utilizadas en las pruebas unitarias
// operen correctamente.

using Firmeza.Application;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Firmeza.Tests;

public class DependencyInjectionTests
{
    // Comprueba que el método encargado de registrar los servicios de la capa de aplicación se ejecute y devuelva la colección lista.
    [Fact]
    public void AddApplicationServices_ShouldRegisterServicesSuccessfully()
    {
        // Preparar la colección de servicios
        var services = new ServiceCollection();

        // Registrar los servicios de aplicación
        var result = services.AddApplicationServices();

        // Comprobar que el registro fue exitoso
        result.Should().NotBeNull();
        result.Should().BeSameAs(services);
    }

    // Verifica que la librería de pruebas permita simular objetos y comprobar cuántas veces se llaman sus métodos.
    [Fact]
    public void Moq_ShouldBeFunctionalForMockingDependencies()
    {
        // Preparar un objeto simulado de prueba
        var mockService = new Mock<IDisposable>();
        mockService.Setup(s => s.Dispose());

        // Ejecutar el método simulado
        mockService.Object.Dispose();

        // Comprobar que el método se llamó exactamente una vez
        mockService.Verify(s => s.Dispose(), Times.Once);
    }
}
