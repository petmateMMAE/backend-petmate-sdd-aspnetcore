using MongoDB.Bson;
using PetMate.Application.Businesses;
using MongoDB.Driver;
using PetMate.Domain.Businesses;
using PetMate.Infrastructure.Persistence;

namespace PetMate.IntegrationTests.Mongo;

/// <summary>
/// Los índices de negocios se crean por separado y el geoespacial sobrevive a un fallo del único (pm-105).
///
/// Estas pruebas existen por un defecto real: los dos índices se creaban en un solo <c>CreateManyAsync</c>,
/// que es atómico. Bastaba un par de negocios del mismo dueño en la colección para que el índice único fallara
/// y, con él, tampoco se creara el geoespacial. Sin ese índice, <c>$geoNear</c> falla en toda consulta de
/// cercanía: el mapa público —la pantalla principal de la app— devolvería HTTP 500.
/// </summary>
public sealed class BusinessIndexIntegrationTests
{
    private const string Uri = "MONGODB_TEST_URI";
    private const string Database = "MONGODB_TEST_DATABASE";
    private static readonly DateTime Now = DateTime.UtcNow;

    private static Business NewBusiness(Guid ownerId, double lat, double lng) => Business.Create(
        ownerId,
        new BusinessDetails(BusinessType.Veterinary, "Veterinaria de prueba", "5512345678", null, "Calle 1", null, lat, lng, "C-1", null),
        Now);

    [RequiresEnvironmentFact(Uri, Database)]
    public async Task Ensure_indexes_creates_both_business_indexes()
    {
        await using var context = await MongoTestContext.Create();
        await context.Indexes.EnsureIndexes(CancellationToken.None);

        var names = await IndexNames(context);

        Assert.Contains(MongoIndexInitializer.BusinessLocationIndex, names);
        Assert.Contains(MongoIndexInitializer.BusinessOwnerIndex, names);
    }

    [RequiresEnvironmentFact(Uri, Database)]
    public async Task Geospatial_index_is_created_even_when_the_unique_index_cannot_be()
    {
        await using var context = await MongoTestContext.Create();

        // Dos negocios del MISMO dueño. El repositorio impide este caso, así que se escriben directo a la
        // colección: lo que se reproduce es una base que YA quedó con datos duplicados, no una operación nueva.
        var owner = Guid.NewGuid();
        var collection = context.Database.GetCollection<BsonDocument>(context.Names.Businesses);
        await context.Businesses.TryAdd(NewBusiness(owner, 20.05, -99.34), CancellationToken.None);

        var duplicate = await collection.Find(FilterDefinition<BsonDocument>.Empty).FirstAsync();
        duplicate["_id"] = new BsonBinaryData(Guid.NewGuid(), GuidRepresentation.Standard);
        await collection.InsertOneAsync(duplicate);

        // El inicializador falla, y es lo esperado: el índice único es imposible con datos duplicados.
        await Assert.ThrowsAnyAsync<MongoException>(
            () => context.Indexes.EnsureIndexes(CancellationToken.None));

        // Lo que importa: el geoespacial SÍ quedó creado. Antes del arreglo este Assert fallaba, porque la
        // operación atómica descartaba ambos índices y dejaba el mapa público sin poder responder.
        var names = await IndexNames(context);
        Assert.Contains(MongoIndexInitializer.BusinessLocationIndex, names);
        Assert.DoesNotContain(MongoIndexInitializer.BusinessOwnerIndex, names);
    }

    [RequiresEnvironmentFact(Uri, Database)]
    public async Task Nearby_search_works_with_the_geospatial_index()
    {
        await using var context = await MongoTestContext.Create();
        await context.Indexes.EnsureIndexes(CancellationToken.None);
        await context.Businesses.TryAdd(NewBusiness(Guid.NewGuid(), 20.0539, -99.3417), CancellationToken.None);

        // Si el índice 2dsphere faltara, $geoNear lanzaría y esto no devolvería nada.
        var page = await context.Businesses.FindNearby(
            new NearbyQuery(20.0539, -99.3417, 5000, null, null, 0, 20),
            CancellationToken.None);

        Assert.Equal(1, page.Total);
        Assert.True(page.Items[0].DistanceMeters < 1.0, "el negocio está en el punto consultado");
    }

    private static async Task<IReadOnlyCollection<string>> IndexNames(MongoTestContext context)
    {
        var collection = context.Database.GetCollection<BsonDocument>(context.Names.Businesses);
        var cursor = await collection.Indexes.ListAsync();
        var indexes = await cursor.ToListAsync();
        return indexes.Select(index => index["name"].AsString).ToArray();
    }
}
