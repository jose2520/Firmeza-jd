// Servicio que gestiona la consulta, creación, edición y borrado de productos del inventario.
// Trabaja con la tabla "productos" y consulta las tablas "categorias", "proveedores", "detalles_venta" y "movimientos_inventario".
// Si un producto ya tiene ventas o movimientos registrados, solo lo desactiva en lugar de borrarlo para conservar el historial.
using Firmeza.Application.DTOs;
using Firmeza.Application.Interfaces;
using Firmeza.Domain.Entities;
using Firmeza.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.Infrastructure.Services;

public class ProductoService : IProductoService
{
    private readonly ApplicationDbContext _context;

    // Inicializa el servicio de productos con la conexión a la base de datos.
    public ProductoService(ApplicationDbContext context)
    {
        _context = context;
    }

    // Obtiene la lista de todos los productos de la tabla "productos" incluyendo el nombre de su categoría y proveedor.
    public async Task<IEnumerable<ProductoDto>> GetAllAsync()
    {
        return await _context.Productos
            .AsNoTracking()
            .Include(p => p.Categoria)
            .Include(p => p.Proveedor)
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
                p.Categoria != null ? p.Categoria.Nombre : null,
                p.ProveedorId,
                p.Proveedor != null ? p.Proveedor.RazonSocial : null))
            .ToListAsync();
    }

    // Busca y devuelve un producto específico por su ID junto con su categoría y proveedor.
    public async Task<ProductoDto?> GetByIdAsync(int id)
    {
        var p = await _context.Productos
            .AsNoTracking()
            .Include(p => p.Categoria)
            .Include(p => p.Proveedor)
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
            p.Categoria != null ? p.Categoria.Nombre : null,
            p.ProveedorId,
            p.Proveedor != null ? p.Proveedor.RazonSocial : null);
    }

    // Crea un nuevo producto en la tabla "productos" validando que el código no exista y que el precio y stock no sean negativos.
    public async Task<ProductoDto> CreateAsync(CreateProductoDto dto)
    {
        var codigoNormalizado = dto.Codigo.Trim().ToUpperInvariant();
        var codigoExistente = await _context.Productos
            .AnyAsync(p => p.Codigo.ToUpper() == codigoNormalizado);

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
            Codigo = codigoNormalizado,
            Nombre = dto.Nombre.Trim(),
            Descripcion = dto.Descripcion?.Trim(),
            Precio = dto.Precio,
            Stock = dto.Stock,
            CategoriaId = dto.CategoriaId,
            ProveedorId = dto.ProveedorId,
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

        string? provNombre = null;
        if (producto.ProveedorId.HasValue)
        {
            var prov = await _context.Proveedores.FindAsync(producto.ProveedorId.Value);
            provNombre = prov?.RazonSocial;
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
            catNombre,
            producto.ProveedorId,
            provNombre);
    }

    // Actualiza el nombre, descripción, precio, stock, categoría y proveedor de un producto existente.
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
        if (dto.ProveedorId.HasValue)
        {
            producto.ProveedorId = dto.ProveedorId;
        }

        await _context.SaveChangesAsync();

        string? catNombre = null;
        if (producto.CategoriaId.HasValue)
        {
            var cat = await _context.Categorias.FindAsync(producto.CategoriaId.Value);
            catNombre = cat?.Nombre;
        }

        string? provNombre = null;
        if (producto.ProveedorId.HasValue)
        {
            var prov = await _context.Proveedores.FindAsync(producto.ProveedorId.Value);
            provNombre = prov?.RazonSocial;
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
            catNombre,
            producto.ProveedorId,
            provNombre);
    }

    // Borra un producto si nunca se ha usado, o solo lo desactiva si ya aparece en ventas o movimientos de inventario.
    public async Task<bool> DeleteAsync(int id)
    {
        var producto = await _context.Productos.FindAsync(id);
        if (producto == null) return false;

        var tieneHistorial = await _context.DetallesVenta.AnyAsync(d => d.ProductoId == id)
                          || await _context.MovimientosInventario.AnyAsync(m => m.ProductoId == id);
        if (tieneHistorial)
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
