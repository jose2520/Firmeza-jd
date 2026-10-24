// Archivo: TipoVehiculo.cs
// Define las categorías de camiones y maquinaria pesada que maneja la empresa.
// Sirve para clasificar la flota según el tipo de trabajo o transporte que realiza.

namespace Firmeza.Domain.Enums;

// Categorías de vehículos de carga y maquinaria de construcción disponibles en Firmeza.
public enum TipoVehiculo
{
    // Camión con carrocería de estacas ideal para transportar bultos, tubería y varillas.
    CamionEstacas = 1,

    // Volqueta de un solo eje trasero para mover arena, triturado o escombros en volumen medio.
    VolquetaSencilla = 2,

    // Volqueta pesada de doble eje trasero para grandes cargas de agregados y tierra.
    VolquetaDobletroque = 3,

    // Camión mezclador o trompo para transportar concreto fresco hasta la obra.
    MezcladoraConcreto = 4,

    // Vehículo liviano para entregas rápidas de herramientas y pedidos pequeños.
    CamionetaReparto = 5,

    // Máquina compacta tipo Bobcat para cargar y mover materiales en espacios reducidos.
    Minicargador = 6,

    // Máquina excavadora y cargadora para abrir zanjas y mover tierra en obra.
    Retroexcavadora = 7,

    // Máquina pesada con cilindro metálico para aplanar y compactar terrenos o asfalto.
    RodilloCompactador = 8,

    // Equipo portátil con reflectores de alta potencia para trabajos nocturnos en obra.
    TorreIluminacion = 9,

    // Máquina con disco diamantado para hacer cortes rectos en pisos de concreto o asfalto.
    CortadoraPavimento = 10
}
