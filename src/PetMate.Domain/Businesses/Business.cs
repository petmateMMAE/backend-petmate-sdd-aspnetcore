using PetMate.Domain.Common;
using PetMate.Domain.Users;

namespace PetMate.Domain.Businesses;

/// <summary>Tipo de negocio; cada negocio tiene uno solo (decisión del usuario, 2026-11-09).</summary>
public enum BusinessType
{
    Veterinary = 1,
    Store = 2,
    Grooming = 3,
}

/// <summary>Horario tal como llega del cliente: día y franjas <c>HH:mm</c>.</summary>
public sealed record ScheduleInput(IReadOnlyDictionary<string, IReadOnlyList<(string Open, string Close)>> Days);

/// <summary>Datos de alta del negocio (pm-107). La cédula profesional solo se pide a las veterinarias y no se valida.</summary>
public sealed record BusinessDetails(
    BusinessType? Type,
    string? Name,
    string? Phone,
    string? WhatsApp,
    string? Address,
    string? Description,
    double? Latitude,
    double? Longitude,
    string? LicenseNumber,
    ScheduleInput? Schedule)
{
    /// <summary>Campos inválidos (nombres JSON).</summary>
    public IReadOnlyList<string> InvalidFields()
    {
        var fields = new List<string>();
        if (Type is not { } type || !Enum.IsDefined(type))
        {
            fields.Add("type");
        }

        fields.AddRange(Business.InvalidProfileFields(Name, Phone, WhatsApp, Description, Schedule));
        if (!IsValidAddressText(Address, Business.MaxAddressLength, required: true))
        {
            fields.Add("address");
        }

        fields.AddRange(Business.InvalidLocationFields(Latitude, Longitude));
        if (!IsValidAddressText(LicenseNumber, Business.MaxLicenseLength, required: false)
            || (Type == BusinessType.Veterinary && string.IsNullOrWhiteSpace(LicenseNumber)))
        {
            fields.Add("licenseNumber");
        }

        return fields;
    }

    private static bool IsValidAddressText(string? value, int max, bool required) => Business.IsValidText(value, max, required);
}

/// <summary>Datos editables del perfil (pm-109). El tipo no se puede cambiar.</summary>
public sealed record BusinessProfile(string? Name, string? Description, string? Phone, string? WhatsApp, ScheduleInput? Schedule)
{
    public IReadOnlyList<string> InvalidFields() => Business.InvalidProfileFields(Name, Phone, WhatsApp, Description, Schedule);
}

/// <summary>
/// Negocio afiliado (pm-105): uno por cuenta, con ubicación geográfica y horario estructurado. La cédula profesional es privada y
/// no forma parte de ningún modelo de lectura público.
/// </summary>
public sealed class Business
{
    public const int MaxNameLength = 100;
    public const int MaxAddressLength = 200;
    public const int MaxDescriptionLength = 500;
    public const int MaxLicenseLength = 30;
    public const int MaxPhotoUrlLength = 2048;

    private Business(Guid id, Guid ownerId, BusinessType type, DateTime createdAtUtc)
    {
        Id = id;
        OwnerId = ownerId;
        Type = type;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = createdAtUtc;
        Name = string.Empty;
        Phone = string.Empty;
        Address = string.Empty;
        Schedule = WeeklySchedule.Closed;
    }

    public Guid Id { get; }

    public Guid OwnerId { get; }

    public BusinessType Type { get; }

    public string Name { get; private set; }

    public string Phone { get; private set; }

    public string? WhatsApp { get; private set; }

    public string Address { get; private set; }

    public string? Description { get; private set; }

    public double Latitude { get; private set; }

    public double Longitude { get; private set; }

    /// <summary>Cédula profesional (privada, solo veterinarias): se captura sin validarse y nunca se publica.</summary>
    public string? LicenseNumber { get; private set; }

    public WeeklySchedule Schedule { get; private set; }

    /// <summary>
    /// URL pública y permanente de la foto del local (pm-111). El contenedor es público de solo lectura y está
    /// separado del de las mascotas, así que aquí no hay nada privado que proteger.
    /// </summary>
    public string? PhotoUrl { get; private set; }

    public DateTime CreatedAtUtc { get; }

    public DateTime UpdatedAtUtc { get; private set; }

    public static Business Create(Guid ownerId, BusinessDetails details, DateTime nowUtc)
    {
        ArgumentNullException.ThrowIfNull(details);
        var now = Guard.Utc(nowUtc, nameof(nowUtc));
        if (details.InvalidFields().Count != 0)
        {
            throw new DomainException("Los datos del negocio no son válidos.");
        }

        var business = new Business(Guid.CreateVersion7(), Guard.NotEmpty(ownerId, nameof(ownerId)), details.Type!.Value, now)
        {
            Address = details.Address!.Trim(),
            Latitude = details.Latitude!.Value,
            Longitude = details.Longitude!.Value,
            LicenseNumber = Empty(details.LicenseNumber),
        };
        business.Apply(new BusinessProfile(details.Name, details.Description, details.Phone, details.WhatsApp, details.Schedule));
        return business;
    }

    /// <summary>Edita el perfil (pm-109). El tipo y la ubicación no cambian aquí.</summary>
    public void UpdateProfile(BusinessProfile profile, DateTime nowUtc)
    {
        ArgumentNullException.ThrowIfNull(profile);
        var now = Guard.Utc(nowUtc, nameof(nowUtc));
        if (profile.InvalidFields().Count != 0)
        {
            throw new DomainException("Los datos del negocio no son válidos.");
        }

        Apply(profile);
        UpdatedAtUtc = now;
    }

    /// <summary>Guarda la URL pública de la foto recién subida (pm-111), o la quita con <c>null</c>.</summary>
    public void SetPhoto(string? publicUrl, DateTime nowUtc)
    {
        var trimmed = publicUrl?.Trim();
        if (!string.IsNullOrEmpty(trimmed)
            && !(trimmed.Length <= MaxPhotoUrlLength
                && Uri.TryCreate(trimmed, UriKind.Absolute, out var uri)
                && uri.Scheme == Uri.UriSchemeHttps
                && string.IsNullOrEmpty(uri.UserInfo)))
        {
            throw new DomainException("La URL de la fotografía del negocio no es válida.");
        }

        PhotoUrl = string.IsNullOrEmpty(trimmed) ? null : trimmed;
        UpdatedAtUtc = Guard.Utc(nowUtc, nameof(nowUtc));
    }

    /// <summary>Cambia las coordenadas (pm-108). La geocodificación ocurre en la app.</summary>
    public void MoveTo(double latitude, double longitude, DateTime nowUtc)
    {
        var now = Guard.Utc(nowUtc, nameof(nowUtc));
        if (InvalidLocationFields(latitude, longitude).Count != 0)
        {
            throw new DomainException("Las coordenadas no son válidas.");
        }

        Latitude = latitude;
        Longitude = longitude;
        UpdatedAtUtc = now;
    }

    /// <summary>Rehidrata un negocio persistido. Interno: Infrastructure y pruebas.</summary>
    internal static Business Restore(
        Guid id,
        Guid ownerId,
        BusinessType type,
        BusinessProfile profile,
        string address,
        double latitude,
        double longitude,
        string? licenseNumber,
        WeeklySchedule schedule,
        DateTime createdAtUtc,
        DateTime updatedAtUtc,
        string? photoUrl = null)
    {
        var created = Guard.Utc(createdAtUtc, nameof(createdAtUtc));
        var updated = Guard.Utc(updatedAtUtc, nameof(updatedAtUtc));
        if (!Enum.IsDefined(type) || updated < created || InvalidLocationFields(latitude, longitude).Count != 0
            || profile.Name is null || !IsValidText(address, MaxAddressLength, required: true))
        {
            throw new DomainException("El negocio guardado no es válido.");
        }

        var business = new Business(Guard.NotEmpty(id, nameof(id)), Guard.NotEmpty(ownerId, nameof(ownerId)), type, created)
        {
            Address = address.Trim(),
            Latitude = latitude,
            Longitude = longitude,
            LicenseNumber = licenseNumber,
            Name = profile.Name.Trim(),
            Phone = profile.Phone ?? string.Empty,
            WhatsApp = profile.WhatsApp,
            Description = profile.Description,
            Schedule = schedule,
            UpdatedAtUtc = updated,
            PhotoUrl = string.IsNullOrWhiteSpace(photoUrl) ? null : photoUrl.Trim(),
        };
        return business;
    }

    public static IReadOnlyList<string> InvalidProfileFields(string? name, string? phone, string? whatsApp, string? description, ScheduleInput? schedule)
    {
        var fields = new List<string>();
        if (!IsValidText(name, MaxNameLength, required: true))
        {
            fields.Add("name");
        }

        if (string.IsNullOrWhiteSpace(phone) || !User.TryNormalizePhone(phone, out _))
        {
            fields.Add("phone");
        }

        if (!User.TryNormalizePhone(whatsApp, out _))
        {
            fields.Add("whatsApp");
        }

        if (!IsValidText(description, MaxDescriptionLength, required: false))
        {
            fields.Add("description");
        }

        if (schedule is not null && !WeeklySchedule.TryParse(schedule.Days, out _))
        {
            fields.Add("schedule");
        }

        return fields;
    }

    public static IReadOnlyList<string> InvalidLocationFields(double? latitude, double? longitude)
    {
        var fields = new List<string>();
        if (latitude is not { } lat || double.IsNaN(lat) || lat is < -90 or > 90)
        {
            fields.Add("latitude");
        }

        if (longitude is not { } lng || double.IsNaN(lng) || lng is < -180 or > 180)
        {
            fields.Add("longitude");
        }

        return fields;
    }

    /// <summary>
    /// Clave de búsqueda por nombre (pm-118#CA-01): minúsculas, sin acentos y con espacios colapsados. Se guarda junto al nombre y
    /// la consulta usa el texto escapado como dato, nunca como expresión.
    /// </summary>
    public static string SearchKey(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        var decomposed = text.Trim().Normalize(System.Text.NormalizationForm.FormD);
        var builder = new System.Text.StringBuilder(decomposed.Length);
        foreach (var c in decomposed)
        {
            if (System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) != System.Globalization.UnicodeCategory.NonSpacingMark)
            {
                builder.Append(char.ToLowerInvariant(c));
            }
        }

        return string.Join(' ', builder.ToString().Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }

    public static bool IsValidText(string? value, int maxLength, bool required)
    {
        var trimmed = value?.Trim();
        if (string.IsNullOrEmpty(trimmed))
        {
            return !required;
        }

        return trimmed.Length <= maxLength && !trimmed.Any(char.IsControl);
    }

    private void Apply(BusinessProfile profile)
    {
        // Los datos ya se validaron: si algo no se pudiera convertir es un error de programación y se rechaza.
        if (!User.TryNormalizePhone(profile.Phone, out var phone) || !User.TryNormalizePhone(profile.WhatsApp, out var whatsApp))
        {
            throw new DomainException("Los teléfonos del negocio no son válidos.");
        }

        var schedule = WeeklySchedule.Closed;
        if (profile.Schedule is not null)
        {
            schedule = WeeklySchedule.TryParse(profile.Schedule.Days, out var parsed) && parsed is not null
                ? parsed
                : throw new DomainException("El horario del negocio no es válido.");
        }

        Name = profile.Name!.Trim();
        Description = Empty(profile.Description);
        Phone = phone ?? string.Empty;
        WhatsApp = whatsApp;
        Schedule = schedule;
    }

    private static string? Empty(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
