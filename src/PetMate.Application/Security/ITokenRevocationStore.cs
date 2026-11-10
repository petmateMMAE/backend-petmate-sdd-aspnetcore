namespace PetMate.Application.Security;

/// <summary>
/// Lista negra de Access Tokens revocados (por <c>jti</c>), respaldada exclusivamente por Redis de autenticación.
/// Si no se puede comprobar o escribir, lanza <see cref="AuthDependencyUnavailableException"/> (falla cerrada).
/// </summary>
public interface ITokenRevocationStore
{
    /// <summary>Revoca el <c>jti</c> hasta <paramref name="expiresAtUtc"/> (TTL = vida restante del JWT). No hace nada si ya expiró.</summary>
    Task Revoke(string jti, DateTime expiresAtUtc, CancellationToken cancellationToken);

    Task<bool> IsRevoked(string jti, CancellationToken cancellationToken);

    /// <summary>
    /// Revoca todos los Access Tokens de una sesión (claim <c>sid</c>) hasta <paramref name="untilUtc"/>, normalmente ahora más la vida
    /// de un token (pm-028 CA-04, hallazgo H7). Cierra la ventana en la que un token vivía hasta 15 minutos tras revocar la sesión.
    /// </summary>
    Task RevokeSession(Guid sessionId, DateTime untilUtc, CancellationToken cancellationToken);

    Task<bool> IsSessionRevoked(Guid sessionId, CancellationToken cancellationToken);
}
