namespace PetMate.Application.Security;

/// <summary>
/// Una dependencia de seguridad (Redis de autenticación o la capacidad de hashing) no puede atender la solicitud.
/// La API responde 503 y nunca concede acceso por omisión. El mensaje es genérico.
/// </summary>
public sealed class AuthDependencyUnavailableException : Exception
{
    public AuthDependencyUnavailableException()
        : base("Un servicio de seguridad no está disponible.")
    {
    }

    public AuthDependencyUnavailableException(Exception innerException)
        : base("Un servicio de seguridad no está disponible.", innerException)
    {
    }
}
