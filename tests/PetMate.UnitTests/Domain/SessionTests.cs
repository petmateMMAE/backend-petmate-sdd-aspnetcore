using PetMate.Domain.Common;
using PetMate.Domain.Sessions;

namespace PetMate.UnitTests.Domain;

public sealed class SessionTests
{
    private static readonly DateTime Now = new(2026, 11, 3, 12, 0, 0, DateTimeKind.Utc);
    private static readonly string Sha256Hex = new('a', 64);

    [Fact]
    public void Default_policy_is_30_days_idle_and_90_days_absolute()
    {
        Assert.Equal(TimeSpan.FromDays(30), SessionPolicy.Default.IdleTimeout);
        Assert.Equal(TimeSpan.FromDays(90), SessionPolicy.Default.AbsoluteLifetime);
    }

    [Theory]
    [InlineData(0, 90)]
    [InlineData(30, 0)]
    [InlineData(91, 90)]
    public void Policy_rejects_invalid_durations(int idleDays, int absoluteDays)
        => Assert.Throws<DomainException>(() => new SessionPolicy(TimeSpan.FromDays(idleDays), TimeSpan.FromDays(absoluteDays)));

    [Fact]
    public void Start_sets_idle_and_absolute_expiry_from_policy()
    {
        var session = RefreshSession.Start(Guid.NewGuid(), Now, SessionPolicy.Default);

        Assert.Equal(Now.AddDays(30), session.IdleExpiresAtUtc);
        Assert.Equal(Now.AddDays(90), session.AbsoluteExpiresAtUtc);
        Assert.True(session.IsActive(Now));
        Assert.False(session.IsRevoked);
    }

    [Fact]
    public void Session_expires_after_idle_timeout()
    {
        var session = RefreshSession.Start(Guid.NewGuid(), Now, SessionPolicy.Default);

        Assert.True(session.IsActive(Now.AddDays(30).AddTicks(-1)));
        Assert.False(session.IsActive(Now.AddDays(30)));
    }

    [Fact]
    public void RegisterUse_renews_idle_window_but_never_beyond_absolute_limit()
    {
        var session = RefreshSession.Start(Guid.NewGuid(), Now, SessionPolicy.Default);

        session.RegisterUse(Now.AddDays(25), SessionPolicy.Default);
        Assert.Equal(Now.AddDays(55), session.IdleExpiresAtUtc);

        session.RegisterUse(Now.AddDays(50), SessionPolicy.Default);
        session.RegisterUse(Now.AddDays(75), SessionPolicy.Default);
        Assert.Equal(Now.AddDays(90), session.IdleExpiresAtUtc);
        Assert.False(session.IsActive(Now.AddDays(90)));
    }

    [Fact]
    public void RegisterUse_fails_for_expired_or_revoked_session()
    {
        var session = RefreshSession.Start(Guid.NewGuid(), Now, SessionPolicy.Default);
        Assert.Throws<DomainException>(() => session.RegisterUse(Now.AddDays(31), SessionPolicy.Default));

        var revoked = RefreshSession.Start(Guid.NewGuid(), Now, SessionPolicy.Default);
        revoked.Revoke(Now, SessionRevocationReason.Logout);
        Assert.Throws<DomainException>(() => revoked.RegisterUse(Now, SessionPolicy.Default));
    }

    [Fact]
    public void Revoke_is_idempotent_and_keeps_first_reason()
    {
        var session = RefreshSession.Start(Guid.NewGuid(), Now, SessionPolicy.Default);

        session.Revoke(Now.AddMinutes(1), SessionRevocationReason.ReplayDetected);
        session.Revoke(Now.AddMinutes(2), SessionRevocationReason.Logout);

        Assert.Equal(Now.AddMinutes(1), session.RevokedAtUtc);
        Assert.Equal(SessionRevocationReason.ReplayDetected, session.RevocationReason);
        Assert.False(session.IsActive(Now.AddMinutes(3)));
    }

    [Fact]
    public void Restore_rejects_inconsistent_revocation_state()
        => Assert.Throws<DomainException>(() => RefreshSession.Restore(
            Guid.NewGuid(), Guid.NewGuid(), Now, Now, Now.AddDays(1), Now.AddDays(2), Now, null));

    [Fact]
    public void Session_requires_utc_and_non_empty_user()
    {
        Assert.Throws<DomainException>(() => RefreshSession.Start(Guid.Empty, Now, SessionPolicy.Default));
        Assert.Throws<DomainException>(() => RefreshSession.Start(
            Guid.NewGuid(), DateTime.SpecifyKind(Now, DateTimeKind.Local), SessionPolicy.Default));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("abc")]
    [InlineData("zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzz")]
    public void TokenHash_rejects_values_that_are_not_sha256_hex(string? value)
        => Assert.Throws<DomainException>(() => RefreshTokenHash.Create(value));

    [Fact]
    public void TokenHash_normalizes_to_lowercase_and_hides_value()
    {
        var hash = RefreshTokenHash.Create(new string('A', 64));

        Assert.Equal(new string('a', 64), hash.Value);
        Assert.DoesNotContain("aaaa", hash.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void Token_is_consumed_only_once()
    {
        var token = RefreshToken.Issue(Guid.NewGuid(), RefreshTokenHash.Create(Sha256Hex), Now);
        Assert.False(token.IsConsumed);

        token.Consume(Now.AddMinutes(1));

        Assert.True(token.IsConsumed);
        Assert.Equal(Now.AddMinutes(1), token.ConsumedAtUtc);
        Assert.Throws<DomainException>(() => token.Consume(Now.AddMinutes(2)));
    }

    [Fact]
    public void Token_requires_session_and_utc()
    {
        var hash = RefreshTokenHash.Create(Sha256Hex);

        Assert.Throws<DomainException>(() => RefreshToken.Issue(Guid.Empty, hash, Now));
        Assert.Throws<DomainException>(() => RefreshToken.Issue(Guid.NewGuid(), hash, DateTime.SpecifyKind(Now, DateTimeKind.Unspecified)));
    }
}
