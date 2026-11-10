using PetMate.Application.Authentication;
using PetMate.Application.Persistence;
using PetMate.Domain.Sessions;
using PetMate.TestDoubles;

namespace PetMate.UnitTests.Authentication;

public sealed class RefreshResilienceTests
{
    private const string Password = "contraseña-muy-segura-123";

    [Fact]
    public async Task Refresh_does_not_issue_tokens_if_the_session_is_revoked_between_validation_and_activity_update()
    {
        var stack = new AuthTestHarness();
        var first = (await stack.Register.Handle(new RegisterCommand("ana@example.com", Password, "Ana"), CancellationToken.None)).Value!;
        var racing = new RevokeBeforeActivityRepository(stack.Sessions);
        var handler = new RefreshHandler(stack.Users, racing, stack.Tokens, stack.Access, SessionPolicy.Default, stack.Clock);

        var result = await handler.Handle(new RefreshCommand(first.RefreshToken), CancellationToken.None);

        Assert.Equal(AuthError.InvalidRefreshToken, result.Error);
        Assert.Equal(1, stack.Sessions.TokenCount); // no se añadió un token nuevo a una sesión revocada
    }

    [Fact]
    public async Task Refresh_tolerates_a_clock_slightly_behind_the_last_use_instead_of_failing_with_a_domain_error()
    {
        var stack = new AuthTestHarness();
        var first = (await stack.Register.Handle(new RegisterCommand("ana@example.com", Password, "Ana"), CancellationToken.None)).Value!;
        stack.Clock.Advance(TimeSpan.FromDays(5));
        var second = (await stack.Refresh.Handle(new RefreshCommand(first.RefreshToken), CancellationToken.None)).Value!;

        stack.Clock.Advance(TimeSpan.FromMinutes(-10)); // otro nodo con el reloj atrasado
        var third = await stack.Refresh.Handle(new RefreshCommand(second.RefreshToken), CancellationToken.None);

        Assert.True(third.IsSuccess);
    }

    /// <summary>Simula una revocación concurrente (Logout o replay) justo antes de persistir la actividad.</summary>
    private sealed class RevokeBeforeActivityRepository(InMemorySessionRepository inner) : ISessionRepository
    {
        public Task AddSession(RefreshSession session, RefreshToken firstToken, CancellationToken cancellationToken)
            => inner.AddSession(session, firstToken, cancellationToken);

        public Task AddToken(RefreshToken token, DateTime sessionAbsoluteExpiresAtUtc, CancellationToken cancellationToken)
            => inner.AddToken(token, sessionAbsoluteExpiresAtUtc, cancellationToken);

        public Task<RefreshToken?> FindTokenByHash(RefreshTokenHash tokenHash, CancellationToken cancellationToken)
            => inner.FindTokenByHash(tokenHash, cancellationToken);

        public Task<bool> TryConsumeToken(Guid tokenId, DateTime consumedAtUtc, CancellationToken cancellationToken)
            => inner.TryConsumeToken(tokenId, consumedAtUtc, cancellationToken);

        public Task<RefreshSession?> FindSession(Guid sessionId, CancellationToken cancellationToken)
            => inner.FindSession(sessionId, cancellationToken);

        public async Task<bool> UpdateActivity(RefreshSession session, CancellationToken cancellationToken)
        {
            var stored = (await inner.FindSession(session.Id, cancellationToken))!;
            stored.Revoke(DateTime.UtcNow, SessionRevocationReason.Logout);
            await inner.Revoke(stored, cancellationToken);
            return await inner.UpdateActivity(session, cancellationToken);
        }

        public Task Revoke(RefreshSession session, CancellationToken cancellationToken)
            => inner.Revoke(session, cancellationToken);

        public Task<IReadOnlyList<Guid>> RevokeAllForUser(Guid userId, DateTime nowUtc, SessionRevocationReason reason, CancellationToken cancellationToken)
            => inner.RevokeAllForUser(userId, nowUtc, reason, cancellationToken);
    }
}
