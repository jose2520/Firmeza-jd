// Archivo: EstadoAlquiler.cs
// Define los posibles estados por los que pasa un alquiler de maquinaria o vehículos.
// Sirve para controlar si un equipo sigue en manos del cliente, ya fue devuelto o se canceló.

namespace Firmeza.Domain.Enums;

// Lista de estados que puede tener un contrato de alquiler desde su creación hasta su cierre.
public enum EstadoAlquiler
{
    // El contrato está vigente y la maquinaria se encuentra trabajando en la obra del cliente.
    Activo = 1,

    // El alquiler terminó y el equipo ya fue devuelto y revisado en el patio de la empresa.
    Finalizado = 2,

    // El contrato se anuló antes de empezar o por incumplimiento de alguna de las partes.
    Cancelado = 3
}
