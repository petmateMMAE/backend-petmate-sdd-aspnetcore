using MongoDB.Driver;
using PetMate.Infrastructure.Persistence;

namespace PetMate.IntegrationTests.Mongo;

/// <summary>
/// Protege la base de desarrollo: solo se escribe en <c>MONGODB_TEST_DATABASE</c>, que debe ser distinta de
/// <c>MONGODB_DATABASE</c> y contener «test». Nunca se leen ni se usan la URI ni las credenciales de desarrollo.
/// </summary>
public static class MongoTestGuard
{
    public static void EnsureSafe(string? developmentDatabase, string? testDatabase)
    {
        if (string.IsNullOrWhiteSpace(testDatabase))
        {
            throw new InvalidOperationException("MONGODB_TEST_DATABASE es obligatoria para las pruebas de integración.");
        }

        if (!testDatabase.Contains("test", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("La base de pruebas debe contener 'test' en su nombre.");
        }

        if (string.Equals(developmentDatabase, testDatabase, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("La base de pruebas no puede ser la de desarrollo.");
        }
    }
}

/// <summary>Contexto por prueba: colecciones con prefijo único que se eliminan al terminar (nunca otras).</summary>
internal sealed class MongoTestContext : IAsyncDisposable
{
    private readonly IMongoDatabase database;

    private MongoTestContext(IMongoDatabase database, MongoCollectionNames names)
    {
        this.database = database;
        Names = names;
        Users = new MongoUserRepository(database, names);
        Sessions = new MongoSessionRepository(database, names);
        Pets = new MongoPetRepository(database, names);
        Vaccines = new MongoVaccineRepository(database, names);
        Dewormings = new MongoDewormingRepository(database, names);
        Weights = new MongoWeightRepository(database, names);
        Allergies = new MongoAllergyRepository(database, names);
        PasswordResets = new MongoPasswordResetRepository(database, names);
        Indexes = new MongoIndexInitializer(database, names);
    }

    public MongoCollectionNames Names { get; }

    public MongoUserRepository Users { get; }

    public MongoSessionRepository Sessions { get; }

    public MongoPetRepository Pets { get; }

    public MongoVaccineRepository Vaccines { get; }

    public MongoDewormingRepository Dewormings { get; }

    public MongoWeightRepository Weights { get; }

    public MongoAllergyRepository Allergies { get; }

    public MongoPasswordResetRepository PasswordResets { get; }

    public MongoIndexInitializer Indexes { get; }

    public IMongoDatabase Database => database;

    public static async Task<MongoTestContext> Create()
    {
        var testDatabase = Environment.GetEnvironmentVariable("MONGODB_TEST_DATABASE");
        MongoTestGuard.EnsureSafe(Environment.GetEnvironmentVariable("MONGODB_DATABASE"), testDatabase);

        var settings = MongoClientSettings.FromConnectionString(Environment.GetEnvironmentVariable("MONGODB_TEST_URI"));
        settings.ServerSelectionTimeout = TimeSpan.FromSeconds(15);
        var database = new MongoClient(settings).GetDatabase(testDatabase);
        var context = new MongoTestContext(database, new MongoCollectionNames($"it_{Guid.NewGuid():N}"[..11] + "_"));
        try
        {
            await context.Indexes.EnsureIndexes(CancellationToken.None);
        }
        catch
        {
            await context.DisposeAsync();
            throw;
        }

        return context;
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var name in new[] { Names.Users, Names.Sessions, Names.Tokens, Names.Pets, Names.Vaccines, Names.Dewormings, Names.Weights, Names.Allergies, Names.PasswordResets })
        {
            await database.DropCollectionAsync(name);
        }
    }
}
