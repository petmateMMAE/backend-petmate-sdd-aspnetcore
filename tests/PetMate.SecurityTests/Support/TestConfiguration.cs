using Microsoft.Extensions.Configuration;

namespace PetMate.SecurityTests.Support;

/// <summary>Configuración ficticia válida: permite componer Application + Infrastructure sin abrir ninguna conexión.</summary>
internal static class TestConfiguration
{
    public static IConfiguration Valid() => new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["MONGODB_URI"] = "mongodb+srv://usuario_ficticio:password_ficticio@cluster-ficticio.mongodb.net/?retryWrites=true&w=majority",
            ["MONGODB_DATABASE"] = "petmate_test_ficticio",
            ["REDIS_AUTH_CONNECTION"] = "redis-auth-ficticio:6380,password=pw-a,ssl=True",
            ["REDIS_CACHE_CONNECTION"] = "redis-cache-ficticio:6381,password=pw-b,ssl=True",
            ["JWT_ISSUER"] = "https://issuer-prueba.local",
            ["JWT_AUDIENCE"] = "petmate-api-prueba",
            ["JWT_SIGNING_KEY"] = "clave-de-prueba-0123456789-abcdefghijkl",
        })
        .Build();
}
