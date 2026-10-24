// Archivo: PrioridadDespacho.cs
// Define qué tan urgente es el envío de un pedido de materiales hacia la obra.
// Sirve para organizar el orden de salida de los camiones según la necesidad del cliente.

namespace Firmeza.Domain.Enums;

// Niveles de urgencia para programar el transporte y entrega de materiales o maquinaria.
public enum PrioridadDespacho
{
    // Envío estándar que puede entregarse en un plazo normal de 24 a 48 horas.
    Baja = 1,

    // Envío regular programado para entregarse durante el transcurso del mismo día.
    Normal = 2,

    // Envío importante que debe salir en los primeros viajes disponibles de la ruta.
    Alta = 3,

    // Envío inmediato porque la obra está detenida esperando este material.
    Urgente = 4
}
