using Microsoft.Extensions.Configuration;
using PetMate.Api.Configuration;

namespace PetMate.SecurityTests.Configuration;

/// <summary>
/// Verifica la validación de configuración crítica al arrancar (T004).
/// Los valores usados son ficticios y no se imprimen en ningún caso.
/// </summary>
public sealed class StartupConfigurationValidatorTests
{
    private const string DummyMongoUri = "mongodb://usuario-ficticio:clave-ficticia@host-ficticio/petmate";
    private const string DummyDatabase = "petmate_dev_ficticio";
    private const string DummyRedisAuth = "redis-auth-ficticio:6379";
    private const string DummyRedisCache = "redis-cache-ficticio:6379";
    private const string DummyIssuer = "https://issuer-ficticio.local";
    private const string DummyAudience = "audience-ficticia";
    private const string DummySigningKey = "clave-de-firma-ficticia-solo-pruebas";

    private static readonly string[] DummyValues =
    [
        DummyMongoUri,
        DummyDatabase,
        DummyRedisAuth,
        DummyRedisCache,
        DummyIssuer,
        DummyAudience,
        DummySigningKey,
    ];

    [Fact]
    public void GetMissingKeys_ReturnsOnlyAbsentOrEmptyKeys()
    {
        var configuration = CreateConfiguration(values =>
        {
            values.Remove("JWT_SIGNING_KEY");
            values["MONGODB_DATABASE"] = "   ";
        });

        var missing = StartupConfigurationValidator.GetMissingKeys(configuration);

        Assert.Collection(
            missing,
            key => Assert.Equal("MONGODB_DATABASE", key),
            key => Assert.Equal("JWT_SIGNING_KEY", key));
    }

    [Fact]
    public void GetMissingKeys_ReturnsEmpty_WhenAllKeysArePresent()
    {
        var configuration = CreateConfiguration();

        var missing = StartupConfigurationValidator.GetMissingKeys(configuration);

        Assert.Empty(missing);
    }

    [Fact]
    public void EnsureValid_DoesNotThrow_WhenAllRequiredKeysArePresent()
    {
        var configuration = CreateConfiguration();

        var exception = Record.Exception(() => StartupConfigurationValidator.EnsureValid(configuration));

        Assert.Null(exception);
    }

    [Fact]
    public void EnsureValid_ThrowsSafeMessage_WhenAKeyIsMissing()
    {
        var configuration = CreateConfiguration(values => values.Remove("JWT_SIGNING_KEY"));

        var exception = Assert.Throws<InvalidOperationException>(
            () => StartupConfigurationValidator.EnsureValid(configuration));

        Assert.Contains("JWT_SIGNING_KEY", exception.Message, StringComparison.Ordinal);

        // El mensaje no debe filtrar valores configurados ni contenido de `.env`.
        foreach (var value in DummyValues)
        {
            Assert.DoesNotContain(value, exception.Message, StringComparison.Ordinal);
        }
    }

    private static IConfigurationRoot CreateConfiguration(Action<Dictionary<string, string?>>? configure = null)
    {
        var values = new Dictionary<string, string?>(StringComparer.Ordinal)
        {
            ["MONGODB_URI"] = DummyMongoUri,
            ["MONGODB_DATABASE"] = DummyDatabase,
            ["REDIS_AUTH_CONNECTION"] = DummyRedisAuth,
            ["REDIS_CACHE_CONNECTION"] = DummyRedisCache,
            ["JWT_ISSUER"] = DummyIssuer,
            ["JWT_AUDIENCE"] = DummyAudience,
            ["JWT_SIGNING_KEY"] = DummySigningKey,
        };

        configure?.Invoke(values);

        return new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();
    }
}
