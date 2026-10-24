// Servicio que gestiona la creación, consulta, actualización y eliminación de clientes.
// Trabaja principalmente con la tabla "clientes" de PostgreSQL y consulta las tablas "ventas" y "alquileres"
// para decidir si un cliente se puede borrar por completo o solo desactivar para no perder su historial.
using Firmeza.Application.DTOs;
using Firmeza.Application.Interfaces;
using Firmeza.Domain.Entities;
using Firmeza.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.Infrastructure.Services;

public class ClienteService : IClienteService
{
    private readonly ApplicationDbContext _context;

    // Inicializa el servicio de clientes con la conexión a la base de datos.
    public ClienteService(ApplicationDbContext context)
    {
        _context = context;
    }

    // Obtiene la lista de todos los clientes registrados en la tabla "clientes", ordenados por su ID.
    public async Task<IEnumerable<ClienteDto>> GetAllAsync()
    {
        return await _context.Clientes
            .AsNoTracking()
            .OrderBy(c => c.Id)
            .Select(c => new ClienteDto(
                c.Id,
                c.Identificacion,
                c.Nombres,
                c.Apellidos,
                c.Email,
                c.Telefono,
                c.Direccion,
                c.Activo,
                c.CreadoEn,
                c.UserId,
                c.Edad))
            .ToListAsync();
    }

    // Busca y devuelve los datos de un cliente específico según su ID en la tabla "clientes".
    public async Task<ClienteDto?> GetByIdAsync(int id)
    {
        var c = await _context.Clientes.FindAsync(id);
        if (c == null) return null;

        return new ClienteDto(
            c.Id,
            c.Identificacion,
            c.Nombres,
            c.Apellidos,
            c.Email,
            c.Telefono,
            c.Direccion,
            c.Activo,
            c.CreadoEn,
            c.UserId,
            c.Edad);
    }

    // Registra un nuevo cliente en la tabla "clientes" tras verificar que su documento y correo no estén repetidos.
    public async Task<ClienteDto> CreateAsync(CreateClienteDto dto)
    {
        if (dto.Edad.HasValue && dto.Edad.Value < 0)
        {
            throw new InvalidOperationException("La edad del cliente no puede ser un número negativo.");
        }

        var identificacionNormalizada = dto.Identificacion.Trim();
        var identificacionExistente = await _context.Clientes
            .AnyAsync(c => c.Identificacion == identificacionNormalizada);

        if (identificacionExistente)
        {
            throw new InvalidOperationException($"Ya existe un cliente con la identificación '{dto.Identificacion}'.");
        }

        if (!string.IsNullOrWhiteSpace(dto.Email))
        {
            var emailNormalizado = dto.Email.Trim().ToLower();
            var emailExistente = await _context.Clientes
                .AnyAsync(c => c.Email.ToLower() == emailNormalizado);

            if (emailExistente)
            {
                throw new InvalidOperationException($"Ya existe un cliente con el correo electrónico '{dto.Email}'.");
            }
        }

        var cliente = new Cliente
        {
            Identificacion = identificacionNormalizada,
            Nombres = dto.Nombres.Trim(),
            Apellidos = dto.Apellidos.Trim(),
            Email = (dto.Email ?? string.Empty).Trim(),
            Telefono = dto.Telefono?.Trim(),
            Direccion = dto.Direccion?.Trim(),
            UserId = dto.UserId,
            Edad = dto.Edad,
            Activo = true,
            CreadoEn = DateTime.UtcNow
        };

        _context.Clientes.Add(cliente);
        await _context.SaveChangesAsync();

        return new ClienteDto(
            cliente.Id,
            cliente.Identificacion,
            cliente.Nombres,
            cliente.Apellidos,
            cliente.Email,
            cliente.Telefono,
            cliente.Direccion,
            cliente.Activo,
            cliente.CreadoEn,
            cliente.UserId,
            cliente.Edad);
    }

    // Actualiza la información personal y de contacto de un cliente existente en la tabla "clientes".
    public async Task<ClienteDto?> UpdateAsync(int id, UpdateClienteDto dto)
    {
        if (dto.Edad.HasValue && dto.Edad.Value < 0)
        {
            throw new InvalidOperationException("La edad del cliente no puede ser un número negativo.");
        }

        var cliente = await _context.Clientes.FindAsync(id);
        if (cliente == null) return null;

        if (!string.IsNullOrWhiteSpace(dto.Email))
        {
            var emailNormalizado = dto.Email.Trim().ToLower();
            var emailExistente = await _context.Clientes
                .AnyAsync(c => c.Email.ToLower() == emailNormalizado && c.Id != id);

            if (emailExistente)
            {
                throw new InvalidOperationException($"Ya existe otro cliente con el correo electrónico '{dto.Email}'.");
            }
        }

        cliente.Nombres = dto.Nombres.Trim();
        cliente.Apellidos = dto.Apellidos.Trim();
        cliente.Email = (dto.Email ?? cliente.Email).Trim();
        cliente.Telefono = dto.Telefono?.Trim();
        cliente.Direccion = dto.Direccion?.Trim();
        cliente.UserId = dto.UserId ?? cliente.UserId;
        cliente.Edad = dto.Edad ?? cliente.Edad;
        cliente.Activo = dto.Activo;

        await _context.SaveChangesAsync();

        return new ClienteDto(
            cliente.Id,
            cliente.Identificacion,
            cliente.Nombres,
            cliente.Apellidos,
            cliente.Email,
            cliente.Telefono,
            cliente.Direccion,
            cliente.Activo,
            cliente.CreadoEn,
            cliente.UserId,
            cliente.Edad);
    }

    // Elimina al cliente si no tiene compras ni alquileres; si ya tiene historial, solo lo marca como inactivo.
    public async Task<bool> DeleteAsync(int id)
    {
        var cliente = await _context.Clientes.FindAsync(id);
        if (cliente == null) return false;

        var tieneHistorial = await _context.Ventas.AnyAsync(v => v.ClienteId == id)
                          || await _context.Alquileres.AnyAsync(a => a.ClienteId == id);
        if (tieneHistorial)
        {
            cliente.Activo = false;
        }
        else
        {
            _context.Clientes.Remove(cliente);
        }

        await _context.SaveChangesAsync();
        return true;
    }
}
