using Microsoft.Extensions.Configuration;
using PetMate.Infrastructure.Configuration;

namespace PetMate.SecurityTests.Configuration;

public sealed class InfrastructureSettingsTests
{
    private const string AtlasUri = "mongodb+srv://usuario_ficticio:password_ficticio@cluster-ficticio.mongodb.net/?retryWrites=true&w=majority";
    private const string RedisAuth = "redis-auth:6379,password=pw-ficticio-a,ssl=True";
    private const string RedisCache = "redis-cache:6379,password=pw-ficticio-b,ssl=True";

    [Fact]
    public void Parse_AcceptsFictitiousAtlasUriWithoutConnecting()
    {
        var settings = InfrastructureSettings.Parse(Build(), allowPlainRedis: false);

        Assert.Equal(AtlasUri, settings.Mongo.Uri);
        Assert.Equal("petmate_test_ficticio", settings.Mongo.Database);
    }

    [Fact]
    public void Parse_LocalAndAtlasUrisShareTheSameContract()
    {
        // La estructura de configuración es idéntica; solo cambia el valor del entorno.
        var local = InfrastructureSettings.Parse(Build(mongoUri: "mongodb://usuario:clave@host.invalid:27017/"), false);
        var atlas = InfrastructureSettings.Parse(Build(), false);

        Assert.IsType<MongoSettings>(local.Mongo);
        Assert.IsType<MongoSettings>(atlas.Mongo);
    }

    [Theory]
    [InlineData("MONGODB_URI")]
    [InlineData("MONGODB_DATABASE")]
    [InlineData("REDIS_AUTH_CONNECTION")]
    [InlineData("REDIS_CACHE_CONNECTION")]
    public void Parse_MissingKey_ReportsKeyNameOnly(string key)
    {
        var exception = Assert.Throws<InvalidOperationException>(
            () => InfrastructureSettings.Parse(Build(remove: key), false));

        Assert.Contains(key, exception.Message, StringComparison.Ordinal);
        Assert.DoesNotContain("password_ficticio", exception.Message, StringComparison.Ordinal);
        Assert.DoesNotContain("pw-ficticio", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Parse_InvalidMongoUri_DoesNotLeakValue()
    {
        var exception = Assert.Throws<InvalidOperationException>(
            () => InfrastructureSettings.Parse(Build(mongoUri: "http://secreto-que-no-debe-salir"), false));

        Assert.Contains("MONGODB_URI", exception.Message, StringComparison.Ordinal);
        Assert.DoesNotContain("secreto-que-no-debe-salir", exception.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("db.con.punto")]
    [InlineData("db con espacio")]
    [InlineData("db/slash")]
    public void Parse_InvalidDatabaseName_IsRejected(string name)
    {
        var exception = Assert.Throws<InvalidOperationException>(
            () => InfrastructureSettings.Parse(Build(database: name), false));

        Assert.Contains("MONGODB_DATABASE", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Parse_RedisWithoutTls_RejectedOutsideDevelopment()
    {
        var plain = "redis-auth:6379,password=pw-ficticio-a,ssl=False";

        Assert.Throws<InvalidOperationException>(() => InfrastructureSettings.Parse(Build(redisAuth: plain), false));
        var allowed = InfrastructureSettings.Parse(Build(redisAuth: plain), allowPlainRedis: true);
        Assert.Equal(plain, allowed.RedisAuth.Connection);
    }

    [Fact]
    public void Parse_RedisWithoutPassword_IsRejectedEvenInDevelopment()
        => Assert.Throws<InvalidOperationException>(
            () => InfrastructureSettings.Parse(Build(redisCache: "redis-cache:6379,ssl=False"), true));

    [Fact]
    public void Parse_AuthAndCacheMustBeIndependentInstances()
    {
        var exception = Assert.Throws<InvalidOperationException>(
            () => InfrastructureSettings.Parse(Build(redisCache: RedisAuth), false));

        Assert.Contains("independientes", exception.Message, StringComparison.Ordinal);
    }

    private static IConfiguration Build(
        string mongoUri = AtlasUri,
        string database = "petmate_test_ficticio",
        string redisAuth = RedisAuth,
        string redisCache = RedisCache,
        string? remove = null)
    {
        var values = new Dictionary<string, string?>
        {
            ["MONGODB_URI"] = mongoUri,
            ["MONGODB_DATABASE"] = database,
            ["REDIS_AUTH_CONNECTION"] = redisAuth,
            ["REDIS_CACHE_CONNECTION"] = redisCache,
        };
        if (remove is not null)
        {
            values.Remove(remove);
        }

        return new ConfigurationBuilder().AddInMemoryCollection(values).Build();
    }
}
