using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using PetMate.Application.Access;
using PetMate.Application.Followups;
using PetMate.Application.Persistence;
using PetMate.Domain.Carnet;
using PetMate.Domain.Chat;
using PetMate.Domain.Common;
using PetMate.Domain.Followups;

namespace PetMate.Infrastructure.Persistence;

// Consultas, seguimientos y chat en Atlas (pm-131 a pm-149).

internal sealed class MedicationDocument
{
    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    [BsonElement("dose")]
    public string Dose { get; set; } = string.Empty;

    [BsonElement("frequency")]
    public string Frequency { get; set; } = string.Empty;

    [BsonElement("duration")]
    public string Duration { get; set; } = string.Empty;

    [BsonElement("instructions")]
    [BsonIgnoreIfNull]
    public string? Instructions { get; set; }
}

internal sealed class ConsultationDocument : HealthRecordDocument
{
    [BsonElement("consultedOn")]
    public string ConsultedOn { get; set; } = string.Empty;

    [BsonElement("reason")]
    public string Reason { get; set; } = string.Empty;

    [BsonElement("diagnosis")]
    public string Diagnosis { get; set; } = string.Empty;

    [BsonElement("weightKg")]
    [BsonRepresentation(BsonType.Decimal128)]
    [BsonIgnoreIfNull]
    public decimal? WeightKg { get; set; }

    [BsonElement("medications")]
    public List<MedicationDocument> Medications { get; set; } = [];
}

[BsonIgnoreExtraElements]
internal sealed class FollowupDocument
{
    [BsonId]
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid Id { get; set; }

    [BsonElement("consultationId")]
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid ConsultationId { get; set; }

    [BsonElement("petId")]
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid PetId { get; set; }

    [BsonElement("petName")]
    public string PetName { get; set; } = string.Empty;

    [BsonElement("ownerId")]
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid OwnerId { get; set; }

    [BsonElement("businessId")]
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid BusinessId { get; set; }

    [BsonElement("status")]
    public int Status { get; set; }

    [BsonElement("nextReviewOn")]
    [BsonIgnoreIfNull]
    public string? NextReviewOn { get; set; }

    [BsonElement("notes")]
    public List<FollowupNoteDocument> Notes { get; set; } = [];

    [BsonElement("createdAtUtc")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime CreatedAtUtc { get; set; }

    [BsonElement("updatedAtUtc")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime UpdatedAtUtc { get; set; }
}

internal sealed class FollowupNoteDocument
{
    [BsonElement("atUtc")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime AtUtc { get; set; }

    [BsonElement("text")]
    public string Text { get; set; } = string.Empty;
}

[BsonIgnoreExtraElements]
internal sealed class ConversationDocument
{
    [BsonId]
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid Id { get; set; }

    [BsonElement("followupId")]
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid FollowupId { get; set; }

    [BsonElement("petId")]
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid PetId { get; set; }

    [BsonElement("ownerAccountId")]
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid OwnerAccountId { get; set; }

    [BsonElement("businessId")]
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid BusinessId { get; set; }

    [BsonElement("businessAccountId")]
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid BusinessAccountId { get; set; }

    [BsonElement("isClosed")]
    public bool IsClosed { get; set; }

    [BsonElement("lastSeq")]
    public long LastSeq { get; set; }

    [BsonElement("ownerReadSeq")]
    public long OwnerReadSeq { get; set; }

    [BsonElement("businessReadSeq")]
    public long BusinessReadSeq { get; set; }

    [BsonElement("createdAtUtc")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime CreatedAtUtc { get; set; }

    [BsonElement("updatedAtUtc")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime UpdatedAtUtc { get; set; }
}

[BsonIgnoreExtraElements]
internal sealed class MessageDocument
{
    [BsonId]
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid Id { get; set; }

    [BsonElement("conversationId")]
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid ConversationId { get; set; }

    [BsonElement("seq")]
    public long Seq { get; set; }

    [BsonElement("kind")]
    public int Kind { get; set; }

    [BsonElement("senderAccountId")]
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    [BsonIgnoreIfNull]
    public Guid? SenderAccountId { get; set; }

    [BsonElement("text")]
    public string Text { get; set; } = string.Empty;

    [BsonElement("createdAtUtc")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime CreatedAtUtc { get; set; }
}

internal static class VeterinaryMapping
{
    private static DateTime AsUtc(DateTime value) => value.Kind == DateTimeKind.Utc ? value : DateTime.SpecifyKind(value, DateTimeKind.Utc);

    public static ConsultationDocument ToDocument(Consultation c) => CarnetMapping.Fill(
        new ConsultationDocument
        {
            ConsultedOn = CarnetMapping.Format(c.ConsultedOn),
            Reason = c.Reason,
            Diagnosis = c.Diagnosis,
            WeightKg = c.WeightKg,
            Medications = c.Medications.Select(m => new MedicationDocument { Name = m.Name, Dose = m.Dose, Frequency = m.Frequency, Duration = m.Duration, Instructions = m.Instructions }).ToList(),
        }, c);

    public static Consultation ToDomain(ConsultationDocument d) => Consultation.Restore(
        d.Id, d.PetId, d.OwnerId, CarnetMapping.Provenance(d), CarnetMapping.Parse(d.ConsultedOn), d.Reason, d.Diagnosis, d.WeightKg,
        d.Medications.Select(m => new Medication(m.Name, m.Dose, m.Frequency, m.Duration, m.Instructions)), AsUtc(d.CreatedAtUtc));

    public static FollowupDocument ToDocument(Followup f) => new()
    {
        Id = f.Id,
        ConsultationId = f.ConsultationId,
        PetId = f.PetId,
        PetName = f.PetName,
        OwnerId = f.OwnerId,
        BusinessId = f.BusinessId,
        Status = (int)f.Status,
        NextReviewOn = CarnetMapping.Format(f.NextReviewOn),
        Notes = f.Notes.Select(n => new FollowupNoteDocument { AtUtc = n.AtUtc, Text = n.Text }).ToList(),
        CreatedAtUtc = f.CreatedAtUtc,
        UpdatedAtUtc = f.UpdatedAtUtc,
    };

    public static Followup ToDomain(FollowupDocument d) => Followup.Restore(
        d.Id, d.ConsultationId, d.PetId, d.PetName, d.OwnerId, d.BusinessId, (FollowupStatus)d.Status, CarnetMapping.ParseOptional(d.NextReviewOn),
        d.Notes.Select(n => new FollowupNote(AsUtc(n.AtUtc), n.Text)), AsUtc(d.CreatedAtUtc), AsUtc(d.UpdatedAtUtc));

    public static ConversationDocument ToDocument(Conversation c) => new()
    {
        Id = c.Id,
        FollowupId = c.FollowupId,
        PetId = c.PetId,
        OwnerAccountId = c.OwnerAccountId,
        BusinessId = c.BusinessId,
        BusinessAccountId = c.BusinessAccountId,
        IsClosed = c.IsClosed,
        LastSeq = c.LastSeq,
        OwnerReadSeq = c.OwnerReadSeq,
        BusinessReadSeq = c.BusinessReadSeq,
        CreatedAtUtc = c.CreatedAtUtc,
        UpdatedAtUtc = c.UpdatedAtUtc,
    };

    public static Conversation ToDomain(ConversationDocument d) => Conversation.Restore(
        d.Id, d.FollowupId, d.PetId, d.OwnerAccountId, d.BusinessId, d.BusinessAccountId, d.IsClosed, d.LastSeq, d.OwnerReadSeq, d.BusinessReadSeq, AsUtc(d.CreatedAtUtc), AsUtc(d.UpdatedAtUtc));

    public static MessageDocument ToDocument(Message m) => new()
    {
        Id = m.Id,
        ConversationId = m.ConversationId,
        Seq = m.Seq,
        Kind = (int)m.Kind,
        SenderAccountId = m.SenderAccountId,
        Text = m.Text,
        CreatedAtUtc = m.CreatedAtUtc,
    };

    public static Message ToDomain(MessageDocument d) => Message.Restore(d.Id, d.ConversationId, d.Seq, (MessageKind)d.Kind, d.SenderAccountId, d.Text, AsUtc(d.CreatedAtUtc));
}

/// <summary>Ejecuta operaciones de Atlas traduciendo los fallos a <see cref="PersistenceUnavailableException"/>.</summary>
internal static class MongoCall
{
    public static async Task Run(Func<Task> action)
    {
        try
        {
            await action().ConfigureAwait(false);
        }
        catch (Exception exception) when (exception is MongoException or TimeoutException or DomainException or FormatException)
        {
            throw new PersistenceUnavailableException(exception);
        }
    }

    public static async Task<T> Run<T>(Func<Task<T>> action)
    {
        try
        {
            return await action().ConfigureAwait(false);
        }
        catch (Exception exception) when (exception is MongoException or TimeoutException or DomainException or FormatException)
        {
            throw new PersistenceUnavailableException(exception);
        }
    }
}

/// <summary>Consultas en Atlas (pm-131 a pm-134), de la más reciente a la más antigua.</summary>
public sealed class MongoConsultationRepository : IConsultationRepository
{
    private readonly HealthRecordStore<Consultation, ConsultationDocument> store;

    public MongoConsultationRepository(IMongoDatabase database, MongoCollectionNames names)
    {
        ArgumentNullException.ThrowIfNull(database);
        ArgumentNullException.ThrowIfNull(names);
        store = new(database.GetCollection<ConsultationDocument>(names.Consultations), VeterinaryMapping.ToDocument, VeterinaryMapping.ToDomain,
            Builders<ConsultationDocument>.Sort.Descending(d => d.ConsultedOn).Descending(d => d.CreatedAtUtc).Descending(d => d.Id));
    }

    public Task Add(Consultation record, CancellationToken cancellationToken) => store.Add(record, cancellationToken);

    public Task<Consultation?> FindOwned(Guid recordId, Guid ownerId, CancellationToken cancellationToken) => store.FindOwned(recordId, ownerId, cancellationToken);

    public Task<IReadOnlyList<Consultation>> ListByPet(Guid petId, Guid ownerId, int skip, int take, CancellationToken cancellationToken) => store.ListByPet(petId, ownerId, skip, take, cancellationToken);

    public Task<long> CountByPet(Guid petId, Guid ownerId, CancellationToken cancellationToken) => store.CountByPet(petId, ownerId, cancellationToken);

    public Task<bool> DeleteOwned(Guid recordId, Guid ownerId, CancellationToken cancellationToken) => store.DeleteOwned(recordId, ownerId, cancellationToken);

    public Task<long> DeleteByPet(Guid petId, Guid ownerId, CancellationToken cancellationToken) => store.DeleteByPet(petId, ownerId, cancellationToken);

    public async Task<Consultation?> FindByIdForBusiness(Guid consultationId, Guid businessId, CancellationToken cancellationToken)
    {
        var filter = Builders<ConsultationDocument>.Filter.And(
            Builders<ConsultationDocument>.Filter.Eq(d => d.Id, consultationId),
            Builders<ConsultationDocument>.Filter.Eq(d => d.BusinessId, businessId));
        var document = await MongoCall.Run(() => store.Collection.Find(filter).FirstOrDefaultAsync(cancellationToken)).ConfigureAwait(false);
        return document is null ? null : await MongoCall.Run(() => Task.FromResult(VeterinaryMapping.ToDomain(document))).ConfigureAwait(false);
    }

    public async Task<bool> ReplaceForBusiness(Consultation consultation, Guid businessId, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(consultation);
        var filter = Builders<ConsultationDocument>.Filter.And(
            Builders<ConsultationDocument>.Filter.Eq(d => d.Id, consultation.Id),
            Builders<ConsultationDocument>.Filter.Eq(d => d.BusinessId, businessId));
        var result = await MongoCall.Run(() => store.Collection.ReplaceOneAsync(filter, VeterinaryMapping.ToDocument(consultation), cancellationToken: cancellationToken)).ConfigureAwait(false);
        return result.MatchedCount == 1;
    }
}

public sealed class MongoFollowupRepository : IFollowupRepository
{
    private readonly IMongoCollection<FollowupDocument> followups;

    public MongoFollowupRepository(IMongoDatabase database, MongoCollectionNames names)
    {
        ArgumentNullException.ThrowIfNull(database);
        ArgumentNullException.ThrowIfNull(names);
        followups = database.GetCollection<FollowupDocument>(names.Followups);
    }

    public async Task<bool> TryAdd(Followup followup, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(followup);
        try
        {
            await followups.InsertOneAsync(VeterinaryMapping.ToDocument(followup), cancellationToken: cancellationToken).ConfigureAwait(false);
            return true;
        }
        catch (MongoWriteException exception) when (exception.WriteError.Category == ServerErrorCategory.DuplicateKey)
        {
            return false;
        }
        catch (Exception exception) when (exception is MongoException or TimeoutException)
        {
            throw new PersistenceUnavailableException(exception);
        }
    }

    private async Task<Followup?> FindOne(FilterDefinition<FollowupDocument> filter, CancellationToken cancellationToken)
    {
        var document = await MongoCall.Run(() => followups.Find(filter).FirstOrDefaultAsync(cancellationToken)).ConfigureAwait(false);
        return document is null ? null : await MongoCall.Run(() => Task.FromResult(VeterinaryMapping.ToDomain(document))).ConfigureAwait(false);
    }

    public Task<Followup?> FindForBusiness(Guid followupId, Guid businessId, CancellationToken cancellationToken)
        => FindOne(Builders<FollowupDocument>.Filter.And(Builders<FollowupDocument>.Filter.Eq(d => d.Id, followupId), Builders<FollowupDocument>.Filter.Eq(d => d.BusinessId, businessId)), cancellationToken);

    public Task<Followup?> FindByConsultation(Guid consultationId, Guid ownerId, CancellationToken cancellationToken)
        => FindOne(Builders<FollowupDocument>.Filter.And(Builders<FollowupDocument>.Filter.Eq(d => d.ConsultationId, consultationId), Builders<FollowupDocument>.Filter.Eq(d => d.OwnerId, ownerId)), cancellationToken);

    public Task<Followup?> FindById(Guid followupId, CancellationToken cancellationToken)
        => FindOne(Builders<FollowupDocument>.Filter.Eq(d => d.Id, followupId), cancellationToken);

    public async Task<bool> Update(Followup followup, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(followup);
        var filter = Builders<FollowupDocument>.Filter.And(Builders<FollowupDocument>.Filter.Eq(d => d.Id, followup.Id), Builders<FollowupDocument>.Filter.Eq(d => d.BusinessId, followup.BusinessId));
        var result = await MongoCall.Run(() => followups.ReplaceOneAsync(filter, VeterinaryMapping.ToDocument(followup), cancellationToken: cancellationToken)).ConfigureAwait(false);
        return result.MatchedCount == 1;
    }

    private static FilterDefinition<FollowupDocument> ForBusiness(Guid businessId, DateOnly? reviewOn, bool onlyActive)
    {
        var builder = Builders<FollowupDocument>.Filter;
        var filters = new List<FilterDefinition<FollowupDocument>> { builder.Eq(d => d.BusinessId, businessId) };
        if (reviewOn is { } day)
        {
            filters.Add(builder.Eq(d => d.NextReviewOn, CarnetMapping.Format(day)));
        }

        if (onlyActive)
        {
            filters.Add(builder.Ne(d => d.Status, (int)FollowupStatus.Discharged));
        }

        return builder.And(filters);
    }

    public async Task<IReadOnlyList<Followup>> ListByBusiness(Guid businessId, DateOnly? reviewOn, bool onlyActive, int skip, int take, CancellationToken cancellationToken)
    {
        var documents = await MongoCall.Run(() => followups.Find(ForBusiness(businessId, reviewOn, onlyActive))
            .SortByDescending(d => d.UpdatedAtUtc).ThenByDescending(d => d.Id).Skip(skip).Limit(take).ToListAsync(cancellationToken)).ConfigureAwait(false);
        return await MongoCall.Run(() => Task.FromResult<IReadOnlyList<Followup>>(documents.Select(VeterinaryMapping.ToDomain).ToArray())).ConfigureAwait(false);
    }

    public Task<long> CountByBusiness(Guid businessId, DateOnly? reviewOn, bool onlyActive, CancellationToken cancellationToken)
        => MongoCall.Run(() => followups.CountDocumentsAsync(ForBusiness(businessId, reviewOn, onlyActive), cancellationToken: cancellationToken));

    public async Task<long> DeleteByPet(Guid petId, CancellationToken cancellationToken)
        => (await MongoCall.Run(() => followups.DeleteManyAsync(Builders<FollowupDocument>.Filter.Eq(d => d.PetId, petId), cancellationToken)).ConfigureAwait(false)).DeletedCount;
}

public sealed class MongoConversationRepository : IConversationRepository
{
    private readonly IMongoCollection<ConversationDocument> conversations;

    public MongoConversationRepository(IMongoDatabase database, MongoCollectionNames names)
    {
        ArgumentNullException.ThrowIfNull(database);
        ArgumentNullException.ThrowIfNull(names);
        conversations = database.GetCollection<ConversationDocument>(names.Conversations);
    }

    public async Task<bool> TryAdd(Conversation conversation, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(conversation);
        try
        {
            await conversations.InsertOneAsync(VeterinaryMapping.ToDocument(conversation), cancellationToken: cancellationToken).ConfigureAwait(false);
            return true;
        }
        catch (MongoWriteException exception) when (exception.WriteError.Category == ServerErrorCategory.DuplicateKey)
        {
            return false;
        }
        catch (Exception exception) when (exception is MongoException or TimeoutException)
        {
            throw new PersistenceUnavailableException(exception);
        }
    }

    private async Task<Conversation?> FindOne(FilterDefinition<ConversationDocument> filter, CancellationToken cancellationToken)
    {
        var document = await MongoCall.Run(() => conversations.Find(filter).FirstOrDefaultAsync(cancellationToken)).ConfigureAwait(false);
        return document is null ? null : await MongoCall.Run(() => Task.FromResult(VeterinaryMapping.ToDomain(document))).ConfigureAwait(false);
    }

    public Task<Conversation?> FindById(Guid conversationId, CancellationToken cancellationToken)
        => FindOne(Builders<ConversationDocument>.Filter.Eq(d => d.Id, conversationId), cancellationToken);

    public Task<Conversation?> FindByFollowup(Guid followupId, CancellationToken cancellationToken)
        => FindOne(Builders<ConversationDocument>.Filter.Eq(d => d.FollowupId, followupId), cancellationToken);

    private static FilterDefinition<ConversationDocument> ForAccount(Guid accountId)
        => Builders<ConversationDocument>.Filter.Or(
            Builders<ConversationDocument>.Filter.Eq(d => d.OwnerAccountId, accountId),
            Builders<ConversationDocument>.Filter.Eq(d => d.BusinessAccountId, accountId));

    public async Task<IReadOnlyList<Conversation>> ListForAccount(Guid accountId, int skip, int take, CancellationToken cancellationToken)
    {
        var documents = await MongoCall.Run(() => conversations.Find(ForAccount(accountId)).SortByDescending(d => d.UpdatedAtUtc).ThenByDescending(d => d.Id).Skip(skip).Limit(take).ToListAsync(cancellationToken)).ConfigureAwait(false);
        return await MongoCall.Run(() => Task.FromResult<IReadOnlyList<Conversation>>(documents.Select(VeterinaryMapping.ToDomain).ToArray())).ConfigureAwait(false);
    }

    public Task<long> CountForAccount(Guid accountId, CancellationToken cancellationToken)
        => MongoCall.Run(() => conversations.CountDocumentsAsync(ForAccount(accountId), cancellationToken: cancellationToken));

    public async Task<long> NextSeq(Guid conversationId, DateTime nowUtc, CancellationToken cancellationToken)
    {
        var updated = await MongoCall.Run(() => conversations.FindOneAndUpdateAsync(
            Builders<ConversationDocument>.Filter.Eq(d => d.Id, conversationId),
            Builders<ConversationDocument>.Update.Inc(d => d.LastSeq, 1).Set(d => d.UpdatedAtUtc, nowUtc),
            new FindOneAndUpdateOptions<ConversationDocument> { ReturnDocument = ReturnDocument.After },
            cancellationToken)).ConfigureAwait(false);
        return updated?.LastSeq ?? throw new PersistenceUnavailableException(new InvalidOperationException("Conversación inexistente."));
    }

    public Task MarkRead(Guid conversationId, bool businessSide, long upToSeq, CancellationToken cancellationToken)
    {
        var update = businessSide
            ? Builders<ConversationDocument>.Update.Max(d => d.BusinessReadSeq, upToSeq)
            : Builders<ConversationDocument>.Update.Max(d => d.OwnerReadSeq, upToSeq);
        return MongoCall.Run(() => conversations.UpdateOneAsync(Builders<ConversationDocument>.Filter.Eq(d => d.Id, conversationId), update, cancellationToken: cancellationToken));
    }

    public Task SetClosed(Guid conversationId, bool closed, DateTime nowUtc, CancellationToken cancellationToken)
        => MongoCall.Run(() => conversations.UpdateOneAsync(
            Builders<ConversationDocument>.Filter.Eq(d => d.Id, conversationId),
            Builders<ConversationDocument>.Update.Set(d => d.IsClosed, closed).Set(d => d.UpdatedAtUtc, nowUtc),
            cancellationToken: cancellationToken));

    public async Task<IReadOnlyList<Guid>> IdsByPet(Guid petId, CancellationToken cancellationToken)
        => await MongoCall.Run(() => conversations.Find(Builders<ConversationDocument>.Filter.Eq(d => d.PetId, petId)).Project(d => d.Id).ToListAsync(cancellationToken)).ConfigureAwait(false);

    public async Task<long> DeleteByPet(Guid petId, CancellationToken cancellationToken)
        => (await MongoCall.Run(() => conversations.DeleteManyAsync(Builders<ConversationDocument>.Filter.Eq(d => d.PetId, petId), cancellationToken)).ConfigureAwait(false)).DeletedCount;
}

public sealed class MongoMessageRepository : IMessageRepository
{
    private readonly IMongoCollection<MessageDocument> messages;

    public MongoMessageRepository(IMongoDatabase database, MongoCollectionNames names)
    {
        ArgumentNullException.ThrowIfNull(database);
        ArgumentNullException.ThrowIfNull(names);
        messages = database.GetCollection<MessageDocument>(names.Messages);
    }

    public Task Add(Message message, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(message);
        return MongoCall.Run(() => messages.InsertOneAsync(VeterinaryMapping.ToDocument(message), cancellationToken: cancellationToken));
    }

    public async Task<IReadOnlyList<Message>> List(Guid conversationId, long afterSeq, int take, CancellationToken cancellationToken)
    {
        var filter = Builders<MessageDocument>.Filter.And(Builders<MessageDocument>.Filter.Eq(d => d.ConversationId, conversationId), Builders<MessageDocument>.Filter.Gt(d => d.Seq, afterSeq));
        var documents = await MongoCall.Run(() => messages.Find(filter).SortBy(d => d.Seq).Limit(take).ToListAsync(cancellationToken)).ConfigureAwait(false);
        return await MongoCall.Run(() => Task.FromResult<IReadOnlyList<Message>>(documents.Select(VeterinaryMapping.ToDomain).ToArray())).ConfigureAwait(false);
    }

    public async Task<Message?> Find(Guid conversationId, Guid messageId, CancellationToken cancellationToken)
    {
        var filter = Builders<MessageDocument>.Filter.And(Builders<MessageDocument>.Filter.Eq(d => d.ConversationId, conversationId), Builders<MessageDocument>.Filter.Eq(d => d.Id, messageId));
        var document = await MongoCall.Run(() => messages.Find(filter).FirstOrDefaultAsync(cancellationToken)).ConfigureAwait(false);
        return document is null ? null : await MongoCall.Run(() => Task.FromResult(VeterinaryMapping.ToDomain(document))).ConfigureAwait(false);
    }

    public async Task<Message?> Last(Guid conversationId, CancellationToken cancellationToken)
    {
        var document = await MongoCall.Run(() => messages.Find(Builders<MessageDocument>.Filter.Eq(d => d.ConversationId, conversationId)).SortByDescending(d => d.Seq).FirstOrDefaultAsync(cancellationToken)).ConfigureAwait(false);
        return document is null ? null : await MongoCall.Run(() => Task.FromResult(VeterinaryMapping.ToDomain(document))).ConfigureAwait(false);
    }

    public Task<long> CountUnread(Guid conversationId, long afterSeq, Guid exceptSender, CancellationToken cancellationToken)
    {
        var builder = Builders<MessageDocument>.Filter;
        var filter = builder.And(builder.Eq(d => d.ConversationId, conversationId), builder.Gt(d => d.Seq, afterSeq), builder.Ne(d => d.SenderAccountId, exceptSender));
        return MongoCall.Run(() => messages.CountDocumentsAsync(filter, cancellationToken: cancellationToken));
    }

    public async Task<long> DeleteByConversations(IReadOnlyCollection<Guid> conversationIds, CancellationToken cancellationToken)
    {
        if (conversationIds.Count == 0)
        {
            return 0;
        }

        return (await MongoCall.Run(() => messages.DeleteManyAsync(Builders<MessageDocument>.Filter.In(d => d.ConversationId, conversationIds), cancellationToken)).ConfigureAwait(false)).DeletedCount;
    }
}
