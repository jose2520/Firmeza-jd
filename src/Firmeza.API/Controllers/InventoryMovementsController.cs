// Controlador encargado de registrar y consultar el historial de movimientos de bodega.
// Permite revisar todas las entradas, salidas y ajustes de inventario por producto,
// actualizando las cantidades disponibles en tiempo real.

using System.Security.Claims;
using AutoMapper;
using Firmeza.Application.DTOs;
using Firmeza.Domain.Entities;
using Firmeza.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class InventoryMovementsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    // Recibe la conexión a la base de datos y el convertidor de datos de movimientos.
    public InventoryMovementsController(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    // Devuelve el historial completo de todos los movimientos de inventario ordenados del más reciente al más antiguo.
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MovimientoInventarioDto>>> GetAll()
    {
        var movimientos = await _context.MovimientosInventario
            .AsNoTracking()
            .Include(m => m.Producto)
            .OrderByDescending(m => m.Fecha)
            .ToListAsync();

        return Ok(_mapper.Map<IEnumerable<MovimientoInventarioDto>>(movimientos));
    }

    // Devuelve el historial de entradas, salidas y ajustes de un producto específico.
    [HttpGet("product/{productId:int}")]
    public async Task<ActionResult<IEnumerable<MovimientoInventarioDto>>> GetByProductId(int productId)
    {
        var movimientos = await _context.MovimientosInventario
            .AsNoTracking()
            .Include(m => m.Producto)
            .Where(m => m.ProductoId == productId)
            .OrderByDescending(m => m.Fecha)
            .ToListAsync();

        return Ok(_mapper.Map<IEnumerable<MovimientoInventarioDto>>(movimientos));
    }

    // Registra manualmente una entrada, salida o ajuste de inventario y actualiza las existencias del producto.
    [HttpPost]
    public async Task<ActionResult<MovimientoInventarioDto>> Create([FromBody] CreateMovimientoInventarioDto dto)
    {
        if (dto.Cantidad <= 0)
            return BadRequest(new { message = "La cantidad del movimiento debe ser estrictamente mayor que cero." });

        var tipo = dto.TipoMovimiento.Trim();
        if (tipo != "Entrada" && tipo != "Salida" && tipo != "Ajuste")
            return BadRequest(new { message = "El tipo de movimiento debe ser 'Entrada', 'Salida' o 'Ajuste'." });

        var producto = await _context.Productos.FindAsync(dto.ProductoId);
        if (producto == null)
            return BadRequest(new { message = $"El producto con ID {dto.ProductoId} no existe." });

        int stockPrevio = producto.Stock;
        int stockPosterior = stockPrevio;

        if (tipo == "Entrada")
        {
            stockPosterior = stockPrevio + dto.Cantidad;
        }
        else if (tipo == "Salida")
        {
            if (stockPrevio < dto.Cantidad)
                return BadRequest(new { message = $"Stock insuficiente para realizar salida. Stock actual: {stockPrevio}, solicitado: {dto.Cantidad}." });

            stockPosterior = stockPrevio - dto.Cantidad;
        }
        else if (tipo == "Ajuste")
        {
            // En un ajuste, la cantidad indicada pasa a ser la nueva cantidad total disponible
            stockPosterior = dto.Cantidad;
        }

        producto.Stock = stockPosterior;

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var movimiento = new MovimientoInventario
        {
            ProductoId = dto.ProductoId,
            UsuarioId = userId,
            TipoMovimiento = tipo,
            Cantidad = dto.Cantidad,
            StockPrevio = stockPrevio,
            StockPosterior = stockPosterior,
            Referencia = dto.Referencia,
            Motivo = dto.Motivo,
            Fecha = DateTime.UtcNow
        };

        _context.MovimientosInventario.Add(movimiento);
        await _context.SaveChangesAsync();

        await _context.Entry(movimiento).Reference(m => m.Producto).LoadAsync();

        return CreatedAtAction(nameof(GetAll), new { id = movimiento.Id }, _mapper.Map<MovimientoInventarioDto>(movimiento));
    }
}
