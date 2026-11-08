using PetMate.Application.Users;
using PetMate.Domain.Users;
using PetMate.TestDoubles;

namespace PetMate.UnitTests.Users;

/// <summary>Casos de uso del perfil propio (pm-037, pm-038) con repositorio en memoria y reloj controlado.</summary>
public sealed class ProfileHandlersTests
{
    private const string ValidHash = "$argon2id$v=19$m=65536,t=3,p=1$c2FsdHNhbHRzYWx0$aGFzaGhhc2hoYXNoaGFzaGhhc2hoYXNoaGFzaGhhc2g";
    private readonly ManualClock clock = new(new DateTimeOffset(2026, 11, 8, 15, 0, 0, TimeSpan.Zero));
    private readonly InMemoryUserRepository users = new();

    private async Task<User> AddUser(string email)
    {
        var user = User.RegisterOwner(Email.Create(email), PasswordHash.Create(ValidHash), clock.GetUtcNow().UtcDateTime);
        Assert.True(await users.TryAdd(user, CancellationToken.None));
        return user;
    }

    [Fact]
    public async Task Get_returns_the_own_profile_without_name_until_it_is_set_and_never_the_hash()
    {
        var user = await AddUser("ana@example.com");

        var profile = await new GetMyProfileHandler(users).Handle(new GetMyProfileQuery(user.Id), CancellationToken.None);

        Assert.NotNull(profile);
        Assert.Equal(user.Id, profile.Id);
        Assert.Equal("ana@example.com", profile.Email);
        Assert.Equal(UserRole.Owner, profile.Role);
        Assert.Null(profile.DisplayName);
        Assert.DoesNotContain(nameof(PasswordHash), typeof(UserProfileResult).GetProperties().Select(p => p.Name));
        Assert.DoesNotContain("ana@example.com", profile.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task Get_returns_null_when_the_account_of_the_token_does_not_exist()
        => Assert.Null(await new GetMyProfileHandler(users).Handle(new GetMyProfileQuery(Guid.NewGuid()), CancellationToken.None));

    [Fact]
    public async Task Update_trims_the_name_moves_the_update_date_and_keeps_email_role_and_creation()
    {
        var user = await AddUser("ana@example.com");
        var created = user.CreatedAtUtc;
        clock.Advance(TimeSpan.FromHours(2));

        var updated = await new UpdateMyProfileHandler(users, clock).Handle(new UpdateMyProfileCommand(user.Id, "  Ana Pérez  "), CancellationToken.None);

        Assert.NotNull(updated);
        Assert.Equal("Ana Pérez", updated.DisplayName);
        Assert.Equal("ana@example.com", updated.Email);
        Assert.Equal(UserRole.Owner, updated.Role);
        Assert.Equal(created, updated.CreatedAtUtc);
        Assert.Equal(clock.GetUtcNow().UtcDateTime, user.UpdatedAtUtc);
    }

    [Fact]
    public async Task Update_only_changes_the_account_of_the_token()
    {
        var ana = await AddUser("ana@example.com");
        var luis = await AddUser("luis@example.com");

        await new UpdateMyProfileHandler(users, clock).Handle(new UpdateMyProfileCommand(ana.Id, "Ana"), CancellationToken.None);

        Assert.Equal("Ana", ana.DisplayName);
        Assert.Null(luis.DisplayName);
    }

    [Fact]
    public async Task Update_returns_null_and_creates_nothing_when_the_account_does_not_exist()
    {
        var result = await new UpdateMyProfileHandler(users, clock).Handle(new UpdateMyProfileCommand(Guid.NewGuid(), "Ana"), CancellationToken.None);

        Assert.Null(result);
    }

    [Theory]
    [InlineData("", "El nombre es obligatorio.")]
    [InlineData("   ", "El nombre es obligatorio.")]
    [InlineData("Ana\nPérez", "El nombre debe tener entre 1 y 80 caracteres y no contener caracteres de control.")]
    public async Task Validator_rejects_blank_or_control_characters_naming_the_field(string name, string message)
    {
        var failures = await new UpdateMyProfileCommandValidator().Validate(new UpdateMyProfileCommand(Guid.NewGuid(), name), CancellationToken.None);

        var failure = Assert.Single(failures);
        Assert.Equal("displayName", failure.Field);
        Assert.Equal(message, failure.Message);
    }

    [Fact]
    public async Task Validator_accepts_80_characters_and_rejects_81()
    {
        var validator = new UpdateMyProfileCommandValidator();

        Assert.Empty(await validator.Validate(new UpdateMyProfileCommand(Guid.NewGuid(), new string('a', 80)), CancellationToken.None));
        Assert.Single(await validator.Validate(new UpdateMyProfileCommand(Guid.NewGuid(), new string('a', 81)), CancellationToken.None));
    }

    [Fact]
    public void The_update_command_has_no_member_to_change_role_or_email_and_does_not_print_the_name()
    {
        var members = typeof(UpdateMyProfileCommand).GetProperties().Select(p => p.Name).ToArray();

        Assert.Equal(["DisplayName", "UserId"], members.OrderBy(name => name, StringComparer.Ordinal));
        Assert.Equal("UpdateMyProfileCommand", new UpdateMyProfileCommand(Guid.NewGuid(), "Secreto").ToString());
    }
}
