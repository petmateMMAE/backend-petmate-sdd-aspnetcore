using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using PetMate.Application.Notifications;
using PetMate.Application.Persistence;

namespace PetMate.Infrastructure.Persistence;

/// <summary>Preferencias de aviso (pm-157): un documento por usuario, con el usuario como <c>_id</c>.</summary>
internal sealed class NotificationSettingsDocument
{
    [BsonId]
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid UserId { get; set; }

    [BsonElement("vaccines")]
    public bool Vaccines { get; set; }

    [BsonElement("reviews")]
    public bool Reviews { get; set; }

    [BsonElement("chat")]
    public bool Chat { get; set; }

    [BsonElement("followups")]
    public bool Followups { get; set; }

    [BsonElement("sound")]
    public string Sound { get; set; } = string.Empty;

    [BsonElement("reminderDaysBefore")]
    public int ReminderDaysBefore { get; set; }

    [BsonElement("updatedAtUtc")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime UpdatedAtUtc { get; set; }
}

public sealed class MongoNotificationSettingsRepository : INotificationSettingsRepository
{
    private readonly IMongoCollection<NotificationSettingsDocument> collection;

    public MongoNotificationSettingsRepository(IMongoDatabase database, MongoCollectionNames names)
    {
        ArgumentNullException.ThrowIfNull(database);
        ArgumentNullException.ThrowIfNull(names);
        collection = database.GetCollection<NotificationSettingsDocument>(names.NotificationSettings);
    }

    public async Task<NotificationSettings?> Find(Guid userId, CancellationToken cancellationToken)
    {
        try
        {
            var d = await collection.Find(item => item.UserId == userId).FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
            return d is null ? null : new NotificationSettings(d.UserId, d.Vaccines, d.Reviews, d.Chat, d.Followups, d.Sound, d.ReminderDaysBefore, d.UpdatedAtUtc);
        }
        catch (Exception exception) when (exception is MongoException or TimeoutException)
        {
            throw new PersistenceUnavailableException(exception);
        }
    }

    public async Task Delete(Guid userId, CancellationToken cancellationToken)
    {
        try
        {
            await collection.DeleteOneAsync(item => item.UserId == userId, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception exception) when (exception is MongoException or TimeoutException)
        {
            throw new PersistenceUnavailableException(exception);
        }
    }

    public async Task Save(NotificationSettings settings, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(settings);
        try
        {
            await collection.ReplaceOneAsync(
                item => item.UserId == settings.UserId,
                new NotificationSettingsDocument
                {
                    UserId = settings.UserId,
                    Vaccines = settings.Vaccines,
                    Reviews = settings.Reviews,
                    Chat = settings.Chat,
                    Followups = settings.Followups,
                    Sound = settings.Sound,
                    ReminderDaysBefore = settings.ReminderDaysBefore,
                    UpdatedAtUtc = settings.UpdatedAtUtc ?? DateTime.UtcNow,
                },
                new ReplaceOptions { IsUpsert = true },
                cancellationToken).ConfigureAwait(false);
        }
        catch (Exception exception) when (exception is MongoException or TimeoutException)
        {
            throw new PersistenceUnavailableException(exception);
        }
    }
}
