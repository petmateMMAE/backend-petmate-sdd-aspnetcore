using PetMate.Application.Mediator;
using PetMate.Application.Persistence;
using PetMate.Application.Security;
using PetMate.Domain.Sessions;

namespace PetMate.Application.Authentication;

/// <summary>
/// Rotación de refresh tokens. El consumo es atómico en Atlas (<c>TryConsumeToken</c>): ante dos solicitudes
/// concurrentes con el mismo token solo una lo consume; la otra se trata como replay y revoca la sesión.
/// Un token ya consumido presentado de nuevo (replay) revoca toda la sesión/familia.
/// </summary>
public sealed class RefreshHandler(
    IUserRepository users,
    ISessionRepository sessions,
    IRefreshTokenFactory refreshTokens,
    IAccessTokenService accessTokens,
    SessionPolicy policy,
    TimeProvider timeProvider,
    IAuthAttemptThrottle? throttle = null) : IRequestHandler<RefreshCommand, AuthResult<AuthTokensResult>>
{
    private static readonly AuthResult<AuthTokensResult> Invalid = AuthResult.Failure<AuthTokensResult>(AuthError.InvalidRefreshToken);

    public async Task<AuthResult<AuthTokensResult>> Handle(RefreshCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!refreshTokens.TryHash(request.RefreshToken, out var hash) || hash is null)
        {
            return Invalid;
        }

        var token = await sessions.FindTokenByHash(hash, cancellationToken).ConfigureAwait(false);
        if (token is null)
        {
            return Invalid;
        }

        var session = await sessions.FindSession(token.SessionId, cancellationToken).ConfigureAwait(false);
        var now = timeProvider.GetUtcNow().UtcDateTime;
        if (session is null || !session.IsActive(now))
        {
            // Sesión revocada o vencida (inactividad o límite absoluto): no se puede renovar.
            return Invalid;
        }

        // Cuota por usuario (pm-034), independiente de la IP: se aplica antes de consumir el token para no gastarlo en vano.
        if (throttle is not null)
        {
            var subject = session.UserId.ToString("N");
            if (await throttle.GetRemainingLock(AuthThrottleRules.RefreshScope, subject, cancellationToken).ConfigureAwait(false) is { } remaining)
            {
                return AuthResult.Throttled<AuthTokensResult>(remaining);
            }

            if (await throttle.RecordAttempt(AuthThrottleRules.RefreshScope, subject, AuthThrottleRules.Refresh, cancellationToken).ConfigureAwait(false) is { } lockedFor)
            {
                return AuthResult.Throttled<AuthTokensResult>(lockedFor);
            }
        }

        if (token.IsConsumed || !await sessions.TryConsumeToken(token.Id, now, cancellationToken).ConfigureAwait(false))
        {
            await RevokeForReplay(session, now, cancellationToken).ConfigureAwait(false);
            return Invalid;
        }

        var user = await users.FindById(session.UserId, cancellationToken).ConfigureAwait(false);
        if (user is null)
        {
            session.Revoke(now, SessionRevocationReason.Logout);
            await sessions.Revoke(session, cancellationToken).ConfigureAwait(false);
            return Invalid;
        }

        // Tolera desfases de reloj entre nodos: nunca se retrocede el último uso.
        session.RegisterUse(now < session.LastUsedAtUtc ? session.LastUsedAtUtc : now, policy);
        if (!await sessions.UpdateActivity(session, cancellationToken).ConfigureAwait(false))
        {
            // La sesión fue revocada entre la comprobación y la actualización (Logout o replay concurrente): no se emite nada.
            return Invalid;
        }

        var material = refreshTokens.Create();
        await sessions.AddToken(RefreshToken.Issue(session.Id, material.Hash, now), session.AbsoluteExpiresAtUtc, cancellationToken)
            .ConfigureAwait(false);

        return AuthResult.Success(new AuthTokensResult(
            accessTokens.Issue(user.Id, user.Role),
            material.RawToken,
            session.IdleExpiresAtUtc,
            user.Id,
            user.Email.Value,
            user.Role));
    }

    private async Task RevokeForReplay(RefreshSession session, DateTime now, CancellationToken cancellationToken)
    {
        session.Revoke(now, SessionRevocationReason.ReplayDetected);
        await sessions.Revoke(session, cancellationToken).ConfigureAwait(false);
    }
}
