using PetMate.Domain.Common;

namespace PetMate.Domain.Carnet;

/// <summary>Datos de una vacuna (pm-097): fecha de aplicación y nombre obligatorios; lote, quién aplicó y próxima dosis opcionales.</summary>
public sealed record VaccineDetails(DateOnly? AppliedOn, string? Name, string? Batch, string? AppliedBy, DateOnly? NextDoseOn)
{
    /// <summary>Campos inválidos (nombres JSON). La fecha de aplicación no puede ser futura y la próxima dosis es posterior a ella.</summary>
    public IReadOnlyList<string> InvalidFields(DateOnly today)
    {
        var fields = new List<string>();
        if (AppliedOn is not { } applied || applied > today)
        {
            fields.Add("appliedOn");
        }

        if (!HealthRecord.IsValidText(Name, Vaccine.MaxNameLength, required: true))
        {
            fields.Add("name");
        }

        if (!HealthRecord.IsValidText(Batch, Vaccine.MaxBatchLength, required: false))
        {
            fields.Add("batch");
        }

        if (!HealthRecord.IsValidText(AppliedBy, Vaccine.MaxAppliedByLength, required: false))
        {
            fields.Add("appliedBy");
        }

        if (NextDoseOn is { } next && (AppliedOn is not { } from || next <= from))
        {
            fields.Add("nextDoseOn");
        }

        return fields;
    }
}

/// <summary>Vacuna registrada en el carnet de una mascota.</summary>
public sealed class Vaccine : HealthRecord
{
    public const int MaxNameLength = 100;
    public const int MaxBatchLength = 50;
    public const int MaxAppliedByLength = 100;

    private Vaccine(Guid id, Guid petId, Guid ownerId, RecordProvenance provenance, VaccineDetails details, DateTime createdAtUtc)
        : base(id, petId, ownerId, provenance, createdAtUtc)
    {
        AppliedOn = details.AppliedOn!.Value;
        Name = details.Name!.Trim();
        Batch = EmptyToNull(details.Batch);
        AppliedBy = EmptyToNull(details.AppliedBy);
        NextDoseOn = details.NextDoseOn;
    }

    public DateOnly AppliedOn { get; }

    public string Name { get; }

    public string? Batch { get; }

    public string? AppliedBy { get; }

    public DateOnly? NextDoseOn { get; }

    public static Vaccine Create(Guid petId, Guid ownerId, RecordProvenance provenance, VaccineDetails details, DateOnly today, DateTime nowUtc)
    {
        ArgumentNullException.ThrowIfNull(details);
        return details.InvalidFields(today).Count == 0
            ? new Vaccine(Guid.CreateVersion7(), petId, ownerId, provenance, details, nowUtc)
            : throw new DomainException("Los datos de la vacuna no son válidos.");
    }

    /// <summary>Rehidrata una vacuna persistida. Interno: Infrastructure y pruebas.</summary>
    internal static Vaccine Restore(Guid id, Guid petId, Guid ownerId, RecordProvenance provenance, VaccineDetails details, DateTime createdAtUtc)
    {
        ArgumentNullException.ThrowIfNull(details);
        // Las fechas ya guardadas no se comparan con hoy: solo se exige la coherencia entre ellas.
        return details.InvalidFields(DateOnly.MaxValue).Count == 0
            ? new Vaccine(id, petId, ownerId, provenance, details, createdAtUtc)
            : throw new DomainException("La vacuna guardada no es válida.");
    }
}
