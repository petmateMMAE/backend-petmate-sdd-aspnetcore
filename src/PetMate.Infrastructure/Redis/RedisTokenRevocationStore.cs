using PetMate.Application.Security;
using StackExchange.Redis;

namespace PetMate.Infrastructure.Redis;

/// <summary>
/// Lista negra de <c>jti</c> en Redis de autenticación (clave <c>jwt:blacklist:{jti}</c>, TTL = vida restante del JWT).
/// Depende únicamente de <see cref="RedisAuthConnection"/>: el tipo impide usar Redis de caché por error.
/// Cualquier fallo de Redis se traduce a <see cref="AuthDependencyUnavailableException"/> (falla cerrada).
/// </summary>
public sealed class RedisTokenRevocationStore(RedisAuthConnection redis, TimeProvider timeProvider) : ITokenRevocationStore
{
    public const string KeyPrefix = "jwt:blacklist:";
    private const int MaxJtiLength = 64;
    private static readonly TimeSpan OperationTimeout = TimeSpan.FromSeconds(3);

    public async Task Revoke(string jti, DateTime expiresAtUtc, CancellationToken cancellationToken)
    {
        EnsureValid(jti);
        var remaining = expiresAtUtc - timeProvider.GetUtcNow().UtcDateTime;
        if (remaining <= TimeSpan.Zero)
        {
            // El JWT ya no puede aceptarse: no hace falta conservar la entrada.
            return;
        }

        await Execute(async () =>
        {
            var database = await redis.GetDatabase().ConfigureAwait(false);
            await database.StringSetAsync(KeyPrefix + jti, "1", remaining).ConfigureAwait(false);
        }, cancellationToken).ConfigureAwait(false);
    }

    public async Task<bool> IsRevoked(string jti, CancellationToken cancellationToken)
    {
        EnsureValid(jti);
        var revoked = false;
        await Execute(async () =>
        {
            var database = await redis.GetDatabase().ConfigureAwait(false);
            revoked = await database.KeyExistsAsync(KeyPrefix + jti).ConfigureAwait(false);
        }, cancellationToken).ConfigureAwait(false);
        return revoked;
    }

    private static void EnsureValid(string jti)
    {
        if (string.IsNullOrWhiteSpace(jti) || jti.Length > MaxJtiLength || !jti.All(c => char.IsAsciiLetterOrDigit(c) || c is '-' or '_'))
        {
            throw new ArgumentException("El identificador del token no es válido.", nameof(jti));
        }
    }

    private static async Task Execute(Func<Task> action, CancellationToken cancellationToken)
    {
        try
        {
            await action().WaitAsync(OperationTimeout, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception exception) when (exception is RedisException or TimeoutException)
        {
            throw new AuthDependencyUnavailableException(exception);
        }
    }
}
