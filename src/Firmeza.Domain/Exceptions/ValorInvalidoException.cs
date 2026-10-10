namespace Firmeza.Domain.Exceptions;

/// <summary>
/// Excepción lanzada cuando un Objeto de Valor (Value Object) recibe datos que violan sus invariantes.
/// </summary>
public class ValorInvalidoException : DomainException
{
    public string NombreCampo { get; }

    public ValorInvalidoException(string nombreCampo, string message) 
        : base($"El valor proporcionado para '{nombreCampo}' es inválido: {message}")
    {
        NombreCampo = nombreCampo;
    }
}
