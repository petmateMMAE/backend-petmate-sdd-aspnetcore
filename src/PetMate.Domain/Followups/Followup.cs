using PetMate.Domain.Common;

namespace PetMate.Domain.Followups;

/// <summary>Estado del seguimiento de una consulta (pm-138, pm-139).</summary>
public enum FollowupStatus
{
    Treating = 1,
    Improving = 2,
    Discharged = 3,
}

/// <summary>Nota con fecha del seguimiento (pm-140).</summary>
public sealed record FollowupNote(DateTime AtUtc, string Text);

/// <summary>
/// Seguimiento de una consulta (pm-138): lo abre y lo lleva la veterinaria autora de la consulta; el dueño solo lo lee. Tiene un
/// estado y una próxima revisión opcional (3, 7 o 15 días); darlo de alta elimina la revisión.
/// </summary>
public sealed class Followup
{
    public const int MaxNoteLength = 500;
    public const int MaxNotes = 200;

    /// <summary>Plazos permitidos para la próxima revisión, en días (pm-138#CA-01).</summary>
    public static readonly IReadOnlyList<int> AllowedReviewDays = [3, 7, 15];

    private readonly List<FollowupNote> notes;

    private Followup(Guid id, Guid consultationId, Guid petId, string petName, Guid ownerId, Guid businessId, FollowupStatus status, DateOnly? nextReviewOn, IEnumerable<FollowupNote> notes, DateTime createdAtUtc, DateTime updatedAtUtc)
    {
        Id = id;
        ConsultationId = consultationId;
        PetId = petId;
        PetName = petName;
        OwnerId = ownerId;
        BusinessId = businessId;
        Status = status;
        NextReviewOn = nextReviewOn;
        this.notes = [.. notes];
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = updatedAtUtc;
    }

    public Guid Id { get; }

    public Guid ConsultationId { get; }

    public Guid PetId { get; }

    /// <summary>Nombre de la mascota al abrir el seguimiento (para el listado de la veterinaria).</summary>
    public string PetName { get; }

    public Guid OwnerId { get; }

    public Guid BusinessId { get; }

    public FollowupStatus Status { get; private set; }

    public DateOnly? NextReviewOn { get; private set; }

    public IReadOnlyList<FollowupNote> Notes => notes;

    public DateTime CreatedAtUtc { get; }

    public DateTime UpdatedAtUtc { get; private set; }

    public static bool IsValidReviewDays(int? days) => days is null || AllowedReviewDays.Contains(days.Value);

    public static Followup Open(Guid consultationId, Guid petId, string petName, Guid ownerId, Guid businessId, int? reviewInDays, DateOnly today, DateTime nowUtc)
    {
        var now = Guard.Utc(nowUtc, nameof(nowUtc));
        if (!IsValidReviewDays(reviewInDays))
        {
            throw new DomainException("La próxima revisión debe ser en 3, 7 o 15 días.");
        }

        return new Followup(
            Guid.CreateVersion7(), Guard.NotEmpty(consultationId, nameof(consultationId)), Guard.NotEmpty(petId, nameof(petId)), petName, Guard.NotEmpty(ownerId, nameof(ownerId)),
            Guard.NotEmpty(businessId, nameof(businessId)), FollowupStatus.Treating, reviewInDays is { } d ? today.AddDays(d) : null, [], now, now);
    }

    /// <summary>Cambia el estado y la próxima revisión. «Dado de alta» elimina la revisión (pm-139#CA-02).</summary>
    public void Update(FollowupStatus status, int? reviewInDays, DateOnly today, DateTime nowUtc)
    {
        var now = Guard.Utc(nowUtc, nameof(nowUtc));
        if (!Enum.IsDefined(status) || !IsValidReviewDays(reviewInDays))
        {
            throw new DomainException("Los datos del seguimiento no son válidos.");
        }

        Status = status;
        NextReviewOn = status == FollowupStatus.Discharged ? null : reviewInDays is { } d ? today.AddDays(d) : NextReviewOn;
        UpdatedAtUtc = now;
    }

    /// <summary>Vuelve a «En tratamiento» al reabrir la conversación (pm-147#CA-02).</summary>
    public void Reopen(DateTime nowUtc)
    {
        Status = FollowupStatus.Treating;
        UpdatedAtUtc = Guard.Utc(nowUtc, nameof(nowUtc));
    }

    public void AddNote(string text, DateTime nowUtc)
    {
        var now = Guard.Utc(nowUtc, nameof(nowUtc));
        var trimmed = text?.Trim();
        if (string.IsNullOrEmpty(trimmed) || trimmed.Length > MaxNoteLength || trimmed.Any(c => char.IsControl(c) && c is not ('\n' or '\r')) || notes.Count >= MaxNotes)
        {
            throw new DomainException("La nota no es válida.");
        }

        notes.Add(new FollowupNote(now, trimmed));
        UpdatedAtUtc = now;
    }

    /// <summary>Una revisión vencida es la que ya pasó y el seguimiento sigue abierto (pm-142#CA-02).</summary>
    public bool IsOverdue(DateOnly today) => Status != FollowupStatus.Discharged && NextReviewOn is { } review && review < today;

    /// <summary>Rehidrata un seguimiento persistido. Interno: Infrastructure y pruebas.</summary>
    internal static Followup Restore(Guid id, Guid consultationId, Guid petId, string petName, Guid ownerId, Guid businessId, FollowupStatus status, DateOnly? nextReviewOn, IEnumerable<FollowupNote> notes, DateTime createdAtUtc, DateTime updatedAtUtc)
        => Enum.IsDefined(status)
            ? new Followup(id, consultationId, petId, petName, ownerId, businessId, status, nextReviewOn, notes, Guard.Utc(createdAtUtc, nameof(createdAtUtc)), Guard.Utc(updatedAtUtc, nameof(updatedAtUtc)))
            : throw new DomainException("El seguimiento guardado no es válido.");
}
