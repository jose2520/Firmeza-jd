// =================================================================================================
// SERVICIO: ProductoService.cs
// CAPA: Firmeza.Infrastructure (Capa de Infraestructura y Persistencia)
// CONTRATO: IProductoService (Firmeza.Application)
// PROPÓSITO:
//   Implementa las operaciones CRUD sobre el catálogo de productos:
//   - Listado y búsqueda por ID.
//   - Creación con validación de código único y reglas de precios/stock no negativos.
//   - Actualización de precios y disponibilidad.
//   - Borrado inteligente: Eliminación física si no tiene historial de ventas, o borrado lógico
//     (Activo = false) si ya figura en comprobantes previos para evitar violar restricciones de FK.
//
// ORIGEN Y DESTINO DE LOS DATOS:
//   - Base de Datos Relacional: PostgreSQL (tabla 'productos' y consulta en 'detalles_venta').
// =================================================================================================

using Firmeza.Application.DTOs;
using Firmeza.Application.Interfaces;
using Firmeza.Domain.Entities;
using Firmeza.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.Infrastructure.Services;

public class ProductoService : IProductoService
{
    private readonly ApplicationDbContext _context;

    public ProductoService(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Lista todos los productos registrados ordenados ascendentemente por su ID.
    /// ORIGEN DE DATOS: PostgreSQL (tabla 'productos').
    /// </summary>
    public async Task<IEnumerable<ProductoDto>> GetAllAsync()
    {
        return await _context.Productos
            .AsNoTracking()
            .Include(p => p.Categoria)
            .OrderBy(p => p.Id)
            .Select(p => new ProductoDto(
                p.Id,
                p.Codigo,
                p.Nombre,
                p.Descripcion,
                p.Precio,
                p.Stock,
                p.Activo,
                p.CreadoEn,
                p.CategoriaId,
                p.Categoria != null ? p.Categoria.Nombre : null))
            .ToListAsync();
    }

    /// <summary>
    /// Consulta un producto por su clave primaria.
    /// ORIGEN DE DATOS: PostgreSQL (tabla 'productos' donde id = {id}).
    /// </summary>
    public async Task<ProductoDto?> GetByIdAsync(int id)
    {
        var p = await _context.Productos
            .AsNoTracking()
            .Include(p => p.Categoria)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (p == null) return null;

        return new ProductoDto(
            p.Id,
            p.Codigo,
            p.Nombre,
            p.Descripcion,
            p.Precio,
            p.Stock,
            p.Activo,
            p.CreadoEn,
            p.CategoriaId,
            p.Categoria != null ? p.Categoria.Nombre : null);
    }

    /// <summary>
    /// Crea un producto validando unicidad de código y valores positivos de precio y stock.
    /// ORIGEN DE DATOS: Insertado en PostgreSQL (tabla 'productos').
    /// </summary>
    public async Task<ProductoDto> CreateAsync(CreateProductoDto dto)
    {
        var codigoExistente = await _context.Productos
            .AnyAsync(p => p.Codigo == dto.Codigo);

        if (codigoExistente)
        {
            throw new InvalidOperationException($"Ya existe un producto con el código '{dto.Codigo}'.");
        }

        if (dto.Precio < 0)
        {
            throw new InvalidOperationException("El precio del producto no puede ser negativo.");
        }

        if (dto.Stock < 0)
        {
            throw new InvalidOperationException("El stock inicial no puede ser negativo.");
        }

        var producto = new Producto
        {
            Codigo = dto.Codigo.Trim().ToUpperInvariant(),
            Nombre = dto.Nombre.Trim(),
            Descripcion = dto.Descripcion?.Trim(),
            Precio = dto.Precio,
            Stock = dto.Stock,
            CategoriaId = dto.CategoriaId,
            Activo = true,
            CreadoEn = DateTime.UtcNow
        };

        _context.Productos.Add(producto);
        await _context.SaveChangesAsync();

        string? catNombre = null;
        if (producto.CategoriaId.HasValue)
        {
            var cat = await _context.Categorias.FindAsync(producto.CategoriaId.Value);
            catNombre = cat?.Nombre;
        }

        return new ProductoDto(
            producto.Id,
            producto.Codigo,
            producto.Nombre,
            producto.Descripcion,
            producto.Precio,
            producto.Stock,
            producto.Activo,
            producto.CreadoEn,
            producto.CategoriaId,
            catNombre);
    }

    /// <summary>
    /// Actualiza los atributos de un producto existente.
    /// ORIGEN DE DATOS: Modifica el registro en PostgreSQL (tabla 'productos').
    /// </summary>
    public async Task<ProductoDto?> UpdateAsync(int id, UpdateProductoDto dto)
    {
        var producto = await _context.Productos.FindAsync(id);
        if (producto == null) return null;

        if (dto.Precio < 0)
        {
            throw new InvalidOperationException("El precio del producto no puede ser negativo.");
        }

        if (dto.Stock < 0)
        {
            throw new InvalidOperationException("El stock no puede ser negativo.");
        }

        producto.Nombre = dto.Nombre.Trim();
        producto.Descripcion = dto.Descripcion?.Trim();
        producto.Precio = dto.Precio;
        producto.Stock = dto.Stock;
        producto.Activo = dto.Activo;
        if (dto.CategoriaId.HasValue)
        {
            producto.CategoriaId = dto.CategoriaId;
        }

        await _context.SaveChangesAsync();

        string? catNombre = null;
        if (producto.CategoriaId.HasValue)
        {
            var cat = await _context.Categorias.FindAsync(producto.CategoriaId.Value);
            catNombre = cat?.Nombre;
        }

        return new ProductoDto(
            producto.Id,
            producto.Codigo,
            producto.Nombre,
            producto.Descripcion,
            producto.Precio,
            producto.Stock,
            producto.Activo,
            producto.CreadoEn,
            producto.CategoriaId,
            catNombre);
    }

    /// <summary>
    /// Elimina un producto. Si el producto ya figura en ventas pasadas, aplica borrado lógico (inactivación)
    /// para preservar la integridad del historial de ventas y reportes contables.
    /// ORIGEN DE DATOS: PostgreSQL (evalúa existencia en 'detalles_venta', modifica o elimina en 'productos').
    /// </summary>
    public async Task<bool> DeleteAsync(int id)
    {
        var producto = await _context.Productos.FindAsync(id);
        if (producto == null) return false;

        var tieneVentas = await _context.DetallesVenta.AnyAsync(d => d.ProductoId == id);
        if (tieneVentas)
        {
            producto.Activo = false;
        }
        else
        {
            _context.Productos.Remove(producto);
        }

        await _context.SaveChangesAsync();
        return true;
    }
}
