using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace PetMate.Infrastructure.Persistence;

/// <summary>Crea (de forma idempotente) los índices de autenticación. Ver plan §11.2.</summary>
public sealed class MongoIndexInitializer(IMongoDatabase database, MongoCollectionNames names)
{
    public const string UserEmailIndex = "ux_users_email";
    public const string TokenHashIndex = "ux_refresh_tokens_tokenHash";
    public const string TokenSessionIndex = "ix_refresh_tokens_sessionId";
    public const string TokenTtlIndex = "ttl_refresh_tokens_absoluteExpiresAtUtc";
    public const string SessionUserIndex = "ix_refresh_sessions_userId";
    public const string SessionTtlIndex = "ttl_refresh_sessions_absoluteExpiresAtUtc";

    public async Task EnsureIndexes(CancellationToken cancellationToken)
    {
        var users = database.GetCollection<UserDocument>(names.Users);
        var sessions = database.GetCollection<RefreshSessionDocument>(names.Sessions);
        var tokens = database.GetCollection<RefreshTokenDocument>(names.Tokens);

        await users.Indexes.CreateOneAsync(
            new CreateIndexModel<UserDocument>(
                Builders<UserDocument>.IndexKeys.Ascending(document => document.Email),
                new CreateIndexOptions { Unique = true, Name = UserEmailIndex }),
            cancellationToken: cancellationToken).ConfigureAwait(false);

        await tokens.Indexes.CreateManyAsync(
            [
                new CreateIndexModel<RefreshTokenDocument>(
                    Builders<RefreshTokenDocument>.IndexKeys.Ascending(document => document.TokenHash),
                    new CreateIndexOptions { Unique = true, Name = TokenHashIndex }),
                new CreateIndexModel<RefreshTokenDocument>(
                    Builders<RefreshTokenDocument>.IndexKeys.Ascending(document => document.SessionId),
                    new CreateIndexOptions { Name = TokenSessionIndex }),
                new CreateIndexModel<RefreshTokenDocument>(
                    Builders<RefreshTokenDocument>.IndexKeys.Ascending(document => document.AbsoluteExpiresAtUtc),
                    new CreateIndexOptions { ExpireAfter = TimeSpan.Zero, Name = TokenTtlIndex }),
            ],
            cancellationToken).ConfigureAwait(false);

        await sessions.Indexes.CreateManyAsync(
            [
                new CreateIndexModel<RefreshSessionDocument>(
                    Builders<RefreshSessionDocument>.IndexKeys.Ascending(document => document.UserId),
                    new CreateIndexOptions { Name = SessionUserIndex }),
                new CreateIndexModel<RefreshSessionDocument>(
                    Builders<RefreshSessionDocument>.IndexKeys.Ascending(document => document.AbsoluteExpiresAtUtc),
                    new CreateIndexOptions { ExpireAfter = TimeSpan.Zero, Name = SessionTtlIndex }),
            ],
            cancellationToken).ConfigureAwait(false);
    }
}

/// <summary>
/// Intenta crear los índices al arrancar, con reintentos. Nunca detiene la API: si Atlas no responde,
/// el readiness lo refleja y se reintenta. Solo registra el tipo de error, nunca su mensaje.
/// </summary>
public sealed partial class MongoIndexHostedService(MongoIndexInitializer initializer, ILogger<MongoIndexHostedService> logger) : BackgroundService
{
    private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(30);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await initializer.EnsureIndexes(stoppingToken).ConfigureAwait(false);
                LogIndexesReady(logger);
                return;
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception exception) when (exception is not OutOfMemoryException)
            {
                LogIndexesFailed(logger, exception.GetType().Name);
                try
                {
                    await Task.Delay(RetryDelay, stoppingToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    return;
                }
            }
        }
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Índices de autenticación verificados.")]
    private static partial void LogIndexesReady(ILogger logger);

    [LoggerMessage(Level = LogLevel.Warning, Message = "No se pudieron crear los índices de autenticación ({ErrorType}); se reintentará.")]
    private static partial void LogIndexesFailed(ILogger logger, string errorType);
}
