using PetMate.Application.Mediator;
using PetMate.Application.Validation;

namespace PetMate.Application.Notifications;

/// <summary>Preferencias de aviso de una cuenta (pm-157). Sin registro guardado se aplican los valores por defecto (CA-03).</summary>
public sealed record NotificationSettings(
    Guid UserId, bool Vaccines, bool Reviews, bool Chat, bool Followups, string Sound, int ReminderDaysBefore, DateTime? UpdatedAtUtc)
{
    public const int MinDaysBefore = 1;

    public const int MaxDaysBefore = 14;

    public const int DefaultDaysBefore = 3;

    public const string DefaultSound = "default";

    /// <summary>Sonidos permitidos.</summary>
    public static readonly IReadOnlyList<string> Sounds = ["default", "soft", "silent"];

    public static NotificationSettings Default(Guid userId) => new(userId, true, true, true, true, DefaultSound, DefaultDaysBefore, null);
}

public interface INotificationSettingsRepository
{
    Task<NotificationSettings?> Find(Guid userId, CancellationToken cancellationToken);

    Task Save(NotificationSettings settings, CancellationToken cancellationToken);

    /// <summary>Borra las preferencias del usuario (eliminación de cuenta).</summary>
    Task Delete(Guid userId, CancellationToken cancellationToken);
}

public sealed record GetNotificationSettingsQuery(Guid UserId) : IQuery<NotificationSettings>;

/// <summary>Reemplazo completo: todos los campos son obligatorios.</summary>
public sealed record UpdateNotificationSettingsCommand(
    Guid UserId, bool? Vaccines, bool? Reviews, bool? Chat, bool? Followups, string? Sound, int? ReminderDaysBefore) : ICommand<NotificationSettings>;

public sealed class GetNotificationSettingsHandler(INotificationSettingsRepository settings) : IRequestHandler<GetNotificationSettingsQuery, NotificationSettings>
{
    public async Task<NotificationSettings> Handle(GetNotificationSettingsQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        return await settings.Find(request.UserId, cancellationToken).ConfigureAwait(false) ?? NotificationSettings.Default(request.UserId);
    }
}

public sealed class UpdateNotificationSettingsHandler(INotificationSettingsRepository settings, TimeProvider timeProvider)
    : IRequestHandler<UpdateNotificationSettingsCommand, NotificationSettings>
{
    public async Task<NotificationSettings> Handle(UpdateNotificationSettingsCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var value = new NotificationSettings(
            request.UserId, request.Vaccines!.Value, request.Reviews!.Value, request.Chat!.Value, request.Followups!.Value,
            request.Sound!, request.ReminderDaysBefore!.Value, timeProvider.GetUtcNow().UtcDateTime);
        await settings.Save(value, cancellationToken).ConfigureAwait(false);
        return value;
    }
}

public sealed class UpdateNotificationSettingsCommandValidator : IRequestValidator<UpdateNotificationSettingsCommand>
{
    public Task<IReadOnlyList<ValidationFailure>> Validate(UpdateNotificationSettingsCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var failures = new List<ValidationFailure>();
        void Required(bool? value, string field)
        {
            if (value is null)
            {
                failures.Add(new ValidationFailure(field, "El campo es obligatorio."));
            }
        }

        Required(request.Vaccines, "vaccines");
        Required(request.Reviews, "reviews");
        Required(request.Chat, "chat");
        Required(request.Followups, "followups");
        if (request.Sound is null || !NotificationSettings.Sounds.Contains(request.Sound, StringComparer.Ordinal))
        {
            failures.Add(new ValidationFailure("sound", $"El sonido debe ser uno de: {string.Join(", ", NotificationSettings.Sounds)}."));
        }

        if (request.ReminderDaysBefore is not (>= NotificationSettings.MinDaysBefore and <= NotificationSettings.MaxDaysBefore))
        {
            failures.Add(new ValidationFailure("reminderDaysBefore", $"Los días de anticipación deben estar entre {NotificationSettings.MinDaysBefore} y {NotificationSettings.MaxDaysBefore}."));
        }

        return Task.FromResult<IReadOnlyList<ValidationFailure>>(failures);
    }
}
