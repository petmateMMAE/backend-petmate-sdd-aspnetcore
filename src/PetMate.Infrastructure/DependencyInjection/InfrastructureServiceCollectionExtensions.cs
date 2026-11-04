using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MongoDB.Driver;
using PetMate.Application.Authentication;
using PetMate.Application.Persistence;
using PetMate.Application.Security;
using PetMate.Infrastructure.Configuration;
using PetMate.Infrastructure.Health;
using PetMate.Infrastructure.Persistence;
using PetMate.Infrastructure.Redis;
using PetMate.Infrastructure.Security;

namespace PetMate.Infrastructure.DependencyInjection;

public static class InfrastructureServiceCollectionExtensions
{
    /// <summary>
    /// Registra MongoDB (Atlas), Redis Auth y Redis Cache como conexiones independientes y sus health checks.
    /// El destino se decide solo por configuración (<c>MONGODB_URI</c>, <c>REDIS_*_CONNECTION</c>); no hay
    /// implementaciones ni ramas por proveedor. Ninguna conexión se abre aquí: son perezosas.
    /// </summary>
    /// <param name="isDevelopment">Development: permite Redis sin TLS (red interna de Docker) y claves JWT de plantilla (solo local).</param>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        bool isDevelopment = false)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var settings = InfrastructureSettings.Parse(configuration, isDevelopment);
        services.AddSingleton(settings);

        services.AddSingleton<IMongoClient>(_ =>
        {
            var clientSettings = MongoClientSettings.FromConnectionString(settings.Mongo.Uri);
            clientSettings.ServerSelectionTimeout = TimeSpan.FromSeconds(5);
            return new MongoClient(clientSettings);
        });
        services.AddSingleton(provider => provider.GetRequiredService<IMongoClient>().GetDatabase(settings.Mongo.Database));

        // Persistencia de autenticación (T014): mismo contrato para cualquier URI de Atlas.
        services.AddSingleton(new MongoCollectionNames());
        services.AddSingleton<IUserRepository, MongoUserRepository>();
        services.AddSingleton<ISessionRepository, MongoSessionRepository>();
        services.AddSingleton<MongoIndexInitializer>();
        services.AddHostedService<MongoIndexHostedService>();

        // Hashing de contraseñas (T015): parámetros validados al arrancar, opcionales por entorno.
        services.AddSingleton(Argon2idOptions.Parse(configuration));
        services.AddSingleton<IPasswordHasher, Argon2idPasswordHasher>();
        services.AddSingleton<IRefreshTokenFactory, RefreshTokenFactory>();

        // JWT (T016) y revocación en Redis Auth (T017).
        services.TryAddSingleton(TimeProvider.System);
        services.AddSingleton(JwtOptions.Parse(configuration, isDevelopment));
        services.AddSingleton<IAccessTokenService, JwtAccessTokenService>();
        services.AddSingleton<ITokenRevocationStore, RedisTokenRevocationStore>();

        services.AddSingleton(new RedisAuthConnection(settings.RedisAuth.Connection));
        services.AddSingleton(new RedisCacheConnection(settings.RedisCache.Connection));

        services.AddHealthChecks()
            .AddCheck<MongoHealthCheck>(HealthCheckNames.Mongo, tags: [HealthCheckNames.ReadyTag])
            .AddCheck<RedisHealthCheck<RedisAuthConnection>>(HealthCheckNames.RedisAuth, tags: [HealthCheckNames.ReadyTag])
            .AddCheck<RedisHealthCheck<RedisCacheConnection>>(HealthCheckNames.RedisCache, tags: [HealthCheckNames.ReadyTag]);

        return services;
    }
}
