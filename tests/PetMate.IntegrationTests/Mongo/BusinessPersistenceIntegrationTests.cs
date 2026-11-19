using MongoDB.Bson;
using MongoDB.Driver;
using PetMate.Application.Businesses;
using PetMate.Domain.Businesses;

namespace PetMate.IntegrationTests.Mongo;

/// <summary>
/// pm-105 a pm-115 contra la base Atlas de pruebas: un negocio por cuenta (índice único), ubicación GeoJSON con índice 2dsphere,
/// horario estructurado, cédula privada y catálogo con orden estable. Skipped si no hay credenciales de prueba reales.
/// </summary>
public sealed class BusinessPersistenceIntegrationTests
{
    private const string Uri = "MONGODB_TEST_URI";
    private const string Database = "MONGODB_TEST_DATABASE";

    private static Business NewBusiness(Guid owner, double lat = 19.4326, double lng = -99.1332, BusinessType type = BusinessType.Veterinary)
        => Business.Create(
            owner,
            new BusinessDetails(
                type, "Vet Sol", "5512345678", "5598765432", "Calle 5", "Urgencias", lat, lng, type == BusinessType.Veterinary ? "CED-1" : null,
                new ScheduleInput(new Dictionary<string, IReadOnlyList<(string Open, string Close)>> { ["monday"] = [("09:00", "14:00"), ("15:00", "18:00")] })),
            DateTime.UtcNow);

    [RequiresEnvironmentFact(Uri, Database)]
    public async Task Business_round_trips_with_schedule_location_and_the_private_license()
    {
        await using var context = await MongoTestContext.Create();
        var owner = Guid.NewGuid();
        var business = NewBusiness(owner);

        Assert.True(await context.Businesses.TryAdd(business, CancellationToken.None));
        var stored = await context.Businesses.FindByOwner(owner, CancellationToken.None);

        Assert.NotNull(stored);
        Assert.Equal(business.Id, stored.Id);
        Assert.Equal(("Vet Sol", "5512345678", "5598765432"), (stored.Name, stored.Phone, stored.WhatsApp));
        Assert.Equal((19.4326, -99.1332), (stored.Latitude, stored.Longitude));
        Assert.Equal("CED-1", stored.LicenseNumber);
        Assert.Equal(2, stored.Schedule.RangesFor(DayOfWeek.Monday).Count);
        Assert.Equal(DateTimeKind.Utc, stored.CreatedAtUtc.Kind);
    }

    [RequiresEnvironmentFact(Uri, Database)]
    public async Task The_unique_index_allows_a_single_business_per_account_even_when_creations_race()
    {
        await using var context = await MongoTestContext.Create();
        var owner = Guid.NewGuid();

        var results = await Task.WhenAll(Enumerable.Range(0, 6).Select(_ => context.Businesses.TryAdd(NewBusiness(owner), CancellationToken.None)));

        Assert.Equal(1, results.Count(r => r));
        Assert.Equal(1, await context.Database.GetCollection<BsonDocument>(context.Names.Businesses).CountDocumentsAsync(new BsonDocument("ownerId", new BsonBinaryData(owner, GuidRepresentation.Standard))));
    }

    [RequiresEnvironmentFact(Uri, Database)]
    public async Task The_2dsphere_index_serves_proximity_queries_and_reflects_a_new_location()
    {
        await using var context = await MongoTestContext.Create();
        var near = NewBusiness(Guid.NewGuid(), 19.4326, -99.1332, BusinessType.Store);
        var far = NewBusiness(Guid.NewGuid(), 25.6866, -100.3161, BusinessType.Store);
        await context.Businesses.TryAdd(near, CancellationToken.None);
        await context.Businesses.TryAdd(far, CancellationToken.None);
        var collection = context.Database.GetCollection<BsonDocument>(context.Names.Businesses);
        var center = GeoPoint(-99.14, 19.43);

        var withinFiveKm = await collection.Find(new BsonDocument("location", new BsonDocument("$nearSphere", new BsonDocument { { "$geometry", center }, { "$maxDistance", 5000 } }))).ToListAsync();
        Assert.Equal([near.Id], withinFiveKm.Select(d => d["_id"].AsGuid));

        far.MoveTo(19.431, -99.135, DateTime.UtcNow);
        Assert.True(await context.Businesses.UpdateOwned(far, CancellationToken.None));
        var afterMove = await collection.Find(new BsonDocument("location", new BsonDocument("$nearSphere", new BsonDocument { { "$geometry", center }, { "$maxDistance", 5000 } }))).ToListAsync();
        Assert.Equal(2, afterMove.Count);
    }

    [RequiresEnvironmentFact(Uri, Database)]
    public async Task Update_and_delete_only_touch_the_business_of_the_account()
    {
        await using var context = await MongoTestContext.Create();
        var ana = NewBusiness(Guid.NewGuid());
        var luis = NewBusiness(Guid.NewGuid());
        await context.Businesses.TryAdd(ana, CancellationToken.None);
        await context.Businesses.TryAdd(luis, CancellationToken.None);

        ana.UpdateProfile(new BusinessProfile("Nuevo", null, "5511112222", null, null), DateTime.UtcNow);
        Assert.True(await context.Businesses.UpdateOwned(ana, CancellationToken.None));
        Assert.Equal("Nuevo", (await context.Businesses.FindByOwner(ana.OwnerId, CancellationToken.None))!.Name);
        Assert.Equal("Vet Sol", (await context.Businesses.FindByOwner(luis.OwnerId, CancellationToken.None))!.Name);

        Assert.False(await context.Businesses.DeleteByOwner(Guid.NewGuid(), CancellationToken.None));
        Assert.True(await context.Businesses.DeleteByOwner(ana.OwnerId, CancellationToken.None));
        Assert.Null(await context.Businesses.FindByOwner(ana.OwnerId, CancellationToken.None));
        Assert.NotNull(await context.Businesses.FindByOwner(luis.OwnerId, CancellationToken.None));
    }

    [RequiresEnvironmentFact(Uri, Database)]
    public async Task Services_keep_a_null_price_list_in_a_stable_order_and_are_scoped_to_their_business()
    {
        await using var context = await MongoTestContext.Create();
        var mine = Guid.NewGuid();
        var other = Guid.NewGuid();
        var now = DateTime.UtcNow;
        var services = Enumerable.Range(0, 5).Select(i => BusinessService.Create(mine, new ServiceDetails($"S{i}", null, i == 0 ? null : i * 10m), now)).ToArray();
        foreach (var service in services)
        {
            await context.BusinessServices.Add(service, CancellationToken.None);
        }

        var foreign = BusinessService.Create(other, new ServiceDetails("Ajeno", null, null), now);
        await context.BusinessServices.Add(foreign, CancellationToken.None);

        var page1 = await context.BusinessServices.ListByBusiness(mine, 0, 3, CancellationToken.None);
        var page2 = await context.BusinessServices.ListByBusiness(mine, 3, 3, CancellationToken.None);
        Assert.Equal(5, await context.BusinessServices.CountByBusiness(mine, CancellationToken.None));
        Assert.Equal(5, page1.Concat(page2).Select(s => s.Id).Distinct().Count());
        Assert.Null(page1.Concat(page2).Single(s => s.Name == "S0").Price);
        Assert.Equal(20m, page1.Concat(page2).Single(s => s.Name == "S2").Price);

        Assert.Null(await context.BusinessServices.FindOwned(foreign.Id, mine, CancellationToken.None));
        Assert.False(await context.BusinessServices.DeleteOwned(foreign.Id, mine, CancellationToken.None));
        Assert.True(await context.BusinessServices.DeleteOwned(services[1].Id, mine, CancellationToken.None));
        Assert.Equal(4, await context.BusinessServices.DeleteByBusiness(mine, CancellationToken.None));
        Assert.NotNull(await context.BusinessServices.FindOwned(foreign.Id, other, CancellationToken.None));
    }

    private static BsonDocument GeoPoint(double lng, double lat) => new() { { "type", "Point" }, { "coordinates", new BsonArray { lng, lat } } };
}
