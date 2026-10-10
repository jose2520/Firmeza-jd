using AutoMapper;
using Firmeza.Application.DTOs;
using Firmeza.Application.Mappings;
using Firmeza.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;

namespace Firmeza.Tests;

public class AutoMapperTests
{
    private readonly IMapper _mapper;

    public AutoMapperTests()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        }, NullLoggerFactory.Instance);

        _mapper = config.CreateMapper();
    }

    [Fact]
    public void Mapper_ShouldMapClienteToClienteDtoCorrectly()
    {
        // Arrange
        var cliente = new Cliente
        {
            Id = 1,
            Identificacion = "1020304050",
            Nombres = "Carlos",
            Apellidos = "Gómez",
            Email = "carlos@example.com",
            Telefono = "3001234567",
            Direccion = "Calle 100",
            UserId = "USER-01",
            Edad = 35,
            Activo = true,
            CreadoEn = DateTime.UtcNow
        };

        // Act
        var dto = _mapper.Map<ClienteDto>(cliente);

        // Assert
        dto.Should().NotBeNull();
        dto.Id.Should().Be(1);
        dto.Identificacion.Should().Be("1020304050");
        dto.Nombres.Should().Be("Carlos");
        dto.Apellidos.Should().Be("Gómez");
        dto.Email.Should().Be("carlos@example.com");
        dto.UserId.Should().Be("USER-01");
        dto.Edad.Should().Be(35);
    }

    [Fact]
    public void Mapper_ShouldMapProductoToProductoDtoCorrectly()
    {
        // Arrange
        var producto = new Producto
        {
            Id = 10,
            Codigo = "PROD-999",
            Nombre = "Martillo de Uña",
            Descripcion = "Herramienta de impacto",
            Precio = 35000m,
            Stock = 50,
            Activo = true
        };

        // Act
        var dto = _mapper.Map<ProductoDto>(producto);

        // Assert
        dto.Should().NotBeNull();
        dto.Id.Should().Be(10);
        dto.Codigo.Should().Be("PROD-999");
        dto.Nombre.Should().Be("Martillo de Uña");
        dto.Precio.Should().Be(35000m);
        dto.Stock.Should().Be(50);
    }
}
