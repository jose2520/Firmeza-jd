using Firmeza.Application.DTOs;

namespace Firmeza.Application.Interfaces;

public interface IVentaService
{
    Task<IEnumerable<VentaDto>> GetAllAsync();
    Task<VentaDto?> GetByIdAsync(int id);
    Task<VentaDto> CreateAsync(CreateVentaDto dto);
    Task<bool> CancelAsync(int id);
}
