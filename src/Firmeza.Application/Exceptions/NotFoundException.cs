using System;

namespace Firmeza.Application.Exceptions;

/// <summary>
/// Excepción lanzada cuando un recurso o entidad solicitada no se encuentra en el almacén de datos.
/// Se traduce automáticamente a HTTP 404 (Not Found) a través del Global Exception Handler.
/// </summary>
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message)
    {
    }

    public NotFoundException(string entityName, object key)
        : base($"La entidad '{entityName}' con identificador '{key}' no fue encontrada.")
    {
    }
}
