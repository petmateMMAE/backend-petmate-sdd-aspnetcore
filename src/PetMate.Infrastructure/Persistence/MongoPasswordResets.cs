using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using PetMate.Application.Authentication;
using PetMate.Application.Persistence;
using PetMate.Domain.Common;
using PetMate.Domain.Sessions;
using PetMate.Domain.Users;

namespace PetMate.Infrastructure.Persistence;

/// <summary>Token de restablecimiento (pm-089, pm-090): solo el hash; un índice TTL borra los documentos vencidos.</summary>
internal sealed class PasswordResetDocument
{
    [BsonId]
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid Id { get; set; }

    [BsonElement("userId")]
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid UserId { get; set; }

    [BsonElement("tokenHash")]
    public string TokenHash { get; set; } = string.Empty;

    [BsonElement("createdAtUtc")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime CreatedAtUtc { get; set; }

    [BsonElement("expiresAtUtc")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime ExpiresAtUtc { get; set; }

    [BsonElement("usedAtUtc")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    [BsonIgnoreIfNull]
    public DateTime? UsedAtUtc { get; set; }
}

public sealed class MongoPasswordResetRepository : IPasswordResetRepository
{
    private readonly IMongoCollection<PasswordResetDocument> resets;

    public MongoPasswordResetRepository(IMongoDatabase database, MongoCollectionNames names)
        : this(database, (names ?? throw new ArgumentNullException(nameof(names))).PasswordResets)
    {
    }

    internal MongoPasswordResetRepository(IMongoDatabase database, string collectionName)
    {
        ArgumentNullException.ThrowIfNull(database);
        resets = database.GetCollection<PasswordResetDocument>(collectionName);
    }

    public async Task Add(PasswordResetToken token, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(token);
        await Execute(() => resets.InsertOneAsync(
            new PasswordResetDocument
            {
                Id = token.Id,
                UserId = token.UserId,
                TokenHash = token.TokenHash.Value,
                CreatedAtUtc = token.CreatedAtUtc,
                ExpiresAtUtc = token.ExpiresAtUtc,
                UsedAtUtc = token.UsedAtUtc,
            },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<Guid?> TryConsume(RefreshTokenHash tokenHash, DateTime nowUtc, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(tokenHash);
        var filter = Builders<PasswordResetDocument>.Filter.And(
            Builders<PasswordResetDocument>.Filter.Eq(item => item.TokenHash, tokenHash.Value),
            Builders<PasswordResetDocument>.Filter.Eq(item => item.UsedAtUtc, null),
            Builders<PasswordResetDocument>.Filter.Gt(item => item.ExpiresAtUtc, nowUtc));
        var update = Builders<PasswordResetDocument>.Update.Set(item => item.UsedAtUtc, nowUtc);

        // FindOneAndUpdate es atómico por documento: dos usos simultáneos tienen un único ganador.
        var document = await Execute(() => resets.FindOneAndUpdateAsync(filter, update, cancellationToken: cancellationToken)).ConfigureAwait(false);
        return document?.UserId;
    }

    public async Task InvalidateAllForUser(Guid userId, DateTime nowUtc, CancellationToken cancellationToken)
    {
        var filter = Builders<PasswordResetDocument>.Filter.And(
            Builders<PasswordResetDocument>.Filter.Eq(item => item.UserId, userId),
            Builders<PasswordResetDocument>.Filter.Eq(item => item.UsedAtUtc, null));
        await Execute(() => resets.UpdateManyAsync(filter, Builders<PasswordResetDocument>.Update.Set(item => item.UsedAtUtc, nowUtc), cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task DeleteByUser(Guid userId, CancellationToken cancellationToken)
        => await Execute(() => resets.DeleteManyAsync(Builders<PasswordResetDocument>.Filter.Eq(item => item.UserId, userId), cancellationToken)).ConfigureAwait(false);

    private static async Task Execute(Func<Task> action)
    {
        try
        {
            await action().ConfigureAwait(false);
        }
        catch (Exception exception) when (exception is MongoException or TimeoutException or DomainException)
        {
            throw new PersistenceUnavailableException(exception);
        }
    }

    private static async Task<T> Execute<T>(Func<Task<T>> action)
    {
        try
        {
            return await action().ConfigureAwait(false);
        }
        catch (Exception exception) when (exception is MongoException or TimeoutException or DomainException)
        {
            throw new PersistenceUnavailableException(exception);
        }
    }
}

/// <summary>
/// Tokens de confirmación de eliminación de cuenta (pm-164): misma mecánica que los de restablecimiento (solo hash, un solo uso,
/// TTL) en su propia colección, de modo que un token de un flujo nunca sirve en el otro.
/// </summary>
public sealed class MongoAccountDeletionTokenRepository : PetMate.Application.Accounts.IAccountDeletionTokenRepository
{
    private readonly MongoPasswordResetRepository inner;

    public MongoAccountDeletionTokenRepository(IMongoDatabase database, MongoCollectionNames names)
    {
        ArgumentNullException.ThrowIfNull(names);
        inner = new MongoPasswordResetRepository(database, names.AccountDeletionTokens);
    }

    public Task Add(PasswordResetToken token, CancellationToken cancellationToken) => inner.Add(token, cancellationToken);

    public Task<Guid?> TryConsume(RefreshTokenHash tokenHash, DateTime nowUtc, CancellationToken cancellationToken) => inner.TryConsume(tokenHash, nowUtc, cancellationToken);

    public Task InvalidateAllForUser(Guid userId, DateTime nowUtc, CancellationToken cancellationToken) => inner.InvalidateAllForUser(userId, nowUtc, cancellationToken);

    public Task DeleteByUser(Guid userId, CancellationToken cancellationToken) => inner.DeleteByUser(userId, cancellationToken);
}
