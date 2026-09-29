using Firmeza.Application.DTOs;
using Firmeza.Application.Interfaces;
using Firmeza.Domain.Entities;
using Firmeza.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.Infrastructure.Services;

public class ClienteService : IClienteService
{
    private readonly ApplicationDbContext _context;

    public ClienteService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ClienteDto>> GetAllAsync()
    {
        return await _context.Clientes
            .AsNoTracking()
            .Select(c => new ClienteDto(
                c.Id,
                c.Identificacion,
                c.Nombres,
                c.Apellidos,
                c.Email,
                c.Telefono,
                c.Direccion,
                c.Activo,
                c.CreadoEn))
            .ToListAsync();
    }

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
            c.CreadoEn);
    }

    public async Task<ClienteDto> CreateAsync(CreateClienteDto dto)
    {
        var cliente = new Cliente
        {
            Identificacion = dto.Identificacion,
            Nombres = dto.Nombres,
            Apellidos = dto.Apellidos,
            Email = dto.Email ?? string.Empty,
            Telefono = dto.Telefono,
            Direccion = dto.Direccion,
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
            cliente.CreadoEn);
    }

    public async Task<ClienteDto?> UpdateAsync(int id, UpdateClienteDto dto)
    {
        var cliente = await _context.Clientes.FindAsync(id);
        if (cliente == null) return null;

        cliente.Nombres = dto.Nombres;
        cliente.Apellidos = dto.Apellidos;
        cliente.Email = dto.Email ?? cliente.Email;
        cliente.Telefono = dto.Telefono;
        cliente.Direccion = dto.Direccion;
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
            cliente.CreadoEn);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var cliente = await _context.Clientes.FindAsync(id);
        if (cliente == null) return false;

        _context.Clientes.Remove(cliente);
        await _context.SaveChangesAsync();
        return true;
    }
}
