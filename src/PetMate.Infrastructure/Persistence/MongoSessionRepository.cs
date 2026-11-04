using MongoDB.Driver;
using PetMate.Application.Persistence;
using PetMate.Domain.Common;
using PetMate.Domain.Sessions;

namespace PetMate.Infrastructure.Persistence;

public sealed class MongoSessionRepository : ISessionRepository
{
    private readonly IMongoCollection<RefreshSessionDocument> sessions;
    private readonly IMongoCollection<RefreshTokenDocument> tokens;

    public MongoSessionRepository(IMongoDatabase database, MongoCollectionNames names)
    {
        ArgumentNullException.ThrowIfNull(database);
        ArgumentNullException.ThrowIfNull(names);
        sessions = database.GetCollection<RefreshSessionDocument>(names.Sessions);
        tokens = database.GetCollection<RefreshTokenDocument>(names.Tokens);
    }

    public Task AddSession(RefreshSession session, RefreshToken firstToken, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(firstToken);
        return Execute(async () =>
        {
            // Sin transacciones (tier $0): si el segundo insert falla queda una sesión sin tokens, inutilizable y limpiada por el TTL.
            await sessions.InsertOneAsync(MongoMapping.ToDocument(session), cancellationToken: cancellationToken).ConfigureAwait(false);
            await tokens.InsertOneAsync(MongoMapping.ToDocument(firstToken, session.AbsoluteExpiresAtUtc), cancellationToken: cancellationToken)
                .ConfigureAwait(false);
        });
    }

    public Task AddToken(RefreshToken token, DateTime sessionAbsoluteExpiresAtUtc, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(token);
        return Execute(() => tokens.InsertOneAsync(
            MongoMapping.ToDocument(token, sessionAbsoluteExpiresAtUtc), cancellationToken: cancellationToken));
    }

    public async Task<RefreshToken?> FindTokenByHash(RefreshTokenHash tokenHash, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(tokenHash);
        return await Execute(async () =>
        {
            var document = await tokens
                .Find(Builders<RefreshTokenDocument>.Filter.Eq(item => item.TokenHash, tokenHash.Value))
                .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
            return document is null ? null : MongoMapping.ToDomain(document);
        }).ConfigureAwait(false);
    }

    public async Task<bool> TryConsumeToken(Guid tokenId, DateTime consumedAtUtc, CancellationToken cancellationToken)
    {
        // Actualización condicional atómica por documento: solo un llamador concurrente la aplica.
        var result = await Execute(() => tokens.UpdateOneAsync(
            Builders<RefreshTokenDocument>.Filter.And(
                Builders<RefreshTokenDocument>.Filter.Eq(item => item.Id, tokenId),
                Builders<RefreshTokenDocument>.Filter.Eq(item => item.ConsumedAtUtc, null)),
            Builders<RefreshTokenDocument>.Update.Set(item => item.ConsumedAtUtc, consumedAtUtc),
            cancellationToken: cancellationToken)).ConfigureAwait(false);
        return result.ModifiedCount == 1;
    }

    public async Task<RefreshSession?> FindSession(Guid sessionId, CancellationToken cancellationToken)
    {
        return await Execute(async () =>
        {
            var document = await sessions
                .Find(Builders<RefreshSessionDocument>.Filter.Eq(item => item.Id, sessionId))
                .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
            return document is null ? null : MongoMapping.ToDomain(document);
        }).ConfigureAwait(false);
    }

    public async Task<bool> UpdateActivity(RefreshSession session, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(session);
        var result = await Execute(() => sessions.UpdateOneAsync(
            Builders<RefreshSessionDocument>.Filter.And(
                Builders<RefreshSessionDocument>.Filter.Eq(item => item.Id, session.Id),
                Builders<RefreshSessionDocument>.Filter.Eq(item => item.RevokedAtUtc, null)),
            Builders<RefreshSessionDocument>.Update
                .Set(item => item.LastUsedAtUtc, session.LastUsedAtUtc)
                .Set(item => item.IdleExpiresAtUtc, session.IdleExpiresAtUtc),
            cancellationToken: cancellationToken)).ConfigureAwait(false);

        // MatchedCount == 1 solo si la sesión existe y no estaba revocada en este instante.
        return result.MatchedCount == 1;
    }

    public Task Revoke(RefreshSession session, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(session);
        if (session.RevokedAtUtc is null || session.RevocationReason is null)
        {
            throw new ArgumentException("La sesión debe estar revocada antes de persistir la revocación.", nameof(session));
        }

        return Execute(() => sessions.UpdateOneAsync(
            Builders<RefreshSessionDocument>.Filter.And(
                Builders<RefreshSessionDocument>.Filter.Eq(item => item.Id, session.Id),
                Builders<RefreshSessionDocument>.Filter.Eq(item => item.RevokedAtUtc, null)),
            Builders<RefreshSessionDocument>.Update
                .Set(item => item.RevokedAtUtc, session.RevokedAtUtc)
                .Set(item => item.RevocationReason, (int)session.RevocationReason),
            cancellationToken: cancellationToken));
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
