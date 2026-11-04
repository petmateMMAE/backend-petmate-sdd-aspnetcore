using Microsoft.Extensions.Diagnostics.HealthChecks;
using MongoDB.Bson;
using MongoDB.Driver;
using PetMate.Infrastructure.Redis;

namespace PetMate.Infrastructure.Health;

/// <summary>Nombres y etiquetas de los health checks de dependencias.</summary>
public static class HealthCheckNames
{
    public const string Mongo = "mongodb";
    public const string RedisAuth = "redis-auth";
    public const string RedisCache = "redis-cache";
    public const string ReadyTag = "ready";
}

internal static class HealthTimeouts
{
    public static readonly TimeSpan Probe = TimeSpan.FromSeconds(3);
}

/// <summary>
/// Hace <c>ping</c> a MongoDB. El resultado es Healthy solo si el servidor respondió; cualquier fallo se
/// reporta como Unhealthy sin detalles (ni excepción, ni host, ni URI).
/// </summary>
public sealed class MongoHealthCheck(IMongoDatabase database) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(HealthTimeouts.Probe);

        try
        {
            await database.RunCommandAsync<BsonDocument>(new BsonDocument("ping", 1), cancellationToken: timeout.Token)
                .ConfigureAwait(false);
            return HealthCheckResult.Healthy();
        }
        catch (Exception exception) when (exception is not OutOfMemoryException)
        {
            return HealthCheckResult.Unhealthy();
        }
    }
}

/// <summary>Hace <c>PING</c> a una instancia Redis concreta; sin detalles en caso de fallo.</summary>
public sealed class RedisHealthCheck<TConnection>(TConnection connection) : IHealthCheck
    where TConnection : RedisConnection
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var probe = Task.Run(async () =>
            {
                var database = await connection.GetDatabase().ConfigureAwait(false);
                return await database.PingAsync().ConfigureAwait(false);
            }, cancellationToken);

            await probe.WaitAsync(HealthTimeouts.Probe, cancellationToken).ConfigureAwait(false);
            return HealthCheckResult.Healthy();
        }
        catch (Exception exception) when (exception is not OutOfMemoryException)
        {
            return HealthCheckResult.Unhealthy();
        }
    }
}
