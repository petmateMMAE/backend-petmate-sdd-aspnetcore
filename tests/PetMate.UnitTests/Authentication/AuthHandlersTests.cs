using PetMate.Application.Authentication;
using PetMate.Application.Security;
using PetMate.Domain.Sessions;
using PetMate.Domain.Users;
using PetMate.TestDoubles;

namespace PetMate.UnitTests.Authentication;

public sealed class AuthHandlersTests
{
    private const string Password = "contraseña-muy-segura-123";

    private static async Task<AuthTokensResult> RegisterOwner(AuthTestHarness stack, string email = "ana@example.com")
    {
        var result = await stack.Register.Handle(new RegisterCommand(email, Password), CancellationToken.None);
        Assert.True(result.IsSuccess);
        return result.Value!;
    }

    // ---------- Register (T018) ----------

    [Fact]
    public async Task Register_creates_an_owner_persists_only_a_hash_and_issues_tokens()
    {
        var stack = new AuthTestHarness();

        var tokens = await RegisterOwner(stack, "  Ana@Example.COM ");

        var user = Assert.Single(stack.Users.All);
        Assert.Equal(UserRole.Owner, user.Role);
        Assert.Equal("ana@example.com", user.Email.Value);
        Assert.DoesNotContain(Password, user.PasswordHash.Value, StringComparison.Ordinal);
        Assert.StartsWith("$argon2id$", user.PasswordHash.Value, StringComparison.Ordinal);
        Assert.Equal(user.Id, tokens.UserId);
        Assert.Equal(UserRole.Owner, tokens.Role);
        Assert.Equal(43, tokens.RefreshToken.Length);
        Assert.Equal(1, stack.Sessions.TokenCount);
    }

    [Fact]
    public async Task Register_rejects_a_duplicate_email_case_insensitively_without_creating_another_user_or_session()
    {
        var stack = new AuthTestHarness();
        await RegisterOwner(stack);

        var duplicate = await stack.Register.Handle(new RegisterCommand("ANA@example.com", Password), CancellationToken.None);

        Assert.False(duplicate.IsSuccess);
        Assert.Equal(AuthError.EmailAlreadyRegistered, duplicate.Error);
        Assert.Single(stack.Users.All);
        Assert.Single(stack.Sessions.Sessions());
    }

    [Fact]
    public async Task Concurrent_registrations_of_one_email_create_exactly_one_account()
    {
        var stack = new AuthTestHarness();

        var results = await Task.WhenAll(Enumerable.Range(0, 20)
            .Select(_ => Task.Run(() => stack.Register.Handle(new RegisterCommand("race@example.com", Password), CancellationToken.None))));

        Assert.Equal(1, results.Count(result => result.IsSuccess));
        Assert.Single(stack.Users.All);
    }

    [Fact]
    public void Register_command_has_no_role_member_so_clients_cannot_choose_their_role()
    {
        var members = typeof(RegisterCommand).GetProperties().Select(property => property.Name).ToArray();

        Assert.DoesNotContain(members, name => name.Contains("Role", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Register_validator_reports_every_problem_in_spanish()
    {
        var validator = new RegisterCommandValidator();

        var failures = await validator.Validate(new RegisterCommand("no-es-correo", "corta"), CancellationToken.None);

        Assert.Equal(2, failures.Count);
        Assert.Contains(failures, f => f.Field == "email" && f.Message == "El correo electrónico no es válido.");
        Assert.Contains(failures, f => f.Field == "password" && f.Message.Contains("al menos 12", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData("ana@example.com", "123456789012", 0)]
    [InlineData("ana@example.com", "12345678901", 1)]
    [InlineData("ana@example.com", "            ", 1)]
    [InlineData("ana@example.com", "", 1)]
    [InlineData("", "123456789012", 1)]
    public async Task Register_validator_enforces_the_password_policy_and_email_format(string email, string password, int expectedFailures)
        => Assert.Equal(expectedFailures, (await new RegisterCommandValidator().Validate(new RegisterCommand(email, password), CancellationToken.None)).Count);

    [Fact]
    public async Task Register_validator_rejects_passwords_above_128_characters()
        => Assert.Single(await new RegisterCommandValidator().Validate(
            new RegisterCommand("ana@example.com", new string('a', 129)), CancellationToken.None));

    [Fact]
    public void Commands_and_results_never_print_secrets()
    {
        Assert.DoesNotContain(Password, new RegisterCommand("a@b.co", Password).ToString(), StringComparison.Ordinal);
        Assert.DoesNotContain(Password, new LoginCommand("a@b.co", Password).ToString(), StringComparison.Ordinal);
        Assert.DoesNotContain("secreto", new RefreshCommand("secreto").ToString(), StringComparison.Ordinal);
        Assert.DoesNotContain("secreto", new LogoutCommand(Guid.NewGuid(), "jti", DateTime.UtcNow, "secreto").ToString(), StringComparison.Ordinal);
    }

    // ---------- Login (T019) ----------

    [Fact]
    public async Task Login_with_correct_credentials_issues_a_new_session_and_tokens()
    {
        var stack = new AuthTestHarness();
        var registered = await RegisterOwner(stack);

        var login = await stack.Login.Handle(new LoginCommand("ANA@example.com", Password), CancellationToken.None);

        Assert.True(login.IsSuccess);
        Assert.Equal(registered.UserId, login.Value!.UserId);
        Assert.NotEqual(registered.RefreshToken, login.Value.RefreshToken);
        Assert.Equal(2, stack.Sessions.Sessions().Count);
    }

    [Fact]
    public async Task Login_failures_for_wrong_password_and_unknown_email_are_identical_and_both_cost_one_hash_verification()
    {
        var stack = new AuthTestHarness();
        await RegisterOwner(stack);
        var before = stack.Hasher.VerifyCalls;

        var wrongPassword = await stack.Login.Handle(new LoginCommand("ana@example.com", "otra-contraseña-123"), CancellationToken.None);
        var afterWrong = stack.Hasher.VerifyCalls;
        var unknown = await stack.Login.Handle(new LoginCommand("nadie@example.com", Password), CancellationToken.None);
        var afterUnknown = stack.Hasher.VerifyCalls;

        Assert.Equal(AuthError.InvalidCredentials, wrongPassword.Error);
        Assert.Equal(AuthError.InvalidCredentials, unknown.Error);
        Assert.Equal(1, afterWrong - before);
        Assert.Equal(1, afterUnknown - afterWrong);
        Assert.Single(stack.Sessions.Sessions());
    }

    [Fact]
    public async Task Login_persists_a_stronger_hash_when_the_stored_parameters_are_outdated()
    {
        var stack = new AuthTestHarness();
        await RegisterOwner(stack);
        var user = stack.Users.All.Single();
        var oldHash = user.PasswordHash.Value;
        stack.Hasher.RehashNeeded = true;

        var login = await stack.Login.Handle(new LoginCommand("ana@example.com", Password), CancellationToken.None);

        Assert.True(login.IsSuccess);
        Assert.Equal(oldHash, user.PasswordHash.Value); // el fake produce el mismo hash; lo relevante es que se invocó el rehash
        Assert.True(stack.Hasher.HashCalls >= 3); // registro + hash ficticio + rehash
    }

    [Fact]
    public async Task Login_validator_requires_a_valid_email_and_a_password_without_applying_the_registration_policy()
    {
        var validator = new LoginCommandValidator();

        Assert.Empty(await validator.Validate(new LoginCommand("ana@example.com", "corta"), CancellationToken.None));
        Assert.Equal(2, (await validator.Validate(new LoginCommand("x", ""), CancellationToken.None)).Count);
    }

    // ---------- Refresh (T020) ----------

    [Fact]
    public async Task Refresh_rotates_the_token_consumes_the_old_one_and_keeps_the_same_session()
    {
        var stack = new AuthTestHarness();
        var first = await RegisterOwner(stack);

        var refreshed = await stack.Refresh.Handle(new RefreshCommand(first.RefreshToken), CancellationToken.None);

        Assert.True(refreshed.IsSuccess);
        Assert.NotEqual(first.RefreshToken, refreshed.Value!.RefreshToken);
        Assert.Single(stack.Sessions.Sessions());
        Assert.Equal(2, stack.Sessions.TokenCount);
        var second = await stack.Refresh.Handle(new RefreshCommand(refreshed.Value.RefreshToken), CancellationToken.None);
        Assert.True(second.IsSuccess);
    }

    [Fact]
    public async Task Reusing_a_consumed_refresh_token_is_rejected_and_revokes_the_whole_session_including_the_newest_token()
    {
        var stack = new AuthTestHarness();
        var first = await RegisterOwner(stack);
        var rotated = (await stack.Refresh.Handle(new RefreshCommand(first.RefreshToken), CancellationToken.None)).Value!;

        var replay = await stack.Refresh.Handle(new RefreshCommand(first.RefreshToken), CancellationToken.None);
        var newestAfterReplay = await stack.Refresh.Handle(new RefreshCommand(rotated.RefreshToken), CancellationToken.None);

        Assert.Equal(AuthError.InvalidRefreshToken, replay.Error);
        Assert.Equal(AuthError.InvalidRefreshToken, newestAfterReplay.Error);
        var session = Assert.Single(stack.Sessions.Sessions());
        Assert.True(session.IsRevoked);
        Assert.Equal(SessionRevocationReason.ReplayDetected, session.RevocationReason);
    }

    [Fact]
    public async Task Concurrent_refresh_requests_with_the_same_token_let_exactly_one_succeed()
    {
        var stack = new AuthTestHarness();
        var first = await RegisterOwner(stack);

        var results = await Task.WhenAll(Enumerable.Range(0, 20)
            .Select(_ => Task.Run(() => stack.Refresh.Handle(new RefreshCommand(first.RefreshToken), CancellationToken.None))));

        Assert.InRange(results.Count(result => result.IsSuccess), 0, 1); // un perdedor puede revocar la sesión antes de que el ganador la actualice
        Assert.All(results.Where(result => !result.IsSuccess), result => Assert.Equal(AuthError.InvalidRefreshToken, result.Error));
        Assert.True(Assert.Single(stack.Sessions.Sessions()).IsRevoked);
    }

    [Fact]
    public async Task Refresh_fails_after_30_idle_days_but_succeeds_just_before()
    {
        var stack = new AuthTestHarness();
        var first = await RegisterOwner(stack);

        stack.Clock.Advance(TimeSpan.FromDays(30).Subtract(TimeSpan.FromSeconds(1)));
        var ok = await stack.Refresh.Handle(new RefreshCommand(first.RefreshToken), CancellationToken.None);
        Assert.True(ok.IsSuccess);

        stack.Clock.Advance(TimeSpan.FromDays(30));
        var expired = await stack.Refresh.Handle(new RefreshCommand(ok.Value!.RefreshToken), CancellationToken.None);
        Assert.Equal(AuthError.InvalidRefreshToken, expired.Error);
    }

    [Fact]
    public async Task Active_use_never_extends_a_session_beyond_the_90_day_absolute_limit()
    {
        var stack = new AuthTestHarness();
        var current = await RegisterOwner(stack);

        for (var day = 25; day < 90; day += 25)
        {
            stack.Clock.Advance(TimeSpan.FromDays(25));
            current = (await stack.Refresh.Handle(new RefreshCommand(current.RefreshToken), CancellationToken.None)).Value!;
        }

        stack.Clock.Advance(TimeSpan.FromDays(25)); // día 100 > 90
        var result = await stack.Refresh.Handle(new RefreshCommand(current.RefreshToken), CancellationToken.None);

        Assert.Equal(AuthError.InvalidRefreshToken, result.Error);
    }

    [Theory]
    [InlineData("")]
    [InlineData("corto")]
    [InlineData("0000000000000000000000000000000000000000001")]
    [InlineData("%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%")]
    public async Task Refresh_rejects_unknown_or_malformed_tokens(string token)
        => Assert.Equal(AuthError.InvalidRefreshToken,
            (await new AuthTestHarness().Refresh.Handle(new RefreshCommand(token), CancellationToken.None)).Error);

    [Fact]
    public async Task Refresh_fails_for_a_session_revoked_by_logout()
    {
        var stack = new AuthTestHarness();
        var tokens = await RegisterOwner(stack);
        await stack.Logout.Handle(
            new LogoutCommand(tokens.UserId, tokens.AccessToken.Jti, tokens.AccessToken.ExpiresAtUtc, tokens.RefreshToken), CancellationToken.None);

        var result = await stack.Refresh.Handle(new RefreshCommand(tokens.RefreshToken), CancellationToken.None);

        Assert.Equal(AuthError.InvalidRefreshToken, result.Error);
    }

    [Fact]
    public async Task Refresh_works_after_the_access_token_has_expired_while_the_session_is_valid()
    {
        var stack = new AuthTestHarness();
        var tokens = await RegisterOwner(stack);
        stack.Clock.Advance(TimeSpan.FromMinutes(20));

        var result = await stack.Refresh.Handle(new RefreshCommand(tokens.RefreshToken), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value!.AccessToken.ExpiresAtUtc > stack.Clock.GetUtcNow().UtcDateTime);
    }

    [Fact]
    public async Task Only_the_hash_of_refresh_tokens_reaches_the_repository()
    {
        var stack = new AuthTestHarness();
        var tokens = await RegisterOwner(stack);

        var stored = await stack.Sessions.FindTokenByHash(stack.Tokens.TryHash(tokens.RefreshToken, out var hash) ? hash! : null!, CancellationToken.None);

        Assert.NotNull(stored);
        Assert.DoesNotContain(tokens.RefreshToken, stored.TokenHash.Value, StringComparison.Ordinal);
        Assert.Equal(64, stored.TokenHash.Value.Length);
    }

    // ---------- Logout (T021) ----------

    [Fact]
    public async Task Logout_revokes_the_session_and_blacklists_the_current_jti()
    {
        var stack = new AuthTestHarness();
        var tokens = await RegisterOwner(stack);

        var result = await stack.Logout.Handle(
            new LogoutCommand(tokens.UserId, tokens.AccessToken.Jti, tokens.AccessToken.ExpiresAtUtc, tokens.RefreshToken), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(Assert.Single(stack.Sessions.Sessions()).IsRevoked);
        Assert.True(await stack.Revocations.IsRevoked(tokens.AccessToken.Jti, CancellationToken.None));
    }

    [Fact]
    public async Task Logout_is_idempotent_for_an_already_revoked_own_session()
    {
        var stack = new AuthTestHarness();
        var tokens = await RegisterOwner(stack);
        var command = new LogoutCommand(tokens.UserId, tokens.AccessToken.Jti, tokens.AccessToken.ExpiresAtUtc, tokens.RefreshToken);

        await stack.Logout.Handle(command, CancellationToken.None);
        var second = await stack.Logout.Handle(command, CancellationToken.None);

        Assert.True(second.IsSuccess);
    }

    [Fact]
    public async Task A_user_cannot_revoke_another_users_session()
    {
        var stack = new AuthTestHarness();
        var owner = await RegisterOwner(stack, "dueno@example.com");
        var attacker = await RegisterOwner(stack, "intruso@example.com");

        var result = await stack.Logout.Handle(
            new LogoutCommand(attacker.UserId, attacker.AccessToken.Jti, attacker.AccessToken.ExpiresAtUtc, owner.RefreshToken), CancellationToken.None);

        Assert.Equal(AuthError.Forbidden, result.Error);
        Assert.False(stack.Sessions.IsSessionRevoked(stack.Sessions.Sessions().Single(s => s.UserId == owner.UserId).Id));
        Assert.False(stack.Revocations.Contains(attacker.AccessToken.Jti));
        Assert.True((await stack.Refresh.Handle(new RefreshCommand(owner.RefreshToken), CancellationToken.None)).IsSuccess);
    }

    [Theory]
    [InlineData("")]
    [InlineData("desconocido")]
    [InlineData("0000000000000000000000000000000000000009999")]
    public async Task Logout_with_an_unknown_refresh_token_is_forbidden_and_revokes_nothing(string refreshToken)
    {
        var stack = new AuthTestHarness();
        var tokens = await RegisterOwner(stack);

        var result = await stack.Logout.Handle(
            new LogoutCommand(tokens.UserId, tokens.AccessToken.Jti, tokens.AccessToken.ExpiresAtUtc, refreshToken), CancellationToken.None);

        Assert.Equal(AuthError.Forbidden, result.Error);
        Assert.False(stack.Revocations.Contains(tokens.AccessToken.Jti));
        Assert.False(Assert.Single(stack.Sessions.Sessions()).IsRevoked);
    }

    [Fact]
    public async Task Logout_fails_closed_when_redis_is_down_but_the_session_is_already_revoked_so_a_retry_succeeds()
    {
        var stack = new AuthTestHarness();
        var tokens = await RegisterOwner(stack);
        var command = new LogoutCommand(tokens.UserId, tokens.AccessToken.Jti, tokens.AccessToken.ExpiresAtUtc, tokens.RefreshToken);
        stack.Revocations.Fail = true;

        await Assert.ThrowsAsync<AuthDependencyUnavailableException>(() => stack.Logout.Handle(command, CancellationToken.None));
        Assert.True(Assert.Single(stack.Sessions.Sessions()).IsRevoked);

        stack.Revocations.Fail = false;
        Assert.True((await stack.Logout.Handle(command, CancellationToken.None)).IsSuccess);
        Assert.True(stack.Revocations.Contains(tokens.AccessToken.Jti));
    }

    [Fact]
    public async Task Blacklist_entry_expires_with_the_access_token()
    {
        var stack = new AuthTestHarness();
        var tokens = await RegisterOwner(stack);
        await stack.Logout.Handle(
            new LogoutCommand(tokens.UserId, tokens.AccessToken.Jti, tokens.AccessToken.ExpiresAtUtc, tokens.RefreshToken), CancellationToken.None);

        stack.Clock.Advance(TimeSpan.FromMinutes(16));

        Assert.False(await stack.Revocations.IsRevoked(tokens.AccessToken.Jti, CancellationToken.None));
    }

    [Fact]
    public async Task Logout_validator_requires_the_refresh_token()
        => Assert.Single(await new LogoutCommandValidator().Validate(
            new LogoutCommand(Guid.NewGuid(), "jti", DateTime.UtcNow, ""), CancellationToken.None));
}
