using PetMate.Domain.Common;

namespace PetMate.Domain.Pets;

/// <summary>Sexo de la mascota. <see cref="Unknown"/> cubre animales rescatados o sin dato.</summary>
public enum PetSex
{
    Male = 1,
    Female = 2,
    Unknown = 3,
}

/// <summary>
/// Datos editables de una mascota (pm-039), tal como los captura la maqueta móvil: nombre, especie, raza, fecha de
/// nacimiento, sexo y fotografía. Los textos se recortan antes de validarse.
/// </summary>
public sealed record PetDetails(string Name, string Species, string? Breed, DateOnly? BirthDate, PetSex Sex, string? PhotoUrl);

/// <summary>
/// Mascota de un Owner (pm-039). El propietario se fija al crearla y no cambia nunca: cualquier consulta, actualización o
/// eliminación se filtra por él. La eliminación es física (decisión del usuario, 2026-11-08), por eso no hay campo de estado.
/// </summary>
public sealed class Pet
{
    public const int MaxNameLength = 50;
    /// <summary>Especies permitidas (lista cerrada, decisión 2026-11-09).</summary>
    public const string SpeciesDog = "Perro";

    public const string SpeciesCat = "Gato";
    public const int MaxBreedLength = 60;
    public const int MaxPhotoUrlLength = 2048;
    public const int MaxAgeInYears = 50;

    private Pet(Guid id, Guid ownerId, PetDetails details, PetQr qr, DateTime createdAtUtc, DateTime updatedAtUtc)
    {
        Qr = qr;
        Id = id;
        OwnerId = ownerId;
        Name = details.Name;
        Species = details.Species;
        Breed = details.Breed;
        BirthDate = details.BirthDate;
        Sex = details.Sex;
        PhotoUrl = details.PhotoUrl;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = updatedAtUtc;
    }

    public Guid Id { get; }

    public Guid OwnerId { get; }

    public string Name { get; private set; }

    public string Species { get; private set; }

    public string? Breed { get; private set; }

    public DateOnly? BirthDate { get; private set; }

    public PetSex Sex { get; private set; }

    /// <summary>URL HTTPS de la fotografía. La carga de archivos llega con pm-079/pm-080; nunca se guarda en disco local.</summary>
    public string? PhotoUrl { get; private set; }

    /// <summary>QR permanente de la mascota (pm-093). Nunca se incluye en las respuestas de mascotas.</summary>
    public PetQr Qr { get; private set; }

    public DateTime CreatedAtUtc { get; }

    public DateTime UpdatedAtUtc { get; private set; }

    /// <summary>Crea una mascota para el propietario indicado (tomado siempre del token, nunca del cuerpo).</summary>
    public static Pet Create(Guid ownerId, PetDetails details, DateTime nowUtc)
    {
        var now = Guard.Utc(nowUtc, nameof(nowUtc));
        return new Pet(Guid.CreateVersion7(), Guard.NotEmpty(ownerId, nameof(ownerId)), Normalize(details, now), PetQr.Generate(), now, now);
    }

    /// <summary>Genera un QR nuevo (pm-095). El anterior deja de ser válido de inmediato.</summary>
    public void RegenerateQr(DateTime nowUtc)
    {
        Qr = PetQr.Generate();
        UpdatedAtUtc = Guard.Utc(nowUtc, nameof(nowUtc));
    }

    /// <summary>Reemplaza los datos editables. El identificador, el propietario y la fecha de alta no cambian.</summary>
    public void Update(PetDetails details, DateTime nowUtc)
    {
        var now = Guard.Utc(nowUtc, nameof(nowUtc));
        var normalized = Normalize(details, now);
        Name = normalized.Name;
        Species = normalized.Species;
        Breed = normalized.Breed;
        BirthDate = normalized.BirthDate;
        Sex = normalized.Sex;
        PhotoUrl = normalized.PhotoUrl;
        UpdatedAtUtc = now;
    }

    /// <summary>Rehidrata una mascota persistida y vuelve a validar sus invariantes. Interno: Infrastructure y pruebas.</summary>
    internal static Pet Restore(Guid id, Guid ownerId, PetDetails details, DateTime createdAtUtc, DateTime updatedAtUtc, PetQr? qr = null)
    {
        var created = Guard.Utc(createdAtUtc, nameof(createdAtUtc));
        var updated = Guard.Utc(updatedAtUtc, nameof(updatedAtUtc));
        if (updated < created)
        {
            throw new DomainException("Las fechas de la mascota no son coherentes.");
        }

        // La fecha de nacimiento se valida contra la última modificación: así un registro antiguo sigue siendo válido.
        return new Pet(Guard.NotEmpty(id, nameof(id)), Guard.NotEmpty(ownerId, nameof(ownerId)), Normalize(details, updated, legacySpecies: true), qr ?? PetQr.Generate(), created, updated);
    }

    public static bool IsValidName(string? value) => IsValidText(value, MaxNameLength, required: true);

    public static bool IsValidSpecies(string? value) => value?.Trim() is SpeciesDog or SpeciesCat;

    public static bool IsValidBreed(string? value) => IsValidText(value, MaxBreedLength, required: false);

    public static bool IsValidSex(PetSex value) => Enum.IsDefined(value);

    /// <summary>La fecha de nacimiento es opcional; si existe, no puede ser futura ni de hace más de 50 años.</summary>
    public static bool IsValidBirthDate(DateOnly? value, DateOnly today)
        => value is null || (value.Value <= today && value.Value >= today.AddYears(-MaxAgeInYears));

    /// <summary>La fotografía es opcional; si existe, debe ser una URL absoluta HTTPS sin credenciales.</summary>
    public static bool IsValidPhotoUrl(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        var trimmed = value.Trim();
        return trimmed.Length <= MaxPhotoUrlLength
            && Uri.TryCreate(trimmed, UriKind.Absolute, out var uri)
            && uri.Scheme == Uri.UriSchemeHttps
            && string.IsNullOrEmpty(uri.UserInfo);
    }

    // legacySpecies: al rehidratar se acepta una especie guardada antes de la lista cerrada, para que una mascota heredada no rompa los listados.
    private static PetDetails Normalize(PetDetails details, DateTime nowUtc, bool legacySpecies = false)
    {
        ArgumentNullException.ThrowIfNull(details);
        var today = DateOnly.FromDateTime(nowUtc);
        if (!IsValidName(details.Name) || !(IsValidSpecies(details.Species) || (legacySpecies && IsValidText(details.Species, 40, required: true))) || !IsValidBreed(details.Breed)
            || !IsValidSex(details.Sex) || !IsValidBirthDate(details.BirthDate, today) || !IsValidPhotoUrl(details.PhotoUrl))
        {
            throw new DomainException("Los datos de la mascota no son válidos.");
        }

        return new PetDetails(
            details.Name.Trim(),
            details.Species.Trim(),
            EmptyToNull(details.Breed),
            details.BirthDate,
            details.Sex,
            EmptyToNull(details.PhotoUrl));
    }

    private static bool IsValidText(string? value, int maxLength, bool required)
    {
        var trimmed = value?.Trim();
        if (string.IsNullOrEmpty(trimmed))
        {
            return !required;
        }

        return trimmed.Length <= maxLength && !trimmed.Any(char.IsControl);
    }

    private static string? EmptyToNull(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
