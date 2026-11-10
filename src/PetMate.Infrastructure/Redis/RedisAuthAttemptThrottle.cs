using System.Security.Cryptography;
using System.Text;
using PetMate.Application.Security;
using StackExchange.Redis;

namespace PetMate.Infrastructure.Redis;

/// <summary>
/// Cuota y bloqueo temporal por sujeto en Redis de autenticación (pm-034). Claves <c>auth:throttle:{ámbito}:{sha256(sujeto)}</c>
/// (contador con TTL de la ventana) y <c>auth:lock:{ámbito}:{sha256(sujeto)}</c> (bloqueo con TTL); el correo nunca se guarda en
/// claro. El conteo es atómico (script Lua) para que réplicas concurrentes no se pisen. Cualquier fallo de Redis se traduce a
/// <see cref="AuthDependencyUnavailableException"/> (falla cerrada).
/// </summary>
public sealed class RedisAuthAttemptThrottle(RedisAuthConnection redis) : IAuthAttemptThrottle
{
    private const string CounterPrefix = "auth:throttle:";
    private const string LockPrefix = "auth:lock:";
    private static readonly TimeSpan OperationTimeout = TimeSpan.FromSeconds(3);

    // KEYS[1] contador, KEYS[2] bloqueo; ARGV[1] ventana (ms), ARGV[2] bloqueo (ms), ARGV[3] máximo de intentos.
    // Devuelve los milisegundos de bloqueo restantes (> 0) o 0 si el intento está permitido. Si ya hay un bloqueo no cuenta:
    // así una ráfaga que pasó la comprobación previa no reinicia el contador ni consigue más intentos.
    private const string RecordScript = """
        local remaining = redis.call('PTTL', KEYS[2])
        if remaining > 0 then return remaining end
        local n = redis.call('INCR', KEYS[1])
        if n == 1 then redis.call('PEXPIRE', KEYS[1], ARGV[1]) end
        if n > tonumber(ARGV[3]) then
          redis.call('SET', KEYS[2], '1', 'PX', ARGV[2])
          redis.call('DEL', KEYS[1])
          return tonumber(ARGV[2])
        end
        return 0
        """;

    public async Task<TimeSpan?> GetRemainingLock(string scope, string subject, CancellationToken cancellationToken)
    {
        var key = LockPrefix + KeySuffix(scope, subject);
        TimeSpan? remaining = null;
        await Execute(async () =>
        {
            var database = await redis.GetDatabase().ConfigureAwait(false);
            remaining = await database.KeyTimeToLiveAsync(key).ConfigureAwait(false);
        }, cancellationToken).ConfigureAwait(false);
        return remaining;
    }

    public async Task<TimeSpan?> RecordAttempt(string scope, string subject, AuthThrottleRule rule, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(rule);
        var suffix = KeySuffix(scope, subject);
        long lockedMilliseconds = 0;
        await Execute(async () =>
        {
            var database = await redis.GetDatabase().ConfigureAwait(false);
            var result = await database.ScriptEvaluateAsync(
                RecordScript,
                [new RedisKey(CounterPrefix + suffix), new RedisKey(LockPrefix + suffix)],
                [(long)rule.Window.TotalMilliseconds, (long)rule.LockDuration.TotalMilliseconds, rule.MaxAttempts]).ConfigureAwait(false);
            lockedMilliseconds = (long)result;
        }, cancellationToken).ConfigureAwait(false);
        return lockedMilliseconds > 0 ? TimeSpan.FromMilliseconds(lockedMilliseconds) : null;
    }

    public async Task Clear(string scope, string subject, CancellationToken cancellationToken)
    {
        var key = CounterPrefix + KeySuffix(scope, subject);
        await Execute(async () =>
        {
            var database = await redis.GetDatabase().ConfigureAwait(false);
            await database.KeyDeleteAsync(key).ConfigureAwait(false);
        }, cancellationToken).ConfigureAwait(false);
    }

    private static string KeySuffix(string scope, string subject)
    {
        if (string.IsNullOrWhiteSpace(scope) || scope.Length > 16 || !scope.All(c => char.IsAsciiLetterLower(c) || c == '-'))
        {
            throw new ArgumentException("El ámbito no es válido.", nameof(scope));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(subject);
        return scope + ":" + Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(subject))).ToLowerInvariant();
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
