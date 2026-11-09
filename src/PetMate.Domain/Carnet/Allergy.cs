using PetMate.Domain.Common;

namespace PetMate.Domain.Carnet;

/// <summary>Tipo de entrada: una alergia o una condición médica.</summary>
public enum AllergyKind
{
    Allergy = 1,
    Condition = 2,
}

/// <summary>Estado de la entrada: activa o superada.</summary>
public enum AllergyStatus
{
    Active = 1,
    Resolved = 2,
}

/// <summary>Datos editables de una alergia o condición (pm-102, pm-103).</summary>
public sealed record AllergyDetails(string? Name, AllergyKind? Kind, string? Notes, AllergyStatus? Status)
{
    public IReadOnlyList<string> InvalidFields()
    {
        var fields = new List<string>();
        if (!HealthRecord.IsValidText(Name, Allergy.MaxNameLength, required: true))
        {
            fields.Add("name");
        }

        if (Kind is not { } kind || !Enum.IsDefined(kind))
        {
            fields.Add("kind");
        }

        if (!HealthRecord.IsValidText(Notes, Allergy.MaxNotesLength, required: false))
        {
            fields.Add("notes");
        }

        if (Status is { } status && !Enum.IsDefined(status))
        {
            fields.Add("status");
        }

        return fields;
    }
}

/// <summary>Alergia o condición registrada en el carnet de una mascota. Es lo único del carnet que el dueño edita.</summary>
public sealed class Allergy : HealthRecord
{
    public const int MaxNameLength = 100;
    public const int MaxNotesLength = 500;

    private Allergy(Guid id, Guid petId, Guid ownerId, RecordProvenance provenance, AllergyDetails details, DateTime createdAtUtc, DateTime updatedAtUtc)
        : base(id, petId, ownerId, provenance, createdAtUtc)
    {
        UpdatedAtUtc = Guard.Utc(updatedAtUtc, nameof(updatedAtUtc));
        Name = details.Name!.Trim();
        Kind = details.Kind!.Value;
        Notes = EmptyToNull(details.Notes);
        Status = details.Status ?? AllergyStatus.Active;
    }

    public string Name { get; private set; }

    public AllergyKind Kind { get; private set; }

    public string? Notes { get; private set; }

    public AllergyStatus Status { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    public static Allergy Create(Guid petId, Guid ownerId, RecordProvenance provenance, AllergyDetails details, DateTime nowUtc)
    {
        ArgumentNullException.ThrowIfNull(details);
        return details.InvalidFields().Count == 0
            ? new Allergy(Guid.CreateVersion7(), petId, ownerId, provenance, details with { Status = AllergyStatus.Active }, nowUtc, nowUtc)
            : throw new DomainException("Los datos de la alergia no son válidos.");
    }

    /// <summary>Edita los datos y el estado (pm-103). Solo si el registro lo creó el dueño.</summary>
    public void Update(AllergyDetails details, DateTime nowUtc)
    {
        ArgumentNullException.ThrowIfNull(details);
        if (!CanOwnerModify)
        {
            throw new DomainException("El registro lo creó un negocio y no se puede modificar.");
        }

        if (details.InvalidFields().Count != 0 || details.Status is null)
        {
            throw new DomainException("Los datos de la alergia no son válidos.");
        }

        Name = details.Name!.Trim();
        Kind = details.Kind!.Value;
        Notes = EmptyToNull(details.Notes);
        Status = details.Status.Value;
        UpdatedAtUtc = Guard.Utc(nowUtc, nameof(nowUtc));
    }

    /// <summary>Rehidrata una alergia persistida. Interno: Infrastructure y pruebas.</summary>
    internal static Allergy Restore(Guid id, Guid petId, Guid ownerId, RecordProvenance provenance, AllergyDetails details, DateTime createdAtUtc, DateTime updatedAtUtc)
    {
        ArgumentNullException.ThrowIfNull(details);
        return details.InvalidFields().Count == 0
            ? new Allergy(id, petId, ownerId, provenance, details, createdAtUtc, updatedAtUtc)
            : throw new DomainException("La alergia guardada no es válida.");
    }
}
