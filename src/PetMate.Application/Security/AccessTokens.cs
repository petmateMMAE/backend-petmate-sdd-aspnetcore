using PetMate.Domain.Users;

namespace PetMate.Application.Security;

/// <summary>Access Token emitido. <c>ToString</c> no imprime el token para evitar fugas en logs.</summary>
public sealed record AccessToken(string Value, string Jti, DateTime ExpiresAtUtc)
{
    public override string ToString() => $"AccessToken(jti={Jti})";
}

/// <summary>Claims verificados de un Access Token válido. No contiene datos personales ni el token.</summary>
public sealed record AccessTokenClaims(Guid UserId, UserRole Role, string Jti, DateTime ExpiresAtUtc);

/// <summary>Emisión y validación de JWT de corta vida (HS256, algoritmo fijo).</summary>
public interface IAccessTokenService
{
    AccessToken Issue(Guid userId, UserRole role);

    /// <summary>Devuelve los claims si firma, algoritmo, issuer, audience, vigencia y claims obligatorios son válidos; si no, <c>null</c>.</summary>
    Task<AccessTokenClaims?> Validate(string token, CancellationToken cancellationToken);
}
