using PetMate.Application.Mediator;
using PetMate.Application.Validation;

namespace PetMate.Application.Devices;

/// <summary>Límites del token de dispositivo (pm-151).</summary>
public static class DeviceLimits
{
    /// <summary>Máximo de dispositivos por usuario (pm-151#CA-02).</summary>
    public const int MaxPerUser = 5;

    public const int MinTokenLength = 20;

    public const int MaxTokenLength = 4096;
}

public enum RegisterDeviceStatus
{
    Registered,
    LimitReached,
}

/// <summary>Tokens de dispositivo para push (pm-151). El token es secreto: no se registra en logs (pm-151#CA-03).</summary>
public interface IDeviceRepository
{
    /// <summary>
    /// Guarda el token del usuario. Mismo token → no se duplica (se actualiza); si estaba a nombre de otra cuenta
    /// (el dispositivo cambió de sesión) pasa a la nueva. Un token nuevo con el límite alcanzado no se guarda.
    /// </summary>
    Task<RegisterDeviceStatus> Register(Guid userId, string token, int maxPerUser, DateTime nowUtc, CancellationToken cancellationToken);

    /// <summary>Quita el token solo si es de este usuario (pm-152); uno ajeno o inexistente no cambia nada.</summary>
    Task Unregister(Guid userId, string token, CancellationToken cancellationToken);

    /// <summary>Borra todos los tokens del usuario (eliminación de cuenta, pm-152#CA-03).</summary>
    Task DeleteByUser(Guid userId, CancellationToken cancellationToken);

    /// <summary>Tokens de los dispositivos del usuario, para enviarle avisos (pm-155).</summary>
    Task<IReadOnlyList<string>> ListTokens(Guid userId, CancellationToken cancellationToken);
}

/// <summary>pm-151: el usuario autenticado registra el token de su dispositivo. <c>ToString</c> no imprime el token.</summary>
public sealed record RegisterDeviceCommand(Guid UserId, string Token) : ICommand<RegisterDeviceStatus>
{
    public override string ToString() => nameof(RegisterDeviceCommand);
}

public sealed class RegisterDeviceHandler(IDeviceRepository devices, TimeProvider timeProvider)
    : IRequestHandler<RegisterDeviceCommand, RegisterDeviceStatus>
{
    public Task<RegisterDeviceStatus> Handle(RegisterDeviceCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        return devices.Register(request.UserId, request.Token.Trim(), DeviceLimits.MaxPerUser, timeProvider.GetUtcNow().UtcDateTime, cancellationToken);
    }
}

/// <summary>Token obligatorio, de 20 a 4096 caracteres, sin espacios ni caracteres de control.</summary>
public sealed class RegisterDeviceCommandValidator : IRequestValidator<RegisterDeviceCommand>
{
    public Task<IReadOnlyList<ValidationFailure>> Validate(RegisterDeviceCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var token = request.Token?.Trim() ?? string.Empty;
        IReadOnlyList<ValidationFailure> failures = token.Length is >= DeviceLimits.MinTokenLength and <= DeviceLimits.MaxTokenLength
            && !token.Any(c => char.IsControl(c) || char.IsWhiteSpace(c))
            ? []
            : [new ValidationFailure("token", $"El token debe tener entre {DeviceLimits.MinTokenLength} y {DeviceLimits.MaxTokenLength} caracteres, sin espacios.")];
        return Task.FromResult(failures);
    }
}

/// <summary>pm-152: el usuario quita el token de su dispositivo (p. ej. al cerrar sesión). Idempotente.</summary>
public sealed record UnregisterDeviceCommand(Guid UserId, string Token) : ICommand<bool>
{
    public override string ToString() => nameof(UnregisterDeviceCommand);
}

public sealed class UnregisterDeviceHandler(IDeviceRepository devices) : IRequestHandler<UnregisterDeviceCommand, bool>
{
    public async Task<bool> Handle(UnregisterDeviceCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        await devices.Unregister(request.UserId, request.Token.Trim(), cancellationToken).ConfigureAwait(false);
        return true;
    }
}

public sealed class UnregisterDeviceCommandValidator : IRequestValidator<UnregisterDeviceCommand>
{
    public Task<IReadOnlyList<ValidationFailure>> Validate(UnregisterDeviceCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        return new RegisterDeviceCommandValidator().Validate(new RegisterDeviceCommand(request.UserId, request.Token), cancellationToken);
    }
}
