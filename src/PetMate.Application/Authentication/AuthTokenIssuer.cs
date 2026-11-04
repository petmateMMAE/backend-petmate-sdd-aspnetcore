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
    SessionPolicy policy)
{
    public async Task<AuthTokensResult> StartSession(User user, DateTime nowUtc, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user);

        var session = RefreshSession.Start(user.Id, nowUtc, policy);
        var material = refreshTokens.Create();
        var token = RefreshToken.Issue(session.Id, material.Hash, nowUtc);
        await sessions.AddSession(session, token, cancellationToken).ConfigureAwait(false);

        return new AuthTokensResult(
            accessTokens.Issue(user.Id, user.Role),
            material.RawToken,
            session.IdleExpiresAtUtc,
            user.Id,
            user.Email.Value,
            user.Role);
    }
}
