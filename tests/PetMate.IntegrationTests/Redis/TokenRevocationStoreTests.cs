using PetMate.Application.Security;
using PetMate.Infrastructure.Redis;

namespace PetMate.IntegrationTests.Redis;

/// <summary>Revocación de JWT contra el Redis de autenticación de Compose. Skipped si faltan las variables de Redis.</summary>
public sealed class TokenRevocationStoreTests
{
    private const string AuthVariable = "PETMATE_IT_REDIS_AUTH";
    private const string CacheVariable = "PETMATE_IT_REDIS_CACHE";

    private static string NewJti() => "it-" + Guid.NewGuid().ToString("N");

    [RequiresEnvironmentFact(AuthVariable, CacheVariable)]
    public async Task A_revoked_session_is_reported_revoked_with_a_ttl_and_never_reaches_the_cache_redis()
    {
        await using var auth = new RedisAuthConnection(Environment.GetEnvironmentVariable(AuthVariable)!);
        await using var cache = new RedisCacheConnection(Environment.GetEnvironmentVariable(CacheVariable)!);
        var store = new RedisTokenRevocationStore(auth, TimeProvider.System);
        var sessionId = Guid.NewGuid();
        var key = RedisTokenRevocationStore.SessionKeyPrefix + sessionId.ToString("N");
        var database = await auth.GetDatabase();
        try
        {
            Assert.False(await store.IsSessionRevoked(sessionId, CancellationToken.None));

            await store.RevokeSession(sessionId, DateTime.UtcNow.AddMinutes(10), CancellationToken.None);

            Assert.True(await store.IsSessionRevoked(sessionId, CancellationToken.None));
            Assert.False(await store.IsSessionRevoked(Guid.NewGuid(), CancellationToken.None));
            var ttl = await database.KeyTimeToLiveAsync(key);
            Assert.NotNull(ttl);
            Assert.InRange(ttl.Value.TotalSeconds, 590, 600);
            Assert.False(await (await cache.GetDatabase()).KeyExistsAsync(key));
        }
        finally
        {
            await database.KeyDeleteAsync(key);
        }
    }

    [RequiresEnvironmentFact(AuthVariable, CacheVariable)]
    public async Task Revoked_jti_is_reported_revoked_with_ttl_equal_to_the_remaining_lifetime_and_never_reaches_the_cache_redis()
    {
        await using var auth = new RedisAuthConnection(Environment.GetEnvironmentVariable(AuthVariable)!);
        await using var cache = new RedisCacheConnection(Environment.GetEnvironmentVariable(CacheVariable)!);
        var store = new RedisTokenRevocationStore(auth, TimeProvider.System);
        var jti = NewJti();
        var key = RedisTokenRevocationStore.KeyPrefix + jti;
        var database = await auth.GetDatabase();
        try
        {
            Assert.False(await store.IsRevoked(jti, CancellationToken.None));

            await store.Revoke(jti, DateTime.UtcNow.AddMinutes(10), CancellationToken.None);

            Assert.True(await store.IsRevoked(jti, CancellationToken.None));
            var ttl = await database.KeyTimeToLiveAsync(key);
            Assert.NotNull(ttl);
            Assert.InRange(ttl.Value.TotalSeconds, 590, 600);
            Assert.False(await (await cache.GetDatabase()).KeyExistsAsync(key));
        }
        finally
        {
            await database.KeyDeleteAsync(key);
        }
    }

    [RequiresEnvironmentFact(AuthVariable)]
    public async Task Revocation_expires_automatically_when_the_jwt_would_have_expired()
    {
        await using var auth = new RedisAuthConnection(Environment.GetEnvironmentVariable(AuthVariable)!);
        var store = new RedisTokenRevocationStore(auth, TimeProvider.System);
        var jti = NewJti();
        try
        {
            await store.Revoke(jti, DateTime.UtcNow.AddSeconds(2), CancellationToken.None);
            Assert.True(await store.IsRevoked(jti, CancellationToken.None));

            await Task.Delay(TimeSpan.FromSeconds(3.2));

            Assert.False(await store.IsRevoked(jti, CancellationToken.None));
        }
        finally
        {
            await (await auth.GetDatabase()).KeyDeleteAsync(RedisTokenRevocationStore.KeyPrefix + jti);
        }
    }

    [RequiresEnvironmentFact(AuthVariable)]
    public async Task Already_expired_tokens_are_not_stored()
    {
        await using var auth = new RedisAuthConnection(Environment.GetEnvironmentVariable(AuthVariable)!);
        var store = new RedisTokenRevocationStore(auth, TimeProvider.System);
        var jti = NewJti();

        await store.Revoke(jti, DateTime.UtcNow.AddSeconds(-5), CancellationToken.None);

        Assert.False(await (await auth.GetDatabase()).KeyExistsAsync(RedisTokenRevocationStore.KeyPrefix + jti));
    }

    [Fact]
    public async Task Redis_outage_fails_closed_for_both_reads_and_writes()
    {
        await using var unreachable = new RedisAuthConnection("127.0.0.1:1,password=x,connectTimeout=300,abortConnect=false");
        var store = new RedisTokenRevocationStore(unreachable, TimeProvider.System);

        var read = await Assert.ThrowsAsync<AuthDependencyUnavailableException>(() => store.IsRevoked(NewJti(), CancellationToken.None));
        var write = await Assert.ThrowsAsync<AuthDependencyUnavailableException>(
            () => store.Revoke(NewJti(), DateTime.UtcNow.AddMinutes(5), CancellationToken.None));

        Assert.DoesNotContain("127.0.0.1", read.Message, StringComparison.Ordinal);
        Assert.DoesNotContain("127.0.0.1", write.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Store_depends_only_on_the_auth_redis_connection_never_on_the_cache()
    {
        var parameterTypes = typeof(RedisTokenRevocationStore).GetConstructors().Single().GetParameters().Select(p => p.ParameterType).ToArray();

        Assert.Contains(typeof(RedisAuthConnection), parameterTypes);
        Assert.DoesNotContain(typeof(RedisCacheConnection), parameterTypes);
    }

    [Theory]
    [InlineData("")]
    [InlineData("con espacio")]
    [InlineData("a:b")]
    public async Task Invalid_jti_values_are_rejected_before_touching_redis(string jti)
    {
        await using var unreachable = new RedisAuthConnection("127.0.0.1:1,password=x,connectTimeout=300,abortConnect=false");
        var store = new RedisTokenRevocationStore(unreachable, TimeProvider.System);

        await Assert.ThrowsAsync<ArgumentException>(() => store.IsRevoked(jti, CancellationToken.None));
        await Assert.ThrowsAsync<ArgumentException>(() => store.Revoke(jti, DateTime.UtcNow.AddMinutes(1), CancellationToken.None));
    }
}
