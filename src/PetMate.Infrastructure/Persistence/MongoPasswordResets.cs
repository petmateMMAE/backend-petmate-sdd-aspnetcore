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
    {
        ArgumentNullException.ThrowIfNull(database);
        ArgumentNullException.ThrowIfNull(names);
        resets = database.GetCollection<PasswordResetDocument>(names.PasswordResets);
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
