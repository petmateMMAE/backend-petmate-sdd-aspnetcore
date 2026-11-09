using PetMate.Application.Security;
using PetMate.Infrastructure.Redis;
using StackExchange.Redis;

namespace PetMate.IntegrationTests.Redis;

/// <summary>Cuota y bloqueo por correo contra el Redis de autenticación (pm-034). Skipped si falta la variable de Redis.</summary>
public sealed class AuthAttemptThrottleTests
{
    private const string AuthVariable = "PETMATE_IT_REDIS_AUTH";

    private static readonly AuthThrottleRule Rule = new(3, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30));

    private static string NewSubject() => "it-" + Guid.NewGuid().ToString("N") + "@example.com";

    [RequiresEnvironmentFact(AuthVariable)]
    public async Task Attempts_beyond_the_maximum_lock_the_subject_with_a_ttl_and_the_raw_subject_is_never_stored()
    {
        await using var auth = new RedisAuthConnection(Environment.GetEnvironmentVariable(AuthVariable)!);
        var throttle = new RedisAuthAttemptThrottle(auth);
        var subject = NewSubject();
        var database = await auth.GetDatabase();
        try
        {
            for (var i = 0; i < 3; i++)
            {
                Assert.Null(await throttle.RecordAttempt("login", subject, Rule, CancellationToken.None));
            }

            Assert.Null(await throttle.GetRemainingLock("login", subject, CancellationToken.None));
            var lockedFor = await throttle.RecordAttempt("login", subject, Rule, CancellationToken.None);
            var remaining = await throttle.GetRemainingLock("login", subject, CancellationToken.None);

            Assert.Equal(Rule.LockDuration, lockedFor);
            Assert.NotNull(remaining);
            Assert.InRange(remaining.Value.TotalSeconds, 25, 30);

            var server = database.Multiplexer.GetServers()[0];
            var keys = server.Keys(database.Database, "auth:*").Select(key => key.ToString()).ToArray();
            Assert.DoesNotContain(keys, key => key.Contains(subject, StringComparison.OrdinalIgnoreCase) || key.Contains("example.com", StringComparison.OrdinalIgnoreCase));
        }
        finally
        {
            await Cleanup(database, subject);
        }
    }

    [RequiresEnvironmentFact(AuthVariable)]
    public async Task The_hyphenated_scopes_of_the_ip_limit_and_the_email_ceiling_are_accepted_and_independent()
    {
        await using var auth = new RedisAuthConnection(Environment.GetEnvironmentVariable(AuthVariable)!);
        var throttle = new RedisAuthAttemptThrottle(auth);
        var subject = NewSubject();
        var rule = new AuthThrottleRule(2, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30));
        var database = await auth.GetDatabase();
        try
        {
            Assert.Null(await throttle.RecordAttempt(AuthThrottleRules.IpScope, subject, rule, CancellationToken.None));
            Assert.Null(await throttle.RecordAttempt(AuthThrottleRules.IpScope, subject, rule, CancellationToken.None));
            Assert.NotNull(await throttle.RecordAttempt(AuthThrottleRules.IpScope, subject, rule, CancellationToken.None));

            Assert.NotNull(await throttle.GetRemainingLock(AuthThrottleRules.IpScope, subject, CancellationToken.None));
            Assert.Null(await throttle.GetRemainingLock(AuthThrottleRules.LoginEmailCeilingScope, subject, CancellationToken.None));
        }
        finally
        {
            await Cleanup(database, subject);
        }
    }

    [RequiresEnvironmentFact(AuthVariable)]
    public async Task Clear_resets_the_counter_without_lifting_an_active_lock_and_subjects_are_independent()
    {
        await using var auth = new RedisAuthConnection(Environment.GetEnvironmentVariable(AuthVariable)!);
        var throttle = new RedisAuthAttemptThrottle(auth);
        var first = NewSubject();
        var second = NewSubject();
        var database = await auth.GetDatabase();
        try
        {
            _ = await throttle.RecordAttempt("login", first, Rule, CancellationToken.None);
            _ = await throttle.RecordAttempt("login", first, Rule, CancellationToken.None);
            await throttle.Clear("login", first, CancellationToken.None);
            for (var i = 0; i < 3; i++)
            {
                Assert.Null(await throttle.RecordAttempt("login", first, Rule, CancellationToken.None));
            }

            for (var i = 0; i < 4; i++)
            {
                _ = await throttle.RecordAttempt("login", second, Rule, CancellationToken.None);
            }

            await throttle.Clear("login", second, CancellationToken.None);

            Assert.NotNull(await throttle.GetRemainingLock("login", second, CancellationToken.None));
            Assert.Null(await throttle.GetRemainingLock("login", first, CancellationToken.None));
            Assert.Null(await throttle.GetRemainingLock("refresh", second, CancellationToken.None));
        }
        finally
        {
            await Cleanup(database, first);
            await Cleanup(database, second);
        }
    }

    [RequiresEnvironmentFact(AuthVariable)]
    public async Task Attempts_during_an_active_lock_are_refused_and_do_not_restart_the_counter()
    {
        await using var auth = new RedisAuthConnection(Environment.GetEnvironmentVariable(AuthVariable)!);
        var throttle = new RedisAuthAttemptThrottle(auth);
        var subject = NewSubject();
        var database = await auth.GetDatabase();
        try
        {
            for (var i = 0; i < 4; i++)
            {
                _ = await throttle.RecordAttempt("login", subject, Rule, CancellationToken.None);
            }

            // Mientras dura el bloqueo, cualquier intento sigue bloqueado: el contador no vuelve a empezar de cero.
            for (var i = 0; i < 10; i++)
            {
                var remaining = await throttle.RecordAttempt("login", subject, Rule, CancellationToken.None);
                Assert.NotNull(remaining);
                Assert.InRange(remaining.Value.TotalSeconds, 1, 30);
            }
        }
        finally
        {
            await Cleanup(database, subject);
        }
    }

    [RequiresEnvironmentFact(AuthVariable)]
    public async Task Concurrent_attempts_are_counted_atomically_and_lock_exactly_once_past_the_maximum()
    {
        await using var auth = new RedisAuthConnection(Environment.GetEnvironmentVariable(AuthVariable)!);
        var throttle = new RedisAuthAttemptThrottle(auth);
        var subject = NewSubject();
        var database = await auth.GetDatabase();
        try
        {
            var results = await Task.WhenAll(Enumerable.Range(0, 10).Select(_ => throttle.RecordAttempt("login", subject, Rule, CancellationToken.None)));

            Assert.Contains(results, lockedFor => lockedFor is not null);
            Assert.NotNull(await throttle.GetRemainingLock("login", subject, CancellationToken.None));
        }
        finally
        {
            await Cleanup(database, subject);
        }
    }

    [RequiresEnvironmentFact(AuthVariable)]
    public async Task An_unreachable_redis_fails_closed_with_the_auth_dependency_error()
    {
        await using var broken = new RedisAuthConnection("127.0.0.1:1,connectTimeout=300,abortConnect=false");
        var throttle = new RedisAuthAttemptThrottle(broken);

        await Assert.ThrowsAsync<AuthDependencyUnavailableException>(() => throttle.GetRemainingLock("login", NewSubject(), CancellationToken.None));
        await Assert.ThrowsAsync<AuthDependencyUnavailableException>(() => throttle.RecordAttempt("login", NewSubject(), Rule, CancellationToken.None));
    }

    private static async Task Cleanup(IDatabase database, string subject)
    {
        var suffix = "login:" + Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(subject))).ToLowerInvariant();
        await database.KeyDeleteAsync(["auth:throttle:" + suffix, "auth:lock:" + suffix]);
    }
}
