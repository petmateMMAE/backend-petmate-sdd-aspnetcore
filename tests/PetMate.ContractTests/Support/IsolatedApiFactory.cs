using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace PetMate.ContractTests.Support;

/// <summary>
/// Serializa las pruebas que arrancan la API: comparten variables de entorno de proceso.
/// </summary>
[CollectionDefinition(Name)]
public sealed class ApiEnvironmentGroup
{
    public const string Name = "ApiEnvironment";
}

/// <summary>
/// Arranca la API con configuración de prueba fijada como variables de entorno (que prevalecen sobre cualquier
/// <c>.env</c> real del entorno) y las restaura al liberar la fábrica. Nunca toca servicios reales.
/// </summary>
public class IsolatedApiFactory : WebApplicationFactory<Program>
{
    public const string JwtIssuer = "https://issuer-prueba.local";
    public const string JwtAudience = "petmate-api-prueba";
    public const string JwtKey = "clave-de-prueba-0123456789-abcdefghijkl";

    private readonly string environmentName;
    private readonly Dictionary<string, string?> previous = [];

    public IsolatedApiFactory(string environmentName = "Development", IDictionary<string, string>? overrides = null)
    {
        this.environmentName = environmentName;
        var settings = new Dictionary<string, string>
        {
            ["MONGODB_URI"] = "mongodb://usuario:clave-test@test.invalid:27017/",
            ["MONGODB_DATABASE"] = "petmate-tests",
            ["REDIS_AUTH_CONNECTION"] = "test.invalid:6380,password=test-auth-pw,ssl=True,connectTimeout=500",
            ["REDIS_CACHE_CONNECTION"] = "test.invalid:6381,password=test-cache-pw,ssl=True,connectTimeout=500",
            ["JWT_ISSUER"] = JwtIssuer,
            ["JWT_AUDIENCE"] = JwtAudience,
            ["JWT_SIGNING_KEY"] = JwtKey,
            ["JWT_ACCESS_TOKEN_MINUTES"] = "15",
            ["AUTH_RATE_LIMIT_PER_MINUTE"] = "1000",
            ["PETS_RATE_LIMIT_PER_MINUTE"] = "1000",
            ["USERS_RATE_LIMIT_PER_MINUTE"] = "1000",
        };
        foreach (var (key, value) in overrides ?? new Dictionary<string, string>())
        {
            settings[key] = value;
        }

        foreach (var (key, value) in settings)
        {
            previous[key] = Environment.GetEnvironmentVariable(key);
            Environment.SetEnvironmentVariable(key, value);
        }
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder) => builder.UseEnvironment(environmentName);

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        foreach (var (key, value) in previous)
        {
            Environment.SetEnvironmentVariable(key, value);
        }
    }
}
