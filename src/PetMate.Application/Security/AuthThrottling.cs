namespace PetMate.Application.Security;

/// <summary>Regla de una cuota: intentos permitidos en una ventana y duración del bloqueo temporal al superarla.</summary>
public sealed record AuthThrottleRule(int MaxAttempts, TimeSpan Window, TimeSpan LockDuration);

/// <summary>Reglas del 2026-11-08 (pm-034): se permiten 5 intentos fallidos de login por correo en 15 minutos y el sexto bloquea 15 minutos; se permiten 30 refrescos por usuario en 15 minutos y el 31.º bloquea.</summary>
public static class AuthThrottleRules
{
    public const string LoginScope = "login";
    public const string RefreshScope = "refresh";

    public static readonly AuthThrottleRule Login = new(5, TimeSpan.FromMinutes(15), TimeSpan.FromMinutes(15));

    public static readonly AuthThrottleRule Refresh = new(30, TimeSpan.FromMinutes(15), TimeSpan.FromMinutes(15));
}

/// <summary>
/// Cuota y bloqueo temporal independientes de la IP (pm-034), respaldados por Redis de autenticación para que valgan en todas
/// las réplicas. El sujeto (correo o identificador de usuario) se guarda solo como hash. Si Redis falla lanza
/// <see cref="AuthDependencyUnavailableException"/> (falla cerrada, igual que la lista negra de tokens).
/// </summary>
public interface IAuthAttemptThrottle
{
    /// <summary>Tiempo restante de bloqueo del sujeto, o <c>null</c> si no está bloqueado.</summary>
    Task<TimeSpan?> GetRemainingLock(string scope, string subject, CancellationToken cancellationToken);

    /// <summary>Cuenta un intento; si supera el máximo de la regla bloquea al sujeto y devuelve la duración del bloqueo.</summary>
    Task<TimeSpan?> RecordAttempt(string scope, string subject, AuthThrottleRule rule, CancellationToken cancellationToken);

    /// <summary>Borra el contador de intentos (por ejemplo tras un login correcto). No levanta un bloqueo ya activo.</summary>
    Task Clear(string scope, string subject, CancellationToken cancellationToken);
}

/// <summary>Resultado de ejecutar trabajo con el limitador de hashing: <see cref="Accepted"/> es <c>false</c> si había demasiado trabajo en cola.</summary>
public readonly record struct HashingOutcome<T>(bool Accepted, T? Value);

/// <summary>
/// Limita cuántos hashes Argon2id se calculan a la vez (pm-034 CA-03): el trabajo se ejecuta fuera del hilo de la solicitud, con
/// una concurrencia máxima y una cola corta; lo que no cabe se rechaza al instante en vez de agotar la CPU y la memoria.
/// </summary>
public interface IPasswordHashingLimiter
{
    Task<HashingOutcome<T>> Run<T>(Func<T> work, CancellationToken cancellationToken);
}

/// <summary>Implementación con semáforo: <c>maxConcurrency</c> hashes simultáneos y <c>maxQueue</c> solicitudes esperando.</summary>
public sealed class BoundedPasswordHashingLimiter : IPasswordHashingLimiter, IDisposable
{
    private readonly SemaphoreSlim slots;
    private readonly int maxQueue;
    private int waiting;

    public BoundedPasswordHashingLimiter(int maxConcurrency, int maxQueue)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(maxConcurrency, 1);
        ArgumentOutOfRangeException.ThrowIfNegative(maxQueue);
        slots = new SemaphoreSlim(maxConcurrency, maxConcurrency);
        this.maxQueue = maxQueue;
    }

    public async Task<HashingOutcome<T>> Run<T>(Func<T> work, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(work);

        if (!slots.Wait(0, CancellationToken.None))
        {
            if (Interlocked.Increment(ref waiting) > maxQueue)
            {
                Interlocked.Decrement(ref waiting);
                return new HashingOutcome<T>(false, default);
            }

            try
            {
                await slots.WaitAsync(cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                Interlocked.Decrement(ref waiting);
            }
        }

        // El hash corre en el pool y el slot se libera cuando TERMINA, no cuando el cliente cancela la espera: así un
        // cliente que corta la conexión no deja hashes en marcha sin ocupar cupo (pm-034 CA-03).
        Task<T> running;
        try
        {
            running = Task.Run(() =>
            {
                try
                {
                    return work();
                }
                finally
                {
                    slots.Release();
                }
            });
        }
        catch
        {
            slots.Release();
            throw;
        }

        return new HashingOutcome<T>(true, await running.WaitAsync(cancellationToken).ConfigureAwait(false));
    }

    public void Dispose() => slots.Dispose();
}
