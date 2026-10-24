// Archivo: IDashboardService.cs
// Qué es: Plantilla para el servicio que calcula los resúmenes y estadísticas del negocio.
// Para qué sirve: Define la función que reúne las cifras de ventas, ingresos, alertas de inventario y productos más vendidos para el panel de control.

using Firmeza.Application.DTOs;

namespace Firmeza.Application.Interfaces;

// Define las operaciones para obtener las estadísticas generales del panel administrativo.
public interface IDashboardService
{
    // Calcula y devuelve todos los totales del día, ingresos acumulados, productos por agotarse y los más vendidos.
    Task<DashboardSummaryDto> GetSummaryAsync();
}
