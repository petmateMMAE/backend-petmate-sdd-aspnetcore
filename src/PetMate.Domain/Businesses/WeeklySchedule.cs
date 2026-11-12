using System.Globalization;

namespace PetMate.Domain.Businesses;

/// <summary>Franja de apertura de un día: cierre posterior a la apertura (pm-106#CA-02).</summary>
public sealed record TimeRange(TimeOnly Open, TimeOnly Close);

/// <summary>
/// Horario semanal estructurado del negocio (pm-106): por día, cero o más franjas que no se solapan. Un día sin franjas es un día
/// cerrado. Las horas son locales del negocio; «abierto ahora» se calcula en America/Mexico_City.
/// </summary>
public sealed class WeeklySchedule
{
    public const int MaxRangesPerDay = 6;

    /// <summary>Nombres de día aceptados en el contrato JSON.</summary>
    public static readonly IReadOnlyDictionary<string, DayOfWeek> DayNames = new Dictionary<string, DayOfWeek>(StringComparer.Ordinal)
    {
        ["monday"] = DayOfWeek.Monday,
        ["tuesday"] = DayOfWeek.Tuesday,
        ["wednesday"] = DayOfWeek.Wednesday,
        ["thursday"] = DayOfWeek.Thursday,
        ["friday"] = DayOfWeek.Friday,
        ["saturday"] = DayOfWeek.Saturday,
        ["sunday"] = DayOfWeek.Sunday,
    };

    private static readonly Lazy<TimeZoneInfo> MexicoCityZone = new(FindMexicoCity);

    private readonly Dictionary<DayOfWeek, IReadOnlyList<TimeRange>> days;

    private WeeklySchedule(Dictionary<DayOfWeek, IReadOnlyList<TimeRange>> days) => this.days = days;

    /// <summary>Horario sin ninguna franja: todos los días cerrado.</summary>
    public static WeeklySchedule Closed { get; } = new([]);

    /// <summary>Zona horaria del negocio. Si el sistema no tiene la base de zonas, usa UTC-6 fijo (México no usa horario de verano).</summary>
    public static TimeZoneInfo MexicoCity => MexicoCityZone.Value;

    public IReadOnlyDictionary<DayOfWeek, IReadOnlyList<TimeRange>> Days => days;

    public IReadOnlyList<TimeRange> RangesFor(DayOfWeek day) => days.TryGetValue(day, out var ranges) ? ranges : [];

    /// <summary>Crea un horario validando las franjas de cada día; <c>false</c> si alguna es inválida o se solapa.</summary>
    public static bool TryCreate(IReadOnlyDictionary<DayOfWeek, IReadOnlyList<TimeRange>>? input, out WeeklySchedule? schedule)
    {
        schedule = null;
        var result = new Dictionary<DayOfWeek, IReadOnlyList<TimeRange>>();
        foreach (var (day, ranges) in input ?? new Dictionary<DayOfWeek, IReadOnlyList<TimeRange>>())
        {
            if (!Enum.IsDefined(day) || ranges is null || ranges.Count > MaxRangesPerDay)
            {
                return false;
            }

            var ordered = ranges.OrderBy(r => r.Open).ToArray();
            for (var i = 0; i < ordered.Length; i++)
            {
                if (ordered[i].Close <= ordered[i].Open || (i > 0 && ordered[i].Open < ordered[i - 1].Close))
                {
                    return false;
                }
            }

            if (ordered.Length > 0)
            {
                result[day] = ordered;
            }
        }

        schedule = new WeeklySchedule(result);
        return true;
    }

    /// <summary>Crea un horario desde texto: días <c>monday</c>…<c>sunday</c> y horas <c>HH:mm</c>. Un día o una hora desconocidos lo invalidan.</summary>
    public static bool TryParse(IReadOnlyDictionary<string, IReadOnlyList<(string Open, string Close)>>? input, out WeeklySchedule? schedule)
    {
        schedule = null;
        var parsed = new Dictionary<DayOfWeek, IReadOnlyList<TimeRange>>();
        foreach (var (name, ranges) in input ?? new Dictionary<string, IReadOnlyList<(string, string)>>())
        {
            if (!DayNames.TryGetValue(name, out var day) || ranges is null)
            {
                return false;
            }

            var list = new List<TimeRange>();
            foreach (var (open, close) in ranges)
            {
                if (!TimeOnly.TryParseExact(open, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var o)
                    || !TimeOnly.TryParseExact(close, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var c))
                {
                    return false;
                }

                list.Add(new TimeRange(o, c));
            }

            parsed[day] = list;
        }

        return TryCreate(parsed, out schedule);
    }

    /// <summary>Estado «abierto ahora» (pm-106#CA-03): la hora local de la zona cae dentro de alguna franja del día. El cierre no cuenta como abierto.</summary>
    public bool IsOpenAt(DateTimeOffset instant, TimeZoneInfo zone)
    {
        ArgumentNullException.ThrowIfNull(zone);
        var local = TimeZoneInfo.ConvertTime(instant, zone);
        var time = TimeOnly.FromTimeSpan(local.TimeOfDay);
        return RangesFor(local.DayOfWeek).Any(r => time >= r.Open && time < r.Close);
    }

    public bool IsOpenNow(TimeProvider clock)
    {
        ArgumentNullException.ThrowIfNull(clock);
        return IsOpenAt(clock.GetUtcNow(), MexicoCity);
    }

    private static TimeZoneInfo FindMexicoCity()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("America/Mexico_City");
        }
        catch (Exception exception) when (exception is TimeZoneNotFoundException or InvalidTimeZoneException)
        {
            return TimeZoneInfo.CreateCustomTimeZone("America/Mexico_City", TimeSpan.FromHours(-6), "America/Mexico_City", "America/Mexico_City");
        }
    }
}
