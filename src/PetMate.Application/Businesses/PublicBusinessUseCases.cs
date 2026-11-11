using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using PetMate.Application.Caching;
using PetMate.Application.Mediator;
using PetMate.Application.Pagination;
using PetMate.Application.Validation;
using PetMate.Domain.Businesses;

namespace PetMate.Application.Businesses;

// Lecturas públicas de negocios (pm-116 a pm-121). No exigen cuenta (Visitante) y nunca incluyen la cédula profesional.

public sealed record TimeRangeDto(string Open, string Close);

/// <summary>Elemento del mapa: foto, nombre, tipo, si está abierto ahora, el horario de hoy y la distancia (pm-116#CA-02).</summary>
public sealed record NearbyBusinessItem(Guid Id, string Name, BusinessType Type, string? PhotoUrl, bool IsOpenNow, IReadOnlyList<TimeRangeDto> TodayHours, double DistanceMeters);

/// <summary>Perfil público (pm-119): descripción, foto, horario, ubicación, servicios y contacto. Un contacto ausente es nulo.</summary>
public sealed record PublicBusinessProfile(
    Guid Id,
    string Name,
    BusinessType Type,
    string? Description,
    string? PhotoUrl,
    bool IsOpenNow,
    IReadOnlyList<TimeRangeDto> TodayHours,
    IReadOnlyDictionary<string, IReadOnlyList<TimeRangeDto>> Schedule,
    double Latitude,
    double Longitude,
    string Address,
    string? Phone,
    string? WhatsApp,
    IReadOnlyList<PublicService> Services);

public sealed record PublicService(Guid Id, string Name, string? Description, decimal? Price);

/// <summary>Consulta por cercanía; el repositorio la resuelve con el índice 2dsphere.</summary>
public sealed record NearbyQuery(double Latitude, double Longitude, double RadiusMeters, IReadOnlyList<BusinessType>? Types, string? SearchKey, int Skip, int Take);

public sealed record NearbyHit(Business Business, double DistanceMeters);

public sealed record NearbyPage(IReadOnlyList<NearbyHit> Items, long Total);

public static class PublicBusinessLimits
{
    public const double DefaultRadiusKm = 5;
    public const double MaxRadiusKm = 50;
    public const int MinSearchLength = 2;
    public const int MaxSearchLength = 50;
    public const int MaxServicesInProfile = 100;

    /// <summary>
    /// TTL de la caché (pm-121#CA-01): 5 minutos. Los datos del mapa cambian poco y cada cambio invalida la caché; el TTL solo
    /// acota cuánto dura un dato viejo si la invalidación falla. «Abierto ahora» no se cachea: se calcula en cada respuesta.
    /// </summary>
    public static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(5);
}

/// <summary>Lectura de tipos y texto de búsqueda de la consulta pública.</summary>
public static class BusinessSearch
{
    public static bool TryParseTypes(string? text, out IReadOnlyList<BusinessType>? types)
    {
        types = null;
        if (string.IsNullOrWhiteSpace(text))
        {
            return true;
        }

        var list = new List<BusinessType>();
        foreach (var part in text.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
        {
            if (!Enum.TryParse<BusinessType>(part, ignoreCase: false, out var type) || !Enum.IsDefined(type) || Enum.GetName(type) != part)
            {
                return false;
            }

            list.Add(type);
        }

        types = list.Count == 0 ? null : list.Distinct().Order().ToArray();
        return true;
    }

    public static bool IsValidSearch(string? text)
        => string.IsNullOrWhiteSpace(text)
            || (text.Trim().Length is >= PublicBusinessLimits.MinSearchLength and <= PublicBusinessLimits.MaxSearchLength && !text.Any(char.IsControl));
}

/// <summary>
/// Caché de las lecturas públicas (pm-121). Guarda solo datos públicos con TTL y clave opaca. La invalidación usa una versión: cada
/// cambio de un negocio, sus servicios o su foto cambia la versión y las claves anteriores dejan de usarse. Un fallo de la caché
/// nunca afecta la respuesta (pm-121#CA-04).
/// </summary>
public sealed class PublicBusinessCache(ICacheStore store)
{
    private static readonly CacheKey VersionKey = CacheKey.Create("biz", "version");

    public async Task<T?> Get<T>(string area, string id, CancellationToken cancellationToken)
        where T : class
    {
        try
        {
            var raw = await store.Read(CacheKey.Create(area, await Scoped(id, cancellationToken).ConfigureAwait(false)), cancellationToken).ConfigureAwait(false);
            return raw is null ? null : JsonSerializer.Deserialize<T>(raw);
        }
        catch (Exception exception) when (exception is JsonException or ArgumentException or NotSupportedException)
        {
            return null;
        }
    }

    public async Task Set<T>(string area, string id, T value, CancellationToken cancellationToken)
    {
        try
        {
            await store.Write(CacheKey.Create(area, await Scoped(id, cancellationToken).ConfigureAwait(false)), JsonSerializer.Serialize(value), PublicBusinessLimits.CacheTtl, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception exception) when (exception is JsonException or ArgumentException or NotSupportedException)
        {
            // La caché es una optimización: se continúa sin ella.
        }
    }

    /// <summary>Invalida todas las respuestas públicas de negocios (cambia la versión).</summary>
    public async Task InvalidateAll(CancellationToken cancellationToken)
        => await store.Write(VersionKey, NewVersion(), CacheTtl1Hour, cancellationToken).ConfigureAwait(false);

    private static readonly TimeSpan CacheTtl1Hour = CacheTtl.Maximum;

    private static string NewVersion() => Guid.NewGuid().ToString("N")[..8];

    private async Task<string> Scoped(string id, CancellationToken cancellationToken)
    {
        var version = await store.Read(VersionKey, cancellationToken).ConfigureAwait(false);
        if (version is null)
        {
            version = NewVersion();
            await store.Write(VersionKey, version, CacheTtl1Hour, cancellationToken).ConfigureAwait(false);
        }

        return $"{version}-{id}";
    }

    public static string Hash(string text) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text)))[..32].ToLowerInvariant();
}

// ---------- Datos que se guardan en la caché (sin «abierto ahora») ----------

public sealed record CachedBusinessCore(Guid Id, string Name, BusinessType Type, string? PhotoUrl, Dictionary<string, List<TimeRangeDto>> Schedule, double DistanceMeters);

public sealed record CachedNearby(List<CachedBusinessCore> Items, long Total);

public sealed record CachedProfile(
    Guid Id,
    string Name,
    BusinessType Type,
    string? Description,
    string? PhotoUrl,
    Dictionary<string, List<TimeRangeDto>> Schedule,
    double Latitude,
    double Longitude,
    string Address,
    string? Phone,
    string? WhatsApp,
    List<PublicService> Services);

internal static class ScheduleView
{
    public static Dictionary<string, List<TimeRangeDto>> Dto(WeeklySchedule schedule)
        => schedule.Days.ToDictionary(
            d => WeeklySchedule.DayNames.First(n => n.Value == d.Key).Key,
            d => d.Value.Select(r => new TimeRangeDto(r.Open.ToString("HH:mm", System.Globalization.CultureInfo.InvariantCulture), r.Close.ToString("HH:mm", System.Globalization.CultureInfo.InvariantCulture))).ToList());

    /// <summary>Calcula «abierto ahora» y el horario de hoy en America/Mexico_City a partir del horario guardado.</summary>
    public static (bool IsOpen, IReadOnlyList<TimeRangeDto> Today) Now(Dictionary<string, List<TimeRangeDto>> schedule, TimeProvider clock)
    {
        var days = schedule.ToDictionary(d => d.Key, d => (IReadOnlyList<(string Open, string Close)>)d.Value.Select(r => (r.Open, r.Close)).ToArray());
        if (!WeeklySchedule.TryParse(days, out var parsed) || parsed is null)
        {
            return (false, []);
        }

        var instant = clock.GetUtcNow();
        var local = TimeZoneInfo.ConvertTime(instant, WeeklySchedule.MexicoCity);
        var key = WeeklySchedule.DayNames.First(n => n.Value == local.DayOfWeek).Key;
        return (parsed.IsOpenAt(instant, WeeklySchedule.MexicoCity), schedule.TryGetValue(key, out var today) ? today : []);
    }
}

// ---------- Consulta por cercanía (pm-116, pm-117, pm-118) ----------

public sealed record GetNearbyBusinessesQuery(double? Latitude, double? Longitude, double? RadiusKm, string? Types, string? Search, int? Page, int? PageSize)
    : IQuery<PagedResult<NearbyBusinessItem>>;

public sealed class GetNearbyBusinessesHandler(IBusinessRepository businesses, PublicBusinessCache cache, TimeProvider timeProvider)
    : IRequestHandler<GetNearbyBusinessesQuery, PagedResult<NearbyBusinessItem>>
{
    public async Task<PagedResult<NearbyBusinessItem>> Handle(GetNearbyBusinessesQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var page = PaginationConvention.ResolvePage(request.Page);
        var pageSize = PaginationConvention.ResolvePageSize(request.PageSize);
        // Las coordenadas se redondean a 3 decimales (unos 110 m): mejora el acierto de la caché y no guarda la ubicación exacta.
        var lat = Math.Round(request.Latitude!.Value, 3);
        var lng = Math.Round(request.Longitude!.Value, 3);
        var radiusKm = request.RadiusKm ?? PublicBusinessLimits.DefaultRadiusKm;
        // El validador ya comprobó los tipos; un valor inválido aquí significa «sin filtro».
        var types = BusinessSearch.TryParseTypes(request.Types, out var parsedTypes) ? parsedTypes : null;
        var searchKey = string.IsNullOrWhiteSpace(request.Search) ? null : Business.SearchKey(request.Search);

        var cacheId = PublicBusinessCache.Hash(string.Join('|', FormattableString.Invariant($"{lat}"), FormattableString.Invariant($"{lng}"), FormattableString.Invariant($"{radiusKm}"), types is null ? "*" : string.Join(',', types), searchKey ?? "-", page, pageSize));
        var cached = await cache.Get<CachedNearby>("biz-near", cacheId, cancellationToken).ConfigureAwait(false);
        if (cached is null)
        {
            var result = await businesses.FindNearby(new NearbyQuery(lat, lng, radiusKm * 1000, types, searchKey, (int)Math.Min(((long)page - 1) * pageSize, int.MaxValue), pageSize), cancellationToken).ConfigureAwait(false);
            cached = new CachedNearby(
                result.Items.Select(h => new CachedBusinessCore(h.Business.Id, h.Business.Name, h.Business.Type, null, ScheduleView.Dto(h.Business.Schedule), Math.Round(h.DistanceMeters))).ToList(),
                result.Total);
            await cache.Set("biz-near", cacheId, cached, cancellationToken).ConfigureAwait(false);
        }

        var items = cached.Items.Select(c =>
        {
            var (isOpen, today) = ScheduleView.Now(c.Schedule, timeProvider);
            return new NearbyBusinessItem(c.Id, c.Name, c.Type, c.PhotoUrl, isOpen, today, c.DistanceMeters);
        }).ToArray();
        return new PagedResult<NearbyBusinessItem>(items, page, pageSize, cached.Total);
    }
}

public sealed class GetNearbyBusinessesQueryValidator : IRequestValidator<GetNearbyBusinessesQuery>
{
    public Task<IReadOnlyList<ValidationFailure>> Validate(GetNearbyBusinessesQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var failures = new List<ValidationFailure>();
        foreach (var field in Business.InvalidLocationFields(request.Latitude, request.Longitude))
        {
            failures.Add(new ValidationFailure(field, field == "latitude" ? "La latitud es obligatoria y debe estar entre -90 y 90." : "La longitud es obligatoria y debe estar entre -180 y 180."));
        }

        if (request.RadiusKm is { } radius && (double.IsNaN(radius) || radius <= 0 || radius > PublicBusinessLimits.MaxRadiusKm))
        {
            failures.Add(new ValidationFailure("radiusKm", $"El radio debe ser mayor que 0 y de hasta {PublicBusinessLimits.MaxRadiusKm:0} km (por defecto {PublicBusinessLimits.DefaultRadiusKm:0})."));
        }

        if (!BusinessSearch.TryParseTypes(request.Types, out _))
        {
            failures.Add(new ValidationFailure("types", "Los tipos deben ser Veterinary, Store o Grooming separados por comas."));
        }

        if (!BusinessSearch.IsValidSearch(request.Search))
        {
            failures.Add(new ValidationFailure("search", $"El texto de búsqueda debe tener de {PublicBusinessLimits.MinSearchLength} a {PublicBusinessLimits.MaxSearchLength} caracteres."));
        }

        failures.AddRange(PaginationConvention.Check(request.Page, request.PageSize));
        return Task.FromResult<IReadOnlyList<ValidationFailure>>(failures);
    }
}

// ---------- Perfil público (pm-119) ----------

public sealed record GetPublicBusinessQuery(Guid BusinessId) : IQuery<PublicBusinessProfile?>;

public sealed class GetPublicBusinessHandler(IBusinessRepository businesses, IBusinessServiceRepository services, PublicBusinessCache cache, TimeProvider timeProvider)
    : IRequestHandler<GetPublicBusinessQuery, PublicBusinessProfile?>
{
    public async Task<PublicBusinessProfile?> Handle(GetPublicBusinessQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var id = request.BusinessId.ToString("N");
        var cached = await cache.Get<CachedProfile>("biz-prof", id, cancellationToken).ConfigureAwait(false);
        if (cached is null)
        {
            var business = await businesses.FindById(request.BusinessId, cancellationToken).ConfigureAwait(false);
            if (business is null)
            {
                return null;
            }

            var list = await services.ListByBusiness(business.Id, 0, PublicBusinessLimits.MaxServicesInProfile, cancellationToken).ConfigureAwait(false);
            // La cédula profesional nunca forma parte del perfil público (pm-119#CA-02).
            cached = new CachedProfile(
                business.Id, business.Name, business.Type, business.Description, null, ScheduleView.Dto(business.Schedule),
                business.Latitude, business.Longitude, business.Address, business.Phone, business.WhatsApp,
                list.Select(s => new PublicService(s.Id, s.Name, s.Description, s.Price)).ToList());
            await cache.Set("biz-prof", id, cached, cancellationToken).ConfigureAwait(false);
        }

        var (isOpen, today) = ScheduleView.Now(cached.Schedule, timeProvider);
        return new PublicBusinessProfile(
            cached.Id, cached.Name, cached.Type, cached.Description, cached.PhotoUrl, isOpen, today,
            cached.Schedule.ToDictionary(d => d.Key, d => (IReadOnlyList<TimeRangeDto>)d.Value),
            cached.Latitude, cached.Longitude, cached.Address, cached.Phone, cached.WhatsApp, cached.Services);
    }
}
