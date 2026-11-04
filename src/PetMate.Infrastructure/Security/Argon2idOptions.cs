using Microsoft.Extensions.Configuration;

namespace PetMate.Infrastructure.Security;

/// <summary>
/// Parámetros de Argon2id. Valores por defecto: t=3, m=64 MiB (65 536 KiB), p=1 (libsodium fija el paralelismo en 1).
/// Se rechaza cualquier valor por debajo del mínimo OWASP (m=19 MiB, t=2): no se pueden bajar para acelerar pruebas.
/// </summary>
public sealed record Argon2idOptions
{
    public const string MemoryKey = "ARGON2_MEMORY_KIB";
    public const string IterationsKey = "ARGON2_ITERATIONS";
    public const int MinMemoryKiB = 19_456;
    public const int MaxMemoryKiB = 262_144;
    public const int MinIterations = 2;
    public const int MaxIterations = 10;
    public const string ConcurrencyKey = "ARGON2_MAX_CONCURRENCY";
    public const int MaxConcurrencyLimit = 64;

    public static readonly Argon2idOptions Default = new(65_536, 3);

    public Argon2idOptions(int memoryKiB, int iterations)
    {
        if (memoryKiB is < MinMemoryKiB or > MaxMemoryKiB || iterations is < MinIterations or > MaxIterations)
        {
            throw new ArgumentOutOfRangeException(
                nameof(memoryKiB),
                $"Argon2id requiere memoria entre {MinMemoryKiB} y {MaxMemoryKiB} KiB e iteraciones entre {MinIterations} y {MaxIterations}.");
        }

        MemoryKiB = memoryKiB;
        Iterations = iterations;
    }

    public int MemoryKiB { get; }

    public int Iterations { get; }

    /// <summary>Hashes simultáneos permitidos (acota la memoria: ≈ concurrencia × memoria por hash). Por defecto, núcleos con mínimo 2.</summary>
    public int MaxConcurrency { get; init; } = Math.Max(2, Environment.ProcessorCount);

    /// <summary>Lee los valores opcionales del entorno. Los errores nombran las claves, nunca los valores.</summary>
    public static Argon2idOptions Parse(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var errors = new List<string>();
        var memory = ReadInt(configuration, MemoryKey, Default.MemoryKiB, MinMemoryKiB, MaxMemoryKiB, errors);
        var iterations = ReadInt(configuration, IterationsKey, Default.Iterations, MinIterations, MaxIterations, errors);
        var concurrency = ReadInt(configuration, ConcurrencyKey, Math.Max(2, Environment.ProcessorCount), 1, MaxConcurrencyLimit, errors);
        if (errors.Count > 0)
        {
            throw new InvalidOperationException(
                "Configuración de Argon2id inválida: " + string.Join(" ", errors) + " No se muestran valores por seguridad.");
        }

        return new Argon2idOptions(memory, iterations)
        {
            MaxConcurrency = concurrency,
        };
    }

    private static int ReadInt(IConfiguration configuration, string key, int fallback, int min, int max, List<string> errors)
    {
        var raw = configuration[key];
        if (string.IsNullOrWhiteSpace(raw))
        {
            return fallback;
        }

        if (!int.TryParse(raw, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var value)
            || value < min || value > max)
        {
            errors.Add($"{key}: debe ser un entero entre {min} y {max}.");
            return fallback;
        }

        return value;
    }
}
