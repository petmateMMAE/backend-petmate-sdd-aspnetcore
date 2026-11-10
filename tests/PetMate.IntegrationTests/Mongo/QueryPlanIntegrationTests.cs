using MongoDB.Bson;
using MongoDB.Driver;
using PetMate.Infrastructure.Persistence;

namespace PetMate.IntegrationTests.Mongo;

/// <summary>
/// Evidencia <c>explain</c> (pm-052 CA-02): las consultas críticas usan un índice (IXSCAN o búsqueda por _id) y nunca recorren la
/// colección (COLLSCAN). Se ejecuta contra la base Atlas de pruebas con los índices reales creados por el inicializador.
/// </summary>
public sealed class QueryPlanIntegrationTests
{
    private const string Uri = "MONGODB_TEST_URI";
    private const string Database = "MONGODB_TEST_DATABASE";

    [RequiresEnvironmentFact(Uri, Database)]
    public async Task Critical_queries_use_an_index_and_never_scan_the_collection()
    {
        await using var context = await MongoTestContext.Create();
        await context.Indexes.EnsureIndexes(CancellationToken.None);
        var id = new BsonBinaryData(Guid.NewGuid(), GuidRepresentation.Standard);

        var cases = new (string Name, string Collection, BsonDocument Filter, BsonDocument? Sort, string ExpectedIndex)[]
        {
            ("Login/registro por correo", context.Names.Users, new BsonDocument("email", "ana@example.com"), null, MongoIndexInitializer.UserEmailIndex),
            ("Perfil por id", context.Names.Users, new BsonDocument("_id", id), null, "_id_"),
            ("Refresh por hash de token", context.Names.Tokens, new BsonDocument("tokenHash", "hash"), null, MongoIndexInitializer.TokenHashIndex),
            ("Tokens de una sesión", context.Names.Tokens, new BsonDocument("sessionId", id), null, MongoIndexInitializer.TokenSessionIndex),
            ("Sesiones de un usuario", context.Names.Sessions, new BsonDocument("userId", id), null, MongoIndexInitializer.SessionUserIndex),
            ("Sesión por id", context.Names.Sessions, new BsonDocument("_id", id), null, "_id_"),
            ("Mascota propia por id", context.Names.Pets, new BsonDocument { { "_id", id }, { "ownerId", id } }, null, "_id_"),
            ("Listado de mascotas del propietario", context.Names.Pets, new BsonDocument("ownerId", id), new BsonDocument { { "createdAtUtc", -1 }, { "_id", -1 } }, MongoIndexInitializer.PetOwnerIndex),
        };

        foreach (var (name, collection, filter, sort, expectedIndex) in cases)
        {
            var find = new BsonDocument { { "find", collection }, { "filter", filter }, { "limit", 20 } };
            if (sort is not null)
            {
                find["sort"] = sort;
            }

            var explain = await context.Database.RunCommandAsync<BsonDocument>(new BsonDocument { { "explain", find }, { "verbosity", "queryPlanner" } });
            var plan = explain["queryPlanner"]["winningPlan"].ToJson();

            Assert.DoesNotContain("COLLSCAN", plan, StringComparison.Ordinal);
            Assert.Contains("IXSCAN", plan, StringComparison.Ordinal);
            Assert.True(plan.Contains(expectedIndex, StringComparison.Ordinal), $"{name}: se esperaba el índice {expectedIndex}.");
        }
    }
}
