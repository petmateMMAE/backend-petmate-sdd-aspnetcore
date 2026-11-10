using PetMate.Domain.Common;

namespace PetMate.Domain.Carnet;

/// <summary>Quién creó un registro del carnet (pm-096): el dueño o un negocio afiliado.</summary>
public enum RecordOrigin
{
    Owner = 1,
    Business = 2,
}

/// <summary>
/// Origen de un registro del carnet. Un registro de negocio guarda el identificador y el nombre del negocio y es de solo
/// lectura para el dueño; uno del dueño solo lo modifica su dueño (pm-096#CA-01 a CA-03).
/// </summary>
public sealed record RecordProvenance(RecordOrigin Origin, Guid? BusinessId, string? BusinessName)
{
    public static RecordProvenance ByOwner { get; } = new(RecordOrigin.Owner, null, null);

    public static RecordProvenance ByBusiness(Guid businessId, string businessName)
    {
        if (businessId == Guid.Empty || string.IsNullOrWhiteSpace(businessName))
        {
            throw new DomainException("El registro de un negocio necesita su identificador y su nombre.");
        }

        return new RecordProvenance(RecordOrigin.Business, businessId, businessName.Trim());
    }

    /// <summary>Un registro es coherente si el negocio solo aparece cuando el origen es de negocio.</summary>
    public bool IsConsistent
        => Origin == RecordOrigin.Business
            ? BusinessId is { } id && id != Guid.Empty && !string.IsNullOrWhiteSpace(BusinessName)
            : Origin == RecordOrigin.Owner && BusinessId is null && BusinessName is null;
}

/// <summary>
/// Base común de los registros del carnet (vacunas, desparasitaciones, pesos y alergias). Cada registro guarda la mascota,
/// el dueño de esa mascota (para filtrar toda consulta por propietario), la fecha de creación y el origen.
/// </summary>
public abstract class HealthRecord
{
    protected HealthRecord(Guid id, Guid petId, Guid ownerId, RecordProvenance provenance, DateTime createdAtUtc)
    {
        ArgumentNullException.ThrowIfNull(provenance);
        if (!provenance.IsConsistent)
        {
            throw new DomainException("El origen del registro no es coherente.");
        }

        Id = Guard.NotEmpty(id, nameof(id));
        PetId = Guard.NotEmpty(petId, nameof(petId));
        OwnerId = Guard.NotEmpty(ownerId, nameof(ownerId));
        Provenance = provenance;
        CreatedAtUtc = Guard.Utc(createdAtUtc, nameof(createdAtUtc));
    }

    public Guid Id { get; }

    public Guid PetId { get; }

    public Guid OwnerId { get; }

    public RecordProvenance Provenance { get; }

    public DateTime CreatedAtUtc { get; }

    /// <summary>El dueño solo puede modificar o borrar los registros que creó él mismo.</summary>
    public bool CanOwnerModify => Provenance.Origin == RecordOrigin.Owner;

    /// <summary>Texto con longitud máxima y sin caracteres de control; vacío solo si no es obligatorio.</summary>
    public static bool IsValidText(string? value, int maxLength, bool required)
    {
        var trimmed = value?.Trim();
        if (string.IsNullOrEmpty(trimmed))
        {
            return !required;
        }

        return trimmed.Length <= maxLength && !trimmed.Any(char.IsControl);
    }

    protected static string? EmptyToNull(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
