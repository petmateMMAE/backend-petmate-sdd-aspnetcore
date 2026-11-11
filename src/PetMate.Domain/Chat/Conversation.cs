using PetMate.Domain.Common;

namespace PetMate.Domain.Chat;

/// <summary>Tipo de mensaje: escrito por un participante o aviso del sistema del seguimiento (pm-145#CA-03).</summary>
public enum MessageKind
{
    Text = 1,
    System = 2,
}

/// <summary>
/// Conversación entre el dueño y la veterinaria de un seguimiento (pm-143): exactamente dos participantes y una sola conversación por
/// seguimiento. Se cierra al dar de alta el seguimiento y la veterinaria puede reabrirla (pm-147). Cada mensaje tiene un número de
/// secuencia consecutivo (<see cref="LastSeq"/>) que sirve de cursor y de marca de lectura.
/// </summary>
public sealed class Conversation
{
    private Conversation(Guid id, Guid followupId, Guid petId, Guid ownerAccountId, Guid businessId, Guid businessAccountId, bool isClosed, long lastSeq, long ownerReadSeq, long businessReadSeq, DateTime createdAtUtc, DateTime updatedAtUtc)
    {
        Id = id;
        FollowupId = followupId;
        PetId = petId;
        OwnerAccountId = ownerAccountId;
        BusinessId = businessId;
        BusinessAccountId = businessAccountId;
        IsClosed = isClosed;
        LastSeq = lastSeq;
        OwnerReadSeq = ownerReadSeq;
        BusinessReadSeq = businessReadSeq;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = updatedAtUtc;
    }

    public Guid Id { get; }

    public Guid FollowupId { get; }

    public Guid PetId { get; }

    /// <summary>Cuenta del dueño de la mascota (primer participante).</summary>
    public Guid OwnerAccountId { get; }

    public Guid BusinessId { get; }

    /// <summary>Cuenta dueña de la veterinaria (segundo participante).</summary>
    public Guid BusinessAccountId { get; }

    public bool IsClosed { get; private set; }

    public long LastSeq { get; private set; }

    public long OwnerReadSeq { get; private set; }

    public long BusinessReadSeq { get; private set; }

    public DateTime CreatedAtUtc { get; }

    public DateTime UpdatedAtUtc { get; private set; }

    public static Conversation Open(Guid followupId, Guid petId, Guid ownerAccountId, Guid businessId, Guid businessAccountId, DateTime nowUtc)
    {
        var now = Guard.Utc(nowUtc, nameof(nowUtc));
        if (ownerAccountId == businessAccountId)
        {
            throw new DomainException("La conversación necesita dos participantes distintos.");
        }

        return new Conversation(
            Guid.CreateVersion7(), Guard.NotEmpty(followupId, nameof(followupId)), Guard.NotEmpty(petId, nameof(petId)), Guard.NotEmpty(ownerAccountId, nameof(ownerAccountId)),
            Guard.NotEmpty(businessId, nameof(businessId)), Guard.NotEmpty(businessAccountId, nameof(businessAccountId)), false, 0, 0, 0, now, now);
    }

    /// <summary>Solo los dos participantes pertenecen a la conversación (pm-143#CA-03).</summary>
    public bool HasParticipant(Guid accountId) => accountId == OwnerAccountId || accountId == BusinessAccountId;

    public bool IsBusinessSide(Guid accountId) => accountId == BusinessAccountId;

    public long ReadSeqOf(Guid accountId) => IsBusinessSide(accountId) ? BusinessReadSeq : OwnerReadSeq;

    public void Close(DateTime nowUtc)
    {
        IsClosed = true;
        UpdatedAtUtc = Guard.Utc(nowUtc, nameof(nowUtc));
    }

    public void Reopen(DateTime nowUtc)
    {
        IsClosed = false;
        UpdatedAtUtc = Guard.Utc(nowUtc, nameof(nowUtc));
    }

    /// <summary>Rehidrata una conversación persistida. Interno: Infrastructure y pruebas.</summary>
    internal static Conversation Restore(Guid id, Guid followupId, Guid petId, Guid ownerAccountId, Guid businessId, Guid businessAccountId, bool isClosed, long lastSeq, long ownerReadSeq, long businessReadSeq, DateTime createdAtUtc, DateTime updatedAtUtc)
        => new(id, followupId, petId, ownerAccountId, businessId, businessAccountId, isClosed, lastSeq, ownerReadSeq, businessReadSeq, Guard.Utc(createdAtUtc, nameof(createdAtUtc)), Guard.Utc(updatedAtUtc, nameof(updatedAtUtc)));
}

/// <summary>Mensaje de una conversación (pm-144). Los avisos del sistema no tienen remitente.</summary>
public sealed class Message
{
    public const int MaxTextLength = 1000;

    private Message(Guid id, Guid conversationId, long seq, MessageKind kind, Guid? senderAccountId, string text, DateTime createdAtUtc)
    {
        Id = id;
        ConversationId = conversationId;
        Seq = seq;
        Kind = kind;
        SenderAccountId = senderAccountId;
        Text = text;
        CreatedAtUtc = createdAtUtc;
    }

    public Guid Id { get; }

    public Guid ConversationId { get; }

    public long Seq { get; }

    public MessageKind Kind { get; }

    public Guid? SenderAccountId { get; }

    public string Text { get; }

    public DateTime CreatedAtUtc { get; }

    public static bool IsValidText(string? text)
    {
        var trimmed = text?.Trim();
        return !string.IsNullOrEmpty(trimmed) && trimmed.Length <= MaxTextLength && !trimmed.Any(c => char.IsControl(c) && c is not ('\n' or '\r' or '\t'));
    }

    public static Message FromParticipant(Guid conversationId, long seq, Guid senderAccountId, string text, DateTime nowUtc)
        => IsValidText(text)
            ? new Message(Guid.CreateVersion7(), Guard.NotEmpty(conversationId, nameof(conversationId)), seq, MessageKind.Text, Guard.NotEmpty(senderAccountId, nameof(senderAccountId)), text.Trim(), Guard.Utc(nowUtc, nameof(nowUtc)))
            : throw new DomainException("El mensaje no es válido.");

    public static Message FromSystem(Guid conversationId, long seq, string text, DateTime nowUtc)
        => new(Guid.CreateVersion7(), Guard.NotEmpty(conversationId, nameof(conversationId)), seq, MessageKind.System, null, text, Guard.Utc(nowUtc, nameof(nowUtc)));

    /// <summary>Rehidrata un mensaje persistido. Interno: Infrastructure y pruebas.</summary>
    internal static Message Restore(Guid id, Guid conversationId, long seq, MessageKind kind, Guid? senderAccountId, string text, DateTime createdAtUtc)
        => new(id, conversationId, seq, kind, senderAccountId, text, Guard.Utc(createdAtUtc, nameof(createdAtUtc)));
}
