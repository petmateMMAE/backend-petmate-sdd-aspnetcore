namespace PetMate.Application.Authentication;

/// <summary>Errores de negocio de autenticación. La API los traduce a HTTP con mensajes genéricos en español.</summary>
public enum AuthError
{
    None = 0,

    /// <summary>El correo ya existe (Register).</summary>
    EmailAlreadyRegistered,

    /// <summary>Correo inexistente o contraseña incorrecta: indistinguibles a propósito (Login).</summary>
    InvalidCredentials,

    /// <summary>Refresh token desconocido, expirado, consumido o de una sesión revocada (Refresh).</summary>
    InvalidRefreshToken,

    /// <summary>Intento de operar una sesión que no pertenece al usuario autenticado (Logout).</summary>
    Forbidden,

    /// <summary>Cuota por correo o usuario agotada, bloqueo temporal o demasiado trabajo de hashing en cola (pm-034): 429 con <c>Retry-After</c>.</summary>
    TooManyRequests,
}

/// <summary>Valor vacío para resultados sin datos (<c>data: null</c>).</summary>
public sealed record Unit
{
    public static readonly Unit Value = new();
}

/// <summary>Resultado explícito de un caso de uso de autenticación (sin excepciones para flujos esperados).</summary>
public sealed class AuthResult<T>
{
    internal AuthResult(T? value, AuthError error, TimeSpan? retryAfter = null)
    {
        Value = value;
        Error = error;
        RetryAfter = retryAfter;
    }

    /// <summary>Espera sugerida al cliente cuando <see cref="Error"/> es <see cref="AuthError.TooManyRequests"/>.</summary>
    public TimeSpan? RetryAfter { get; }

    public bool IsSuccess => Error == AuthError.None;

    public T? Value { get; }

    public AuthError Error { get; }
}

/// <summary>Fábricas de <see cref="AuthResult{T}"/>.</summary>
public static class AuthResult
{
    public static AuthResult<T> Success<T>(T value) => new(value, AuthError.None);

    public static AuthResult<T> Throttled<T>(TimeSpan retryAfter)
        => new(default, AuthError.TooManyRequests, retryAfter < TimeSpan.FromSeconds(1) ? TimeSpan.FromSeconds(1) : retryAfter);

    public static AuthResult<T> Failure<T>(AuthError error)
        => error == AuthError.None ? throw new ArgumentException("Un fallo requiere un error.", nameof(error)) : new(default, error);
}
