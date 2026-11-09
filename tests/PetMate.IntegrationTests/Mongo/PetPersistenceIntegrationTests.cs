using MongoDB.Bson;
using MongoDB.Driver;
using PetMate.Domain.Pets;
using PetMate.Infrastructure.Persistence;

namespace PetMate.IntegrationTests.Mongo;

/// <summary>
/// pm-039 contra la base Atlas de pruebas: persistencia de mascotas, filtro por propietario en cada operación, borrado físico
/// e índice por propietario. Skipped si no hay credenciales de prueba reales; no cuentan como aprobadas.
/// </summary>
public sealed class PetPersistenceIntegrationTests
{
    private const string Uri = "MONGODB_TEST_URI";
    private const string Database = "MONGODB_TEST_DATABASE";

    private static Pet NewPet(Guid owner, string name = "Firulais", DateTime? at = null)
        => Pet.Create(owner, new PetDetails(name, "Perro", "Mestizo", new DateOnly(2020, 5, 1), PetSex.Male, "https://cdn.example.com/f.jpg"), at ?? DateTime.UtcNow);

    [RequiresEnvironmentFact(Uri, Database)]
    public async Task Pet_round_trips_with_all_fields_and_utc_dates()
    {
        await using var context = await MongoTestContext.Create();
        var owner = Guid.NewGuid();
        var pet = NewPet(owner);

        await context.Pets.Add(pet, CancellationToken.None);
        var stored = await context.Pets.FindOwned(pet.Id, owner, CancellationToken.None);

        Assert.NotNull(stored);
        Assert.Equal(pet.Name, stored.Name);
        Assert.Equal(pet.Species, stored.Species);
        Assert.Equal(pet.Breed, stored.Breed);
        Assert.Equal(pet.BirthDate, stored.BirthDate);
        Assert.Equal(pet.Sex, stored.Sex);
        Assert.Equal(pet.PhotoUrl, stored.PhotoUrl);
        Assert.Equal(DateTimeKind.Utc, stored.CreatedAtUtc.Kind);
    }

    [RequiresEnvironmentFact(Uri, Database)]
    public async Task Qr_is_persisted_replaced_only_by_the_owner_and_both_values_are_unique()
    {
        await using var context = await MongoTestContext.Create();
        var owner = Guid.NewGuid();
        var pet = NewPet(owner);
        await context.Pets.Add(pet, CancellationToken.None);

        var stored = await context.Pets.FindOwned(pet.Id, owner, CancellationToken.None);
        Assert.Equal(pet.Qr, stored!.Qr);

        stored.RegenerateQr(DateTime.UtcNow);
        var forged = (Pet)typeof(Pet).GetMethod("Restore", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
            .Invoke(null, [pet.Id, Guid.NewGuid(), new PetDetails("X", "Perro", null, null, PetSex.Unknown, null), pet.CreatedAtUtc, DateTime.UtcNow, PetQr.Generate()])!;
        Assert.False(await context.Pets.ReplaceQrOwned(forged, CancellationToken.None));
        Assert.True(await context.Pets.ReplaceQrOwned(stored, CancellationToken.None));
        Assert.Equal(stored.Qr, (await context.Pets.FindOwned(pet.Id, owner, CancellationToken.None))!.Qr);
        Assert.NotEqual(pet.Qr, stored.Qr);

        // Índices únicos: dos mascotas con el mismo token o el mismo código no pueden coexistir.
        var other = NewPet(owner, "Otra");
        await context.Pets.Add(other, CancellationToken.None);
        var collection = context.Database.GetCollection<BsonDocument>(context.Names.Pets);
        await Assert.ThrowsAsync<MongoWriteException>(() => collection.UpdateOneAsync(
            Builders<BsonDocument>.Filter.Eq("_id", new BsonBinaryData(other.Id, GuidRepresentation.Standard)),
            Builders<BsonDocument>.Update.Set("qrToken", stored.Qr.Token)));
        await Assert.ThrowsAsync<MongoWriteException>(() => collection.UpdateOneAsync(
            Builders<BsonDocument>.Filter.Eq("_id", new BsonBinaryData(other.Id, GuidRepresentation.Standard)),
            Builders<BsonDocument>.Update.Set("qrCode", stored.Qr.Code)));
    }

    [RequiresEnvironmentFact(Uri, Database)]
    public async Task A_pet_created_before_the_qr_receives_one_the_first_time_it_is_read_and_keeps_it()
    {
        await using var context = await MongoTestContext.Create();
        var owner = Guid.NewGuid();
        var pet = NewPet(owner);
        await context.Pets.Add(pet, CancellationToken.None);
        var collection = context.Database.GetCollection<BsonDocument>(context.Names.Pets);
        await collection.UpdateOneAsync(
            Builders<BsonDocument>.Filter.Eq("_id", new BsonBinaryData(pet.Id, GuidRepresentation.Standard)),
            Builders<BsonDocument>.Update.Unset("qrToken").Unset("qrCode"));

        var first = await context.Pets.FindOwned(pet.Id, owner, CancellationToken.None);
        var second = await context.Pets.FindOwned(pet.Id, owner, CancellationToken.None);

        Assert.True(PetQr.IsWellFormed(first!.Qr.Token, first.Qr.Code));
        Assert.NotEqual(pet.Qr, first.Qr);
        Assert.Equal(first.Qr, second!.Qr);
    }

    [RequiresEnvironmentFact(Uri, Database)]
    public async Task Another_owner_cannot_read_update_or_delete_the_pet()
    {
        await using var context = await MongoTestContext.Create();
        var owner = Guid.NewGuid();
        var intruder = Guid.NewGuid();
        var pet = NewPet(owner);
        await context.Pets.Add(pet, CancellationToken.None);

        Assert.Null(await context.Pets.FindOwned(pet.Id, intruder, CancellationToken.None));
        Assert.False(await context.Pets.DeleteOwned(pet.Id, intruder, CancellationToken.None));
        // Mascota con el mismo Id pero otro propietario (Restore es interno del dominio; se invoca por reflexión solo en la prueba).
        var restore = typeof(Pet).GetMethod("Restore", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!;
        var forged = (Pet)restore.Invoke(null, [pet.Id, intruder, new PetDetails("Robado", "Perro", null, null, PetSex.Unknown, null), pet.CreatedAtUtc, DateTime.UtcNow, null])!;
        Assert.False(await context.Pets.UpdateOwned(forged, CancellationToken.None));

        var stillThere = await context.Pets.FindOwned(pet.Id, owner, CancellationToken.None);
        Assert.NotNull(stillThere);
        Assert.Equal("Firulais", stillThere.Name);
    }

    [RequiresEnvironmentFact(Uri, Database)]
    public async Task List_returns_only_the_owner_pets_newest_first_with_paging()
    {
        await using var context = await MongoTestContext.Create();
        var owner = Guid.NewGuid();
        var start = DateTime.UtcNow.AddMinutes(-10);
        for (var i = 0; i < 3; i++)
        {
            await context.Pets.Add(NewPet(owner, $"Mascota {i}", start.AddMinutes(i)), CancellationToken.None);
        }

        await context.Pets.Add(NewPet(Guid.NewGuid(), "Ajena"), CancellationToken.None);

        var firstPage = await context.Pets.ListByOwner(owner, skip: 0, take: 2, CancellationToken.None);
        var secondPage = await context.Pets.ListByOwner(owner, skip: 2, take: 2, CancellationToken.None);

        Assert.Equal(["Mascota 2", "Mascota 1"], firstPage.Select(p => p.Name));
        Assert.Equal(["Mascota 0"], secondPage.Select(p => p.Name));
        Assert.All(firstPage.Concat(secondPage), p => Assert.Equal(owner, p.OwnerId));
    }

    [RequiresEnvironmentFact(Uri, Database)]
    public async Task Count_by_owner_counts_only_the_owner_pets_and_follows_deletes()
    {
        await using var context = await MongoTestContext.Create();
        var owner = Guid.NewGuid();
        var first = NewPet(owner, "Uno");
        await context.Pets.Add(first, CancellationToken.None);
        await context.Pets.Add(NewPet(owner, "Dos"), CancellationToken.None);
        await context.Pets.Add(NewPet(Guid.NewGuid(), "Ajena"), CancellationToken.None);

        Assert.Equal(2, await context.Pets.CountByOwner(owner, CancellationToken.None));
        Assert.Equal(0, await context.Pets.CountByOwner(Guid.NewGuid(), CancellationToken.None));

        Assert.True(await context.Pets.DeleteOwned(first.Id, owner, CancellationToken.None));
        Assert.Equal(1, await context.Pets.CountByOwner(owner, CancellationToken.None));
    }

    [RequiresEnvironmentFact(Uri, Database)]
    public async Task Owner_update_is_persisted()
    {
        await using var context = await MongoTestContext.Create();
        var owner = Guid.NewGuid();
        var pet = NewPet(owner);
        await context.Pets.Add(pet, CancellationToken.None);

        pet.Update(new PetDetails("Max", "Perro", null, null, PetSex.Male, null), DateTime.UtcNow.AddSeconds(1));
        Assert.True(await context.Pets.UpdateOwned(pet, CancellationToken.None));

        var stored = await context.Pets.FindOwned(pet.Id, owner, CancellationToken.None);
        Assert.NotNull(stored);
        Assert.Equal("Max", stored.Name);
        Assert.Null(stored.Breed);
        Assert.Null(stored.BirthDate);
        Assert.Null(stored.PhotoUrl);
    }

    [RequiresEnvironmentFact(Uri, Database)]
    public async Task Delete_removes_the_document_physically()
    {
        await using var context = await MongoTestContext.Create();
        var owner = Guid.NewGuid();
        var pet = NewPet(owner);
        await context.Pets.Add(pet, CancellationToken.None);

        Assert.True(await context.Pets.DeleteOwned(pet.Id, owner, CancellationToken.None));
        Assert.False(await context.Pets.DeleteOwned(pet.Id, owner, CancellationToken.None));

        var raw = context.Database.GetCollection<BsonDocument>(context.Names.Pets);
        Assert.Equal(0, await raw.CountDocumentsAsync(new BsonDocument("_id", new BsonBinaryData(pet.Id, GuidRepresentation.Standard))));
        Assert.Null(await context.Pets.FindOwned(pet.Id, owner, CancellationToken.None));
    }

    [RequiresEnvironmentFact(Uri, Database)]
    public async Task Owner_index_exists_and_creation_is_idempotent()
    {
        await using var context = await MongoTestContext.Create();
        await context.Indexes.EnsureIndexes(CancellationToken.None);

        var indexes = await (await context.Database.GetCollection<BsonDocument>(context.Names.Pets).Indexes.ListAsync()).ToListAsync();
        var owner = Assert.Single(indexes, index => index["name"] == MongoIndexInitializer.PetOwnerIndex);

        Assert.Equal(1, owner["key"]["ownerId"].ToInt32());
        Assert.Equal(-1, owner["key"]["createdAtUtc"].ToInt32());
    }

    [RequiresEnvironmentFact(Uri, Database)]
    public async Task Owner_listing_query_is_served_by_the_owner_index()
    {
        await using var context = await MongoTestContext.Create();
        var owner = Guid.NewGuid();
        for (var i = 0; i < 3; i++)
        {
            await context.Pets.Add(NewPet(owner, $"Mascota {i}"), CancellationToken.None);
            await context.Pets.Add(NewPet(Guid.NewGuid(), $"Ajena {i}"), CancellationToken.None);
        }

        // Misma forma que ListByOwner: filtro por propietario y orden por fecha de creación descendente.
        var explain = new BsonDocument("explain", new BsonDocument
        {
            { "find", context.Names.Pets },
            { "filter", new BsonDocument("ownerId", new BsonBinaryData(owner, GuidRepresentation.Standard)) },
            { "sort", new BsonDocument { { "createdAtUtc", -1 }, { "_id", -1 } } },
        });
        var result = await context.Database.RunCommandAsync<BsonDocument>(explain);
        var winningPlan = result["queryPlanner"]["winningPlan"].ToJson();

        Assert.Contains("IXSCAN", winningPlan, StringComparison.Ordinal);
        Assert.Contains(MongoIndexInitializer.PetOwnerIndex, winningPlan, StringComparison.Ordinal);
        Assert.DoesNotContain("COLLSCAN", winningPlan, StringComparison.Ordinal);
    }

    [RequiresEnvironmentFact(Uri, Database)]
    public async Task Concurrent_deletes_of_the_same_pet_let_exactly_one_succeed_and_later_updates_find_nothing()
    {
        await using var context = await MongoTestContext.Create();
        var owner = Guid.NewGuid();
        var pet = NewPet(owner);
        await context.Pets.Add(pet, CancellationToken.None);

        var results = await Task.WhenAll(Enumerable.Range(0, 5)
            .Select(_ => context.Pets.DeleteOwned(pet.Id, owner, CancellationToken.None)));

        Assert.Equal(1, results.Count(deleted => deleted));
        pet.Update(new PetDetails("Max", "Perro", null, null, PetSex.Male, null), DateTime.UtcNow.AddSeconds(1));
        Assert.False(await context.Pets.UpdateOwned(pet, CancellationToken.None));
        Assert.Null(await context.Pets.FindOwned(pet.Id, owner, CancellationToken.None));
    }
}
