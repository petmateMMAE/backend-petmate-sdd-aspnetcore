using PetMate.Application.Notifications;
using PetMate.Application.Reminders;
using PetMate.TestDoubles;

namespace PetMate.UnitTests.Reminders;

/// <summary>Programación de recordatorios (pm-154): anticipación, reprogramación, cancelación e idempotencia.</summary>
public sealed class ReminderSchedulerTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 17, 12, 0, 0, TimeSpan.Zero);

    private sealed class Fixture
    {
        public InMemoryReminderRepository Reminders { get; } = new();

        public InMemoryNotificationSettingsRepository Settings { get; } = new();

        public ReminderScheduler Scheduler => new(Reminders, Settings, new FixedClock(Now));
    }

    private sealed class FixedClock(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    private static readonly Guid Owner = Guid.NewGuid();
    private static readonly Guid Pet = Guid.NewGuid();

    [Fact]
    public async Task It_uses_three_days_by_default_and_the_owner_preference_when_there_is_one()
    {
        var f = new Fixture();
        var due = new DateOnly(2026, 12, 20);

        await f.Scheduler.Schedule(Guid.NewGuid(), ReminderKind.Vaccine, Owner, Pet, "Rabia", due, CancellationToken.None);
        await f.Settings.Save(NotificationSettings.Default(Owner) with { ReminderDaysBefore = 7 }, CancellationToken.None);
        await f.Scheduler.Schedule(Guid.NewGuid(), ReminderKind.Deworming, Owner, Pet, "Drontal", due, CancellationToken.None);

        Assert.Equal([new DateOnly(2026, 12, 17), new DateOnly(2026, 12, 13)], f.Reminders.All.OrderByDescending(r => r.RemindOn).Select(r => r.RemindOn));
    }

    [Fact]
    public async Task Scheduling_twice_does_not_duplicate_and_reschedules_the_date()
    {
        var f = new Fixture();
        var id = Guid.NewGuid();

        await f.Scheduler.Schedule(id, ReminderKind.Vaccine, Owner, Pet, "Rabia", new DateOnly(2026, 12, 20), CancellationToken.None);
        await f.Scheduler.Schedule(id, ReminderKind.Vaccine, Owner, Pet, "Rabia", new DateOnly(2026, 12, 25), CancellationToken.None);

        var only = Assert.Single(f.Reminders.All);
        Assert.Equal(new DateOnly(2026, 12, 25), only.DueOn);
    }

    [Fact]
    public async Task Removing_the_date_cancels_the_reminder_and_a_past_date_creates_none()
    {
        var f = new Fixture();
        var id = Guid.NewGuid();
        await f.Scheduler.Schedule(id, ReminderKind.Vaccine, Owner, Pet, "Rabia", new DateOnly(2026, 12, 20), CancellationToken.None);

        await f.Scheduler.Schedule(id, ReminderKind.Vaccine, Owner, Pet, "Rabia", null, CancellationToken.None);
        await f.Scheduler.Schedule(Guid.NewGuid(), ReminderKind.Vaccine, Owner, Pet, "Vieja", new DateOnly(2026, 11, 1), CancellationToken.None);

        Assert.Empty(f.Reminders.All);
    }

    [Fact]
    public async Task Cancel_only_removes_the_reminder_of_its_owner()
    {
        var f = new Fixture();
        var id = Guid.NewGuid();
        await f.Scheduler.Schedule(id, ReminderKind.Vaccine, Owner, Pet, "Rabia", new DateOnly(2026, 12, 20), CancellationToken.None);

        await f.Scheduler.Cancel(id, Guid.NewGuid(), CancellationToken.None);
        Assert.Single(f.Reminders.All);

        await f.Scheduler.Cancel(id, Owner, CancellationToken.None);
        Assert.Empty(f.Reminders.All);
    }
}
