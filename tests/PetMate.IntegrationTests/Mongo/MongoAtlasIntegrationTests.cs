using Microsoft.Extensions.Diagnostics.HealthChecks;
using MongoDB.Bson;
using MongoDB.Driver;
using PetMate.Infrastructure.Health;

namespace PetMate.IntegrationTests.Mongo;

/// <summary>
/// Pruebas contra la base Atlas de pruebas (<c>MONGODB_TEST_URI</c> / <c>MONGODB_TEST_DATABASE</c>).
/// Quedan Skipped mientras no existan credenciales Atlas; no se consideran aprobadas.
/// </summary>
public sealed class MongoAtlasIntegrationTests
{
    private const string UriVariable = "MONGODB_TEST_URI";
    private const string DatabaseVariable = "MONGODB_TEST_DATABASE";

    [RequiresEnvironmentFact(UriVariable, DatabaseVariable)]
    public async Task Test_database_is_distinct_from_development_and_accepts_basic_persistence()
    {
        var testDatabase = Environment.GetEnvironmentVariable(DatabaseVariable)!;
        Assert.NotEqual(Environment.GetEnvironmentVariable("MONGODB_DATABASE"), testDatabase);
        Assert.Contains("test", testDatabase, StringComparison.OrdinalIgnoreCase);

        var client = new MongoClient(Environment.GetEnvironmentVariable(UriVariable));
        var collection = client.GetDatabase(testDatabase).GetCollection<BsonDocument>("it_" + Guid.NewGuid().ToString("N"));
        try
        {
            await collection.InsertOneAsync(new BsonDocument("name", "luna"));

            Assert.Equal(1, await collection.CountDocumentsAsync(FilterDefinition<BsonDocument>.Empty));
        }
        finally
        {
            await client.GetDatabase(testDatabase).DropCollectionAsync(collection.CollectionNamespace.CollectionName);
        }
    }

    [RequiresEnvironmentFact(UriVariable, DatabaseVariable)]
    public async Task Health_check_reports_healthy_against_atlas_test_database()
    {
        var client = new MongoClient(Environment.GetEnvironmentVariable(UriVariable));
        var check = new MongoHealthCheck(client.GetDatabase(Environment.GetEnvironmentVariable(DatabaseVariable)!));

        var result = await check.CheckHealthAsync(new HealthCheckContext());

        Assert.Equal(HealthStatus.Healthy, result.Status);
    }

    [Fact]
    public async Task Health_check_reports_unhealthy_without_details_when_mongo_is_unreachable()
    {
        var settings = MongoClientSettings.FromConnectionString("mongodb://usuario:clave@127.0.0.1:1/");
        settings.ServerSelectionTimeout = TimeSpan.FromMilliseconds(300);
        var check = new MongoHealthCheck(new MongoClient(settings).GetDatabase("petmate_test_ficticio"));

        var result = await check.CheckHealthAsync(new HealthCheckContext());

        Assert.Equal(HealthStatus.Unhealthy, result.Status);
        Assert.Null(result.Exception);
        Assert.True(string.IsNullOrEmpty(result.Description));
    }
}
