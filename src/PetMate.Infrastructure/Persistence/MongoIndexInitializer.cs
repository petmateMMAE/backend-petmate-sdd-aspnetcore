using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;

namespace PetMate.Infrastructure.Persistence;

/// <summary>Crea (de forma idempotente) los índices de autenticación (plan §11.2) y el de mascotas por propietario (pm-039).</summary>
public sealed class MongoIndexInitializer(IMongoDatabase database, MongoCollectionNames names)
{
    public const string UserEmailIndex = "ux_users_email";
    public const string TokenHashIndex = "ux_refresh_tokens_tokenHash";
    public const string TokenSessionIndex = "ix_refresh_tokens_sessionId";
    public const string TokenTtlIndex = "ttl_refresh_tokens_absoluteExpiresAtUtc";
    public const string SessionUserIndex = "ix_refresh_sessions_userId";
    public const string SessionTtlIndex = "ttl_refresh_sessions_absoluteExpiresAtUtc";
    public const string PetOwnerIndex = "ix_pets_ownerId_createdAtUtc";
    public const string PetQrTokenIndex = "ux_pets_qrToken";
    public const string PetQrCodeIndex = "ux_pets_qrCode";
    public const string PasswordResetHashIndex = "ux_password_resets_tokenHash";
    public const string PasswordResetTtlIndex = "ttl_password_resets_expiresAtUtc";
    public const string VaccinePetIndex = "ix_vaccines_ownerId_petId_appliedOn";
    public const string DewormingPetIndex = "ix_dewormings_ownerId_petId_appliedOn";
    public const string WeightPetIndex = "ix_weights_ownerId_petId_measuredOn";
    public const string AllergyPetIndex = "ix_allergies_ownerId_petId_status";

    public async Task EnsureIndexes(CancellationToken cancellationToken)
    {
        var users = database.GetCollection<UserDocument>(names.Users);
        var sessions = database.GetCollection<RefreshSessionDocument>(names.Sessions);
        var tokens = database.GetCollection<RefreshTokenDocument>(names.Tokens);
        var pets = database.GetCollection<PetDocument>(names.Pets);

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

        // pm-039#CA-02: toda consulta de mascotas filtra por propietario y lista de la más reciente a la más antigua.
        await pets.Indexes.CreateOneAsync(
            new CreateIndexModel<PetDocument>(
                Builders<PetDocument>.IndexKeys.Ascending(document => document.OwnerId).Descending(document => document.CreatedAtUtc),
                new CreateIndexOptions { Name = PetOwnerIndex }),
            cancellationToken: cancellationToken).ConfigureAwait(false);

        // pm-093: el token y el código corto del QR son únicos. El índice es parcial porque las mascotas anteriores al QR
        // no los tienen hasta que se leen por primera vez.
        var hasQr = new BsonDocument("$type", "string");
        await pets.Indexes.CreateManyAsync(
            [
                new CreateIndexModel<PetDocument>(
                    Builders<PetDocument>.IndexKeys.Ascending(document => document.QrToken),
                    new CreateIndexOptions<PetDocument> { Name = PetQrTokenIndex, Unique = true, PartialFilterExpression = new BsonDocument("qrToken", hasQr) }),
                new CreateIndexModel<PetDocument>(
                    Builders<PetDocument>.IndexKeys.Ascending(document => document.QrCode),
                    new CreateIndexOptions<PetDocument> { Name = PetQrCodeIndex, Unique = true, PartialFilterExpression = new BsonDocument("qrCode", hasQr) }),
            ],
            cancellationToken).ConfigureAwait(false);

        // pm-089: el hash del token es único y los documentos se borran al vencer (con un día de margen para auditoría).
        var resetCollection = database.GetCollection<PasswordResetDocument>(names.PasswordResets);
        await resetCollection.Indexes.CreateManyAsync(
            [
                new CreateIndexModel<PasswordResetDocument>(
                    Builders<PasswordResetDocument>.IndexKeys.Ascending(d => d.TokenHash),
                    new CreateIndexOptions { Name = PasswordResetHashIndex, Unique = true }),
                new CreateIndexModel<PasswordResetDocument>(
                    Builders<PasswordResetDocument>.IndexKeys.Ascending(d => d.ExpiresAtUtc),
                    new CreateIndexOptions { Name = PasswordResetTtlIndex, ExpireAfter = TimeSpan.FromDays(1) }),
            ],
            cancellationToken).ConfigureAwait(false);

        // pm-097 a pm-102: los listados del carnet filtran por dueño y mascota y ordenan por fecha (o estado, en alergias).
        await database.GetCollection<VaccineDocument>(names.Vaccines).Indexes.CreateOneAsync(
            new CreateIndexModel<VaccineDocument>(
                Builders<VaccineDocument>.IndexKeys.Ascending(d => d.OwnerId).Ascending(d => d.PetId).Descending(d => d.AppliedOn),
                new CreateIndexOptions { Name = VaccinePetIndex }),
            cancellationToken: cancellationToken).ConfigureAwait(false);
        await database.GetCollection<DewormingDocument>(names.Dewormings).Indexes.CreateOneAsync(
            new CreateIndexModel<DewormingDocument>(
                Builders<DewormingDocument>.IndexKeys.Ascending(d => d.OwnerId).Ascending(d => d.PetId).Descending(d => d.AppliedOn),
                new CreateIndexOptions { Name = DewormingPetIndex }),
            cancellationToken: cancellationToken).ConfigureAwait(false);
        await database.GetCollection<WeightDocument>(names.Weights).Indexes.CreateOneAsync(
            new CreateIndexModel<WeightDocument>(
                Builders<WeightDocument>.IndexKeys.Ascending(d => d.OwnerId).Ascending(d => d.PetId).Descending(d => d.MeasuredOn),
                new CreateIndexOptions { Name = WeightPetIndex }),
            cancellationToken: cancellationToken).ConfigureAwait(false);
        await database.GetCollection<AllergyDocument>(names.Allergies).Indexes.CreateOneAsync(
            new CreateIndexModel<AllergyDocument>(
                Builders<AllergyDocument>.IndexKeys.Ascending(d => d.OwnerId).Ascending(d => d.PetId).Ascending(d => d.Status),
                new CreateIndexOptions { Name = AllergyPetIndex }),
            cancellationToken: cancellationToken).ConfigureAwait(false);
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
