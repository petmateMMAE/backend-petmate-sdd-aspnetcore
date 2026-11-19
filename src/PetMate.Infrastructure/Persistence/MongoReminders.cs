using System.Globalization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using PetMate.Application.Persistence;
using PetMate.Application.Reminders;

namespace PetMate.Infrastructure.Persistence;

/// <summary>Recordatorio (pm-154): el registro de origen es el <c>_id</c>, así que generarlo dos veces no lo duplica. Fechas como <c>yyyy-MM-dd</c> (ordenan como texto).</summary>
internal sealed class ReminderDocument
{
    [BsonId]
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid SourceId { get; set; }

    [BsonElement("kind")]
    public int Kind { get; set; }

    [BsonElement("ownerId")]
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid OwnerId { get; set; }

    [BsonElement("petId")]
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid PetId { get; set; }

    [BsonElement("title")]
    public string Title { get; set; } = string.Empty;

    [BsonElement("dueOn")]
    public string DueOn { get; set; } = string.Empty;

    [BsonElement("remindOn")]
    public string RemindOn { get; set; } = string.Empty;

    [BsonElement("readAtUtc")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    [BsonIgnoreIfNull]
    public DateTime? ReadAtUtc { get; set; }

    [BsonElement("sentAtUtc")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    [BsonIgnoreIfNull]
    public DateTime? SentAtUtc { get; set; }

    [BsonElement("createdAtUtc")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime CreatedAtUtc { get; set; }
}

public sealed class MongoReminderRepository : IReminderRepository
{
    private readonly IMongoCollection<ReminderDocument> reminders;

    public MongoReminderRepository(IMongoDatabase database, MongoCollectionNames names)
    {
        ArgumentNullException.ThrowIfNull(database);
        ArgumentNullException.ThrowIfNull(names);
        reminders = database.GetCollection<ReminderDocument>(names.Reminders);
    }

    public async Task Upsert(Reminder reminder, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(reminder);

        // Si la fecha cambió, el aviso anterior ya no vale: vuelve a pendiente para enviarse en la fecha nueva (pm-160#CA-02).
        var due = Format(reminder.DueOn);
        await Run(() => reminders.UpdateOneAsync(
            d => d.SourceId == reminder.SourceId && d.DueOn != due,
            Builders<ReminderDocument>.Update.Unset(d => d.SentAtUtc),
            cancellationToken: cancellationToken)).ConfigureAwait(false);
        var update = Builders<ReminderDocument>.Update
            .Set(d => d.Kind, (int)reminder.Kind)
            .Set(d => d.OwnerId, reminder.OwnerId)
            .Set(d => d.PetId, reminder.PetId)
            .Set(d => d.Title, reminder.Title)
            .Set(d => d.DueOn, Format(reminder.DueOn))
            .Set(d => d.RemindOn, Format(reminder.RemindOn))
            .SetOnInsert(d => d.CreatedAtUtc, reminder.CreatedAtUtc);
        await Run(() => reminders.UpdateOneAsync(d => d.SourceId == reminder.SourceId, update, new UpdateOptions { IsUpsert = true }, cancellationToken)).ConfigureAwait(false);
    }

    public Task DeleteBySource(Guid sourceId, Guid ownerId, CancellationToken cancellationToken)
        => Run(() => reminders.DeleteOneAsync(d => d.SourceId == sourceId && d.OwnerId == ownerId, cancellationToken));

    public async Task<long> DeleteByPet(Guid petId, Guid ownerId, CancellationToken cancellationToken)
        => (await Run(() => reminders.DeleteManyAsync(d => d.PetId == petId && d.OwnerId == ownerId, cancellationToken)).ConfigureAwait(false)).DeletedCount;

    public async Task<bool> MarkRead(Guid sourceId, Guid ownerId, DateTime nowUtc, CancellationToken cancellationToken)
    {
        // Filtro por dueño: un recordatorio ajeno no se toca. Releer un aviso ya leído no cambia su fecha de lectura.
        var owned = Builders<ReminderDocument>.Filter.And(
            Builders<ReminderDocument>.Filter.Eq(d => d.SourceId, sourceId), Builders<ReminderDocument>.Filter.Eq(d => d.OwnerId, ownerId));
        var result = await Run(() => reminders.UpdateOneAsync(
            Builders<ReminderDocument>.Filter.And(owned, Builders<ReminderDocument>.Filter.Eq(d => d.ReadAtUtc, null)),
            Builders<ReminderDocument>.Update.Set(d => d.ReadAtUtc, nowUtc),
            cancellationToken: cancellationToken)).ConfigureAwait(false);
        return result.ModifiedCount > 0 || await Run(() => reminders.CountDocumentsAsync(owned, cancellationToken: cancellationToken)).ConfigureAwait(false) > 0;
    }

    public async Task<IReadOnlyList<Reminder>> ListUpcoming(Guid ownerId, DateOnly from, int skip, int take, CancellationToken cancellationToken)
    {
        var fromText = Format(from);
        var documents = await Run(() => reminders.Find(UpcomingFilter(ownerId, fromText))
            .SortBy(d => d.DueOn).ThenBy(d => d.SourceId).Skip(skip).Limit(take).ToListAsync(cancellationToken)).ConfigureAwait(false);
        return documents.Select(d => new Reminder(
            d.SourceId, (ReminderKind)d.Kind, d.OwnerId, d.PetId, d.Title, Parse(d.DueOn), Parse(d.RemindOn), d.ReadAtUtc, d.SentAtUtc, d.CreatedAtUtc)).ToArray();
    }

    public Task<long> CountUpcoming(Guid ownerId, DateOnly from, CancellationToken cancellationToken)
    {
        var fromText = Format(from);
        return Run(() => reminders.CountDocumentsAsync(UpcomingFilter(ownerId, fromText), cancellationToken: cancellationToken));
    }

    public async Task<Reminder?> ClaimNextDue(DateOnly today, DateTime nowUtc, CancellationToken cancellationToken)
    {
        var todayText = Format(today);
        var filter = Builders<ReminderDocument>.Filter.And(
            Builders<ReminderDocument>.Filter.Lte(d => d.RemindOn, todayText),
            Builders<ReminderDocument>.Filter.Gte(d => d.DueOn, todayText),
            Builders<ReminderDocument>.Filter.Eq(d => d.SentAtUtc, null));

        // FindOneAndUpdate es atómico por documento: dos instancias nunca reclaman el mismo recordatorio.
        var d = await Run(() => reminders.FindOneAndUpdateAsync(
            filter,
            Builders<ReminderDocument>.Update.Set(x => x.SentAtUtc, nowUtc),
            new FindOneAndUpdateOptions<ReminderDocument> { Sort = Builders<ReminderDocument>.Sort.Ascending(x => x.RemindOn), ReturnDocument = ReturnDocument.After },
            cancellationToken)).ConfigureAwait(false);
        return d is null ? null : new Reminder(d.SourceId, (ReminderKind)d.Kind, d.OwnerId, d.PetId, d.Title, Parse(d.DueOn), Parse(d.RemindOn), d.ReadAtUtc, d.SentAtUtc, d.CreatedAtUtc);
    }

    public Task Release(Guid sourceId, CancellationToken cancellationToken)
        => Run(() => reminders.UpdateOneAsync(d => d.SourceId == sourceId, Builders<ReminderDocument>.Update.Unset(d => d.SentAtUtc), cancellationToken: cancellationToken));

    private static FilterDefinition<ReminderDocument> UpcomingFilter(Guid ownerId, string fromText)
        => Builders<ReminderDocument>.Filter.And(
            Builders<ReminderDocument>.Filter.Eq(d => d.OwnerId, ownerId),
            Builders<ReminderDocument>.Filter.Gte(d => d.DueOn, fromText));

    private static string Format(DateOnly value) => value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    private static DateOnly Parse(string value) => DateOnly.ParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture);

    private static async Task Run(Func<Task> action)
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

    private static async Task<T> Run<T>(Func<Task<T>> action)
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
