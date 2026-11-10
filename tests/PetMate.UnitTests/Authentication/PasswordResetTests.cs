using PetMate.Application.Authentication;
using PetMate.Application.Security;
using PetMate.Domain.Sessions;
using PetMate.Domain.Users;
using PetMate.TestDoubles;

namespace PetMate.UnitTests.Authentication;

/// <summary>pm-088 a pm-090: recuperación de contraseña con token de un solo uso, límites y revocación de sesiones.</summary>
public sealed class PasswordResetTests
{
    private const string NewPassword = "contrasena-nueva-123";

    private sealed class Rig
    {
        public Rig()
        {
            Stack = new AuthTestHarness();
            Resets = new InMemoryPasswordResetRepository();
            Email = new FakeEmailSender();
            Forgot = new ForgotPasswordHandler(Stack.Users, Resets, Stack.Tokens, Email, Stack.Throttle, Stack.Clock);
            Reset = new ResetPasswordHandler(Stack.Users, Resets, Stack.Sessions, Stack.Hasher, Stack.Tokens, Stack.Access, Stack.Clock, Stack.Revocations);
        }

        public AuthTestHarness Stack { get; }

        public InMemoryPasswordResetRepository Resets { get; }

        public FakeEmailSender Email { get; }

        public ForgotPasswordHandler Forgot { get; }

        public ResetPasswordHandler Reset { get; }

        public async Task<AuthTokensResult> Register(string email = "ana@example.com")
        {
            var result = await Stack.Register.Handle(new RegisterCommand(email, "contrasena-vieja-123", "Ana"), default);
            return result.Value!;
        }
    }

    [Fact]
    public async Task Forgot_for_an_existing_account_stores_only_the_hash_and_emails_the_token()
    {
        var rig = new Rig();
        var tokens = await rig.Register();

        var result = await rig.Forgot.Handle(new ForgotPasswordCommand("ANA@example.com"), default);

        Assert.True(result.IsSuccess);
        var sent = Assert.Single(rig.Email.Sent);
        Assert.Equal("ana@example.com", sent.To);
        var stored = rig.Resets.StoredHashOf(tokens.UserId);
        Assert.NotNull(stored);
        Assert.DoesNotContain(sent.Token, stored, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Forgot_for_an_unknown_email_answers_the_same_and_sends_nothing()
    {
        var rig = new Rig();

        var result = await rig.Forgot.Handle(new ForgotPasswordCommand("nadie@example.com"), default);

        Assert.True(result.IsSuccess);
        Assert.Empty(rig.Email.Sent);
        Assert.Equal(0, rig.Resets.Count);
    }

    [Fact]
    public async Task A_provider_failure_does_not_change_the_answer()
    {
        var rig = new Rig();
        await rig.Register();
        rig.Email.Succeeds = false;

        var result = await rig.Forgot.Handle(new ForgotPasswordCommand("ana@example.com"), default);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Only_the_latest_link_works_and_finishing_a_reset_kills_the_pending_ones()
    {
        var rig = new Rig();
        await rig.Register();
        await rig.Forgot.Handle(new ForgotPasswordCommand("ana@example.com"), default);
        await rig.Forgot.Handle(new ForgotPasswordCommand("ana@example.com"), default);
        await rig.Forgot.Handle(new ForgotPasswordCommand("ana@example.com"), default);
        var (first, second, third) = (rig.Email.Sent[0].Token, rig.Email.Sent[1].Token, rig.Email.Sent[2].Token);

        Assert.Equal(AuthError.InvalidResetToken, (await rig.Reset.Handle(new ResetPasswordCommand(first, NewPassword), default)).Error);
        Assert.Equal(AuthError.InvalidResetToken, (await rig.Reset.Handle(new ResetPasswordCommand(second, NewPassword), default)).Error);
        Assert.True((await rig.Reset.Handle(new ResetPasswordCommand(third, NewPassword), default)).IsSuccess);
    }

    [Fact]
    public async Task An_Atlas_failure_for_an_existing_account_still_answers_like_an_unknown_one()
    {
        var rig = new Rig();
        await rig.Register();
        rig.Resets.Throws = true;

        var result = await rig.Forgot.Handle(new ForgotPasswordCommand("ana@example.com"), default);

        Assert.True(result.IsSuccess);
        Assert.Empty(rig.Email.Sent);
    }

    [Fact]
    public async Task Forgot_is_limited_per_email_whether_or_not_the_account_exists()
    {
        var rig = new Rig();
        await rig.Register();

        foreach (var email in new[] { "ana@example.com", "nadie@example.com" })
        {
            for (var i = 0; i < 3; i++)
            {
                Assert.True((await rig.Forgot.Handle(new ForgotPasswordCommand(email), default)).IsSuccess);
            }

            var limited = await rig.Forgot.Handle(new ForgotPasswordCommand(email), default);
            Assert.Equal(AuthError.TooManyRequests, limited.Error);
            Assert.True(limited.RetryAfter > TimeSpan.Zero);
        }

        Assert.Equal(3, rig.Email.Sent.Count);
    }

    [Fact]
    public async Task Reset_changes_the_password_consumes_the_token_and_revokes_every_session()
    {
        var rig = new Rig();
        var first = await rig.Register();
        var second = Ok(await rig.Stack.Login.Handle(new LoginCommand("ana@example.com", "contrasena-vieja-123"), default));
        await rig.Forgot.Handle(new ForgotPasswordCommand("ana@example.com"), default);
        var token = rig.Email.Sent.Single().Token;

        var result = await rig.Reset.Handle(new ResetPasswordCommand(token, NewPassword), default);

        Assert.True(result.IsSuccess);
        var user = await rig.Stack.Users.FindByEmail(Email.Create("ana@example.com"), default);
        Assert.NotEqual(PasswordVerificationResult.Failed, rig.Stack.Hasher.Verify(NewPassword, user!.PasswordHash.Value));
        Assert.Equal(PasswordVerificationResult.Failed, rig.Stack.Hasher.Verify("contrasena-vieja-123", user.PasswordHash.Value));
        Assert.Equal(AuthError.InvalidRefreshToken, (await rig.Stack.Refresh.Handle(new RefreshCommand(first.RefreshToken), default)).Error);
        Assert.Equal(AuthError.InvalidRefreshToken, (await rig.Stack.Refresh.Handle(new RefreshCommand(second.RefreshToken), default)).Error);
        Assert.True(await rig.Stack.Revocations.IsSessionRevoked(await SessionIdOf(rig, first), default));
    }

    [Fact]
    public async Task A_used_unknown_or_expired_token_gets_the_same_error()
    {
        var rig = new Rig();
        await rig.Register();
        await rig.Forgot.Handle(new ForgotPasswordCommand("ana@example.com"), default);
        var token = rig.Email.Sent.Single().Token;
        Assert.True((await rig.Reset.Handle(new ResetPasswordCommand(token, NewPassword), default)).IsSuccess);

        var reused = await rig.Reset.Handle(new ResetPasswordCommand(token, NewPassword), default);
        var unknown = await rig.Reset.Handle(new ResetPasswordCommand("no-existe", NewPassword), default);
        await rig.Forgot.Handle(new ForgotPasswordCommand("ana@example.com"), default);
        rig.Stack.Clock.Advance(TimeSpan.FromMinutes(61));
        var expired = await rig.Reset.Handle(new ResetPasswordCommand(rig.Email.Sent[1].Token, NewPassword), default);

        Assert.Equal(AuthError.InvalidResetToken, reused.Error);
        Assert.Equal(AuthError.InvalidResetToken, unknown.Error);
        Assert.Equal(AuthError.InvalidResetToken, expired.Error);
    }

    [Fact]
    public async Task Concurrent_uses_of_the_same_token_have_a_single_winner()
    {
        var rig = new Rig();
        await rig.Register();
        await rig.Forgot.Handle(new ForgotPasswordCommand("ana@example.com"), default);
        var token = rig.Email.Sent.Single().Token;

        var results = await Task.WhenAll(Enumerable.Range(0, 12).Select(_ => Task.Run(() => rig.Reset.Handle(new ResetPasswordCommand(token, NewPassword), default))));

        Assert.Equal(1, results.Count(r => r.IsSuccess));
    }

    [Theory]
    [InlineData("", "contrasena-nueva-123", "token")]
    [InlineData("abc", "corta", "newPassword")]
    [InlineData("abc", "", "newPassword")]
    public async Task Reset_validator_enforces_the_password_policy_of_12_to_128(string token, string password, string field)
    {
        var failures = await new ResetPasswordCommandValidator().Validate(new ResetPasswordCommand(token, password), default);

        Assert.Contains(failures, f => f.Field == field);
        Assert.Equal("ResetPasswordCommand", new ResetPasswordCommand("secreto", "secreto").ToString());
    }

    [Fact]
    public async Task Forgot_validator_rejects_a_malformed_email()
        => Assert.Contains(await new ForgotPasswordCommandValidator().Validate(new ForgotPasswordCommand("malo"), default), f => f.Field == "email");

    private static async Task<Guid> SessionIdOf(Rig rig, AuthTokensResult tokens)
    {
        Assert.True(rig.Stack.Tokens.TryHash(tokens.RefreshToken, out var hash));
        var stored = await rig.Stack.Sessions.FindTokenByHash(hash!, default);
        return stored!.SessionId;
    }

    private static AuthTokensResult Ok(AuthResult<AuthTokensResult> result)
    {
        Assert.True(result.IsSuccess);
        return result.Value!;
    }
}
