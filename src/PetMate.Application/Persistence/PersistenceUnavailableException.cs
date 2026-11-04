namespace PetMate.Application.Persistence;

/// <summary>
/// La persistencia no pudo completar la operación. No incluye URIs, nombres de host ni mensajes del proveedor;
/// la excepción original se conserva solo como causa interna para el log del servidor.
/// </summary>
public sealed class PersistenceUnavailableException : Exception
{
    public PersistenceUnavailableException()
        : base("El almacenamiento de datos no está disponible.")
    {
    }

    public PersistenceUnavailableException(Exception innerException)
        : base("El almacenamiento de datos no está disponible.", innerException)
    {
    }
}
