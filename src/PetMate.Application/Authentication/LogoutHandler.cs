using PetMate.Application.Mediator;
using PetMate.Application.Persistence;
using PetMate.Application.Security;
using PetMate.Domain.Sessions;

namespace PetMate.Application.Authentication;

/// <summary>
/// Cierra la sesión del usuario autenticado: revoca la sesión en Atlas y registra el <c>jti</c> vigente en Redis Auth
/// con TTL igual a su vida restante. Un refresh token desconocido o de otro usuario produce <see cref="AuthError.Forbidden"/>
/// sin revocar nada; una sesión propia ya revocada es idempotente (igualmente se registra el jti).
/// </summary>
public sealed class LogoutHandler(
    ISessionRepository sessions,
    ITokenRevocationStore revocations,
    IRefreshTokenFactory refreshTokens,
    TimeProvider timeProvider) : IRequestHandler<LogoutCommand, AuthResult<Unit>>
{
    public async Task<AuthResult<Unit>> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var forbidden = AuthResult.Failure<Unit>(AuthError.Forbidden);

        if (!refreshTokens.TryHash(request.RefreshToken, out var hash) || hash is null)
        {
            return forbidden;
        }

        var token = await sessions.FindTokenByHash(hash, cancellationToken).ConfigureAwait(false);
        var session = token is null ? null : await sessions.FindSession(token.SessionId, cancellationToken).ConfigureAwait(false);
        if (session is null || session.UserId != request.UserId)
        {
            // Mismo resultado para token inexistente y ajeno: no revela qué sesiones existen.
            return forbidden;
        }

        if (!session.IsRevoked)
        {
            session.Revoke(timeProvider.GetUtcNow().UtcDateTime, SessionRevocationReason.Logout);
            await sessions.Revoke(session, cancellationToken).ConfigureAwait(false);
        }

        // Si Redis Auth no responde se lanza AuthDependencyUnavailableException (503); la sesión ya quedó revocada y el reintento es idempotente.
        await revocations.Revoke(request.Jti, request.AccessTokenExpiresAtUtc, cancellationToken).ConfigureAwait(false);
        return AuthResult.Success(Unit.Value);
    }
}
