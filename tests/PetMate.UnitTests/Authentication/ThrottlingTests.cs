using PetMate.Application.Authentication;
using PetMate.Application.Security;
using PetMate.Domain.Users;
using PetMate.TestDoubles;

namespace PetMate.UnitTests.Authentication;

/// <summary>Cuota por correo, bloqueo temporal y limitador de hashing (pm-034, T056).</summary>
public sealed class ThrottlingTests
{
    private const string Password = "contraseña-muy-segura-123";
    private const string Wrong = "otra-contraseña-123";

    private static async Task Register(AuthTestHarness stack, string email = "ana@example.com")
        => Assert.True((await stack.Register.Handle(new RegisterCommand(email, Password), CancellationToken.None)).IsSuccess);

    private static Task<AuthResult<AuthTokensResult>> Login(AuthTestHarness stack, string email, string password)
        => stack.Login.Handle(new LoginCommand(email, password), CancellationToken.None);

    [Fact]
    public void The_approved_rules_are_5_failed_logins_and_30_refreshes_per_15_minutes()
    {
        Assert.Equal((5, TimeSpan.FromMinutes(15), TimeSpan.FromMinutes(15)), (AuthThrottleRules.Login.MaxAttempts, AuthThrottleRules.Login.Window, AuthThrottleRules.Login.LockDuration));
        Assert.Equal((30, TimeSpan.FromMinutes(15), TimeSpan.FromMinutes(15)), (AuthThrottleRules.Refresh.MaxAttempts, AuthThrottleRules.Refresh.Window, AuthThrottleRules.Refresh.LockDuration));
    }

    [Fact]
    public async Task Five_failures_are_answered_normally_and_the_sixth_locks_the_email_even_for_the_right_password()
    {
        var stack = new AuthTestHarness();
        await Register(stack);

        for (var i = 0; i < 5; i++)
        {
            Assert.Equal(AuthError.InvalidCredentials, (await Login(stack, "ana@example.com", Wrong)).Error);
        }

        var sixth = await Login(stack, "ana@example.com", Wrong);
        var rightPassword = await Login(stack, "ana@example.com", Password);

        Assert.Equal(AuthError.TooManyRequests, sixth.Error);
        Assert.Equal(AuthThrottleRules.Login.LockDuration, sixth.RetryAfter);
        Assert.Equal(AuthError.TooManyRequests, rightPassword.Error);
        Assert.True(rightPassword.RetryAfter > TimeSpan.FromMinutes(14));
    }

    [Fact]
    public async Task The_lock_does_not_reveal_whether_the_email_exists()
    {
        var stack = new AuthTestHarness();
        await Register(stack);

        var known = new List<(AuthError, bool)>();
        var unknown = new List<(AuthError, bool)>();
        for (var i = 0; i < 8; i++)
        {
            var a = await Login(stack, "ana@example.com", Wrong);
            var b = await Login(stack, "nadie@example.com", Wrong);
            known.Add((a.Error, a.RetryAfter is not null));
            unknown.Add((b.Error, b.RetryAfter is not null));
        }

        Assert.Equal(known, unknown);
        Assert.Equal(AuthError.TooManyRequests, known[^1].Item1);
    }

    [Fact]
    public async Task The_lock_is_per_email_and_ends_after_the_lock_duration()
    {
        var stack = new AuthTestHarness();
        await Register(stack);
        await Register(stack, "luis@example.com");
        for (var i = 0; i < 6; i++)
        {
            _ = await Login(stack, "ana@example.com", Wrong);
        }

        Assert.True((await Login(stack, "luis@example.com", Password)).IsSuccess);
        Assert.Equal(AuthError.TooManyRequests, (await Login(stack, "ana@example.com", Password)).Error);

        stack.Clock.Advance(AuthThrottleRules.Login.LockDuration + TimeSpan.FromSeconds(1));

        Assert.True((await Login(stack, "ana@example.com", Password)).IsSuccess);
    }

    [Fact]
    public async Task A_successful_login_clears_the_failure_count()
    {
        var stack = new AuthTestHarness();
        await Register(stack);

        for (var i = 0; i < 4; i++)
        {
            _ = await Login(stack, "ana@example.com", Wrong);
        }

        Assert.True((await Login(stack, "ana@example.com", Password)).IsSuccess);
        for (var i = 0; i < 5; i++)
        {
            Assert.Equal(AuthError.InvalidCredentials, (await Login(stack, "ana@example.com", Wrong)).Error);
        }
    }

    [Fact]
    public async Task Failures_older_than_the_window_do_not_count()
    {
        var stack = new AuthTestHarness();
        await Register(stack);
        for (var i = 0; i < 4; i++)
        {
            _ = await Login(stack, "ana@example.com", Wrong);
        }

        stack.Clock.Advance(AuthThrottleRules.Login.Window + TimeSpan.FromSeconds(1));

        for (var i = 0; i < 5; i++)
        {
            Assert.Equal(AuthError.InvalidCredentials, (await Login(stack, "ana@example.com", Wrong)).Error);
        }
    }

    [Fact]
    public async Task A_locked_email_does_not_cost_any_password_hash()
    {
        var stack = new AuthTestHarness();
        await Register(stack);
        for (var i = 0; i < 6; i++)
        {
            _ = await Login(stack, "ana@example.com", Wrong);
        }

        var before = stack.Hasher.VerifyCalls;
        _ = await Login(stack, "ana@example.com", Wrong);

        Assert.Equal(before, stack.Hasher.VerifyCalls);
    }

    [Fact]
    public async Task Refresh_is_limited_per_user_to_30_attempts_per_window()
    {
        var stack = new AuthTestHarness();
        var current = (await stack.Register.Handle(new RegisterCommand("ana@example.com", Password), CancellationToken.None)).Value!;
        var other = (await stack.Register.Handle(new RegisterCommand("luis@example.com", Password), CancellationToken.None)).Value!;

        for (var i = 0; i < 30; i++)
        {
            var result = await stack.Refresh.Handle(new RefreshCommand(current.RefreshToken), CancellationToken.None);
            Assert.True(result.IsSuccess);
            current = result.Value!;
        }

        var blocked = await stack.Refresh.Handle(new RefreshCommand(current.RefreshToken), CancellationToken.None);
        var unaffected = await stack.Refresh.Handle(new RefreshCommand(other.RefreshToken), CancellationToken.None);

        Assert.Equal(AuthError.TooManyRequests, blocked.Error);
        Assert.NotNull(blocked.RetryAfter);
        Assert.True(unaffected.IsSuccess);

        stack.Clock.Advance(AuthThrottleRules.Refresh.LockDuration + TimeSpan.FromSeconds(1));
        Assert.True((await stack.Refresh.Handle(new RefreshCommand(current.RefreshToken), CancellationToken.None)).IsSuccess);
    }

    [Fact]
    public async Task A_burst_of_concurrent_guesses_never_verifies_more_passwords_than_the_maximum()
    {
        var stack = new AuthTestHarness();
        await Register(stack);
        var before = stack.Hasher.VerifyCalls;

        var results = await Task.WhenAll(Enumerable.Range(0, 40).Select(_ => Task.Run(() => Login(stack, "ana@example.com", Wrong))));

        Assert.True(stack.Hasher.VerifyCalls - before <= AuthThrottleRules.Login.MaxAttempts);
        Assert.Equal(AuthThrottleRules.Login.MaxAttempts, results.Count(r => r.Error == AuthError.InvalidCredentials));
        Assert.Equal(40 - AuthThrottleRules.Login.MaxAttempts, results.Count(r => r.Error == AuthError.TooManyRequests));
    }

    [Fact]
    public async Task Redis_failures_surface_as_the_auth_dependency_error_so_login_fails_closed()
    {
        var stack = new AuthTestHarness();
        await Register(stack);
        stack.Throttle.Fail = true;

        await Assert.ThrowsAsync<AuthDependencyUnavailableException>(() => Login(stack, "ana@example.com", Password));
    }

    // ---------- Limitador de hashing ----------

    [Fact]
    public void The_limiter_requires_valid_sizes()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new BoundedPasswordHashingLimiter(0, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => new BoundedPasswordHashingLimiter(1, -1));
    }

    [Fact]
    public async Task The_limiter_runs_at_most_the_allowed_hashes_at_once_and_rejects_what_does_not_fit_in_the_queue()
    {
        using var limiter = new BoundedPasswordHashingLimiter(maxConcurrency: 1, maxQueue: 1);
        using var release = new ManualResetEventSlim(false);
        using var started = new ManualResetEventSlim(false);
        var running = 0;
        var peak = 0;

        int Work()
        {
            var now = Interlocked.Increment(ref running);
            int seen;
            while ((seen = Volatile.Read(ref peak)) < now && Interlocked.CompareExchange(ref peak, now, seen) != seen)
            {
            }

            started.Set();
            release.Wait(TimeSpan.FromSeconds(10));
            Interlocked.Decrement(ref running);
            return 1;
        }

        var first = limiter.Run(Work, CancellationToken.None);
        Assert.True(started.Wait(TimeSpan.FromSeconds(10)));
        var second = limiter.Run(Work, CancellationToken.None);
        await Task.Delay(100);
        var third = await limiter.Run(Work, CancellationToken.None);

        Assert.False(third.Accepted);
        release.Set();
        Assert.True((await first).Accepted);
        Assert.True((await second).Accepted);
        Assert.Equal(1, Volatile.Read(ref peak));
    }

    [Fact]
    public async Task A_cancelled_caller_does_not_free_the_slot_while_its_hash_is_still_running()
    {
        using var limiter = new BoundedPasswordHashingLimiter(maxConcurrency: 1, maxQueue: 0);
        using var release = new ManualResetEventSlim(false);
        using var started = new ManualResetEventSlim(false);
        using var cts = new CancellationTokenSource();

        var first = limiter.Run(() =>
        {
            started.Set();
            release.Wait(TimeSpan.FromSeconds(10));
            return 1;
        }, cts.Token);
        Assert.True(started.Wait(TimeSpan.FromSeconds(10)));
        await cts.CancelAsync();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => first);

        // El hash sigue corriendo: el cupo sigue ocupado y un nuevo hash no cabe.
        Assert.False((await limiter.Run(() => 2, CancellationToken.None)).Accepted);

        release.Set();
        var accepted = false;
        for (var i = 0; i < 100 && !accepted; i++)
        {
            accepted = (await limiter.Run(() => 3, CancellationToken.None)).Accepted;
            if (!accepted)
            {
                await Task.Delay(50);
            }
        }

        Assert.True(accepted);
    }

    [Fact]
    public async Task Login_and_register_answer_too_many_requests_when_the_limiter_rejects_and_create_nothing()
    {
        var stack = new AuthTestHarness();
        await Register(stack);
        var rejecting = new RejectingLimiter();
        var issuer = new AuthTokenIssuer(stack.Sessions, stack.Tokens, stack.Access, PetMate.Domain.Sessions.SessionPolicy.Default);
        var login = new LoginHandler(stack.Users, stack.Hasher, issuer, stack.Clock, stack.Throttle, rejecting);
        var register = new RegisterHandler(stack.Users, stack.Hasher, issuer, stack.Clock, rejecting);

        var loginResult = await login.Handle(new LoginCommand("ana@example.com", Password), CancellationToken.None);
        var registerResult = await register.Handle(new RegisterCommand("nuevo@example.com", Password), CancellationToken.None);

        Assert.Equal(AuthError.TooManyRequests, loginResult.Error);
        Assert.Equal(TimeSpan.FromSeconds(1), loginResult.RetryAfter);
        Assert.Equal(AuthError.TooManyRequests, registerResult.Error);
        Assert.Null(await stack.Users.FindByEmail(Email.Create("nuevo@example.com"), CancellationToken.None));
    }

    private sealed class RejectingLimiter : IPasswordHashingLimiter
    {
        public Task<HashingOutcome<T>> Run<T>(Func<T> work, CancellationToken cancellationToken)
            => Task.FromResult(new HashingOutcome<T>(false, default));
    }
}
