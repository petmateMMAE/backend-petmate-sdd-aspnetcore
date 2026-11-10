using PetMate.Domain.Users;

namespace PetMate.Application.Security;

/// <summary>Access Token emitido. <c>ToString</c> no imprime el token para evitar fugas en logs.</summary>
public sealed record AccessToken(string Value, string Jti, DateTime ExpiresAtUtc)
{
    public override string ToString() => $"AccessToken(jti={Jti})";
}

/// <summary>Claims verificados de un Access Token válido. No contiene datos personales ni el token.</summary>
public sealed record AccessTokenClaims(Guid UserId, UserRole Role, string Jti, DateTime ExpiresAtUtc, Guid? SessionId = null);

/// <summary>Emisión y validación de JWT de corta vida (HS256, algoritmo fijo).</summary>
public interface IAccessTokenService
{
    /// <summary>Vida de un Access Token: tiempo máximo durante el que debe recordarse la revocación de una sesión.</summary>
    TimeSpan Lifetime { get; }

    /// <summary>Emite un token. Con <paramref name="sessionId"/> incluye el claim <c>sid</c> y el token deja de valer en cuanto la sesión se revoca (pm-028 CA-04).</summary>
    AccessToken Issue(Guid userId, UserRole role, Guid? sessionId = null);

    /// <summary>Devuelve los claims si firma, algoritmo, issuer, audience, vigencia y claims obligatorios son válidos; si no, <c>null</c>.</summary>
    Task<AccessTokenClaims?> Validate(string token, CancellationToken cancellationToken);
}
