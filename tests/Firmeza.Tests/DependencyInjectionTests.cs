using Firmeza.Application;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Firmeza.Tests;

public class DependencyInjectionTests
{
    [Fact]
    public void AddApplicationServices_ShouldRegisterServicesSuccessfully()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddApplicationServices();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeSameAs(services);
    }

    [Fact]
    public void Moq_ShouldBeFunctionalForMockingDependencies()
    {
        // Arrange
        var mockService = new Mock<IDisposable>();
        mockService.Setup(s => s.Dispose());

        // Act
        mockService.Object.Dispose();

        // Assert
        mockService.Verify(s => s.Dispose(), Times.Once);
    }
}
