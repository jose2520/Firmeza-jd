// =================================================================================================
// SERVICIO: ClienteService.cs
// CAPA: Firmeza.Infrastructure (Capa de Infraestructura y Persistencia)
// CONTRATO: IClienteService (Firmeza.Application)
// PROPÓSITO:
//   Implementa las operaciones CRUD sobre la cartera de clientes:
//   - Listado y consulta por ID.
//   - Registro con validación de unicidad en Identificación (Cédula/NIT) y Correo Electrónico.
//   - Actualización de datos de contacto.
//   - Borrado inteligente: Eliminación física si el cliente no tiene compras, o desactivación
//     lógica (Activo = false) si posee historial de ventas para preservar la integridad referencial.
//
// ORIGEN Y DESTINO DE LOS DATOS:
//   - Base de Datos Relacional: PostgreSQL (tabla 'clientes' y consulta en 'ventas').
// =================================================================================================

using Firmeza.Application.DTOs;
using Firmeza.Application.Interfaces;
using Firmeza.Domain.Entities;
using Firmeza.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.Infrastructure.Services;

public class ClienteService : IClienteService
{
    private readonly ApplicationDbContext _context;

    public ClienteService(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Lista todos los clientes registrados ordenados por ID.
    /// ORIGEN DE DATOS: PostgreSQL (tabla 'clientes').
    /// </summary>
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

    /// <summary>
    /// Consulta un cliente por su clave primaria.
    /// ORIGEN DE DATOS: PostgreSQL (tabla 'clientes' donde id = {id}).
    /// </summary>
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

    /// <summary>
    /// Crea un nuevo cliente validando que no exista duplicidad de Identificación ni Email.
    /// ORIGEN DE DATOS: Insertado en PostgreSQL (tabla 'clientes').
    /// </summary>
    public async Task<ClienteDto> CreateAsync(CreateClienteDto dto)
    {
        if (dto.Edad.HasValue && dto.Edad.Value < 0)
        {
            throw new InvalidOperationException("La edad del cliente no puede ser un número negativo.");
        }

        var identificacionExistente = await _context.Clientes
            .AnyAsync(c => c.Identificacion == dto.Identificacion);

        if (identificacionExistente)
        {
            throw new InvalidOperationException($"Ya existe un cliente con la identificación '{dto.Identificacion}'.");
        }

        if (!string.IsNullOrWhiteSpace(dto.Email))
        {
            var emailExistente = await _context.Clientes
                .AnyAsync(c => c.Email == dto.Email);

            if (emailExistente)
            {
                throw new InvalidOperationException($"Ya existe un cliente con el correo electrónico '{dto.Email}'.");
            }
        }

        var cliente = new Cliente
        {
            Identificacion = dto.Identificacion.Trim(),
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

    /// <summary>
    /// Actualiza los datos de un cliente existente.
    /// ORIGEN DE DATOS: Modifica la fila en PostgreSQL (tabla 'clientes').
    /// </summary>
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
            var emailExistente = await _context.Clientes
                .AnyAsync(c => c.Email == dto.Email && c.Id != id);

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

    /// <summary>
    /// Elimina un cliente. Si tiene ventas asociadas, se realiza un borrado lógico (desactivación: Activo = false)
    /// para proteger la integridad referencial y el historial financiero en la base de datos.
    /// ORIGEN DE DATOS: PostgreSQL (evalúa existencia en 'ventas', modifica o elimina en 'clientes').
    /// </summary>
    public async Task<bool> DeleteAsync(int id)
    {
        var cliente = await _context.Clientes.FindAsync(id);
        if (cliente == null) return false;

        var tieneVentas = await _context.Ventas.AnyAsync(v => v.ClienteId == id);
        if (tieneVentas)
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
