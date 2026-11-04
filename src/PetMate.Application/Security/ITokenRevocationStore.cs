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
}
