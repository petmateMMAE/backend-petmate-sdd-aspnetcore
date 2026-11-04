using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using StackExchange.Redis;

namespace PetMate.Infrastructure.Configuration;

/// <summary>Configuración común de MongoDB Atlas (desarrollo, integración y producción usan el mismo contrato).</summary>
public sealed record MongoSettings(string Uri, string Database);

/// <summary>Cadena de conexión de una instancia Redis independiente.</summary>
public sealed record RedisSettings(string Connection);

/// <summary>
/// Configuración tipada de persistencia y Redis. Se construye únicamente con <see cref="Parse"/>,
/// que valida formato sin conectarse y sin incluir valores en los errores.
/// </summary>
public sealed record InfrastructureSettings(MongoSettings Mongo, RedisSettings RedisAuth, RedisSettings RedisCache)
{
    public const string MongoUriKey = "MONGODB_URI";
    public const string MongoDatabaseKey = "MONGODB_DATABASE";
    public const string RedisAuthKey = "REDIS_AUTH_CONNECTION";
    public const string RedisCacheKey = "REDIS_CACHE_CONNECTION";

    private const int MaxDatabaseNameLength = 63;
    private static readonly System.Buffers.SearchValues<char> InvalidDatabaseChars =
        System.Buffers.SearchValues.Create("/\\. \"$\0");

    /// <summary>
    /// Valida y devuelve la configuración. Con <paramref name="allowPlainRedis"/> falso (cualquier entorno
    /// distinto de Development) Redis exige TLS (<c>ssl=True</c>); en Development se admite texto plano solo
    /// para el Redis de Docker en la red interna. Los mensajes enumeran claves y motivos, nunca valores.
    /// </summary>
    public static InfrastructureSettings Parse(IConfiguration configuration, bool allowPlainRedis)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var errors = new List<string>();

        var mongoUri = configuration[MongoUriKey];
        var database = configuration[MongoDatabaseKey];
        ValidateMongoUri(mongoUri, errors);
        ValidateDatabaseName(database, errors);

        var authEndpoints = ValidateRedis(RedisAuthKey, configuration[RedisAuthKey], allowPlainRedis, errors);
        var cacheEndpoints = ValidateRedis(RedisCacheKey, configuration[RedisCacheKey], allowPlainRedis, errors);
        if (authEndpoints.Intersect(cacheEndpoints, StringComparer.OrdinalIgnoreCase).Any())
        {
            errors.Add($"{RedisAuthKey}/{RedisCacheKey}: deben apuntar a instancias Redis independientes.");
        }

        if (errors.Count > 0)
        {
            throw new InvalidOperationException(
                "Configuración de infraestructura inválida: " + string.Join(" ", errors)
                + " No se muestran valores por seguridad.");
        }

        return new InfrastructureSettings(
            new MongoSettings(mongoUri!, database!),
            new RedisSettings(configuration[RedisAuthKey]!),
            new RedisSettings(configuration[RedisCacheKey]!));
    }

    private static void ValidateMongoUri(string? uri, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(uri))
        {
            errors.Add($"{MongoUriKey}: obligatoria.");
            return;
        }

        try
        {
            // MongoUrl valida el esquema (mongodb:// o mongodb+srv://) sin resolver DNS ni conectar.
            _ = MongoUrl.Create(uri);
        }
        catch (Exception exception) when (exception is MongoConfigurationException or FormatException or ArgumentException)
        {
            errors.Add($"{MongoUriKey}: formato de URI MongoDB no válido.");
        }
    }

    private static void ValidateDatabaseName(string? name, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            errors.Add($"{MongoDatabaseKey}: obligatoria.");
        }
        else if (name.Length > MaxDatabaseNameLength || name.AsSpan().IndexOfAny(InvalidDatabaseChars) >= 0)
        {
            errors.Add($"{MongoDatabaseKey}: nombre de base de datos no válido.");
        }
    }

    private static List<string> ValidateRedis(string key, string? connection, bool allowPlain, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(connection))
        {
            errors.Add($"{key}: obligatoria.");
            return [];
        }

        ConfigurationOptions options;
        try
        {
            options = ConfigurationOptions.Parse(connection);
        }
        catch (ArgumentException)
        {
            errors.Add($"{key}: formato de conexión Redis no válido.");
            return [];
        }

        if (options.EndPoints.Count == 0)
        {
            errors.Add($"{key}: debe indicar al menos un endpoint.");
        }

        if (string.IsNullOrEmpty(options.Password))
        {
            errors.Add($"{key}: debe incluir contraseña.");
        }

        if (!options.Ssl && !allowPlain)
        {
            errors.Add($"{key}: se requiere ssl=True fuera de Development.");
        }

        return [.. options.EndPoints.Select(endpoint => endpoint.ToString() ?? string.Empty)];
    }
}
