using PetMate.Domain.Common;

namespace PetMate.Domain.Access;

/// <summary>Grupos de datos de la mascota que el dueño puede autorizar. Las alergias siempre se incluyen (pm-122#CA-02).</summary>
public enum PetDataCategory
{
    Vaccines = 1,
    Dewormings = 2,
    Weights = 3,
    Consultations = 4,
}

/// <summary>
/// Autorización con la que el dueño da acceso a un negocio (o a cualquier negocio que escanee) a datos de su mascota por un
/// tiempo limitado (pm-122). El dueño no la revoca ni ve quién entró (decisión del usuario, 2026-11-09): la única forma de cortar
/// el acceso antes de que venza es regenerar el QR, que las cancela todas (pm-124).
/// </summary>
public sealed class AccessGrant
{
    /// <summary>Duraciones permitidas en minutos: 30 minutos, 1 día o 7 días (pm-123#CA-01).</summary>
    public static readonly IReadOnlyList<int> AllowedDurationMinutes = [30, 1440, 10080];

    private AccessGrant(Guid id, Guid petId, Guid ownerId, Guid? businessId, IReadOnlyList<PetDataCategory> data, DateTime createdAtUtc, DateTime expiresAtUtc)
    {
        Id = id;
        PetId = petId;
        OwnerId = ownerId;
        BusinessId = businessId;
        Data = data;
        CreatedAtUtc = createdAtUtc;
        ExpiresAtUtc = expiresAtUtc;
    }

    public Guid Id { get; }

    public Guid PetId { get; }

    public Guid OwnerId { get; }

    /// <summary>Negocio destinatario, o <c>null</c> si vale para cualquier negocio que escanee.</summary>
    public Guid? BusinessId { get; }

    /// <summary>Grupos de datos permitidos, sin repetidos. Las alergias van siempre y no se listan aquí.</summary>
    public IReadOnlyList<PetDataCategory> Data { get; }

    public DateTime CreatedAtUtc { get; }

    public DateTime ExpiresAtUtc { get; private set; }

    public static bool IncludesAllergies => true;

    public static bool IsValidDuration(int? minutes) => minutes is { } m && AllowedDurationMinutes.Contains(m);

    public static AccessGrant Create(Guid petId, Guid ownerId, Guid? businessId, IEnumerable<PetDataCategory> data, int durationMinutes, DateTime nowUtc)
    {
        ArgumentNullException.ThrowIfNull(data);
        var now = Guard.Utc(nowUtc, nameof(nowUtc));
        var distinct = data.Distinct().Order().ToArray();
        if (!IsValidDuration(durationMinutes) || distinct.Any(c => !Enum.IsDefined(c)) || businessId == Guid.Empty)
        {
            throw new DomainException("Los datos de la autorización no son válidos.");
        }

        return new AccessGrant(Guid.CreateVersion7(), Guard.NotEmpty(petId, nameof(petId)), Guard.NotEmpty(ownerId, nameof(ownerId)), businessId, distinct, now, now.AddMinutes(durationMinutes));
    }

    /// <summary>Una autorización vencida deja de conceder acceso aunque el registro siga existiendo (pm-122#CA-03).</summary>
    public bool IsActiveAt(DateTime nowUtc) => nowUtc < ExpiresAtUtc;

    /// <summary>Indica si vale para ese negocio: es para cualquier negocio o para ese en concreto.</summary>
    public bool Covers(Guid businessId) => BusinessId is null || BusinessId == businessId;

    /// <summary>Cancela la autorización ahora mismo (regenerar el QR, pm-124).</summary>
    public void Cancel(DateTime nowUtc)
    {
        var now = Guard.Utc(nowUtc, nameof(nowUtc));
        if (now < ExpiresAtUtc)
        {
            ExpiresAtUtc = now;
        }
    }

    /// <summary>Rehidrata una autorización persistida. Interno: Infrastructure y pruebas.</summary>
    internal static AccessGrant Restore(Guid id, Guid petId, Guid ownerId, Guid? businessId, IEnumerable<PetDataCategory> data, DateTime createdAtUtc, DateTime expiresAtUtc)
    {
        var created = Guard.Utc(createdAtUtc, nameof(createdAtUtc));
        var expires = Guard.Utc(expiresAtUtc, nameof(expiresAtUtc));
        var list = data.Distinct().Order().ToArray();
        return expires >= created && list.All(Enum.IsDefined)
            ? new AccessGrant(Guard.NotEmpty(id, nameof(id)), Guard.NotEmpty(petId, nameof(petId)), Guard.NotEmpty(ownerId, nameof(ownerId)), businessId, list, created, expires)
            : throw new DomainException("La autorización guardada no es válida.");
    }
}

/// <summary>Registro interno de un acceso concedido a una mascota (pm-129). El dueño no puede consultarlo.</summary>
public sealed class ScanLog
{
    private ScanLog(Guid id, Guid petId, string petName, string? photoUrl, Guid businessId, DateTime atUtc)
    {
        Id = id;
        PetId = petId;
        PetName = petName;
        PhotoUrl = photoUrl;
        BusinessId = businessId;
        AtUtc = atUtc;
    }

    public Guid Id { get; }

    public Guid PetId { get; }

    /// <summary>Nombre y foto de la mascota al escanear: permiten listar las mascotas atendidas sin abrir su ficha (pm-130).</summary>
    public string PetName { get; }

    public string? PhotoUrl { get; }

    public Guid BusinessId { get; }

    public DateTime AtUtc { get; }

    public static ScanLog Record(Guid petId, string petName, string? photoUrl, Guid businessId, DateTime nowUtc)
        => new(Guid.CreateVersion7(), Guard.NotEmpty(petId, nameof(petId)), petName, photoUrl, Guard.NotEmpty(businessId, nameof(businessId)), Guard.Utc(nowUtc, nameof(nowUtc)));

    /// <summary>Rehidrata un registro persistido. Interno: Infrastructure y pruebas.</summary>
    internal static ScanLog Restore(Guid id, Guid petId, string petName, string? photoUrl, Guid businessId, DateTime atUtc)
        => new(Guard.NotEmpty(id, nameof(id)), Guard.NotEmpty(petId, nameof(petId)), petName, photoUrl, Guard.NotEmpty(businessId, nameof(businessId)), Guard.Utc(atUtc, nameof(atUtc)));
}
