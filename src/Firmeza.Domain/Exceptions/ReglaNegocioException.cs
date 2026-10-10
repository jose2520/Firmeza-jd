namespace Firmeza.Domain.Exceptions;

/// <summary>
/// Excepción lanzada cuando una operación viola una regla de negocio fundamental del dominio comercial.
/// </summary>
public class ReglaNegocioException : DomainException
{
    public string CodigoRegla { get; }

    public ReglaNegocioException(string codigoRegla, string message) : base(message)
    {
        CodigoRegla = codigoRegla;
    }
}
