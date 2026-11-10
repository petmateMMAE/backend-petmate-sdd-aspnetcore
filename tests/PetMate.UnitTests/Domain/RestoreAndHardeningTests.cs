using PetMate.Domain.Common;
using PetMate.Domain.Sessions;
using PetMate.Domain.Users;

namespace PetMate.UnitTests.Domain;

public sealed class RestoreAndHardeningTests
{
    private static readonly DateTime Now = new(2026, 11, 3, 12, 0, 0, DateTimeKind.Utc);
    private const string ValidHash = "$argon2id$v=19$m=65536,t=3,p=1$c2FsdHNhbHRzYWx0$aGFzaGhhc2hoYXNoaGFzaGhhc2hoYXNoaGFzaGhhc2g";
    private static readonly string Sha256Hex = new('b', 64);

    [Fact]
    public void User_restore_accepts_the_user_role_from_persistence()
    {
        var user = User.Restore(Guid.NewGuid(), Email.Create("neg@example.com"), PasswordHash.Create(ValidHash), UserRole.User, Now, Now);

        Assert.Equal(UserRole.User, user.Role);
    }

    [Fact]
    public void User_restore_rejects_updated_before_created()
        => Assert.Throws<DomainException>(() => User.Restore(
            Guid.NewGuid(), Email.Create("a@b.co"), PasswordHash.Create(ValidHash), UserRole.User, Now, Now.AddSeconds(-1)));

    [Fact]
    public void Session_restore_round_trips_valid_state()
    {
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var session = RefreshSession.Restore(id, userId, Now, Now, Now.AddDays(30), Now.AddDays(90), Now.AddDays(1), SessionRevocationReason.Logout);

        Assert.Equal(id, session.Id);
        Assert.True(session.IsRevoked);
        Assert.Equal(SessionRevocationReason.Logout, session.RevocationReason);
    }

    [Theory]
    [InlineData(-1, 30, 90)]
    [InlineData(0, 91, 90)]
    [InlineData(0, 0, 0)]
    public void Session_restore_rejects_incoherent_dates(int lastUsedOffsetDays, int idleDays, int absoluteDays)
        => Assert.Throws<DomainException>(() => RefreshSession.Restore(
            Guid.NewGuid(), Guid.NewGuid(), Now, Now.AddDays(lastUsedOffsetDays), Now.AddDays(idleDays), Now.AddDays(absoluteDays), null, null));

    [Fact]
    public void Token_restore_rejects_consumed_before_issued_and_non_utc()
    {
        var hash = RefreshTokenHash.Create(Sha256Hex);

        Assert.Throws<DomainException>(() => RefreshToken.Restore(Guid.NewGuid(), Guid.NewGuid(), hash, Now, Now.AddSeconds(-1)));
        Assert.Throws<DomainException>(() => RefreshToken.Restore(
            Guid.NewGuid(), Guid.NewGuid(), hash, Now, DateTime.SpecifyKind(Now, DateTimeKind.Local)));
        Assert.True(RefreshToken.Restore(Guid.NewGuid(), Guid.NewGuid(), hash, Now, Now.AddMinutes(1)).IsConsumed);
    }

    [Fact]
    public void Session_is_inactive_exactly_at_idle_and_absolute_limits()
    {
        var session = RefreshSession.Restore(Guid.NewGuid(), Guid.NewGuid(), Now, Now, Now.AddDays(10), Now.AddDays(20), null, null);

        Assert.True(session.IsActive(Now.AddDays(10).AddTicks(-1)));
        Assert.False(session.IsActive(Now.AddDays(10)));
        Assert.False(session.IsActive(Now.AddDays(20)));
    }

    [Fact]
    public void RegisterUse_rejects_a_clock_earlier_than_the_last_use()
    {
        var session = RefreshSession.Start(Guid.NewGuid(), Now, SessionPolicy.Default);
        session.RegisterUse(Now.AddDays(5), SessionPolicy.Default);

        Assert.Throws<DomainException>(() => session.RegisterUse(Now.AddDays(4), SessionPolicy.Default));
    }

    [Fact]
    public void Revoke_rejects_undefined_reason_and_non_utc_dates()
    {
        var session = RefreshSession.Start(Guid.NewGuid(), Now, SessionPolicy.Default);

        Assert.Throws<DomainException>(() => session.Revoke(Now, (SessionRevocationReason)99));
        Assert.Throws<DomainException>(() => session.Revoke(DateTime.SpecifyKind(Now, DateTimeKind.Local), SessionRevocationReason.Logout));
    }

    [Fact]
    public void Consume_rejects_non_utc_dates()
    {
        var token = RefreshToken.Issue(Guid.NewGuid(), RefreshTokenHash.Create(Sha256Hex), Now);

        Assert.Throws<DomainException>(() => token.Consume(DateTime.SpecifyKind(Now, DateTimeKind.Unspecified)));
    }

    [Fact]
    public void SessionPolicy_rejects_absolute_lifetime_above_the_cap()
        => Assert.Throws<DomainException>(() => new SessionPolicy(TimeSpan.FromDays(30), SessionPolicy.MaxAbsoluteLifetime + TimeSpan.FromDays(1)));

    [Fact]
    public void Domain_has_no_admin_concept_anywhere()
    {
        var names = typeof(User).Assembly.GetTypes().SelectMany(type => new[] { type.Name }.Concat(
                type.IsEnum ? Enum.GetNames(type) : type.GetMembers().Select(member => member.Name)))
            .ToArray();

        Assert.DoesNotContain(names, name => name.Contains("Admin", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Domain_assembly_references_only_the_base_class_library()
    {
        var references = typeof(User).Assembly.GetReferencedAssemblies().Select(reference => reference.Name!).ToArray();

        Assert.DoesNotContain(references, name => name.StartsWith("PetMate.", StringComparison.Ordinal)
            || name.StartsWith("MongoDB", StringComparison.Ordinal)
            || name.StartsWith("Microsoft.AspNetCore", StringComparison.Ordinal)
            || name.StartsWith("StackExchange", StringComparison.Ordinal));
    }

    [Fact]
    public void Public_surface_of_Domain_exposes_no_Restore_factories()
    {
        var publicRestore = typeof(User).Assembly.GetExportedTypes()
            .SelectMany(type => type.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static))
            .Where(method => method.Name == "Restore");

        Assert.Empty(publicRestore);
    }
}
