using System.Globalization;
using PetMate.Application.Carnet;
using PetMate.Application.Devices;
using PetMate.Application.Mediator;
using PetMate.Application.Notifications;
using PetMate.Application.Pagination;
using PetMate.Application.Persistence;
using PetMate.Application.Push;

namespace PetMate.Application.Reminders;

public enum ReminderKind
{
    Vaccine = 1,
    Deworming = 2,

    /// <summary>Próxima revisión de un seguimiento (pm-160): se avisa un día antes.</summary>
    Review = 3,
}

/// <summary>
/// Recordatorio de una próxima dosis o fecha del carnet (pm-154). Su identidad es el registro de origen: un registro tiene a
/// lo más un recordatorio, así que generarlo otra vez no lo duplica (pm-154#CA-03).
/// </summary>
public sealed record Reminder(
    Guid SourceId, ReminderKind Kind, Guid OwnerId, Guid PetId, string Title, DateOnly DueOn, DateOnly RemindOn, DateTime? ReadAtUtc, DateTime? SentAtUtc, DateTime CreatedAtUtc);

public interface IReminderRepository
{
    /// <summary>Crea o reprograma el recordatorio del registro sin perder su estado de lectura ni de envío.</summary>
    Task Upsert(Reminder reminder, CancellationToken cancellationToken);

    Task DeleteBySource(Guid sourceId, Guid ownerId, CancellationToken cancellationToken);

    Task<long> DeleteByPet(Guid petId, Guid ownerId, CancellationToken cancellationToken);

    /// <summary>Marca como leído el recordatorio del dueño (pm-156#CA-02). <c>false</c> si no existe o es de otra cuenta.</summary>
    Task<bool> MarkRead(Guid sourceId, Guid ownerId, DateTime nowUtc, CancellationToken cancellationToken);

    /// <summary>Recordatorios del dueño con fecha desde <paramref name="from"/>, el más cercano primero.</summary>
    Task<IReadOnlyList<Reminder>> ListUpcoming(Guid ownerId, DateOnly from, int skip, int take, CancellationToken cancellationToken);

    Task<long> CountUpcoming(Guid ownerId, DateOnly from, CancellationToken cancellationToken);

    /// <summary>
    /// Reclama de forma atómica el siguiente recordatorio que ya corresponde enviar (<c>remindOn</c> llegó, no vencido y sin enviar) y lo
    /// marca como enviado, de modo que dos instancias nunca reciben el mismo (pm-155#CA-02). <c>null</c> si no queda ninguno.
    /// </summary>
    Task<Reminder?> ClaimNextDue(DateOnly today, DateTime nowUtc, CancellationToken cancellationToken);

    /// <summary>Devuelve un recordatorio reclamado a pendiente (el envío falló en todos los dispositivos) para reintentarlo.</summary>
    Task Release(Guid sourceId, CancellationToken cancellationToken);
}

/// <summary>Programa y cancela recordatorios a partir de los registros del carnet (pm-154#CA-01, CA-02).</summary>
public sealed class ReminderScheduler(IReminderRepository reminders, INotificationSettingsRepository settings, TimeProvider timeProvider)
{
    /// <summary>
    /// Programa (o reprograma, o cancela si no hay fecha) el recordatorio de un registro del carnet. Es de mejor esfuerzo: si la
    /// base no responde, el registro ya guardado no se deshace.
    /// </summary>
    // shortcut: un fallo al programar se descarta sin reintento ni cola; si el recordatorio se pierde, no hay reparación automática.
    public async Task Schedule(Guid sourceId, ReminderKind kind, Guid ownerId, Guid petId, string title, DateOnly? dueOn, CancellationToken cancellationToken, int? daysBefore = null)
    {
        try
        {
            await ScheduleCore(sourceId, kind, ownerId, petId, title, dueOn, daysBefore, cancellationToken).ConfigureAwait(false);
        }
        catch (PersistenceUnavailableException)
        {
            // Sin recordatorio por ahora; el registro del carnet sigue guardado.
        }
    }

    public async Task Cancel(Guid sourceId, Guid ownerId, CancellationToken cancellationToken)
    {
        try
        {
            await reminders.DeleteBySource(sourceId, ownerId, cancellationToken).ConfigureAwait(false);
        }
        catch (PersistenceUnavailableException)
        {
            // El recordatorio de un registro borrado queda huérfano; el listado lo oculta si la mascota ya no existe.
        }
    }

    private async Task ScheduleCore(Guid sourceId, ReminderKind kind, Guid ownerId, Guid petId, string title, DateOnly? dueOn, int? daysBeforeOverride, CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;
        if (dueOn is not { } due)
        {
            // Sin próxima fecha no hay recordatorio y, si lo había, se cancela (pm-154#CA-02).
            await reminders.DeleteBySource(sourceId, ownerId, cancellationToken).ConfigureAwait(false);
            return;
        }

        if (due < DateOnly.FromDateTime(now))
        {
            return;
        }

        var days = daysBeforeOverride ?? (await settings.Find(ownerId, cancellationToken).ConfigureAwait(false))?.ReminderDaysBefore ?? NotificationSettings.DefaultDaysBefore;
        await reminders.Upsert(new Reminder(sourceId, kind, ownerId, petId, title, due, due.AddDays(-days), null, null, now), cancellationToken).ConfigureAwait(false);
    }
}

public sealed record ReminderResult(Guid Id, string Kind, Guid PetId, string PetName, string Title, DateOnly DueOn, DateOnly RemindOn, bool IsRead);

/// <summary>pm-156: próximos recordatorios de las mascotas propias, por cercanía.</summary>
public sealed record ListRemindersQuery(Guid OwnerId, int? Page, int? PageSize) : IQuery<PagedResult<ReminderResult>>;

public sealed class ListRemindersHandler(IReminderRepository reminders, IPetRepository pets, TimeProvider timeProvider)
    : IRequestHandler<ListRemindersQuery, PagedResult<ReminderResult>>
{
    public async Task<PagedResult<ReminderResult>> Handle(ListRemindersQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var page = PaginationConvention.ResolvePage(request.Page);
        var pageSize = PaginationConvention.ResolvePageSize(request.PageSize);
        var today = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);
        var total = await reminders.CountUpcoming(request.OwnerId, today, cancellationToken).ConfigureAwait(false);
        var skip = ((long)page - 1) * pageSize;
        if (skip >= total || skip > int.MaxValue)
        {
            return new PagedResult<ReminderResult>([], page, pageSize, total);
        }

        var found = await reminders.ListUpcoming(request.OwnerId, today, (int)skip, pageSize, cancellationToken).ConfigureAwait(false);

        // El nombre se lee al listar para que un cambio de nombre de la mascota se vea sin reprogramar nada.
        var names = new Dictionary<Guid, string>();
        foreach (var petId in found.Select(r => r.PetId).Distinct())
        {
            if (await pets.FindOwned(petId, request.OwnerId, cancellationToken).ConfigureAwait(false) is { } pet)
            {
                names[petId] = pet.Name;
            }
        }

        var items = found
            .Where(r => names.ContainsKey(r.PetId))
            .Select(r => new ReminderResult(r.SourceId, r.Kind.ToString(), r.PetId, names[r.PetId], r.Title, r.DueOn, r.RemindOn, r.ReadAtUtc is not null))
            .ToArray();
        return new PagedResult<ReminderResult>(items, page, pageSize, total);
    }
}

/// <summary>pm-156#CA-02: el dueño marca un recordatorio propio como leído. Idempotente; uno ajeno o inexistente es 404.</summary>
public sealed record MarkReminderReadCommand(Guid OwnerId, Guid ReminderId) : ICommand<bool>;

public sealed class MarkReminderReadHandler(IReminderRepository reminders, TimeProvider timeProvider) : IRequestHandler<MarkReminderReadCommand, bool>
{
    public Task<bool> Handle(MarkReminderReadCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        return reminders.MarkRead(request.ReminderId, request.OwnerId, timeProvider.GetUtcNow().UtcDateTime, cancellationToken);
    }
}

public sealed class MarkReminderReadCommandValidator : PetMate.Application.Validation.IRequestValidator<MarkReminderReadCommand>
{
    public Task<IReadOnlyList<PetMate.Application.Validation.ValidationFailure>> Validate(MarkReminderReadCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        return Task.FromResult<IReadOnlyList<PetMate.Application.Validation.ValidationFailure>>(
            request.ReminderId == Guid.Empty ? [new PetMate.Application.Validation.ValidationFailure("id", "El identificador del recordatorio es obligatorio.")] : []);
    }
}

public sealed class ListRemindersQueryValidator : PetMate.Application.Validation.IRequestValidator<ListRemindersQuery>
{
    public Task<IReadOnlyList<PetMate.Application.Validation.ValidationFailure>> Validate(ListRemindersQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        return Task.FromResult(PaginationConvention.Check(request.Page, request.PageSize));
    }
}

/// <summary>
/// Envía por push los recordatorios que ya corresponden (pm-155). Cada recordatorio se reclama de forma atómica antes de enviarse, así
/// que con dos o más instancias ejecutando el trabajo a la vez nunca se envía dos veces (CA-02). Solo se envía si el dueño tiene el
/// aviso activo (CA-03) y el texto nombra a la mascota y a la vacuna o desparasitación (CA-04).
/// </summary>
public sealed class ReminderDispatcher(
    IReminderRepository reminders,
    IDeviceRepository devices,
    INotificationSettingsRepository settings,
    IPetRepository pets,
    IPushSender push,
    TimeProvider timeProvider)
{
    /// <summary>Tope por pasada para que una ráfaga no deje el trabajo ocupado indefinidamente; el resto sale en la siguiente.</summary>
    public const int MaxPerRun = 200;

    /// <summary>Procesa los recordatorios pendientes y devuelve cuántos reclamó. Nunca lanza por un fallo del proveedor (pm-153#CA-03).</summary>
    // shortcut: el envío se marca al reclamar; si la instancia cae entre reclamar y enviar, ese aviso se pierde sin reintento.
    public async Task<int> RunOnce(CancellationToken cancellationToken)
    {
        if (!push.IsEnabled)
        {
            return 0;
        }

        var claimed = 0;
        while (claimed < MaxPerRun && !cancellationToken.IsCancellationRequested)
        {
            var now = timeProvider.GetUtcNow().UtcDateTime;
            if (await reminders.ClaimNextDue(DateOnly.FromDateTime(now), now, cancellationToken).ConfigureAwait(false) is not { } reminder)
            {
                break;
            }

            claimed++;
            if (!await Deliver(reminder, cancellationToken).ConfigureAwait(false))
            {
                // El proveedor falló y el recordatorio volvió a pendiente: se corta la pasada para no reclamarlo de nuevo de inmediato.
                break;
            }
        }

        return claimed;
    }

    /// <summary>Devuelve <c>false</c> si el recordatorio quedó pendiente para reintentarlo.</summary>
    private async Task<bool> Deliver(Reminder reminder, CancellationToken cancellationToken)
    {
        var preferences = await settings.Find(reminder.OwnerId, cancellationToken).ConfigureAwait(false) ?? NotificationSettings.Default(reminder.OwnerId);
        if (!(reminder.Kind == ReminderKind.Review ? preferences.Reviews : preferences.Vaccines))
        {
            // pm-155#CA-03: el aviso está desactivado; el recordatorio sigue en la lista pero no se envía.
            return true;
        }

        if (await pets.FindOwned(reminder.PetId, reminder.OwnerId, cancellationToken).ConfigureAwait(false) is not { } pet)
        {
            return true;
        }

        var tokens = await devices.ListTokens(reminder.OwnerId, cancellationToken).ConfigureAwait(false);
        var what = reminder.Kind switch
        {
            ReminderKind.Vaccine => "vacuna",
            ReminderKind.Deworming => "desparasitación",
            _ => "revisión",
        };
        var message = new PushMessage(
            $"{pet.Name}: {what}",
            string.Create(CultureInfo.InvariantCulture, $"{reminder.Title}: {reminder.DueOn:dd/MM/yyyy}"));

        var sent = 0;
        foreach (var token in tokens)
        {
            switch (await push.Send(token, message, cancellationToken).ConfigureAwait(false))
            {
                case PushResult.Sent:
                    sent++;
                    break;
                case PushResult.InvalidToken:
                    // pm-152#CA-02: el proveedor dice que el dispositivo ya no existe.
                    await devices.Unregister(reminder.OwnerId, token, cancellationToken).ConfigureAwait(false);
                    break;
            }
        }

        // Si había dispositivos y el envío falló en todos (red o proveedor caído), vuelve a pendiente para la siguiente pasada.
        if (sent == 0 && tokens.Count > 0 && !cancellationToken.IsCancellationRequested)
        {
            await reminders.Release(reminder.SourceId, CancellationToken.None).ConfigureAwait(false);
            return false;
        }

        return true;
    }
}
