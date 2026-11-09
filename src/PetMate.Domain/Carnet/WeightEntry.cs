using PetMate.Domain.Common;

namespace PetMate.Domain.Carnet;

/// <summary>Datos de un peso (pm-101): kilogramos con un decimal, mayores que cero, y una fecha que no sea futura.</summary>
public sealed record WeightDetails(DateOnly? MeasuredOn, decimal? Kilograms)
{
    public IReadOnlyList<string> InvalidFields(DateOnly today)
    {
        var fields = new List<string>();
        if (MeasuredOn is not { } measured || measured > today)
        {
            fields.Add("measuredOn");
        }

        if (!WeightEntry.IsValidKilograms(Kilograms))
        {
            fields.Add("kilograms");
        }

        return fields;
    }
}

/// <summary>Peso registrado en el carnet de una mascota.</summary>
public sealed class WeightEntry : HealthRecord
{
    /// <summary>Tope de cordura (pm-101): ninguna mascota doméstica pesa más.</summary>
    public const decimal MaxKilograms = 200m;

    private WeightEntry(Guid id, Guid petId, Guid ownerId, RecordProvenance provenance, WeightDetails details, DateTime createdAtUtc)
        : base(id, petId, ownerId, provenance, createdAtUtc)
    {
        MeasuredOn = details.MeasuredOn!.Value;
        Kilograms = details.Kilograms!.Value;
    }

    public DateOnly MeasuredOn { get; }

    public decimal Kilograms { get; }

    public static WeightEntry Create(Guid petId, Guid ownerId, RecordProvenance provenance, WeightDetails details, DateOnly today, DateTime nowUtc)
    {
        ArgumentNullException.ThrowIfNull(details);
        return details.InvalidFields(today).Count == 0
            ? new WeightEntry(Guid.CreateVersion7(), petId, ownerId, provenance, details, nowUtc)
            : throw new DomainException("Los datos del peso no son válidos.");
    }

    /// <summary>Rehidrata un peso persistido. Interno: Infrastructure y pruebas.</summary>
    internal static WeightEntry Restore(Guid id, Guid petId, Guid ownerId, RecordProvenance provenance, WeightDetails details, DateTime createdAtUtc)
    {
        ArgumentNullException.ThrowIfNull(details);
        return details.MeasuredOn is not null && IsValidKilograms(details.Kilograms)
            ? new WeightEntry(id, petId, ownerId, provenance, details, createdAtUtc)
            : throw new DomainException("El peso guardado no es válido.");
    }

    public static bool IsValidKilograms(decimal? value)
        => value is { } kg && kg > 0m && kg <= MaxKilograms && decimal.Round(kg, 1) == kg;
}
