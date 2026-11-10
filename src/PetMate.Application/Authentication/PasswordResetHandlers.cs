using PetMate.Application.Mediator;
using PetMate.Application.Persistence;
using PetMate.Application.Security;
using PetMate.Application.Validation;
using PetMate.Domain.Sessions;
using PetMate.Domain.Users;

namespace PetMate.Application.Authentication;

/// <summary>Envío de correos transaccionales (pm-088). Los casos de uso dependen de este puerto, no del proveedor.</summary>
public interface IEmailSender
{
    /// <summary>
    /// Envía el enlace de restablecimiento. Nunca lanza: un fallo o una espera agotada del proveedor devuelve <c>false</c> y el
    /// llamador no lo revela al cliente (pm-088#CA-03). El token en claro solo viaja al correo; nunca se registra.
    /// </summary>
    Task<bool> SendPasswordReset(string toEmail, string resetToken, CancellationToken cancellationToken);
}

/// <summary>Persistencia de los tokens de restablecimiento (solo hashes).</summary>
public interface IPasswordResetRepository
{
    Task Add(PasswordResetToken token, CancellationToken cancellationToken);

    /// <summary>
    /// Marca el token como usado solo si existe, no se usó y no venció, y devuelve el usuario. Es atómico: dos usos simultáneos
    /// del mismo token tienen un único ganador. Devuelve <c>null</c> en cualquier otro caso (mismo resultado para todos).
    /// </summary>
    Task<Guid?> TryConsume(RefreshTokenHash tokenHash, DateTime nowUtc, CancellationToken cancellationToken);

    /// <summary>Invalida los tokens pendientes del usuario (se usan al emitir uno nuevo y al terminar un restablecimiento).</summary>
    Task InvalidateAllForUser(Guid userId, DateTime nowUtc, CancellationToken cancellationToken);
}

// Los comandos transportan secretos (token, contraseña): su ToString nunca imprime valores.
public sealed record ForgotPasswordCommand(string Email) : ICommand<AuthResult<Unit>>
{
    public override string ToString() => nameof(ForgotPasswordCommand);
}

public sealed record ResetPasswordCommand(string Token, string NewPassword) : ICommand<AuthResult<Unit>>
{
    public override string ToString() => nameof(ResetPasswordCommand);
}

/// <summary>
/// pm-089: responde igual exista o no el correo. Cada solicitud cuenta en la cuota por correo (3 en 15 minutos, 15 de espera),
/// exista o no la cuenta; el límite por IP lo aplica el filtro de la ruta. Para una cuenta existente guarda el hash de un token
/// aleatorio de un solo uso (1 hora) y envía el enlace; si el envío falla no se informa al cliente.
/// </summary>
public sealed class ForgotPasswordHandler(
    IUserRepository users,
    IPasswordResetRepository resets,
    IRefreshTokenFactory tokens,
    IEmailSender emailSender,
    IAuthAttemptThrottle throttle,
    TimeProvider timeProvider) : IRequestHandler<ForgotPasswordCommand, AuthResult<Unit>>
{
    public async Task<AuthResult<Unit>> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var address = Domain.Users.Email.Create(request.Email);

        var wait = await throttle.GetRemainingLock(AuthThrottleRules.ForgotPasswordScope, address.Value, cancellationToken).ConfigureAwait(false)
            ?? await throttle.RecordAttempt(AuthThrottleRules.ForgotPasswordScope, address.Value, AuthThrottleRules.ForgotPassword, cancellationToken).ConfigureAwait(false);
        if (wait is { } remaining)
        {
            return AuthResult.Throttled<Unit>(remaining);
        }

        var user = await users.FindByEmail(address, cancellationToken).ConfigureAwait(false);
        if (user is not null)
        {
            try
            {
                // Solo el enlace más reciente sirve: los pendientes anteriores se invalidan.
                var now = timeProvider.GetUtcNow().UtcDateTime;
                var material = tokens.Create();
                await resets.InvalidateAllForUser(user.Id, now, cancellationToken).ConfigureAwait(false);
                await resets.Add(PasswordResetToken.Issue(user.Id, material.Hash, now), cancellationToken).ConfigureAwait(false);
                _ = await emailSender.SendPasswordReset(user.Email.Value, material.RawToken, cancellationToken).ConfigureAwait(false);
            }
            catch (PersistenceUnavailableException)
            {
                // Un fallo de Atlas solo ocurre con cuentas existentes: responder 503 delataría que el correo existe (pm-089#CA-01).
            }
        }

        return AuthResult.Success(Unit.Value);
    }
}

/// <summary>
/// pm-090: fija la contraseña nueva con un token vigente. El token se consume de forma atómica; uno vencido, usado o inexistente
/// da el mismo error. Al cambiar la contraseña se revocan todas las sesiones de la cuenta y sus access tokens vigentes.
/// </summary>
public sealed class ResetPasswordHandler(
    IUserRepository users,
    IPasswordResetRepository resets,
    ISessionRepository sessions,
    IPasswordHasher hasher,
    IRefreshTokenFactory tokens,
    IAccessTokenService accessTokens,
    TimeProvider timeProvider,
    ITokenRevocationStore? revocations = null,
    IPasswordHashingLimiter? hashingLimiter = null) : IRequestHandler<ResetPasswordCommand, AuthResult<Unit>>
{
    public async Task<AuthResult<Unit>> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var invalid = AuthResult.Failure<Unit>(AuthError.InvalidResetToken);

        if (!tokens.TryHash(request.Token, out var hash) || hash is null)
        {
            return invalid;
        }

        // El hash de la contraseña se calcula antes de tocar el token para no variar el tiempo según si el token era válido.
        string rawHash;
        if (hashingLimiter is null)
        {
            rawHash = hasher.Hash(request.NewPassword);
        }
        else
        {
            var hashed = await hashingLimiter.Run(() => hasher.Hash(request.NewPassword), cancellationToken).ConfigureAwait(false);
            if (!hashed.Accepted)
            {
                return AuthResult.Throttled<Unit>(TimeSpan.FromSeconds(1));
            }

            rawHash = hashed.Value!;
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
        if (await resets.TryConsume(hash, now, cancellationToken).ConfigureAwait(false) is not { } userId)
        {
            return invalid;
        }

        var user = await users.FindById(userId, cancellationToken).ConfigureAwait(false);
        if (user is null)
        {
            return invalid;
        }

        // pm-090#CA-03: primero se cierran todas las sesiones y se revocan sus access tokens (H7) y después se cambia la
        // contraseña. Si algo falla a mitad, nadie queda con una sesión viva tras un cambio que no llegó a aplicarse; el
        // usuario solo debe pedir otro enlace.
        var revoked = await sessions.RevokeAllForUser(user.Id, now, SessionRevocationReason.PasswordReset, cancellationToken).ConfigureAwait(false);
        if (revocations is not null)
        {
            foreach (var sessionId in revoked)
            {
                await revocations.RevokeSession(sessionId, now + accessTokens.Lifetime, cancellationToken).ConfigureAwait(false);
            }
        }

        user.ChangePasswordHash(PasswordHash.Create(rawHash), now);
        await users.UpdatePasswordHash(user, cancellationToken).ConfigureAwait(false);

        // Los demás enlaces pendientes de la cuenta dejan de servir.
        await resets.InvalidateAllForUser(user.Id, now, cancellationToken).ConfigureAwait(false);

        return AuthResult.Success(Unit.Value);
    }
}

public sealed class ForgotPasswordCommandValidator : IRequestValidator<ForgotPasswordCommand>
{
    public Task<IReadOnlyList<ValidationFailure>> Validate(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var failures = new List<ValidationFailure>();
        AuthValidation.CheckEmail(request.Email, failures);
        return AuthValidation.Result(failures);
    }
}

/// <summary>Misma política de contraseña que el registro: de 12 a 128 caracteres.</summary>
public sealed class ResetPasswordCommandValidator : IRequestValidator<ResetPasswordCommand>
{
    public Task<IReadOnlyList<ValidationFailure>> Validate(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var failures = new List<ValidationFailure>();
        if (string.IsNullOrWhiteSpace(request.Token))
        {
            failures.Add(new ValidationFailure("token", "El token es obligatorio."));
        }

        if (string.IsNullOrWhiteSpace(request.NewPassword))
        {
            failures.Add(new ValidationFailure("newPassword", "La contraseña es obligatoria."));
        }
        else if (request.NewPassword.Length < AuthValidation.MinPasswordLength)
        {
            failures.Add(new ValidationFailure("newPassword", $"La contraseña debe tener al menos {AuthValidation.MinPasswordLength} caracteres."));
        }
        else if (request.NewPassword.Length > AuthValidation.MaxPasswordLength)
        {
            failures.Add(new ValidationFailure("newPassword", $"La contraseña no debe exceder {AuthValidation.MaxPasswordLength} caracteres."));
        }

        return AuthValidation.Result(failures);
    }
}
