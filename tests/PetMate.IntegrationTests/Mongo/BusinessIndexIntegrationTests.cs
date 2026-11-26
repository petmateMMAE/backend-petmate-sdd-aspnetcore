using MongoDB.Bson;
using MongoDB.Driver;
using PetMate.Infrastructure.Persistence;

namespace PetMate.IntegrationTests.Mongo;

/// <summary>
/// Los índices de negocios se crean por separado y el geoespacial sobrevive a un fallo del único (pm-105).
///
/// Estas pruebas existen por un defecto real: los dos índices se creaban en un solo <c>CreateManyAsync</c>,
/// que es atómico. Bastaba un par de negocios del mismo dueño en la colección para que el índice único fallara
/// y, con él, tampoco se creara el geoespacial. Sin ese índice, <c>$geoNear</c> falla en toda consulta de
/// cercanía: el mapa público —la pantalla principal de la app— devolvía HTTP 500.
/// </summary>
public sealed class BusinessIndexIntegrationTests
{
    private const string Uri = "MONGODB_TEST_URI";
    private const string Database = "MONGODB_TEST_DATABASE";

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
        var businesses = context.Database.GetCollection<BsonDocument>(context.Names.Businesses);

        // Dos negocios del MISMO dueño: con esto el índice único de ownerId no se puede crear.
        var owner = new BsonBinaryData(Guid.NewGuid(), GuidRepresentation.Standard);
        await businesses.InsertManyAsync(
        [
            Document(owner, longitude: -99.34, latitude: 20.05),
            Document(owner, longitude: -99.35, latitude: 20.06),
        ]);

        // El inicializador falla: es lo esperado, porque el único es imposible con datos duplicados.
        await Assert.ThrowsAnyAsync<MongoException>(
            () => context.Indexes.EnsureIndexes(CancellationToken.None));

        // Lo que importa: el geoespacial SÍ quedó creado. Antes del arreglo este Assert fallaba, porque la
        // operación atómica descartaba ambos índices.
        var names = await IndexNames(context);
        Assert.Contains(MongoIndexInitializer.BusinessLocationIndex, names);
        Assert.DoesNotContain(MongoIndexInitializer.BusinessOwnerIndex, names);
    }

    [RequiresEnvironmentFact(Uri, Database)]
    public async Task Nearby_search_works_with_the_geospatial_index()
    {
        await using var context = await MongoTestContext.Create();
        await context.Indexes.EnsureIndexes(CancellationToken.None);
        var businesses = context.Database.GetCollection<BsonDocument>(context.Names.Businesses);

        await businesses.InsertOneAsync(Document(
            new BsonBinaryData(Guid.NewGuid(), GuidRepresentation.Standard),
            longitude: -99.3417,
            latitude: 20.0539));

        // La misma forma de consulta que usa FindNearby: si el índice faltara, $geoNear lanzaría.
        var pipeline = new[]
        {
            new BsonDocument("$geoNear", new BsonDocument
            {
                { "near", new BsonDocument { { "type", "Point" }, { "coordinates", new BsonArray { -99.3417, 20.0539 } } } },
                { "distanceField", "distanceMeters" },
                { "maxDistance", 5000.0 },
                { "spherical", true },
                { "query", new BsonDocument() },
            }),
        };

        var results = await (await businesses.AggregateAsync<BsonDocument>(pipeline)).ToListAsync();

        Assert.Single(results);
        Assert.True(results[0]["distanceMeters"].ToDouble() < 1.0, "el negocio está en el punto consultado");
    }

    private static async Task<IReadOnlyCollection<string>> IndexNames(MongoTestContext context)
    {
        var collection = context.Database.GetCollection<BsonDocument>(context.Names.Businesses);
        var cursor = await collection.Indexes.ListAsync();
        var indexes = await cursor.ToListAsync();
        return indexes.Select(index => index["name"].AsString).ToArray();
    }

    private static BsonDocument Document(BsonBinaryData ownerId, double longitude, double latitude) => new()
    {
        { "_id", new BsonBinaryData(Guid.NewGuid(), GuidRepresentation.Standard) },
        { "ownerId", ownerId },
        { "name", "Veterinaria de prueba" },
        { "nameSearch", "veterinaria de prueba" },
        { "type", 0 },
        { "location", new BsonDocument { { "type", "Point" }, { "coordinates", new BsonArray { longitude, latitude } } } },
    };
}
