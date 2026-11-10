using PetMate.Application.Carnet;
using PetMate.Application.Pets;
using PetMate.Domain.Carnet;
using PetMate.Domain.Pets;
using PetMate.TestDoubles;

namespace PetMate.UnitTests.Pets;

/// <summary>pm-104: eliminar una mascota borra su carnet y su QR sin dejar registros huérfanos.</summary>
public sealed class PetCascadeTests
{
    private static readonly DateTime Now = new(2026, 11, 9, 12, 0, 0, DateTimeKind.Utc);
    private static readonly DateOnly Today = new(2026, 11, 9);

    private sealed class Fixture(IAllergyRepository? allergiesOverride = null)
    {
        public InMemoryPetRepository Pets { get; } = new();

        public InMemoryVaccineRepository Vaccines { get; } = new();

        public InMemoryDewormingRepository Dewormings { get; } = new();

        public InMemoryWeightRepository Weights { get; } = new();

        public InMemoryAllergyRepository Allergies { get; } = new();

        public DeleteMyPetHandler Handler => new(Pets, Vaccines, Dewormings, Weights, allergiesOverride ?? Allergies);

        public async Task<Pet> Seed(Guid owner)
        {
            var pet = Pet.Create(owner, new PetDetails("Firulais", "Perro", null, null, PetSex.Unknown, null), Now);
            await Pets.Add(pet, CancellationToken.None);
            await Vaccines.Add(Vaccine.Create(pet.Id, owner, RecordProvenance.ByOwner, new VaccineDetails(Today, "Rabia", null, null, null), Today, Now), CancellationToken.None);
            await Dewormings.Add(Deworming.Create(pet.Id, owner, RecordProvenance.ByBusiness(Guid.NewGuid(), "Vet"), new DewormingDetails(Today, "Drontal", null, null, null), Today, Now), CancellationToken.None);
            await Weights.Add(WeightEntry.Create(pet.Id, owner, RecordProvenance.ByOwner, new WeightDetails(Today, 8.5m), Today, Now), CancellationToken.None);
            await Allergies.Add(Allergy.Create(pet.Id, owner, RecordProvenance.ByOwner, new AllergyDetails("Pollo", AllergyKind.Allergy, null, null), Now), CancellationToken.None);
            return pet;
        }

        public int RecordCount => Vaccines.All.Count + Dewormings.All.Count + Weights.All.Count + Allergies.All.Count;
    }

    [Fact]
    public async Task Deleting_a_pet_removes_its_whole_carnet_and_the_pet_with_its_qr()
    {
        var owner = Guid.NewGuid();
        var fixture = new Fixture();
        var pet = await fixture.Seed(owner);
        var other = await fixture.Seed(Guid.NewGuid());

        var deleted = await fixture.Handler.Handle(new DeleteMyPetCommand(owner, pet.Id), CancellationToken.None);

        Assert.True(deleted);
        Assert.Null(await fixture.Pets.FindOwned(pet.Id, owner, CancellationToken.None));
        Assert.Equal(4, fixture.RecordCount);
        Assert.All(fixture.Vaccines.All.Concat<HealthRecord>(fixture.Dewormings.All).Concat(fixture.Weights.All).Concat(fixture.Allergies.All), r => Assert.Equal(other.Id, r.PetId));
    }

    [Fact]
    public async Task Deleting_a_foreign_pet_changes_nothing()
    {
        var fixture = new Fixture();
        var pet = await fixture.Seed(Guid.NewGuid());

        var deleted = await fixture.Handler.Handle(new DeleteMyPetCommand(Guid.NewGuid(), pet.Id), CancellationToken.None);

        Assert.False(deleted);
        Assert.Single(fixture.Pets.All);
        Assert.Equal(4, fixture.RecordCount);
    }

    private sealed class FailingAllergies(IAllergyRepository inner) : IAllergyRepository
    {
        public bool Fail { get; set; } = true;

        public Task Add(Allergy record, CancellationToken cancellationToken) => inner.Add(record, cancellationToken);

        public Task<Allergy?> FindOwned(Guid recordId, Guid ownerId, CancellationToken cancellationToken) => inner.FindOwned(recordId, ownerId, cancellationToken);

        public Task<IReadOnlyList<Allergy>> ListByPet(Guid petId, Guid ownerId, int skip, int take, CancellationToken cancellationToken) => inner.ListByPet(petId, ownerId, skip, take, cancellationToken);

        public Task<long> CountByPet(Guid petId, Guid ownerId, CancellationToken cancellationToken) => inner.CountByPet(petId, ownerId, cancellationToken);

        public Task<bool> DeleteOwned(Guid recordId, Guid ownerId, CancellationToken cancellationToken) => inner.DeleteOwned(recordId, ownerId, cancellationToken);

        public Task<long> DeleteByPet(Guid petId, Guid ownerId, CancellationToken cancellationToken)
            => Fail ? throw new InvalidOperationException("falla simulada") : inner.DeleteByPet(petId, ownerId, cancellationToken);

        public Task<bool> UpdateOwned(Allergy allergy, CancellationToken cancellationToken) => inner.UpdateOwned(allergy, cancellationToken);
    }

    [Fact]
    public async Task A_failure_midway_keeps_the_pet_visible_and_retrying_finishes_without_orphans()
    {
        var owner = Guid.NewGuid();
        var probe = new Fixture();
        var failing = new FailingAllergies(probe.Allergies);
        var fixture = new Fixture(failing);
        // El Fixture con doble propio usa los mismos repositorios en memoria que `probe` para las alergias.
        var pet = await fixture.Seed(owner);
        await probe.Allergies.Add(Allergy.Create(pet.Id, owner, RecordProvenance.ByOwner, new AllergyDetails("Pollo", AllergyKind.Allergy, null, null), Now), CancellationToken.None);

        await Assert.ThrowsAsync<InvalidOperationException>(() => fixture.Handler.Handle(new DeleteMyPetCommand(owner, pet.Id), CancellationToken.None));
        Assert.NotNull(await fixture.Pets.FindOwned(pet.Id, owner, CancellationToken.None));

        failing.Fail = false;
        Assert.True(await fixture.Handler.Handle(new DeleteMyPetCommand(owner, pet.Id), CancellationToken.None));
        Assert.Null(await fixture.Pets.FindOwned(pet.Id, owner, CancellationToken.None));
        Assert.Empty(fixture.Vaccines.All);
        Assert.Empty(fixture.Dewormings.All);
        Assert.Empty(fixture.Weights.All);
        Assert.Empty(probe.Allergies.All);
    }

    // pm-104#CA-04: si alguien agrega un tipo de registro del carnet y no lo incluye en la cascada, esta prueba falla.
    [Fact]
    public void Every_health_record_type_takes_part_in_the_pet_cascade()
    {
        var recordTypes = typeof(HealthRecord).Assembly.GetTypes().Where(t => !t.IsAbstract && t.IsSubclassOf(typeof(HealthRecord))).ToArray();
        var handlerDependencies = typeof(DeleteMyPetHandler).GetConstructors().Single().GetParameters().Select(p => p.ParameterType).ToArray();

        Assert.NotEmpty(recordTypes);
        Assert.All(recordTypes, record => Assert.Contains(handlerDependencies, dependency => typeof(IHealthRecordRepository<>).MakeGenericType(record).IsAssignableFrom(dependency)));
    }
}
