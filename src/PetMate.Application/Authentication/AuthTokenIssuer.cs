using PetMate.Application.Persistence;
using PetMate.Application.Security;
using PetMate.Domain.Sessions;
using PetMate.Domain.Users;

namespace PetMate.Application.Authentication;

/// <summary>Crea una sesión nueva (primer refresh token incluido) y emite el Access Token. Compartido por Register y Login.</summary>
public sealed class AuthTokenIssuer(
    ISessionRepository sessions,
    IRefreshTokenFactory refreshTokens,
    IAccessTokenService accessTokens,
    SessionPolicy policy,
    PetMate.Application.Businesses.IBusinessRepository? businesses = null)
{
    /// <summary>Tipo del negocio asociado a la cuenta o <c>null</c> (pm-087, pm-107#CA-04). Lo calcula el servidor.</summary>
    public async Task<string?> FindBusinessType(Guid userId, CancellationToken cancellationToken)
        => businesses is null ? null : (await businesses.FindByOwner(userId, cancellationToken).ConfigureAwait(false))?.Type.ToString();

    public async Task<AuthTokensResult> StartSession(User user, DateTime nowUtc, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user);

        var session = RefreshSession.Start(user.Id, nowUtc, policy);
        var material = refreshTokens.Create();
        var token = RefreshToken.Issue(session.Id, material.Hash, nowUtc);
        await sessions.AddSession(session, token, cancellationToken).ConfigureAwait(false);

        return new AuthTokensResult(
            accessTokens.Issue(user.Id, user.Role, session.Id),
            material.RawToken,
            session.IdleExpiresAtUtc,
            user.Id,
            user.Email.Value,
            user.Role,
            user.DisplayName,
            BusinessType: await FindBusinessType(user.Id, cancellationToken).ConfigureAwait(false));
    }
}
