using Azure.Identity;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MongoDB.Driver;
using PetMate.Application.Access;
using PetMate.Application.Authentication;
using PetMate.Application.Businesses;
using PetMate.Application.Carnet;
using PetMate.Application.Images;
using PetMate.Application.Persistence;
using PetMate.Application.Security;
using PetMate.Infrastructure.Configuration;
using PetMate.Infrastructure.Mail;
using PetMate.Infrastructure.Health;
using PetMate.Infrastructure.Images;
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
        services.AddSingleton<IPetRepository, MongoPetRepository>();
        services.AddSingleton<IVaccineRepository, MongoVaccineRepository>();
        services.AddSingleton<IDewormingRepository, MongoDewormingRepository>();
        services.AddSingleton<IWeightRepository, MongoWeightRepository>();
        services.AddSingleton<IAllergyRepository, MongoAllergyRepository>();
        services.AddSingleton<IPasswordResetRepository, MongoPasswordResetRepository>();
        services.AddSingleton<IBusinessRepository, MongoBusinessRepository>();
        services.AddSingleton<IBusinessServiceRepository, MongoBusinessServiceRepository>();
        services.AddSingleton<IAccessGrantRepository, MongoAccessGrantRepository>();
        services.AddSingleton<IScanLogRepository, MongoScanLogRepository>();
        services.AddSingleton<IConsultationRepository, MongoConsultationRepository>();
        services.AddSingleton<PetMate.Application.Followups.IFollowupRepository, MongoFollowupRepository>();
        services.AddSingleton<PetMate.Application.Followups.IConversationRepository, MongoConversationRepository>();
        services.AddSingleton<PetMate.Application.Followups.IMessageRepository, MongoMessageRepository>();

        // Correo transaccional (pm-088): sin las tres variables no se envía nada (adaptador deshabilitado).
        if (EmailOptions.TryRead(name => configuration[name]) is { } emailOptions)
        {
            services.AddSingleton(emailOptions);
            services.AddSingleton<IEmailSender>(provider => new ResendEmailSender(
                new HttpClient(new SocketsHttpHandler { PooledConnectionLifetime = TimeSpan.FromMinutes(5) }) { Timeout = TimeSpan.FromSeconds(10) },
                emailOptions,
                provider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<ResendEmailSender>>()));
        }
        else
        {
            services.AddSingleton<IEmailSender, DisabledEmailSender>();
        }
        // Imágenes en Blob Storage (pm-079): sin cuenta ni cadena de conexión no hay almacenamiento y los
        // endpoints de fotos responden 503, igual que el correo de pm-088.
        if (ImageStorageOptions.TryRead(name => configuration[name]) is { } imageOptions)
        {
            services.AddSingleton(imageOptions);
            services.AddSingleton(_ => imageOptions.ConnectionString is { } connection
                ? new BlobServiceClient(connection)
                : new BlobServiceClient(imageOptions.ServiceUri, new DefaultAzureCredential()));
            services.AddSingleton<IImageStorage, BlobImageStorage>();
        }
        else
        {
            services.AddSingleton<IImageStorage, DisabledImageStorage>();
        }

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
        services.AddSingleton<IAuthAttemptThrottle, RedisAuthAttemptThrottle>();
        services.AddSingleton<PetMate.Application.Caching.ICacheStore, RedisCacheStore>();

        services.AddSingleton(new RedisAuthConnection(settings.RedisAuth.Connection));
        services.AddSingleton(new RedisCacheConnection(settings.RedisCache.Connection));

        services.AddHealthChecks()
            .AddCheck<MongoHealthCheck>(HealthCheckNames.Mongo, tags: [HealthCheckNames.ReadyTag])
            .AddCheck<RedisHealthCheck<RedisAuthConnection>>(HealthCheckNames.RedisAuth, tags: [HealthCheckNames.ReadyTag])
            .AddCheck<RedisHealthCheck<RedisCacheConnection>>(HealthCheckNames.RedisCache, tags: [HealthCheckNames.ReadyTag]);

        return services;
    }
}
