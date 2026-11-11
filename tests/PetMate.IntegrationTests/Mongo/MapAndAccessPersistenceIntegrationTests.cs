using PetMate.Application.Businesses;
using PetMate.Domain.Access;
using PetMate.Domain.Businesses;
using PetMate.Domain.Pets;
using PetMate.Infrastructure.Persistence;

namespace PetMate.IntegrationTests.Mongo;

/// <summary>
/// pm-116, pm-118, pm-122, pm-125, pm-129 contra la base Atlas de pruebas: consulta por cercanía con <c>$geoNear</c>, búsqueda por
/// nombre, autorizaciones con su cancelación, resolución de QR y registro de escaneos. Skipped si no hay credenciales de prueba.
/// </summary>
public sealed class MapAndAccessPersistenceIntegrationTests
{
    private const string Uri = "MONGODB_TEST_URI";
    private const string Database = "MONGODB_TEST_DATABASE";
    private static readonly DateTime Now = DateTime.UtcNow;

    private static Business NewBusiness(string name, double lat, double lng, BusinessType type)
        => Business.Create(Guid.NewGuid(), new BusinessDetails(type, name, "5512345678", null, "Calle 1", null, lat, lng, type == BusinessType.Veterinary ? "C-1" : null, null), Now);

    [RequiresEnvironmentFact(Uri, Database)]
    public async Task Nearby_orders_by_distance_filters_by_type_and_accent_insensitive_name_and_counts_the_total()
    {
        await using var context = await MongoTestContext.Create();
        var near = NewBusiness("Veterinaria Peñalosa", 19.4330, -99.1330, BusinessType.Veterinary);
        var mid = NewBusiness("Tienda Peñalosa", 19.4500, -99.1330, BusinessType.Store);
        var far = NewBusiness("Estética Luna", 25.6866, -100.3161, BusinessType.Grooming);
        foreach (var business in new[] { near, mid, far })
        {
            await context.Businesses.TryAdd(business, CancellationToken.None);
        }

        var all = await context.Businesses.FindNearby(new NearbyQuery(19.4326, -99.1332, 5000, null, null, 0, 20), CancellationToken.None);
        var stores = await context.Businesses.FindNearby(new NearbyQuery(19.4326, -99.1332, 50000, [BusinessType.Store], null, 0, 20), CancellationToken.None);
        var searched = await context.Businesses.FindNearby(new NearbyQuery(19.4326, -99.1332, 50000, null, Business.SearchKey("PEÑALOSA"), 0, 20), CancellationToken.None);
        var regexLike = await context.Businesses.FindNearby(new NearbyQuery(19.4326, -99.1332, 50000, null, ".*", 0, 20), CancellationToken.None);
        var page2 = await context.Businesses.FindNearby(new NearbyQuery(19.4326, -99.1332, 50000, null, null, 1, 1), CancellationToken.None);

        Assert.Equal([near.Id, mid.Id], all.Items.Select(i => i.Business.Id));
        Assert.True(all.Items[0].DistanceMeters < all.Items[1].DistanceMeters);
        Assert.Equal(2, all.Total);
        Assert.Equal([mid.Id], stores.Items.Select(i => i.Business.Id));
        Assert.Equal(2, searched.Total);
        Assert.Empty(regexLike.Items);
        Assert.Equal((mid.Id, 3), (page2.Items.Single().Business.Id, page2.Total));
    }

    [RequiresEnvironmentFact(Uri, Database)]
    public async Task Grants_find_only_active_ones_for_the_business_cancel_atomically_and_get_deleted_with_the_pet()
    {
        await using var context = await MongoTestContext.Create();
        var grants = new MongoAccessGrantRepository(context.Database, context.Names);
        var petId = Guid.NewGuid();
        var owner = Guid.NewGuid();
        var business = Guid.NewGuid();
        await grants.Add(AccessGrant.Create(petId, owner, business, [PetDataCategory.Vaccines], 30, Now), CancellationToken.None);
        await grants.Add(AccessGrant.Create(petId, owner, null, [], 1440, Now), CancellationToken.None);
        await grants.Add(AccessGrant.Create(petId, owner, Guid.NewGuid(), [], 30, Now), CancellationToken.None);

        Assert.Equal(2, (await grants.FindActive(petId, business, Now.AddMinutes(1), CancellationToken.None)).Count);
        Assert.Single(await grants.FindActive(petId, business, Now.AddMinutes(31), CancellationToken.None));
        Assert.Equal(0, await grants.CancelActiveForPet(petId, Guid.NewGuid(), Now.AddMinutes(2), CancellationToken.None));
        Assert.Equal(3, await grants.CancelActiveForPet(petId, owner, Now.AddMinutes(2), CancellationToken.None));
        Assert.Empty(await grants.FindActive(petId, business, Now.AddMinutes(3), CancellationToken.None));
        Assert.Equal(3, await grants.DeleteByPet(petId, owner, CancellationToken.None));
    }

    [RequiresEnvironmentFact(Uri, Database)]
    public async Task A_pet_is_found_by_its_token_or_short_code_in_any_case_and_scan_logs_are_deleted_with_it()
    {
        await using var context = await MongoTestContext.Create();
        var pet = Pet.Create(Guid.NewGuid(), new PetDetails("Firulais", "Perro", null, null, PetSex.Unknown, null), Now);
        await context.Pets.Add(pet, CancellationToken.None);

        Assert.Equal(pet.Id, (await context.Pets.FindByQr(pet.Qr.Token, CancellationToken.None))!.Id);
        Assert.Equal(pet.Id, (await context.Pets.FindByQr(pet.Qr.Code.ToLowerInvariant(), CancellationToken.None))!.Id);
        Assert.Null(await context.Pets.FindByQr(new string('a', 43), CancellationToken.None));
        Assert.Null(await context.Pets.FindByQr("corto", CancellationToken.None));

        var logs = new MongoScanLogRepository(context.Database, context.Names);
        await logs.Add(ScanLog.Record(pet.Id, pet.Name, null, Guid.NewGuid(), Now), CancellationToken.None);
        await logs.Add(ScanLog.Record(pet.Id, pet.Name, null, Guid.NewGuid(), Now), CancellationToken.None);
        Assert.Equal(2, await logs.DeleteByPet(pet.Id, CancellationToken.None));
    }
}
