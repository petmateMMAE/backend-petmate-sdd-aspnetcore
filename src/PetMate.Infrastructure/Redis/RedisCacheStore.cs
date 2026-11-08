using Microsoft.Extensions.Logging;
using PetMate.Application.Caching;
using StackExchange.Redis;

namespace PetMate.Infrastructure.Redis;

/// <summary>
/// Adaptador de <see cref="ICacheStore"/> sobre Redis Cache. Depende únicamente de <see cref="RedisCacheConnection"/>: el tipo impide
/// usar Redis de autenticación por error (pm-050 CA-01). Política de falla (CA-02/CA-03): cada operación tiene un tiempo máximo
/// corto y cualquier error o tiempo agotado se traduce a un fallo de caché sin propagar la excepción; solo se registra el tipo del
/// error, nunca la clave, el valor ni el mensaje.
/// </summary>
public sealed partial class RedisCacheStore(RedisCacheConnection redis, ILogger<RedisCacheStore> logger) : ICacheStore
{
    /// <summary>Tiempo máximo de cada operación: la caché nunca debe retrasar una solicitud de forma apreciable.</summary>
    public static readonly TimeSpan OperationTimeout = TimeSpan.FromMilliseconds(250);

    public async Task<string?> Read(CacheKey key, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(key);
        string? value = null;
        await Run(
            async () =>
            {
                var database = await redis.GetDatabase().ConfigureAwait(false);
                var stored = await database.StringGetAsync(key.Value).ConfigureAwait(false);
                value = stored.HasValue ? stored.ToString() : null;
            },
            cancellationToken).ConfigureAwait(false);
        return value;
    }

    public async Task<bool> Write(CacheKey key, string value, TimeSpan ttl, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(value);
        CacheTtl.EnsureValid(ttl);
        return await Run(
            async () =>
            {
                var database = await redis.GetDatabase().ConfigureAwait(false);
                await database.StringSetAsync(key.Value, value, ttl).ConfigureAwait(false);
            },
            cancellationToken).ConfigureAwait(false);
    }

    public async Task<bool> Remove(CacheKey key, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(key);
        return await Run(
            async () =>
            {
                var database = await redis.GetDatabase().ConfigureAwait(false);
                await database.KeyDeleteAsync(key.Value).ConfigureAwait(false);
            },
            cancellationToken).ConfigureAwait(false);
    }

    private async Task<bool> Run(Func<Task> operation, CancellationToken cancellationToken)
    {
        try
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(OperationTimeout);
            await operation().WaitAsync(timeout.Token).ConfigureAwait(false);
            return true;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception) when (exception is not OutOfMemoryException)
        {
            LogCacheUnavailable(logger, exception.GetType().Name);
            return false;
        }
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Caché no disponible; se continúa sin caché ({ErrorType}).")]
    private static partial void LogCacheUnavailable(ILogger logger, string errorType);
}
