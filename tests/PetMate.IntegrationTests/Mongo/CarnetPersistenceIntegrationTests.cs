using PetMate.Application.Carnet;
using PetMate.Application.Pets;
using PetMate.Domain.Carnet;
using PetMate.Domain.Pets;

namespace PetMate.IntegrationTests.Mongo;

/// <summary>
/// pm-096 a pm-104 contra la base Atlas de pruebas: persistencia del carnet, filtro por dueño, orden, regla de origen y
/// cascada al eliminar la mascota. Skipped si no hay credenciales de prueba reales.
/// </summary>
public sealed class CarnetPersistenceIntegrationTests
{
    private const string Uri = "MONGODB_TEST_URI";
    private const string Database = "MONGODB_TEST_DATABASE";
    private static readonly DateOnly Today = DateOnly.FromDateTime(DateTime.UtcNow);

    [RequiresEnvironmentFact(Uri, Database)]
    public async Task All_four_record_types_round_trip_in_order_and_only_for_their_owner()
    {
        await using var context = await MongoTestContext.Create();
        var owner = Guid.NewGuid();
        var intruder = Guid.NewGuid();
        var petId = Guid.NewGuid();
        var now = DateTime.UtcNow;
        var old = Vaccine.Create(petId, owner, RecordProvenance.ByOwner, new VaccineDetails(Today.AddDays(-40), "Rabia", "L1", "Dra. Ruiz", Today.AddDays(-40).AddYears(1)), Today, now);
        var recent = Vaccine.Create(petId, owner, RecordProvenance.ByBusiness(Guid.NewGuid(), "Vet Sol"), new VaccineDetails(Today.AddDays(-2), "Parvo", null, null, null), Today, now);
        await context.Vaccines.Add(old, CancellationToken.None);
        await context.Vaccines.Add(recent, CancellationToken.None);
        await context.Dewormings.Add(Deworming.Create(petId, owner, RecordProvenance.ByOwner, new DewormingDetails(Today.AddDays(-1), "Drontal", DewormingType.Internal, null, Today.AddDays(90)), Today, now), CancellationToken.None);
        await context.Weights.Add(WeightEntry.Create(petId, owner, RecordProvenance.ByOwner, new WeightDetails(Today.AddDays(-1), 8.5m), Today, now), CancellationToken.None);
        var allergy = Allergy.Create(petId, owner, RecordProvenance.ByOwner, new AllergyDetails("Pollo", AllergyKind.Allergy, "Urticaria", null), now);
        await context.Allergies.Add(allergy, CancellationToken.None);

        var vaccines = await context.Vaccines.ListByPet(petId, owner, 0, 10, CancellationToken.None);
        Assert.Equal([recent.Id, old.Id], vaccines.Select(v => v.Id));
        Assert.Equal(RecordOrigin.Business, vaccines[0].Provenance.Origin);
        Assert.Equal("Vet Sol", vaccines[0].Provenance.BusinessName);
        Assert.Equal(old.NextDoseOn, vaccines[1].NextDoseOn);
        Assert.Equal(2, await context.Vaccines.CountByPet(petId, owner, CancellationToken.None));
        Assert.Equal(8.5m, (await context.Weights.ListByPet(petId, owner, 0, 10, CancellationToken.None)).Single().Kilograms);
        Assert.Equal(DewormingType.Internal, (await context.Dewormings.ListByPet(petId, owner, 0, 10, CancellationToken.None)).Single().Type);

        // Otro dueño no ve, cuenta, borra ni edita nada.
        Assert.Empty(await context.Vaccines.ListByPet(petId, intruder, 0, 10, CancellationToken.None));
        Assert.Null(await context.Vaccines.FindOwned(old.Id, intruder, CancellationToken.None));
        Assert.False(await context.Vaccines.DeleteOwned(old.Id, intruder, CancellationToken.None));
        Assert.Equal(0, await context.Vaccines.DeleteByPet(petId, intruder, CancellationToken.None));

        // La alergia se edita; la creada por un negocio no se puede reemplazar desde el repositorio.
        allergy.Update(new AllergyDetails("Pollo", AllergyKind.Condition, null, AllergyStatus.Resolved), DateTime.UtcNow);
        Assert.True(await context.Allergies.UpdateOwned(allergy, CancellationToken.None));
        var stored = await context.Allergies.FindOwned(allergy.Id, owner, CancellationToken.None);
        Assert.Equal(AllergyStatus.Resolved, stored!.Status);
        Assert.Equal(AllergyKind.Condition, stored.Kind);
        var business = Allergy.Create(petId, owner, RecordProvenance.ByBusiness(Guid.NewGuid(), "Vet Sol"), new AllergyDetails("Penicilina", AllergyKind.Allergy, null, null), now);
        await context.Allergies.Add(business, CancellationToken.None);
        Assert.False(await context.Allergies.UpdateOwned(business, CancellationToken.None));
    }

    [RequiresEnvironmentFact(Uri, Database)]
    public async Task Deleting_the_pet_removes_the_carnet_records_and_the_qr_in_atlas()
    {
        await using var context = await MongoTestContext.Create();
        var owner = Guid.NewGuid();
        var now = DateTime.UtcNow;
        var pet = Pet.Create(owner, new PetDetails("Firulais", "Perro", null, null, PetSex.Unknown, null), now);
        var otherPet = Pet.Create(owner, new PetDetails("Misu", "Gato", null, null, PetSex.Unknown, null), now);
        foreach (var target in new[] { pet, otherPet })
        {
            await context.Pets.Add(target, CancellationToken.None);
            await context.Vaccines.Add(Vaccine.Create(target.Id, owner, RecordProvenance.ByOwner, new VaccineDetails(Today, "Rabia", null, null, null), Today, now), CancellationToken.None);
            await context.Dewormings.Add(Deworming.Create(target.Id, owner, RecordProvenance.ByOwner, new DewormingDetails(Today, "Drontal", null, null, null), Today, now), CancellationToken.None);
            await context.Weights.Add(WeightEntry.Create(target.Id, owner, RecordProvenance.ByOwner, new WeightDetails(Today, 5m), Today, now), CancellationToken.None);
            await context.Allergies.Add(Allergy.Create(target.Id, owner, RecordProvenance.ByOwner, new AllergyDetails("Pollo", AllergyKind.Allergy, null, null), now), CancellationToken.None);
        }

        var handler = new DeleteMyPetHandler(context.Pets, context.Vaccines, context.Dewormings, context.Weights, context.Allergies, new PetMate.TestDoubles.InMemoryAccessGrantRepository(), new PetMate.TestDoubles.InMemoryScanLogRepository(), new PetMate.TestDoubles.InMemoryConsultationRepository(), new PetMate.TestDoubles.InMemoryFollowupRepository(), new PetMate.TestDoubles.InMemoryConversationRepository(), new PetMate.TestDoubles.InMemoryMessageRepository());
        Assert.True(await handler.Handle(new DeleteMyPetCommand(owner, pet.Id), CancellationToken.None));

        Assert.Null(await context.Pets.FindOwned(pet.Id, owner, CancellationToken.None));
        Assert.Equal(0, await context.Vaccines.CountByPet(pet.Id, owner, CancellationToken.None));
        Assert.Equal(0, await context.Dewormings.CountByPet(pet.Id, owner, CancellationToken.None));
        Assert.Equal(0, await context.Weights.CountByPet(pet.Id, owner, CancellationToken.None));
        Assert.Equal(0, await context.Allergies.CountByPet(pet.Id, owner, CancellationToken.None));
        Assert.NotNull(await context.Pets.FindOwned(otherPet.Id, owner, CancellationToken.None));
        Assert.Equal(1, await context.Vaccines.CountByPet(otherPet.Id, owner, CancellationToken.None));
        Assert.Equal(1, await context.Allergies.CountByPet(otherPet.Id, owner, CancellationToken.None));
    }
}
