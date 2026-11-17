using PetMate.Domain.Common;

namespace PetMate.Domain.Carnet;

/// <summary>Medicamento de la receta (pm-132#CA-01): nombre, dosis, frecuencia, duración e indicaciones.</summary>
public sealed record MedicationDetails(string? Name, string? Dose, string? Frequency, string? Duration, string? Instructions)
{
    public IReadOnlyList<string> InvalidFields()
    {
        var fields = new List<string>();
        if (!HealthRecord.IsValidText(Name, Consultation.MaxMedicationFieldLength, required: true))
        {
            fields.Add("name");
        }

        if (!HealthRecord.IsValidText(Dose, Consultation.MaxMedicationFieldLength, required: true))
        {
            fields.Add("dose");
        }

        if (!HealthRecord.IsValidText(Frequency, Consultation.MaxMedicationFieldLength, required: true))
        {
            fields.Add("frequency");
        }

        if (!HealthRecord.IsValidText(Duration, Consultation.MaxMedicationFieldLength, required: true))
        {
            fields.Add("duration");
        }

        if (!HealthRecord.IsValidText(Instructions, Consultation.MaxInstructionsLength, required: false))
        {
            fields.Add("instructions");
        }

        return fields;
    }
}

/// <summary>Medicamento ya guardado.</summary>
public sealed record Medication(string Name, string Dose, string Frequency, string Duration, string? Instructions)
{
    internal static Medication From(MedicationDetails d)
        => new(d.Name!.Trim(), d.Dose!.Trim(), d.Frequency!.Trim(), d.Duration!.Trim(), string.IsNullOrWhiteSpace(d.Instructions) ? null : d.Instructions.Trim());
}

/// <summary>Datos de una consulta (pm-131): motivo y diagnóstico obligatorios, peso opcional y una receta con varios medicamentos.</summary>
public sealed record ConsultationDetails(string? Reason, string? Diagnosis, decimal? WeightKg, IReadOnlyList<MedicationDetails>? Medications)
{
    public IReadOnlyList<string> InvalidFields()
    {
        var fields = new List<string>();
        if (!HealthRecord.IsValidText(Reason, Consultation.MaxTextLength, required: true))
        {
            fields.Add("reason");
        }

        if (!HealthRecord.IsValidText(Diagnosis, Consultation.MaxTextLength, required: true))
        {
            fields.Add("diagnosis");
        }

        if (WeightKg is not null && !WeightEntry.IsValidKilograms(WeightKg))
        {
            fields.Add("weightKg");
        }

        if (Medications is { } meds && (meds.Count > Consultation.MaxMedications || meds.Any(m => m is null || m.InvalidFields().Count != 0)))
        {
            fields.Add("medications");
        }

        return fields;
    }
}

/// <summary>
/// Consulta veterinaria registrada en el carnet de una mascota (pm-131). Siempre tiene origen de negocio y es de solo lectura para
/// el dueño (pm-096). La receta admite varios medicamentos hasta <see cref="MaxMedications"/>. Su peso no se suma al historial de peso
/// (decisión del plan: el peso de una consulta queda en la consulta).
/// </summary>
public sealed class Consultation : HealthRecord
{
    public const int MaxTextLength = 500;
    public const int MaxMedications = 10;
    public const int MaxMedicationFieldLength = 100;
    public const int MaxInstructionsLength = 300;

    private readonly List<Medication> medications;

    private Consultation(Guid id, Guid petId, Guid ownerId, RecordProvenance provenance, DateOnly consultedOn, string reason, string diagnosis, decimal? weightKg, IEnumerable<Medication> medications, DateTime createdAtUtc)
        : base(id, petId, ownerId, provenance, createdAtUtc)
    {
        ConsultedOn = consultedOn;
        Reason = reason;
        Diagnosis = diagnosis;
        WeightKg = weightKg;
        this.medications = [.. medications];
    }

    public DateOnly ConsultedOn { get; }

    public string Reason { get; }

    public string Diagnosis { get; }

    public decimal? WeightKg { get; }

    public IReadOnlyList<Medication> Medications => medications;

    public static Consultation Create(Guid petId, Guid ownerId, RecordProvenance provenance, ConsultationDetails details, DateOnly today, DateTime nowUtc)
    {
        ArgumentNullException.ThrowIfNull(details);
        ArgumentNullException.ThrowIfNull(provenance);
        if (provenance.Origin != RecordOrigin.Business || details.InvalidFields().Count != 0)
        {
            throw new DomainException("Los datos de la consulta no son válidos.");
        }

        return new Consultation(
            Guid.CreateVersion7(), petId, ownerId, provenance, today, details.Reason!.Trim(), details.Diagnosis!.Trim(), details.WeightKg,
            (details.Medications ?? []).Select(Medication.From), nowUtc);
    }

    /// <summary>Agrega un medicamento a la receta (pm-132). Lo hace la veterinaria autora; el dueño nunca modifica la receta.</summary>
    public void AddMedication(MedicationDetails medication)
    {
        ArgumentNullException.ThrowIfNull(medication);
        if (medication.InvalidFields().Count != 0 || medications.Count >= MaxMedications)
        {
            throw new DomainException("El medicamento no es válido o la receta ya tiene el máximo.");
        }

        medications.Add(Medication.From(medication));
    }

    /// <summary>Rehidrata una consulta persistida. Interno: Infrastructure y pruebas.</summary>
    internal static Consultation Restore(Guid id, Guid petId, Guid ownerId, RecordProvenance provenance, DateOnly consultedOn, string reason, string diagnosis, decimal? weightKg, IEnumerable<Medication> medications, DateTime createdAtUtc)
        => provenance.Origin == RecordOrigin.Business && !string.IsNullOrWhiteSpace(reason) && !string.IsNullOrWhiteSpace(diagnosis)
            ? new Consultation(id, petId, ownerId, provenance, consultedOn, reason, diagnosis, weightKg, medications, createdAtUtc)
            : throw new DomainException("La consulta guardada no es válida.");
}
