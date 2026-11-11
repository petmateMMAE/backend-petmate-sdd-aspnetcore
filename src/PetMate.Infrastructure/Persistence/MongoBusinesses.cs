using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using MongoDB.Driver.GeoJsonObjectModel;
using PetMate.Application.Businesses;
using PetMate.Application.Persistence;
using PetMate.Domain.Businesses;
using PetMate.Domain.Common;

namespace PetMate.Infrastructure.Persistence;

/// <summary>Franja de apertura guardada como texto HH:mm.</summary>
internal sealed class TimeRangeDocument
{
    [BsonElement("open")]
    public string Open { get; set; } = string.Empty;

    [BsonElement("close")]
    public string Close { get; set; } = string.Empty;
}

/// <summary>Negocio afiliado (pm-105): un documento por cuenta, con ubicación GeoJSON para consultas por cercanía.</summary>
[BsonIgnoreExtraElements]
internal sealed class BusinessDocument
{
    [BsonId]
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid Id { get; set; }

    [BsonElement("ownerId")]
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid OwnerId { get; set; }

    [BsonElement("type")]
    public int Type { get; set; }

    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>Nombre en minúsculas y sin acentos, para buscar por nombre (pm-118).</summary>
    [BsonElement("nameSearch")]
    public string NameSearch { get; set; } = string.Empty;

    [BsonElement("phone")]
    public string Phone { get; set; } = string.Empty;

    [BsonElement("whatsApp")]
    [BsonIgnoreIfNull]
    public string? WhatsApp { get; set; }

    [BsonElement("address")]
    public string Address { get; set; } = string.Empty;

    [BsonElement("description")]
    [BsonIgnoreIfNull]
    public string? Description { get; set; }

    /// <summary>Punto GeoJSON [longitud, latitud] con índice 2dsphere.</summary>
    [BsonElement("location")]
    public GeoJsonPoint<GeoJson2DGeographicCoordinates> Location { get; set; } = GeoJson.Point(GeoJson.Geographic(0, 0));

    /// <summary>Cédula profesional: privada, nunca se proyecta a lecturas públicas.</summary>
    [BsonElement("licenseNumber")]
    [BsonIgnoreIfNull]
    public string? LicenseNumber { get; set; }

    [BsonElement("schedule")]
    public Dictionary<string, List<TimeRangeDocument>> Schedule { get; set; } = [];

    [BsonElement("createdAtUtc")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime CreatedAtUtc { get; set; }

    [BsonElement("updatedAtUtc")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime UpdatedAtUtc { get; set; }
}

internal sealed class BusinessServiceDocument
{
    [BsonId]
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid Id { get; set; }

    [BsonElement("businessId")]
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid BusinessId { get; set; }

    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    [BsonElement("description")]
    [BsonIgnoreIfNull]
    public string? Description { get; set; }

    /// <summary>Precio opcional: ausente significa «sin precio», nunca 0.</summary>
    [BsonElement("price")]
    [BsonRepresentation(BsonType.Decimal128)]
    [BsonIgnoreIfNull]
    public decimal? Price { get; set; }

    [BsonElement("createdAtUtc")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime CreatedAtUtc { get; set; }

    [BsonElement("updatedAtUtc")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime UpdatedAtUtc { get; set; }
}

internal static class BusinessMapping
{
    private static DateTime AsUtc(DateTime value) => value.Kind == DateTimeKind.Utc ? value : DateTime.SpecifyKind(value, DateTimeKind.Utc);

    public static BusinessDocument ToDocument(Business b) => new()
    {
        Id = b.Id,
        OwnerId = b.OwnerId,
        Type = (int)b.Type,
        Name = b.Name,
        NameSearch = Business.SearchKey(b.Name),
        Phone = b.Phone,
        WhatsApp = b.WhatsApp,
        Address = b.Address,
        Description = b.Description,
        Location = GeoJson.Point(GeoJson.Geographic(b.Longitude, b.Latitude)),
        LicenseNumber = b.LicenseNumber,
        Schedule = b.Schedule.Days.ToDictionary(
            d => WeeklySchedule.DayNames.First(n => n.Value == d.Key).Key,
            d => d.Value.Select(r => new TimeRangeDocument
            {
                Open = r.Open.ToString("HH:mm", System.Globalization.CultureInfo.InvariantCulture),
                Close = r.Close.ToString("HH:mm", System.Globalization.CultureInfo.InvariantCulture),
            }).ToList()),
        CreatedAtUtc = b.CreatedAtUtc,
        UpdatedAtUtc = b.UpdatedAtUtc,
    };

    public static Business ToDomain(BusinessDocument d)
    {
        var days = d.Schedule.ToDictionary(
            e => e.Key,
            e => (IReadOnlyList<(string Open, string Close)>)e.Value.Select(r => (r.Open, r.Close)).ToArray());
        if (!WeeklySchedule.TryParse(days, out var schedule) || schedule is null)
        {
            throw new DomainException("El horario guardado no es válido.");
        }

        var coordinates = d.Location.Coordinates;
        return Business.Restore(
            d.Id,
            d.OwnerId,
            (BusinessType)d.Type,
            new BusinessProfile(d.Name, d.Description, d.Phone, d.WhatsApp, null),
            d.Address,
            coordinates.Latitude,
            coordinates.Longitude,
            d.LicenseNumber,
            schedule,
            AsUtc(d.CreatedAtUtc),
            AsUtc(d.UpdatedAtUtc));
    }

    public static BusinessServiceDocument ToDocument(BusinessService s) => new()
    {
        Id = s.Id,
        BusinessId = s.BusinessId,
        Name = s.Name,
        Description = s.Description,
        Price = s.Price,
        CreatedAtUtc = s.CreatedAtUtc,
        UpdatedAtUtc = s.UpdatedAtUtc,
    };

    public static BusinessService ToDomain(BusinessServiceDocument d)
        => BusinessService.Restore(d.Id, d.BusinessId, new ServiceDetails(d.Name, d.Description, d.Price), AsUtc(d.CreatedAtUtc), AsUtc(d.UpdatedAtUtc));
}

public sealed class MongoBusinessRepository : IBusinessRepository
{
    private readonly IMongoCollection<BusinessDocument> businesses;

    public MongoBusinessRepository(IMongoDatabase database, MongoCollectionNames names)
    {
        ArgumentNullException.ThrowIfNull(database);
        ArgumentNullException.ThrowIfNull(names);
        businesses = database.GetCollection<BusinessDocument>(names.Businesses);
    }

    public async Task<bool> TryAdd(Business business, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(business);
        try
        {
            await businesses.InsertOneAsync(BusinessMapping.ToDocument(business), cancellationToken: cancellationToken).ConfigureAwait(false);
            return true;
        }
        catch (MongoWriteException exception) when (exception.WriteError.Category == ServerErrorCategory.DuplicateKey)
        {
            return false;
        }
        catch (Exception exception) when (exception is MongoException or TimeoutException or DomainException)
        {
            throw new PersistenceUnavailableException(exception);
        }
    }

    public async Task<Business?> FindByOwner(Guid ownerId, CancellationToken cancellationToken)
    {
        try
        {
            var document = await businesses.Find(Builders<BusinessDocument>.Filter.Eq(d => d.OwnerId, ownerId)).FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
            return document is null ? null : BusinessMapping.ToDomain(document);
        }
        catch (Exception exception) when (exception is MongoException or TimeoutException or DomainException or FormatException)
        {
            throw new PersistenceUnavailableException(exception);
        }
    }

    public async Task<Business?> FindById(Guid businessId, CancellationToken cancellationToken)
    {
        try
        {
            var document = await businesses.Find(Builders<BusinessDocument>.Filter.Eq(d => d.Id, businessId)).FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
            return document is null ? null : BusinessMapping.ToDomain(document);
        }
        catch (Exception exception) when (exception is MongoException or TimeoutException or DomainException or FormatException)
        {
            throw new PersistenceUnavailableException(exception);
        }
    }

    public async Task<NearbyPage> FindNearby(NearbyQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);
        var filter = new BsonDocument();
        if (query.Types is { Count: > 0 } types)
        {
            filter["type"] = new BsonDocument("$in", new BsonArray(types.Select(t => (int)t)));
        }

        if (!string.IsNullOrEmpty(query.SearchKey))
        {
            // El texto se escapa: se trata como dato, nunca como expresión (pm-118#CA-02).
            filter["nameSearch"] = new BsonDocument("$regex", System.Text.RegularExpressions.Regex.Escape(query.SearchKey));
        }

        var geoNear = new BsonDocument("$geoNear", new BsonDocument
        {
            { "near", new BsonDocument { { "type", "Point" }, { "coordinates", new BsonArray { query.Longitude, query.Latitude } } } },
            { "distanceField", "distanceMeters" },
            { "maxDistance", query.RadiusMeters },
            { "spherical", true },
            { "query", filter },
        });
        try
        {
            var page = PipelineDefinition<BusinessDocument, BsonDocument>.Create(
            [
                geoNear,
                new BsonDocument("$sort", new BsonDocument { { "distanceMeters", 1 }, { "_id", 1 } }),
                new BsonDocument("$skip", query.Skip),
                new BsonDocument("$limit", query.Take),
            ]);
            var count = PipelineDefinition<BusinessDocument, BsonDocument>.Create([geoNear, new BsonDocument("$count", "total")]);
            var documents = await (await businesses.AggregateAsync(page, cancellationToken: cancellationToken).ConfigureAwait(false)).ToListAsync(cancellationToken).ConfigureAwait(false);
            var counted = await (await businesses.AggregateAsync(count, cancellationToken: cancellationToken).ConfigureAwait(false)).FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
            var hits = documents
                .Select(d => new NearbyHit(BusinessMapping.ToDomain(MongoDB.Bson.Serialization.BsonSerializer.Deserialize<BusinessDocument>(d)), d["distanceMeters"].ToDouble()))
                .ToArray();
            return new NearbyPage(hits, counted is null ? 0 : counted["total"].ToInt64());
        }
        catch (Exception exception) when (exception is MongoException or TimeoutException or DomainException or FormatException)
        {
            throw new PersistenceUnavailableException(exception);
        }
    }

    public async Task<bool> UpdateOwned(Business business, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(business);
        var filter = Builders<BusinessDocument>.Filter.And(
            Builders<BusinessDocument>.Filter.Eq(d => d.Id, business.Id),
            Builders<BusinessDocument>.Filter.Eq(d => d.OwnerId, business.OwnerId));
        var result = await Execute(() => businesses.ReplaceOneAsync(filter, BusinessMapping.ToDocument(business), cancellationToken: cancellationToken)).ConfigureAwait(false);
        return result.MatchedCount == 1;
    }

    public async Task<bool> DeleteByOwner(Guid ownerId, CancellationToken cancellationToken)
    {
        var result = await Execute(() => businesses.DeleteOneAsync(Builders<BusinessDocument>.Filter.Eq(d => d.OwnerId, ownerId), cancellationToken)).ConfigureAwait(false);
        return result.DeletedCount == 1;
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

public sealed class MongoBusinessServiceRepository : IBusinessServiceRepository
{
    private readonly IMongoCollection<BusinessServiceDocument> services;

    public MongoBusinessServiceRepository(IMongoDatabase database, MongoCollectionNames names)
    {
        ArgumentNullException.ThrowIfNull(database);
        ArgumentNullException.ThrowIfNull(names);
        services = database.GetCollection<BusinessServiceDocument>(names.BusinessServices);
    }

    private static FilterDefinition<BusinessServiceDocument> Owned(Guid serviceId, Guid businessId)
        => Builders<BusinessServiceDocument>.Filter.And(
            Builders<BusinessServiceDocument>.Filter.Eq(d => d.Id, serviceId),
            Builders<BusinessServiceDocument>.Filter.Eq(d => d.BusinessId, businessId));

    public Task Add(BusinessService service, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(service);
        return Execute(() => services.InsertOneAsync(BusinessMapping.ToDocument(service), cancellationToken: cancellationToken));
    }

    public Task<long> CountByBusiness(Guid businessId, CancellationToken cancellationToken)
        => Execute(() => services.CountDocumentsAsync(Builders<BusinessServiceDocument>.Filter.Eq(d => d.BusinessId, businessId), cancellationToken: cancellationToken));

    public async Task<IReadOnlyList<BusinessService>> ListByBusiness(Guid businessId, int skip, int take, CancellationToken cancellationToken)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(skip);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(take);
        var documents = await Execute(() => services.Find(Builders<BusinessServiceDocument>.Filter.Eq(d => d.BusinessId, businessId))
            .SortBy(d => d.CreatedAtUtc).ThenBy(d => d.Id).Skip(skip).Limit(take).ToListAsync(cancellationToken)).ConfigureAwait(false);
        return Restore(documents);
    }

    public async Task<BusinessService?> FindOwned(Guid serviceId, Guid businessId, CancellationToken cancellationToken)
    {
        var document = await Execute(() => services.Find(Owned(serviceId, businessId)).FirstOrDefaultAsync(cancellationToken)).ConfigureAwait(false);
        return document is null ? null : Restore([document])[0];
    }

    public async Task<bool> UpdateOwned(BusinessService service, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(service);
        var result = await Execute(() => services.ReplaceOneAsync(Owned(service.Id, service.BusinessId), BusinessMapping.ToDocument(service), cancellationToken: cancellationToken)).ConfigureAwait(false);
        return result.MatchedCount == 1;
    }

    public async Task<bool> DeleteOwned(Guid serviceId, Guid businessId, CancellationToken cancellationToken)
    {
        var result = await Execute(() => services.DeleteOneAsync(Owned(serviceId, businessId), cancellationToken)).ConfigureAwait(false);
        return result.DeletedCount == 1;
    }

    public async Task<long> DeleteByBusiness(Guid businessId, CancellationToken cancellationToken)
    {
        var result = await Execute(() => services.DeleteManyAsync(Builders<BusinessServiceDocument>.Filter.Eq(d => d.BusinessId, businessId), cancellationToken)).ConfigureAwait(false);
        return result.DeletedCount;
    }

    private static BusinessService[] Restore(List<BusinessServiceDocument> documents)
    {
        try
        {
            return documents.Select(BusinessMapping.ToDomain).ToArray();
        }
        catch (Exception exception) when (exception is DomainException or FormatException)
        {
            throw new PersistenceUnavailableException(exception);
        }
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
