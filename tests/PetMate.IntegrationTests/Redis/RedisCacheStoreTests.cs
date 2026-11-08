using Microsoft.Extensions.Logging.Abstractions;
using PetMate.Application.Caching;
using PetMate.Infrastructure.Redis;

namespace PetMate.IntegrationTests.Redis;

/// <summary>Adaptador de caché sobre Redis Cache (pm-050): falla segura sin Redis y operación real si hay un Redis de pruebas.</summary>
public sealed class RedisCacheStoreTests
{
    private const string CacheVariable = "PETMATE_IT_REDIS_CACHE";

    [Fact]
    public async Task When_redis_cache_is_unreachable_every_operation_degrades_to_a_miss_without_throwing_and_quickly()
    {
        await using var connection = new RedisCacheConnection("host-inexistente.invalid:6399,connectTimeout=200,password=no-se-usa");
        var store = new RedisCacheStore(connection, NullLogger<RedisCacheStore>.Instance);
        var key = CacheKey.Create("catalog", "sin-redis");
        var started = DateTime.UtcNow;

        var value = await store.Read(key, CancellationToken.None);
        var stored = await store.Write(key, "valor", TimeSpan.FromSeconds(30), CancellationToken.None);
        var removed = await store.Remove(key, CancellationToken.None);

        Assert.Null(value);
        Assert.False(stored);
        Assert.False(removed);
        Assert.True(DateTime.UtcNow - started < TimeSpan.FromSeconds(5), "La caché caída no debe retrasar la solicitud.");
    }

    [Fact]
    public async Task A_cancellation_requested_by_the_caller_is_still_propagated()
    {
        await using var connection = new RedisCacheConnection("host-inexistente.invalid:6399,connectTimeout=200,password=no-se-usa");
        var store = new RedisCacheStore(connection, NullLogger<RedisCacheStore>.Instance);
        using var cancelled = new CancellationTokenSource();
        await cancelled.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => store.Read(CacheKey.Create("catalog", "x"), cancelled.Token));
    }

    [RequiresEnvironmentFact(CacheVariable)]
    public async Task Set_get_and_remove_use_the_convention_with_a_ttl()
    {
        await using var connection = new RedisCacheConnection(Environment.GetEnvironmentVariable(CacheVariable)!);
        var store = new RedisCacheStore(connection, NullLogger<RedisCacheStore>.Instance);
        var key = CacheKey.Create("it-catalog", Guid.NewGuid().ToString());
        try
        {
            Assert.Null(await store.Read(key, CancellationToken.None));
            Assert.True(await store.Write(key, "{\"nombre\":\"valor\"}", TimeSpan.FromSeconds(30), CancellationToken.None));
            Assert.Equal("{\"nombre\":\"valor\"}", await store.Read(key, CancellationToken.None));

            var database = await connection.GetDatabase();
            var remaining = await database.KeyTimeToLiveAsync(key.Value);
            Assert.NotNull(remaining);
            Assert.InRange(remaining.Value.TotalSeconds, 1, 30);

            Assert.True(await store.Remove(key, CancellationToken.None));
            Assert.Null(await store.Read(key, CancellationToken.None));
        }
        finally
        {
            var database = await connection.GetDatabase();
            await database.KeyDeleteAsync(key.Value);
        }
    }
}
