using Firmeza.Application.DTOs;
using Firmeza.Application.Interfaces;
using Firmeza.Domain.Entities;
using Firmeza.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.Infrastructure.Services;

public class ProductoService : IProductoService
{
    private readonly ApplicationDbContext _context;

    public ProductoService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ProductoDto>> GetAllAsync()
    {
        return await _context.Productos
            .AsNoTracking()
            .Select(p => new ProductoDto(
                p.Id,
                p.Codigo,
                p.Nombre,
                p.Descripcion,
                p.Precio,
                p.Stock,
                p.Activo,
                p.CreadoEn))
            .ToListAsync();
    }

    public async Task<ProductoDto?> GetByIdAsync(int id)
    {
        var p = await _context.Productos.FindAsync(id);
        if (p == null) return null;

        return new ProductoDto(
            p.Id,
            p.Codigo,
            p.Nombre,
            p.Descripcion,
            p.Precio,
            p.Stock,
            p.Activo,
            p.CreadoEn);
    }

    public async Task<ProductoDto> CreateAsync(CreateProductoDto dto)
    {
        var producto = new Producto
        {
            Codigo = dto.Codigo,
            Nombre = dto.Nombre,
            Descripcion = dto.Descripcion,
            Precio = dto.Precio,
            Stock = dto.Stock,
            Activo = true,
            CreadoEn = DateTime.UtcNow
        };

        _context.Productos.Add(producto);
        await _context.SaveChangesAsync();

        return new ProductoDto(
            producto.Id,
            producto.Codigo,
            producto.Nombre,
            producto.Descripcion,
            producto.Precio,
            producto.Stock,
            producto.Activo,
            producto.CreadoEn);
    }

    public async Task<ProductoDto?> UpdateAsync(int id, UpdateProductoDto dto)
    {
        var producto = await _context.Productos.FindAsync(id);
        if (producto == null) return null;

        producto.Nombre = dto.Nombre;
        producto.Descripcion = dto.Descripcion;
        producto.Precio = dto.Precio;
        producto.Stock = dto.Stock;
        producto.Activo = dto.Activo;

        await _context.SaveChangesAsync();

        return new ProductoDto(
            producto.Id,
            producto.Codigo,
            producto.Nombre,
            producto.Descripcion,
            producto.Precio,
            producto.Stock,
            producto.Activo,
            producto.CreadoEn);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var producto = await _context.Productos.FindAsync(id);
        if (producto == null) return false;

        _context.Productos.Remove(producto);
        await _context.SaveChangesAsync();
        return true;
    }
}
