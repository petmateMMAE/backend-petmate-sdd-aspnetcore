using System.Globalization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using PetMate.Application.Carnet;
using PetMate.Application.Persistence;
using PetMate.Domain.Carnet;
using PetMate.Domain.Common;

namespace PetMate.Infrastructure.Persistence;

// Carnet de salud en Atlas (pm-096 a pm-103). Cada registro guarda la mascota y su dueño: toda consulta filtra por el dueño,
// de modo que un registro ajeno se comporta como inexistente. Las fechas de calendario se guardan como texto ISO yyyy-MM-dd.

/// <summary>Campos comunes de un registro del carnet: mascota, dueño, origen y fecha de creación.</summary>
internal abstract class HealthRecordDocument
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

    [BsonElement("origin")]
    public int Origin { get; set; }

    [BsonElement("businessId")]
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    [BsonIgnoreIfNull]
    public Guid? BusinessId { get; set; }

    [BsonElement("businessName")]
    [BsonIgnoreIfNull]
    public string? BusinessName { get; set; }

    [BsonElement("createdAtUtc")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime CreatedAtUtc { get; set; }
}

internal sealed class VaccineDocument : HealthRecordDocument
{
    [BsonElement("appliedOn")]
    public string AppliedOn { get; set; } = string.Empty;

    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    [BsonElement("batch")]
    [BsonIgnoreIfNull]
    public string? Batch { get; set; }

    [BsonElement("appliedBy")]
    [BsonIgnoreIfNull]
    public string? AppliedBy { get; set; }

    [BsonElement("nextDoseOn")]
    [BsonIgnoreIfNull]
    public string? NextDoseOn { get; set; }
}

internal sealed class DewormingDocument : HealthRecordDocument
{
    [BsonElement("appliedOn")]
    public string AppliedOn { get; set; } = string.Empty;

    [BsonElement("product")]
    public string Product { get; set; } = string.Empty;

    [BsonElement("type")]
    [BsonIgnoreIfNull]
    public int? Type { get; set; }

    [BsonElement("appliedBy")]
    [BsonIgnoreIfNull]
    public string? AppliedBy { get; set; }

    [BsonElement("nextOn")]
    [BsonIgnoreIfNull]
    public string? NextOn { get; set; }
}

internal sealed class WeightDocument : HealthRecordDocument
{
    [BsonElement("measuredOn")]
    public string MeasuredOn { get; set; } = string.Empty;

    [BsonElement("kilograms")]
    [BsonRepresentation(BsonType.Decimal128)]
    public decimal Kilograms { get; set; }
}

internal sealed class AllergyDocument : HealthRecordDocument
{
    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    [BsonElement("kind")]
    public int Kind { get; set; }

    [BsonElement("notes")]
    [BsonIgnoreIfNull]
    public string? Notes { get; set; }

    [BsonElement("status")]
    public int Status { get; set; }

    [BsonElement("updatedAtUtc")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime UpdatedAtUtc { get; set; }
}

internal static class CarnetMapping
{
    private const string DateFormat = "yyyy-MM-dd";

    public static string Format(DateOnly value) => value.ToString(DateFormat, CultureInfo.InvariantCulture);

    public static string? Format(DateOnly? value) => value is null ? null : Format(value.Value);

    public static DateOnly Parse(string value) => DateOnly.ParseExact(value, DateFormat, CultureInfo.InvariantCulture);

    public static DateOnly? ParseOptional(string? value) => value is null ? null : Parse(value);

    private static DateTime AsUtc(DateTime value) => value.Kind == DateTimeKind.Utc ? value : DateTime.SpecifyKind(value, DateTimeKind.Utc);

    private static T Fill<T>(T document, HealthRecord record)
        where T : HealthRecordDocument
    {
        document.Id = record.Id;
        document.PetId = record.PetId;
        document.OwnerId = record.OwnerId;
        document.Origin = (int)record.Provenance.Origin;
        document.BusinessId = record.Provenance.BusinessId;
        document.BusinessName = record.Provenance.BusinessName;
        document.CreatedAtUtc = record.CreatedAtUtc;
        return document;
    }

    private static RecordProvenance Provenance(HealthRecordDocument document)
        => Enum.IsDefined((RecordOrigin)document.Origin)
            ? new RecordProvenance((RecordOrigin)document.Origin, document.BusinessId, document.BusinessName)
            : throw new DomainException("El origen del registro guardado no es válido.");

    public static VaccineDocument ToDocument(Vaccine v) => Fill(
        new VaccineDocument { AppliedOn = Format(v.AppliedOn), Name = v.Name, Batch = v.Batch, AppliedBy = v.AppliedBy, NextDoseOn = Format(v.NextDoseOn) }, v);

    public static Vaccine ToDomain(VaccineDocument d) => Vaccine.Restore(
        d.Id, d.PetId, d.OwnerId, Provenance(d), new VaccineDetails(Parse(d.AppliedOn), d.Name, d.Batch, d.AppliedBy, ParseOptional(d.NextDoseOn)), AsUtc(d.CreatedAtUtc));

    public static DewormingDocument ToDocument(Deworming w) => Fill(
        new DewormingDocument { AppliedOn = Format(w.AppliedOn), Product = w.Product, Type = (int?)w.Type, AppliedBy = w.AppliedBy, NextOn = Format(w.NextOn) }, w);

    public static Deworming ToDomain(DewormingDocument d) => Deworming.Restore(
        d.Id, d.PetId, d.OwnerId, Provenance(d), new DewormingDetails(Parse(d.AppliedOn), d.Product, (DewormingType?)d.Type, d.AppliedBy, ParseOptional(d.NextOn)), AsUtc(d.CreatedAtUtc));

    public static WeightDocument ToDocument(WeightEntry w) => Fill(
        new WeightDocument { MeasuredOn = Format(w.MeasuredOn), Kilograms = w.Kilograms }, w);

    public static WeightEntry ToDomain(WeightDocument d) => WeightEntry.Restore(
        d.Id, d.PetId, d.OwnerId, Provenance(d), new WeightDetails(Parse(d.MeasuredOn), d.Kilograms), AsUtc(d.CreatedAtUtc));

    public static AllergyDocument ToDocument(Allergy a) => Fill(
        new AllergyDocument { Name = a.Name, Kind = (int)a.Kind, Notes = a.Notes, Status = (int)a.Status, UpdatedAtUtc = a.UpdatedAtUtc }, a);

    public static Allergy ToDomain(AllergyDocument d) => Allergy.Restore(
        d.Id, d.PetId, d.OwnerId, Provenance(d), new AllergyDetails(d.Name, (AllergyKind)d.Kind, d.Notes, (AllergyStatus)d.Status), AsUtc(d.CreatedAtUtc), AsUtc(d.UpdatedAtUtc));
}

/// <summary>Operaciones comunes de las cuatro colecciones del carnet. Todas filtran por el dueño.</summary>
internal sealed class HealthRecordStore<TRecord, TDocument>(
    IMongoCollection<TDocument> collection,
    Func<TRecord, TDocument> toDocument,
    Func<TDocument, TRecord> toDomain,
    SortDefinition<TDocument> sort)
    where TRecord : HealthRecord
    where TDocument : HealthRecordDocument
{
    public IMongoCollection<TDocument> Collection => collection;

    public static FilterDefinition<TDocument> Owned(Guid recordId, Guid ownerId)
        => Builders<TDocument>.Filter.And(
            Builders<TDocument>.Filter.Eq(document => document.Id, recordId),
            Builders<TDocument>.Filter.Eq(document => document.OwnerId, ownerId));

    public static FilterDefinition<TDocument> OfPet(Guid petId, Guid ownerId)
        => Builders<TDocument>.Filter.And(
            Builders<TDocument>.Filter.Eq(document => document.PetId, petId),
            Builders<TDocument>.Filter.Eq(document => document.OwnerId, ownerId));

    public Task Add(TRecord record, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(record);
        return Execute(() => collection.InsertOneAsync(toDocument(record), cancellationToken: cancellationToken));
    }

    public async Task<TRecord?> FindOwned(Guid recordId, Guid ownerId, CancellationToken cancellationToken)
    {
        var document = await Execute(() => collection.Find(Owned(recordId, ownerId)).FirstOrDefaultAsync(cancellationToken)).ConfigureAwait(false);
        return document is null ? null : Restore(document);
    }

    public async Task<IReadOnlyList<TRecord>> ListByPet(Guid petId, Guid ownerId, int skip, int take, CancellationToken cancellationToken)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(skip);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(take);
        var documents = await Execute(() => collection.Find(OfPet(petId, ownerId)).Sort(sort).Skip(skip).Limit(take).ToListAsync(cancellationToken)).ConfigureAwait(false);
        return documents.Select(Restore).ToArray();
    }

    public Task<long> CountByPet(Guid petId, Guid ownerId, CancellationToken cancellationToken)
        => Execute(() => collection.CountDocumentsAsync(OfPet(petId, ownerId), cancellationToken: cancellationToken));

    public async Task<bool> DeleteOwned(Guid recordId, Guid ownerId, CancellationToken cancellationToken)
    {
        var result = await Execute(() => collection.DeleteOneAsync(Owned(recordId, ownerId), cancellationToken)).ConfigureAwait(false);
        return result.DeletedCount == 1;
    }

    public async Task<long> DeleteByPet(Guid petId, Guid ownerId, CancellationToken cancellationToken)
    {
        var result = await Execute(() => collection.DeleteManyAsync(OfPet(petId, ownerId), cancellationToken)).ConfigureAwait(false);
        return result.DeletedCount;
    }

    /// <summary>Reemplaza el documento solo si es del dueño y lo creó él (origen dueño): un registro de negocio no se toca.</summary>
    public async Task<bool> ReplaceOwned(TRecord record, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(record);
        var filter = Builders<TDocument>.Filter.And(
            Owned(record.Id, record.OwnerId),
            Builders<TDocument>.Filter.Eq(document => document.Origin, (int)RecordOrigin.Owner));
        var result = await Execute(() => collection.ReplaceOneAsync(filter, toDocument(record), cancellationToken: cancellationToken)).ConfigureAwait(false);
        return result.MatchedCount == 1;
    }

    private TRecord Restore(TDocument document)
    {
        try
        {
            return toDomain(document);
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

/// <summary>Vacunas en Atlas (pm-097, pm-098), de la más reciente a la más antigua.</summary>
public sealed class MongoVaccineRepository : IVaccineRepository
{
    private readonly HealthRecordStore<Vaccine, VaccineDocument> store;

    public MongoVaccineRepository(IMongoDatabase database, MongoCollectionNames names)
    {
        ArgumentNullException.ThrowIfNull(database);
        ArgumentNullException.ThrowIfNull(names);
        store = new(database.GetCollection<VaccineDocument>(names.Vaccines), CarnetMapping.ToDocument, CarnetMapping.ToDomain,
            Builders<VaccineDocument>.Sort.Descending(d => d.AppliedOn).Descending(d => d.CreatedAtUtc).Descending(d => d.Id));
    }

    public Task Add(Vaccine record, CancellationToken cancellationToken) => store.Add(record, cancellationToken);

    public Task<Vaccine?> FindOwned(Guid recordId, Guid ownerId, CancellationToken cancellationToken) => store.FindOwned(recordId, ownerId, cancellationToken);

    public Task<IReadOnlyList<Vaccine>> ListByPet(Guid petId, Guid ownerId, int skip, int take, CancellationToken cancellationToken) => store.ListByPet(petId, ownerId, skip, take, cancellationToken);

    public Task<long> CountByPet(Guid petId, Guid ownerId, CancellationToken cancellationToken) => store.CountByPet(petId, ownerId, cancellationToken);

    public Task<bool> DeleteOwned(Guid recordId, Guid ownerId, CancellationToken cancellationToken) => store.DeleteOwned(recordId, ownerId, cancellationToken);

    public Task<long> DeleteByPet(Guid petId, Guid ownerId, CancellationToken cancellationToken) => store.DeleteByPet(petId, ownerId, cancellationToken);
}

/// <summary>Desparasitaciones en Atlas (pm-099, pm-100), de la más reciente a la más antigua.</summary>
public sealed class MongoDewormingRepository : IDewormingRepository
{
    private readonly HealthRecordStore<Deworming, DewormingDocument> store;

    public MongoDewormingRepository(IMongoDatabase database, MongoCollectionNames names)
    {
        ArgumentNullException.ThrowIfNull(database);
        ArgumentNullException.ThrowIfNull(names);
        store = new(database.GetCollection<DewormingDocument>(names.Dewormings), CarnetMapping.ToDocument, CarnetMapping.ToDomain,
            Builders<DewormingDocument>.Sort.Descending(d => d.AppliedOn).Descending(d => d.CreatedAtUtc).Descending(d => d.Id));
    }

    public Task Add(Deworming record, CancellationToken cancellationToken) => store.Add(record, cancellationToken);

    public Task<Deworming?> FindOwned(Guid recordId, Guid ownerId, CancellationToken cancellationToken) => store.FindOwned(recordId, ownerId, cancellationToken);

    public Task<IReadOnlyList<Deworming>> ListByPet(Guid petId, Guid ownerId, int skip, int take, CancellationToken cancellationToken) => store.ListByPet(petId, ownerId, skip, take, cancellationToken);

    public Task<long> CountByPet(Guid petId, Guid ownerId, CancellationToken cancellationToken) => store.CountByPet(petId, ownerId, cancellationToken);

    public Task<bool> DeleteOwned(Guid recordId, Guid ownerId, CancellationToken cancellationToken) => store.DeleteOwned(recordId, ownerId, cancellationToken);

    public Task<long> DeleteByPet(Guid petId, Guid ownerId, CancellationToken cancellationToken) => store.DeleteByPet(petId, ownerId, cancellationToken);
}

/// <summary>Pesos en Atlas (pm-101), del más reciente al más antiguo.</summary>
public sealed class MongoWeightRepository : IWeightRepository
{
    private readonly HealthRecordStore<WeightEntry, WeightDocument> store;

    public MongoWeightRepository(IMongoDatabase database, MongoCollectionNames names)
    {
        ArgumentNullException.ThrowIfNull(database);
        ArgumentNullException.ThrowIfNull(names);
        store = new(database.GetCollection<WeightDocument>(names.Weights), CarnetMapping.ToDocument, CarnetMapping.ToDomain,
            Builders<WeightDocument>.Sort.Descending(d => d.MeasuredOn).Descending(d => d.CreatedAtUtc).Descending(d => d.Id));
    }

    public Task Add(WeightEntry record, CancellationToken cancellationToken) => store.Add(record, cancellationToken);

    public Task<WeightEntry?> FindOwned(Guid recordId, Guid ownerId, CancellationToken cancellationToken) => store.FindOwned(recordId, ownerId, cancellationToken);

    public Task<IReadOnlyList<WeightEntry>> ListByPet(Guid petId, Guid ownerId, int skip, int take, CancellationToken cancellationToken) => store.ListByPet(petId, ownerId, skip, take, cancellationToken);

    public Task<long> CountByPet(Guid petId, Guid ownerId, CancellationToken cancellationToken) => store.CountByPet(petId, ownerId, cancellationToken);

    public Task<bool> DeleteOwned(Guid recordId, Guid ownerId, CancellationToken cancellationToken) => store.DeleteOwned(recordId, ownerId, cancellationToken);

    public Task<long> DeleteByPet(Guid petId, Guid ownerId, CancellationToken cancellationToken) => store.DeleteByPet(petId, ownerId, cancellationToken);
}

/// <summary>Alergias y condiciones en Atlas (pm-102, pm-103): primero las activas, luego las superadas.</summary>
public sealed class MongoAllergyRepository : IAllergyRepository
{
    private readonly HealthRecordStore<Allergy, AllergyDocument> store;

    public MongoAllergyRepository(IMongoDatabase database, MongoCollectionNames names)
    {
        ArgumentNullException.ThrowIfNull(database);
        ArgumentNullException.ThrowIfNull(names);
        store = new(database.GetCollection<AllergyDocument>(names.Allergies), CarnetMapping.ToDocument, CarnetMapping.ToDomain,
            Builders<AllergyDocument>.Sort.Ascending(d => d.Status).Descending(d => d.CreatedAtUtc).Descending(d => d.Id));
    }

    public Task Add(Allergy record, CancellationToken cancellationToken) => store.Add(record, cancellationToken);

    public Task<Allergy?> FindOwned(Guid recordId, Guid ownerId, CancellationToken cancellationToken) => store.FindOwned(recordId, ownerId, cancellationToken);

    public Task<IReadOnlyList<Allergy>> ListByPet(Guid petId, Guid ownerId, int skip, int take, CancellationToken cancellationToken) => store.ListByPet(petId, ownerId, skip, take, cancellationToken);

    public Task<long> CountByPet(Guid petId, Guid ownerId, CancellationToken cancellationToken) => store.CountByPet(petId, ownerId, cancellationToken);

    public Task<bool> DeleteOwned(Guid recordId, Guid ownerId, CancellationToken cancellationToken) => store.DeleteOwned(recordId, ownerId, cancellationToken);

    public Task<long> DeleteByPet(Guid petId, Guid ownerId, CancellationToken cancellationToken) => store.DeleteByPet(petId, ownerId, cancellationToken);

    public Task<bool> UpdateOwned(Allergy allergy, CancellationToken cancellationToken) => store.ReplaceOwned(allergy, cancellationToken);
}
