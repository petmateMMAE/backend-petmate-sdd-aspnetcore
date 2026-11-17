using PetMate.Application.Pets;
using PetMate.Domain.Pets;
using PetMate.TestDoubles;

namespace PetMate.UnitTests.Pets;

/// <summary>Casos de uso de mascotas (pm-040, pm-042, pm-043, pm-044) con repositorio en memoria y reloj controlado.</summary>
public sealed class PetHandlersTests
{
    private static readonly PetDetails Details = new("Firulais", "Perro", "Mestizo", new DateOnly(2020, 5, 1), PetSex.Male, null);
    private readonly ManualClock clock = new(new DateTimeOffset(2026, 11, 8, 15, 0, 0, TimeSpan.Zero));
    private readonly InMemoryPetRepository pets = new();

    [Fact]
    public async Task Create_assigns_the_owner_from_the_command_and_persists()
    {
        var owner = Guid.NewGuid();

        var outcome = await new CreatePetHandler(pets, clock).Handle(new CreatePetCommand(owner, Details), CancellationToken.None);
        var result = outcome.Pet!;

        var stored = Assert.Single(pets.All);
        Assert.Equal(owner, stored.OwnerId);
        Assert.Equal(stored.Id, result.Id);
        Assert.Equal(clock.GetUtcNow().UtcDateTime, result.CreatedAtUtc);
    }

    [Fact]
    public async Task Create_stops_at_the_maximum_per_owner_without_affecting_other_owners_and_a_delete_frees_a_slot()
    {
        var owner = Guid.NewGuid();
        var handler = new CreatePetHandler(pets, clock);
        for (var i = 0; i < PetLimits.MaxPetsPerOwner; i++)
        {
            Assert.NotNull((await handler.Handle(new CreatePetCommand(owner, Details), CancellationToken.None)).Pet);
        }

        var blocked = await handler.Handle(new CreatePetCommand(owner, Details), CancellationToken.None);
        var other = await handler.Handle(new CreatePetCommand(Guid.NewGuid(), Details), CancellationToken.None);

        Assert.True(blocked.LimitReached);
        Assert.Null(blocked.Pet);
        Assert.Equal(PetLimits.MaxPetsPerOwner, pets.All.Count(p => p.OwnerId == owner));
        Assert.NotNull(other.Pet);

        var first = pets.All.First(p => p.OwnerId == owner);
        Assert.True(await new DeleteMyPetHandler(pets, new InMemoryVaccineRepository(), new InMemoryDewormingRepository(), new InMemoryWeightRepository(), new InMemoryAllergyRepository(), new InMemoryAccessGrantRepository(), new InMemoryScanLogRepository(), new InMemoryConsultationRepository(), new InMemoryFollowupRepository(), new InMemoryConversationRepository(), new InMemoryMessageRepository(), new InMemoryImageStorage()).Handle(new DeleteMyPetCommand(owner, first.Id), CancellationToken.None));
        Assert.NotNull((await handler.Handle(new CreatePetCommand(owner, Details), CancellationToken.None)).Pet);
    }

    [Fact]
    public void The_maximum_is_the_approved_value()
        => Assert.Equal(100, PetLimits.MaxPetsPerOwner);

    [Fact]
    public async Task Get_returns_null_for_another_owner_or_a_missing_pet()
    {
        var owner = Guid.NewGuid();
        var created = (await new CreatePetHandler(pets, clock).Handle(new CreatePetCommand(owner, Details), CancellationToken.None)).Pet!;
        var handler = new GetMyPetHandler(pets);

        Assert.NotNull(await handler.Handle(new GetMyPetQuery(owner, created.Id), CancellationToken.None));
        Assert.Null(await handler.Handle(new GetMyPetQuery(Guid.NewGuid(), created.Id), CancellationToken.None));
        Assert.Null(await handler.Handle(new GetMyPetQuery(owner, Guid.NewGuid()), CancellationToken.None));
    }

    [Fact]
    public async Task Update_changes_only_the_owner_pet_and_moves_the_update_date()
    {
        var owner = Guid.NewGuid();
        var created = (await new CreatePetHandler(pets, clock).Handle(new CreatePetCommand(owner, Details), CancellationToken.None)).Pet!;
        clock.Advance(TimeSpan.FromHours(1));
        var handler = new UpdateMyPetHandler(pets, clock);
        var changes = Details with { Name = "Max" };

        Assert.Null(await handler.Handle(new UpdateMyPetCommand(Guid.NewGuid(), created.Id, changes), CancellationToken.None));
        Assert.Equal("Firulais", Assert.Single(pets.All).Name);

        var updated = await handler.Handle(new UpdateMyPetCommand(owner, created.Id, changes), CancellationToken.None);

        Assert.NotNull(updated);
        Assert.Equal("Max", updated.Name);
        Assert.Equal(created.CreatedAtUtc, updated.CreatedAtUtc);
        Assert.True(updated.UpdatedAtUtc > created.UpdatedAtUtc);
    }

    [Fact]
    public async Task Delete_removes_only_the_owner_pet()
    {
        var owner = Guid.NewGuid();
        var created = (await new CreatePetHandler(pets, clock).Handle(new CreatePetCommand(owner, Details), CancellationToken.None)).Pet!;
        var handler = new DeleteMyPetHandler(pets, new InMemoryVaccineRepository(), new InMemoryDewormingRepository(), new InMemoryWeightRepository(), new InMemoryAllergyRepository(), new InMemoryAccessGrantRepository(), new InMemoryScanLogRepository(), new InMemoryConsultationRepository(), new InMemoryFollowupRepository(), new InMemoryConversationRepository(), new InMemoryMessageRepository(), new InMemoryImageStorage());

        Assert.False(await handler.Handle(new DeleteMyPetCommand(Guid.NewGuid(), created.Id), CancellationToken.None));
        Assert.Single(pets.All);
        Assert.True(await handler.Handle(new DeleteMyPetCommand(owner, created.Id), CancellationToken.None));
        Assert.Empty(pets.All);
    }

    [Fact]
    public async Task Validators_use_the_server_clock_for_the_birth_date()
    {
        var today = DateOnly.FromDateTime(clock.GetUtcNow().UtcDateTime);
        var create = new CreatePetCommandValidator(clock);
        var update = new UpdateMyPetCommandValidator(clock);

        Assert.Empty(await create.Validate(new CreatePetCommand(Guid.NewGuid(), Details with { BirthDate = today }), CancellationToken.None));
        Assert.Equal("birthDate", Assert.Single(await create.Validate(new CreatePetCommand(Guid.NewGuid(), Details with { BirthDate = today.AddDays(1) }), CancellationToken.None)).Field);
        Assert.Equal("name", Assert.Single(await update.Validate(new UpdateMyPetCommand(Guid.NewGuid(), Guid.NewGuid(), Details with { Name = " " }), CancellationToken.None)).Field);
    }
}
