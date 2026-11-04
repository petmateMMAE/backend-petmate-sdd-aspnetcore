using Microsoft.Extensions.Diagnostics.HealthChecks;
using PetMate.Infrastructure.Health;
using PetMate.Infrastructure.Redis;
using StackExchange.Redis;

namespace PetMate.IntegrationTests.Redis;

/// <summary>
/// Pruebas contra los dos Redis de Docker Compose. Se omiten (Skipped, no aprobadas) si
/// <c>PETMATE_IT_REDIS_AUTH</c> / <c>PETMATE_IT_REDIS_CACHE</c> no están definidas.
/// </summary>
public sealed class RedisIntegrationTests
{
    private const string AuthVariable = "PETMATE_IT_REDIS_AUTH";
    private const string CacheVariable = "PETMATE_IT_REDIS_CACHE";
    private const string Prefix = "petmate-it:";

    [RequiresEnvironmentFact(AuthVariable, CacheVariable)]
    public async Task Auth_and_cache_are_independent_instances()
    {
        await using var auth = new RedisAuthConnection(AuthConnection());
        await using var cache = new RedisCacheConnection(CacheConnection());
        var key = Prefix + Guid.NewGuid().ToString("N");
        var authDb = await auth.GetDatabase();
        var cacheDb = await cache.GetDatabase();
        try
        {
            await authDb.StringSetAsync(key, "auth-only", TimeSpan.FromSeconds(30));

            Assert.Equal("auth-only", (string?)await authDb.StringGetAsync(key));
            Assert.False(await cacheDb.KeyExistsAsync(key));
        }
        finally
        {
            await authDb.KeyDeleteAsync(key);
        }
    }

    [RequiresEnvironmentFact(AuthVariable)]
    public async Task Auth_blacklist_key_is_written_with_ttl_and_expires()
    {
        await using var auth = new RedisAuthConnection(AuthConnection());
        var database = await auth.GetDatabase();
        var key = $"{Prefix}jwt:blacklist:{Guid.NewGuid():N}";
        try
        {
            await database.StringSetAsync(key, "1", TimeSpan.FromSeconds(1));

            Assert.NotNull((await database.KeyTimeToLiveAsync(key)));
            await Task.Delay(TimeSpan.FromSeconds(1.5));
            Assert.False(await database.KeyExistsAsync(key));
        }
        finally
        {
            await database.KeyDeleteAsync(key);
        }
    }

    [RequiresEnvironmentFact(CacheVariable)]
    public async Task Cache_supports_write_read_and_expiration()
    {
        await using var cache = new RedisCacheConnection(CacheConnection());
        var database = await cache.GetDatabase();
        var key = Prefix + Guid.NewGuid().ToString("N");
        try
        {
            await database.StringSetAsync(key, "valor", TimeSpan.FromSeconds(1));
            Assert.Equal("valor", (string?)await database.StringGetAsync(key));

            await Task.Delay(TimeSpan.FromSeconds(1.5));
            Assert.False(await database.KeyExistsAsync(key));
        }
        finally
        {
            await database.KeyDeleteAsync(key);
        }
    }

    [RequiresEnvironmentFact(AuthVariable, CacheVariable)]
    public async Task Memory_policies_match_responsibilities()
    {
        Assert.Equal("noeviction", await Policy(AuthConnection()));
        Assert.Equal("allkeys-lru", await Policy(CacheConnection()));
    }

    [RequiresEnvironmentFact(AuthVariable, CacheVariable)]
    public async Task Health_checks_report_healthy_for_reachable_instances()
    {
        await using var auth = new RedisAuthConnection(AuthConnection());
        await using var cache = new RedisCacheConnection(CacheConnection());

        var authResult = await new RedisHealthCheck<RedisAuthConnection>(auth).CheckHealthAsync(new HealthCheckContext());
        var cacheResult = await new RedisHealthCheck<RedisCacheConnection>(cache).CheckHealthAsync(new HealthCheckContext());

        Assert.Equal(HealthStatus.Healthy, authResult.Status);
        Assert.Equal(HealthStatus.Healthy, cacheResult.Status);
    }

    [Fact]
    public async Task Health_check_reports_unhealthy_without_details_when_redis_is_unreachable()
    {
        await using var unreachable = new RedisAuthConnection("127.0.0.1:1,password=x,connectTimeout=300,abortConnect=false");

        var result = await new RedisHealthCheck<RedisAuthConnection>(unreachable).CheckHealthAsync(new HealthCheckContext());

        Assert.Equal(HealthStatus.Unhealthy, result.Status);
        Assert.Null(result.Exception);
        Assert.True(string.IsNullOrEmpty(result.Description));
    }

    private static string AuthConnection() => Environment.GetEnvironmentVariable(AuthVariable)!;

    private static string CacheConnection() => Environment.GetEnvironmentVariable(CacheVariable)!;

    private static async Task<string?> Policy(string connection)
    {
        var options = ConfigurationOptions.Parse(connection);
        options.AllowAdmin = true;
        await using var mux = await ConnectionMultiplexer.ConnectAsync(options);
        var server = mux.GetServer(mux.GetEndPoints()[0]);
        var config = await server.ConfigGetAsync("maxmemory-policy");
        return config.Single().Value;
    }
}
