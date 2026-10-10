using System;

namespace Firmeza.Application.Exceptions;

/// <summary>
/// Excepción lanzada cuando el usuario autenticado carece de los permisos o roles necesarios
/// para ejecutar el caso de uso solicitado.
/// Se traduce automáticamente a HTTP 403 (Forbidden).
/// </summary>
public class ForbiddenAccessException : Exception
{
    public ForbiddenAccessException()
        : base("No tiene los permisos requeridos para acceder o modificar este recurso.")
    {
    }

    public ForbiddenAccessException(string message)
        : base(message)
    {
    }
}
