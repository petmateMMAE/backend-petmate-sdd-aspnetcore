using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using PetMate.Application.Access;
using PetMate.Application.Persistence;
using PetMate.Domain.Access;
using PetMate.Domain.Common;

namespace PetMate.Infrastructure.Persistence;

/// <summary>Autorización de acceso (pm-122). Un índice TTL borra los documentos un día después de vencer.</summary>
internal sealed class AccessGrantDocument
{
    [BsonId]
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid Id { get; set; }

    [BsonElement("petId")]
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid PetId { get; set; }

    [BsonElement("ownerId")]
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid OwnerId { get; set; }

    /// <summary>Ausente = vale para cualquier negocio que escanee.</summary>
    [BsonElement("businessId")]
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    [BsonIgnoreIfNull]
    public Guid? BusinessId { get; set; }

    [BsonElement("data")]
    public List<int> Data { get; set; } = [];

    [BsonElement("createdAtUtc")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime CreatedAtUtc { get; set; }

    [BsonElement("expiresAtUtc")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime ExpiresAtUtc { get; set; }
}

/// <summary>Registro interno de escaneos (pm-129): sin lectura desde la API.</summary>
internal sealed class ScanLogDocument
{
    [BsonId]
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid Id { get; set; }

    [BsonElement("petId")]
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid PetId { get; set; }

    [BsonElement("petName")]
    public string PetName { get; set; } = string.Empty;

    [BsonElement("photoUrl")]
    [BsonIgnoreIfNull]
    public string? PhotoUrl { get; set; }

    [BsonElement("businessId")]
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid BusinessId { get; set; }

    [BsonElement("atUtc")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime AtUtc { get; set; }
}

public sealed class MongoAccessGrantRepository : IAccessGrantRepository
{
    private readonly IMongoCollection<AccessGrantDocument> grants;

    public MongoAccessGrantRepository(IMongoDatabase database, MongoCollectionNames names)
    {
        ArgumentNullException.ThrowIfNull(database);
        ArgumentNullException.ThrowIfNull(names);
        grants = database.GetCollection<AccessGrantDocument>(names.AccessGrants);
    }

    public Task Add(AccessGrant grant, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(grant);
        return Execute(() => grants.InsertOneAsync(
            new AccessGrantDocument
            {
                Id = grant.Id,
                PetId = grant.PetId,
                OwnerId = grant.OwnerId,
                BusinessId = grant.BusinessId,
                Data = grant.Data.Select(c => (int)c).ToList(),
                CreatedAtUtc = grant.CreatedAtUtc,
                ExpiresAtUtc = grant.ExpiresAtUtc,
            },
            cancellationToken: cancellationToken));
    }

    public async Task<IReadOnlyList<AccessGrant>> FindActive(Guid petId, Guid businessId, DateTime nowUtc, CancellationToken cancellationToken)
    {
        var builder = Builders<AccessGrantDocument>.Filter;
        var filter = builder.And(
            builder.Eq(d => d.PetId, petId),
            builder.Gt(d => d.ExpiresAtUtc, nowUtc),
            builder.Or(builder.Eq(d => d.BusinessId, null), builder.Eq(d => d.BusinessId, businessId)));
        var documents = await Execute(() => grants.Find(filter).ToListAsync(cancellationToken)).ConfigureAwait(false);
        try
        {
            return documents.Select(d => AccessGrantMapping.ToDomain(d)).ToArray();
        }
        catch (Exception exception) when (exception is DomainException or FormatException)
        {
            throw new PersistenceUnavailableException(exception);
        }
    }

    public async Task<long> CancelActiveForPet(Guid petId, Guid ownerId, DateTime nowUtc, CancellationToken cancellationToken)
    {
        var builder = Builders<AccessGrantDocument>.Filter;
        var filter = builder.And(builder.Eq(d => d.PetId, petId), builder.Eq(d => d.OwnerId, ownerId), builder.Gt(d => d.ExpiresAtUtc, nowUtc));
        var result = await Execute(() => grants.UpdateManyAsync(filter, Builders<AccessGrantDocument>.Update.Set(d => d.ExpiresAtUtc, nowUtc), cancellationToken: cancellationToken)).ConfigureAwait(false);
        return result.ModifiedCount;
    }

    public async Task<long> DeleteByPet(Guid petId, Guid ownerId, CancellationToken cancellationToken)
    {
        var builder = Builders<AccessGrantDocument>.Filter;
        var result = await Execute(() => grants.DeleteManyAsync(builder.And(builder.Eq(d => d.PetId, petId), builder.Eq(d => d.OwnerId, ownerId)), cancellationToken)).ConfigureAwait(false);
        return result.DeletedCount;
    }

    private static async Task Execute(Func<Task> action)
    {
        try
        {
            await action().ConfigureAwait(false);
        }
        catch (Exception exception) when (exception is MongoException or TimeoutException)
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
        catch (Exception exception) when (exception is MongoException or TimeoutException)
        {
            throw new PersistenceUnavailableException(exception);
        }
    }
}

internal static class AccessGrantMapping
{
    public static AccessGrant ToDomain(AccessGrantDocument d)
        => AccessGrant.Restore(
            d.Id, d.PetId, d.OwnerId, d.BusinessId, d.Data.Select(i => (PetDataCategory)i),
            d.CreatedAtUtc.Kind == DateTimeKind.Utc ? d.CreatedAtUtc : DateTime.SpecifyKind(d.CreatedAtUtc, DateTimeKind.Utc),
            d.ExpiresAtUtc.Kind == DateTimeKind.Utc ? d.ExpiresAtUtc : DateTime.SpecifyKind(d.ExpiresAtUtc, DateTimeKind.Utc));
}

internal static class ScanLogMapping
{
    public static ScanLog ToDomain(ScanLogDocument d)
        => ScanLog.Restore(d.Id, d.PetId, d.PetName, d.PhotoUrl, d.BusinessId, d.AtUtc.Kind == DateTimeKind.Utc ? d.AtUtc : DateTime.SpecifyKind(d.AtUtc, DateTimeKind.Utc));
}

public sealed class MongoScanLogRepository : IScanLogRepository
{
    private readonly IMongoCollection<ScanLogDocument> logs;

    public MongoScanLogRepository(IMongoDatabase database, MongoCollectionNames names)
    {
        ArgumentNullException.ThrowIfNull(database);
        ArgumentNullException.ThrowIfNull(names);
        logs = database.GetCollection<ScanLogDocument>(names.ScanLogs);
    }

    public async Task Add(ScanLog log, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(log);
        try
        {
            await logs.InsertOneAsync(new ScanLogDocument { Id = log.Id, PetId = log.PetId, PetName = log.PetName, PhotoUrl = log.PhotoUrl, BusinessId = log.BusinessId, AtUtc = log.AtUtc }, cancellationToken: cancellationToken).ConfigureAwait(false);
        }
        catch (Exception exception) when (exception is MongoException or TimeoutException)
        {
            throw new PersistenceUnavailableException(exception);
        }
    }

    private static FilterDefinition<ScanLogDocument> ForBusiness(Guid businessId, DateTime? sinceUtc)
    {
        var builder = Builders<ScanLogDocument>.Filter;
        return sinceUtc is { } since ? builder.And(builder.Eq(d => d.BusinessId, businessId), builder.Gte(d => d.AtUtc, since)) : builder.Eq(d => d.BusinessId, businessId);
    }

    public async Task<IReadOnlyList<ScanLog>> ListByBusiness(Guid businessId, DateTime? sinceUtc, int skip, int take, CancellationToken cancellationToken)
    {
        try
        {
            var documents = await logs.Find(ForBusiness(businessId, sinceUtc)).SortByDescending(d => d.AtUtc).ThenByDescending(d => d.Id).Skip(skip).Limit(take).ToListAsync(cancellationToken).ConfigureAwait(false);
            return documents.Select(d => ScanLogMapping.ToDomain(d)).ToArray();
        }
        catch (Exception exception) when (exception is MongoException or TimeoutException or DomainException)
        {
            throw new PersistenceUnavailableException(exception);
        }
    }

    public async Task<long> CountByBusiness(Guid businessId, DateTime? sinceUtc, CancellationToken cancellationToken)
    {
        try
        {
            return await logs.CountDocumentsAsync(ForBusiness(businessId, sinceUtc), cancellationToken: cancellationToken).ConfigureAwait(false);
        }
        catch (Exception exception) when (exception is MongoException or TimeoutException)
        {
            throw new PersistenceUnavailableException(exception);
        }
    }

    public async Task<long> DeleteByPet(Guid petId, CancellationToken cancellationToken)
    {
        try
        {
            var result = await logs.DeleteManyAsync(Builders<ScanLogDocument>.Filter.Eq(d => d.PetId, petId), cancellationToken).ConfigureAwait(false);
            return result.DeletedCount;
        }
        catch (Exception exception) when (exception is MongoException or TimeoutException)
        {
            throw new PersistenceUnavailableException(exception);
        }
    }
}
