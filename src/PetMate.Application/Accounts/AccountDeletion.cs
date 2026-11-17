using PetMate.Application.Access;
using PetMate.Application.Authentication;
using PetMate.Application.Businesses;
using PetMate.Application.Devices;
using PetMate.Application.Mediator;
using PetMate.Application.Notifications;
using PetMate.Application.Persistence;
using PetMate.Application.Pets;
using PetMate.Application.Security;
using PetMate.Application.Validation;
using PetMate.Domain.Sessions;
using PetMate.Domain.Users;

namespace PetMate.Application.Accounts;

/// <summary>
/// Borra una cuenta y todo lo suyo (pm-161 a pm-163). El orden importa: primero se cierran las sesiones y se revocan los access
/// tokens, después se borran los datos y la cuenta va al final, de modo que un fallo a mitad deja la cuenta localizable y repetir
/// la eliminación termina el trabajo. Cada paso es idempotente.
/// </summary>
public sealed class AccountEraser(
    IUserRepository users,
    ISessionRepository sessions,
    IPasswordResetRepository resets,
    IAccountDeletionTokenRepository deletionTokens,
    IAccessTokenService accessTokens,
    IPetRepository pets,
    IRequestHandler<DeleteMyPetCommand, bool> deletePet,
    IBusinessRepository businesses,
    IRequestHandler<DeleteBusinessCommand, BusinessStatus> deleteBusiness,
    IAccessGrantRepository grants,
    IScanLogRepository scans,
    IDeviceRepository devices,
    INotificationSettingsRepository settings,
    TimeProvider timeProvider,
    ITokenRevocationStore? revocations = null)
{
    public async Task Erase(Guid userId, CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;

        // pm-161#CA-02: ninguna sesión ni access token de la cuenta sigue sirviendo.
        var revoked = await sessions.RevokeAllForUser(userId, now, SessionRevocationReason.AccountDeleted, cancellationToken).ConfigureAwait(false);
        if (revocations is not null)
        {
            foreach (var sessionId in revoked)
            {
                await revocations.RevokeSession(sessionId, now + accessTokens.Lifetime, cancellationToken).ConfigureAwait(false);
            }
        }

        // pm-162: mascotas y todo lo asociado (carnet, QR, autorizaciones, escaneos, conversaciones, fotos, consultas de veterinarias).
        while (true)
        {
            var batch = await pets.ListByOwner(userId, 0, 50, cancellationToken).ConfigureAwait(false);
            if (batch.Count == 0)
            {
                break;
            }

            var deleted = 0;
            foreach (var pet in batch)
            {
                if (await deletePet.Handle(new DeleteMyPetCommand(userId, pet.Id), cancellationToken).ConfigureAwait(false))
                {
                    deleted++;
                }
            }

            if (deleted == 0)
            {
                // Sin progreso: se detiene para no quedar en un ciclo; repetir la eliminación lo reintenta.
                throw new InvalidOperationException("No se pudieron eliminar las mascotas de la cuenta.");
            }
        }

        // pm-163: negocio, catálogo, foto pública, escaneos y autorizaciones dirigidas a él. Las consultas que registró en carnets
        // ajenos se conservan (pm-163#CA-03).
        if (await businesses.FindByOwner(userId, cancellationToken).ConfigureAwait(false) is { } business)
        {
            await scans.DeleteByBusiness(business.Id, cancellationToken).ConfigureAwait(false);
            await grants.DeleteByBusiness(business.Id, cancellationToken).ConfigureAwait(false);
            await deleteBusiness.Handle(new DeleteBusinessCommand(userId), cancellationToken).ConfigureAwait(false);
        }

        await devices.DeleteByUser(userId, cancellationToken).ConfigureAwait(false);
        await settings.Delete(userId, cancellationToken).ConfigureAwait(false);
        await resets.DeleteByUser(userId, cancellationToken).ConfigureAwait(false);
        await deletionTokens.DeleteByUser(userId, cancellationToken).ConfigureAwait(false);
        await sessions.DeleteAllForUser(userId, cancellationToken).ConfigureAwait(false);
        await users.Delete(userId, cancellationToken).ConfigureAwait(false);
    }
}

/// <summary>pm-161: elimina la cuenta del usuario autenticado tras confirmar su contraseña. <c>ToString</c> no imprime la contraseña.</summary>
public sealed record DeleteAccountCommand(Guid UserId, string Password) : ICommand<AuthResult<Unit>>
{
    public override string ToString() => nameof(DeleteAccountCommand);
}

public sealed class DeleteAccountHandler(
    IUserRepository users,
    IPasswordHasher hasher,
    AccountEraser eraser,
    IAuthAttemptThrottle? throttle = null,
    IPasswordHashingLimiter? limiter = null) : IRequestHandler<DeleteAccountCommand, AuthResult<Unit>>
{
    public async Task<AuthResult<Unit>> Handle(DeleteAccountCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var subject = request.UserId.ToString("N");

        // pm-161#CA-04: cuota por usuario antes de verificar la contraseña (el conteo es atómico en Redis), para que una sesión
        // robada no pueda adivinar la contraseña con este endpoint.
        if (throttle is not null)
        {
            var locked = await throttle.GetRemainingLock(AuthThrottleRules.DeleteAccountScope, subject, cancellationToken).ConfigureAwait(false)
                ?? await throttle.RecordAttempt(AuthThrottleRules.DeleteAccountScope, subject, AuthThrottleRules.DeleteAccount, cancellationToken).ConfigureAwait(false);
            if (locked is { } remaining)
            {
                return AuthResult.Throttled<Unit>(remaining);
            }
        }

        var user = await users.FindById(request.UserId, cancellationToken).ConfigureAwait(false);
        var hashToCheck = user?.PasswordHash.Value ?? string.Empty;
        PasswordVerificationResult verified;
        if (limiter is null)
        {
            verified = hasher.Verify(request.Password, hashToCheck);
        }
        else
        {
            var outcome = await limiter.Run(() => hasher.Verify(request.Password, hashToCheck), cancellationToken).ConfigureAwait(false);
            if (!outcome.Accepted)
            {
                return AuthResult.Throttled<Unit>(TimeSpan.FromSeconds(1));
            }

            verified = outcome.Value;
        }

        // Cuenta inexistente y contraseña incorrecta dan el mismo resultado: la respuesta no revela datos.
        if (user is null || verified == PasswordVerificationResult.Failed)
        {
            return AuthResult.Failure<Unit>(AuthError.InvalidCredentials);
        }

        await eraser.Erase(user.Id, cancellationToken).ConfigureAwait(false);
        if (throttle is not null)
        {
            await throttle.Clear(AuthThrottleRules.DeleteAccountScope, subject, cancellationToken).ConfigureAwait(false);
        }

        return AuthResult.Success(Unit.Value);
    }
}

public sealed class DeleteAccountCommandValidator : IRequestValidator<DeleteAccountCommand>
{
    public Task<IReadOnlyList<ValidationFailure>> Validate(DeleteAccountCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        IReadOnlyList<ValidationFailure> failures = string.IsNullOrEmpty(request.Password) || request.Password.Length > AuthValidation.MaxPasswordLength
            ? [new ValidationFailure("password", "La contraseña es obligatoria.")]
            : [];
        return Task.FromResult(failures);
    }
}

// ---------- Eliminación por la web, sin iniciar sesión (pm-164, pm-165) ----------

/// <summary>
/// Tokens de confirmación de eliminación (pm-164). Misma forma que los de restablecimiento (solo hash, un solo uso, 1 hora) pero en
/// su propia colección: un token de un flujo nunca sirve en el otro.
/// </summary>
public interface IAccountDeletionTokenRepository
{
    Task Add(PasswordResetToken token, CancellationToken cancellationToken);

    /// <summary>Marca el token como usado solo si existe, no se usó y no venció, y devuelve el usuario (atómico).</summary>
    Task<Guid?> TryConsume(PetMate.Domain.Sessions.RefreshTokenHash tokenHash, DateTime nowUtc, CancellationToken cancellationToken);

    Task InvalidateAllForUser(Guid userId, DateTime nowUtc, CancellationToken cancellationToken);

    Task DeleteByUser(Guid userId, CancellationToken cancellationToken);
}

public sealed record RequestAccountDeletionCommand(string Email) : ICommand<AuthResult<Unit>>
{
    public override string ToString() => nameof(RequestAccountDeletionCommand);
}

public sealed record ConfirmAccountDeletionCommand(string Token) : ICommand<AuthResult<Unit>>
{
    public override string ToString() => nameof(ConfirmAccountDeletionCommand);
}

/// <summary>
/// pm-164: responde igual exista o no el correo (CA-01). Cada solicitud cuenta en la cuota por correo, exista o no la cuenta (CA-03;
/// el límite por IP lo aplica el filtro de la ruta). Para una cuenta existente guarda el hash de un token de un solo uso (1 hora) y
/// envía el enlace (CA-02); un fallo del proveedor o de Atlas no se informa al cliente.
/// </summary>
public sealed class RequestAccountDeletionHandler(
    IUserRepository users,
    IAccountDeletionTokenRepository deletions,
    IRefreshTokenFactory tokens,
    IEmailSender emailSender,
    IAuthAttemptThrottle throttle,
    TimeProvider timeProvider) : IRequestHandler<RequestAccountDeletionCommand, AuthResult<Unit>>
{
    public async Task<AuthResult<Unit>> Handle(RequestAccountDeletionCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var address = Email.Create(request.Email);

        var wait = await throttle.GetRemainingLock(AuthThrottleRules.DeletionRequestScope, address.Value, cancellationToken).ConfigureAwait(false)
            ?? await throttle.RecordAttempt(AuthThrottleRules.DeletionRequestScope, address.Value, AuthThrottleRules.DeletionRequest, cancellationToken).ConfigureAwait(false);
        if (wait is { } remaining)
        {
            return AuthResult.Throttled<Unit>(remaining);
        }

        var user = await users.FindByEmail(address, cancellationToken).ConfigureAwait(false);
        if (user is not null)
        {
            try
            {
                var now = timeProvider.GetUtcNow().UtcDateTime;
                var material = tokens.Create();
                await deletions.InvalidateAllForUser(user.Id, now, cancellationToken).ConfigureAwait(false);
                await deletions.Add(PasswordResetToken.Issue(user.Id, material.Hash, now), cancellationToken).ConfigureAwait(false);
                _ = await emailSender.SendAccountDeletion(user.Email.Value, material.RawToken, cancellationToken).ConfigureAwait(false);
            }
            catch (PersistenceUnavailableException)
            {
                // Un fallo de Atlas solo ocurre con cuentas existentes: responder 503 delataría que el correo existe (pm-164#CA-01).
            }
        }

        return AuthResult.Success(Unit.Value);
    }
}

/// <summary>
/// pm-165: elimina la cuenta con el token del enlace, sin pedir contraseña (CA-01). El token se consume de forma atómica; uno
/// usado, vencido o inexistente da el mismo error (CA-02). La eliminación revoca todas las sesiones (CA-03).
/// </summary>
public sealed class ConfirmAccountDeletionHandler(
    IUserRepository users,
    IAccountDeletionTokenRepository deletions,
    IRefreshTokenFactory tokens,
    AccountEraser eraser,
    TimeProvider timeProvider) : IRequestHandler<ConfirmAccountDeletionCommand, AuthResult<Unit>>
{
    public async Task<AuthResult<Unit>> Handle(ConfirmAccountDeletionCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var invalid = AuthResult.Failure<Unit>(AuthError.InvalidResetToken);
        if (!tokens.TryHash(request.Token, out var hash) || hash is null)
        {
            return invalid;
        }

        if (await deletions.TryConsume(hash, timeProvider.GetUtcNow().UtcDateTime, cancellationToken).ConfigureAwait(false) is not { } userId
            || await users.FindById(userId, cancellationToken).ConfigureAwait(false) is not { } user)
        {
            return invalid;
        }

        await eraser.Erase(user.Id, cancellationToken).ConfigureAwait(false);
        return AuthResult.Success(Unit.Value);
    }
}

public sealed class RequestAccountDeletionCommandValidator : IRequestValidator<RequestAccountDeletionCommand>
{
    public Task<IReadOnlyList<ValidationFailure>> Validate(RequestAccountDeletionCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var failures = new List<ValidationFailure>();
        AuthValidation.CheckEmail(request.Email, failures);
        return AuthValidation.Result(failures);
    }
}

public sealed class ConfirmAccountDeletionCommandValidator : IRequestValidator<ConfirmAccountDeletionCommand>
{
    public Task<IReadOnlyList<ValidationFailure>> Validate(ConfirmAccountDeletionCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        IReadOnlyList<ValidationFailure> failures = string.IsNullOrWhiteSpace(request.Token) ? [new ValidationFailure("token", "El token es obligatorio.")] : [];
        return Task.FromResult(failures);
    }
}
