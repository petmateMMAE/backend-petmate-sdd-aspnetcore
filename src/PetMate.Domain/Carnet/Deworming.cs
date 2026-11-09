using PetMate.Domain.Common;

namespace PetMate.Domain.Carnet;

/// <summary>Tipo de desparasitación.</summary>
public enum DewormingType
{
    Internal = 1,
    External = 2,
}

/// <summary>Datos de una desparasitación (pm-099): fecha y producto obligatorios; tipo, quién aplicó y próxima fecha opcionales.</summary>
public sealed record DewormingDetails(DateOnly? AppliedOn, string? Product, DewormingType? Type, string? AppliedBy, DateOnly? NextOn)
{
    /// <summary>Campos inválidos (nombres JSON). La fecha no puede ser futura y la próxima fecha debe ser futura.</summary>
    public IReadOnlyList<string> InvalidFields(DateOnly today)
    {
        var fields = new List<string>();
        if (AppliedOn is not { } applied || applied > today)
        {
            fields.Add("appliedOn");
        }

        if (!HealthRecord.IsValidText(Product, Deworming.MaxProductLength, required: true))
        {
            fields.Add("product");
        }

        if (Type is { } type && !Enum.IsDefined(type))
        {
            fields.Add("type");
        }

        if (!HealthRecord.IsValidText(AppliedBy, Deworming.MaxAppliedByLength, required: false))
        {
            fields.Add("appliedBy");
        }

        if (NextOn is { } next && next <= today)
        {
            fields.Add("nextOn");
        }

        return fields;
    }
}

/// <summary>Desparasitación registrada en el carnet de una mascota.</summary>
public sealed class Deworming : HealthRecord
{
    public const int MaxProductLength = 100;
    public const int MaxAppliedByLength = 100;

    private Deworming(Guid id, Guid petId, Guid ownerId, RecordProvenance provenance, DewormingDetails details, DateTime createdAtUtc)
        : base(id, petId, ownerId, provenance, createdAtUtc)
    {
        AppliedOn = details.AppliedOn!.Value;
        Product = details.Product!.Trim();
        Type = details.Type;
        AppliedBy = EmptyToNull(details.AppliedBy);
        NextOn = details.NextOn;
    }

    public DateOnly AppliedOn { get; }

    public string Product { get; }

    public DewormingType? Type { get; }

    public string? AppliedBy { get; }

    public DateOnly? NextOn { get; }

    public static Deworming Create(Guid petId, Guid ownerId, RecordProvenance provenance, DewormingDetails details, DateOnly today, DateTime nowUtc)
    {
        ArgumentNullException.ThrowIfNull(details);
        return details.InvalidFields(today).Count == 0
            ? new Deworming(Guid.CreateVersion7(), petId, ownerId, provenance, details, nowUtc)
            : throw new DomainException("Los datos de la desparasitación no son válidos.");
    }

    /// <summary>Rehidrata una desparasitación persistida. Interno: Infrastructure y pruebas.</summary>
    internal static Deworming Restore(Guid id, Guid petId, Guid ownerId, RecordProvenance provenance, DewormingDetails details, DateTime createdAtUtc)
    {
        ArgumentNullException.ThrowIfNull(details);
        if (details.AppliedOn is null || !IsValidText(details.Product, MaxProductLength, required: true) || (details.Type is { } type && !Enum.IsDefined(type)))
        {
            throw new DomainException("La desparasitación guardada no es válida.");
        }

        return new Deworming(id, petId, ownerId, provenance, details, createdAtUtc);
    }
}
