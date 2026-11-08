using PetMate.Application.Mediator;
using PetMate.Application.Persistence;
using PetMate.Application.Security;
using PetMate.Domain.Users;

namespace PetMate.Application.Authentication;

/// <summary>
/// Login. Correo inexistente y contraseña incorrecta devuelven el mismo error y consumen un Argon2id (contra un hash
/// ficticio calculado con los parámetros vigentes) para no filtrar la existencia de la cuenta por tiempo.
/// El modelo de usuario aprobado no tiene estado de cuenta: no hay comprobación de activación.
/// </summary>
public sealed class LoginHandler : IRequestHandler<LoginCommand, AuthResult<AuthTokensResult>>
{
    private readonly IUserRepository users;
    private readonly IPasswordHasher hasher;
    private readonly AuthTokenIssuer issuer;
    private readonly TimeProvider timeProvider;
    private readonly string dummyHash;
    private readonly IAuthAttemptThrottle? throttle;
    private readonly IPasswordHashingLimiter? limiter;

    public LoginHandler(
        IUserRepository users,
        IPasswordHasher hasher,
        AuthTokenIssuer issuer,
        TimeProvider timeProvider,
        IAuthAttemptThrottle? throttle = null,
        IPasswordHashingLimiter? limiter = null)
    {
        this.users = users;
        this.hasher = hasher;
        this.issuer = issuer;
        this.timeProvider = timeProvider;
        this.throttle = throttle;
        this.limiter = limiter;
        dummyHash = hasher.Hash(Guid.NewGuid().ToString("N") + "-sin-cuenta");
    }

    public async Task<AuthResult<AuthTokensResult>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var email = Email.Create(request.Email);

        // Bloqueo temporal por correo (pm-034): se decide antes de buscar la cuenta ni calcular hashes, y es idéntico para
        // correos existentes e inexistentes, así que no revela cuál es cuál. El intento se cuenta ANTES de verificar la
        // contraseña (el conteo es atómico en Redis): una ráfaga concurrente no puede evaluar más contraseñas que el máximo.
        // Un login correcto borra el contador, así que en la práctica solo cuentan los fallos.
        if (throttle is not null)
        {
            if (await throttle.GetRemainingLock(AuthThrottleRules.LoginScope, email.Value, cancellationToken).ConfigureAwait(false) is { } remaining)
            {
                return AuthResult.Throttled<AuthTokensResult>(remaining);
            }

            if (await throttle.RecordAttempt(AuthThrottleRules.LoginScope, email.Value, AuthThrottleRules.Login, cancellationToken).ConfigureAwait(false) is { } lockedFor)
            {
                return AuthResult.Throttled<AuthTokensResult>(lockedFor);
            }
        }

        var user = await users.FindByEmail(email, cancellationToken).ConfigureAwait(false);
        var hashToCheck = user is null ? dummyHash : user.PasswordHash.Value;
        var checkedHash = await Limited(() => hasher.Verify(request.Password, hashToCheck), cancellationToken).ConfigureAwait(false);
        if (!checkedHash.Accepted)
        {
            return AuthResult.Throttled<AuthTokensResult>(TimeSpan.FromSeconds(1));
        }

        if (user is null || checkedHash.Value == PasswordVerificationResult.Failed)
        {
            return AuthResult.Failure<AuthTokensResult>(AuthError.InvalidCredentials);
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
        if (checkedHash.Value == PasswordVerificationResult.SuccessRehashNeeded)
        {
            var rehash = await Limited(() => hasher.Hash(request.Password), cancellationToken).ConfigureAwait(false);
            if (!rehash.Accepted)
            {
                return AuthResult.Throttled<AuthTokensResult>(TimeSpan.FromSeconds(1));
            }

            user.ChangePasswordHash(PasswordHash.Create(rehash.Value!), now);
            await users.UpdatePasswordHash(user, cancellationToken).ConfigureAwait(false);
        }

        if (throttle is not null)
        {
            await throttle.Clear(AuthThrottleRules.LoginScope, email.Value, cancellationToken).ConfigureAwait(false);
        }

        return AuthResult.Success(await issuer.StartSession(user, now, cancellationToken).ConfigureAwait(false));
    }

    private async Task<HashingOutcome<T>> Limited<T>(Func<T> work, CancellationToken cancellationToken)
        => limiter is null ? new HashingOutcome<T>(true, work()) : await limiter.Run(work, cancellationToken).ConfigureAwait(false);
}
