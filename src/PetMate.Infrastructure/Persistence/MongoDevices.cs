using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using PetMate.Application.Devices;
using PetMate.Application.Persistence;

namespace PetMate.Infrastructure.Persistence;

/// <summary>Token de dispositivo (pm-151). Un token pertenece a una sola cuenta (índice único).</summary>
internal sealed class DeviceDocument
{
    [BsonId]
    public string Token { get; set; } = string.Empty;

    [BsonElement("userId")]
    [BsonGuidRepresentation(MongoDB.Bson.GuidRepresentation.Standard)]
    public Guid UserId { get; set; }

    [BsonElement("registeredAtUtc")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime RegisteredAtUtc { get; set; }
}

public sealed class MongoDeviceRepository : IDeviceRepository
{
    private readonly IMongoCollection<DeviceDocument> devices;

    public MongoDeviceRepository(IMongoDatabase database, MongoCollectionNames names)
    {
        ArgumentNullException.ThrowIfNull(database);
        ArgumentNullException.ThrowIfNull(names);
        devices = database.GetCollection<DeviceDocument>(names.Devices);
    }

    public async Task DeleteByUser(Guid userId, CancellationToken cancellationToken)
    {
        try
        {
            await devices.DeleteManyAsync(d => d.UserId == userId, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception exception) when (exception is MongoException or TimeoutException)
        {
            throw new PersistenceUnavailableException(exception);
        }
    }

    public async Task<IReadOnlyList<string>> ListTokens(Guid userId, CancellationToken cancellationToken)
    {
        try
        {
            return await devices.Find(d => d.UserId == userId).Project(d => d.Token).ToListAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception exception) when (exception is MongoException or TimeoutException)
        {
            throw new PersistenceUnavailableException(exception);
        }
    }

    public async Task Unregister(Guid userId, string token, CancellationToken cancellationToken)
    {
        try
        {
            await devices.DeleteOneAsync(d => d.Token == token && d.UserId == userId, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception exception) when (exception is MongoException or TimeoutException)
        {
            throw new PersistenceUnavailableException(exception);
        }
    }

    public async Task<RegisterDeviceStatus> Register(Guid userId, string token, int maxPerUser, DateTime nowUtc, CancellationToken cancellationToken)
    {
        try
        {
            var owned = await devices.Find(d => d.UserId == userId && d.Token == token).AnyAsync(cancellationToken).ConfigureAwait(false);

            // shortcut: contar y luego escribir no es atómico; dos registros simultáneos podrían pasar el límite por uno. Aceptable (no es un control de seguridad).
            if (!owned && await devices.CountDocumentsAsync(d => d.UserId == userId, cancellationToken: cancellationToken).ConfigureAwait(false) >= maxPerUser)
            {
                return RegisterDeviceStatus.LimitReached;
            }

            // _id = token: el upsert no duplica y reasigna el token si venía de otra cuenta.
            await devices.ReplaceOneAsync(
                d => d.Token == token,
                new DeviceDocument { Token = token, UserId = userId, RegisteredAtUtc = nowUtc },
                new ReplaceOptions { IsUpsert = true },
                cancellationToken).ConfigureAwait(false);
            return RegisterDeviceStatus.Registered;
        }
        catch (Exception exception) when (exception is MongoException or TimeoutException)
        {
            throw new PersistenceUnavailableException(exception);
        }
    }
}
