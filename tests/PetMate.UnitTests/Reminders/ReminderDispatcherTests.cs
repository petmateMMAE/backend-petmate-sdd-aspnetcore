using PetMate.Application.Notifications;
using PetMate.Application.Push;
using PetMate.Application.Reminders;
using PetMate.Domain.Pets;
using PetMate.TestDoubles;

namespace PetMate.UnitTests.Reminders;

/// <summary>Envío de recordatorios (pm-155): reclamación atómica, preferencias, texto y tokens inválidos.</summary>
public sealed class ReminderDispatcherTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 17, 12, 0, 0, TimeSpan.Zero);
    private static readonly Guid Owner = Guid.NewGuid();

    private sealed class FixedClock : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => Now;
    }

    private sealed class Fixture
    {
        public InMemoryReminderRepository Reminders { get; } = new();

        public InMemoryDeviceRepository Devices { get; } = new();

        public InMemoryNotificationSettingsRepository Settings { get; } = new();

        public InMemoryPetRepository Pets { get; } = new();

        public FakePushSender Push { get; } = new();

        public Pet Pet { get; private set; } = null!;

        public ReminderDispatcher Dispatcher => new(Reminders, Devices, Settings, Pets, Push, new FixedClock());

        public static async Task<Fixture> Create(int dueInDays = 5, int daysBefore = 3)
        {
            var f = new Fixture();
            f.Pet = Pet.Create(Owner, new PetDetails("Firulais", "Perro", null, null, PetSex.Unknown, null), Now.UtcDateTime);
            await f.Pets.Add(f.Pet, CancellationToken.None);
            var due = DateOnly.FromDateTime(Now.UtcDateTime).AddDays(dueInDays);
            await f.Reminders.Upsert(new Reminder(Guid.NewGuid(), ReminderKind.Vaccine, Owner, f.Pet.Id, "Rabia", due, due.AddDays(-daysBefore), null, null, Now.UtcDateTime), CancellationToken.None);
            await f.Devices.Register(Owner, "token-uno-0123456789abcd", 5, Now.UtcDateTime, CancellationToken.None);
            await f.Devices.Register(Owner, "token-dos-0123456789abcd", 5, Now.UtcDateTime, CancellationToken.None);
            return f;
        }
    }

    [Fact]
    public async Task A_due_reminder_is_sent_to_every_device_naming_the_pet_and_the_vaccine_and_only_once()
    {
        var f = await Fixture.Create(dueInDays: 2);

        var first = await f.Dispatcher.RunOnce(CancellationToken.None);
        var second = await f.Dispatcher.RunOnce(CancellationToken.None);

        Assert.Equal(1, first);
        Assert.Equal(0, second);
        Assert.Equal(2, f.Push.Sent.Count);
        Assert.All(f.Push.Sent, s =>
        {
            Assert.Equal("Firulais: vacuna", s.Message.Title);
            Assert.Equal("Rabia: 19/11/2026", s.Message.Body);
        });
    }

    [Fact]
    public async Task A_reminder_whose_notice_date_has_not_arrived_is_not_sent()
    {
        var f = await Fixture.Create(dueInDays: 10, daysBefore: 3);

        Assert.Equal(0, await f.Dispatcher.RunOnce(CancellationToken.None));
        Assert.Empty(f.Push.Sent);
    }

    [Fact]
    public async Task Two_instances_running_at_the_same_time_never_send_the_same_reminder_twice()
    {
        var f = await Fixture.Create(dueInDays: 2);

        var runs = await Task.WhenAll(Enumerable.Range(0, 8).Select(_ => Task.Run(() => f.Dispatcher.RunOnce(CancellationToken.None))));

        Assert.Equal(1, runs.Sum());
        Assert.Equal(2, f.Push.Sent.Count);
    }

    [Fact]
    public async Task It_does_not_send_when_the_owner_turned_the_notice_off_but_the_reminder_is_not_retried()
    {
        var f = await Fixture.Create(dueInDays: 2);
        await f.Settings.Save(NotificationSettings.Default(Owner) with { Vaccines = false }, CancellationToken.None);

        Assert.Equal(1, await f.Dispatcher.RunOnce(CancellationToken.None));
        Assert.Equal(0, await f.Dispatcher.RunOnce(CancellationToken.None));
        Assert.Empty(f.Push.Sent);
    }

    [Fact]
    public async Task A_token_the_provider_reports_as_invalid_is_deleted_and_the_valid_one_still_gets_the_notice()
    {
        var f = await Fixture.Create(dueInDays: 2);
        f.Push.Results["token-uno-0123456789abcd"] = PushResult.InvalidToken;

        await f.Dispatcher.RunOnce(CancellationToken.None);

        Assert.Null(f.Devices.OwnerOf("token-uno-0123456789abcd"));
        Assert.Equal(Owner, f.Devices.OwnerOf("token-dos-0123456789abcd"));
        Assert.Equal(0, await f.Dispatcher.RunOnce(CancellationToken.None));
    }

    [Fact]
    public async Task When_the_provider_fails_for_every_device_the_reminder_goes_back_to_pending_and_is_retried()
    {
        var f = await Fixture.Create(dueInDays: 2);
        f.Push.Default = PushResult.Failed;
        Assert.Equal(1, await f.Dispatcher.RunOnce(CancellationToken.None));

        f.Push.Default = PushResult.Sent;
        Assert.Equal(1, await f.Dispatcher.RunOnce(CancellationToken.None));
        Assert.Equal(4, f.Push.Sent.Count);
        Assert.Equal(0, await f.Dispatcher.RunOnce(CancellationToken.None));
    }

    [Fact]
    public async Task Without_a_push_provider_nothing_is_claimed_so_nothing_is_lost()
    {
        var f = await Fixture.Create(dueInDays: 2);
        f.Push.IsEnabled = false;

        Assert.Equal(0, await f.Dispatcher.RunOnce(CancellationToken.None));

        f.Push.IsEnabled = true;
        Assert.Equal(1, await f.Dispatcher.RunOnce(CancellationToken.None));
    }

    [Fact]
    public async Task A_reminder_for_a_pet_that_no_longer_exists_is_not_sent()
    {
        var f = await Fixture.Create(dueInDays: 2);
        await f.Pets.DeleteOwned(f.Pet.Id, Owner, CancellationToken.None);

        await f.Dispatcher.RunOnce(CancellationToken.None);

        Assert.Empty(f.Push.Sent);
    }

    [Fact]
    public async Task A_review_reminder_is_governed_by_the_reviews_notice_and_says_review()
    {
        var f = await Fixture.Create(dueInDays: 1, daysBefore: 1);
        var review = f.Reminders.All.Single() with { SourceId = Guid.NewGuid(), Kind = ReminderKind.Review, Title = "Revisión del seguimiento" };
        await f.Reminders.Upsert(review, CancellationToken.None);
        await f.Reminders.DeleteBySource(f.Reminders.All.First(r => r.Kind == ReminderKind.Vaccine).SourceId, Owner, CancellationToken.None);
        await f.Settings.Save(NotificationSettings.Default(Owner) with { Reviews = false }, CancellationToken.None);

        Assert.Equal(1, await f.Dispatcher.RunOnce(CancellationToken.None));
        Assert.Empty(f.Push.Sent);

        var again = review with { SourceId = Guid.NewGuid() };
        await f.Settings.Save(NotificationSettings.Default(Owner) with { Vaccines = false }, CancellationToken.None);
        await f.Reminders.Upsert(again, CancellationToken.None);
        await f.Dispatcher.RunOnce(CancellationToken.None);

        Assert.All(f.Push.Sent, sent => Assert.Equal("Firulais: revisión", sent.Message.Title));
        Assert.Equal(2, f.Push.Sent.Count);
    }
}
