namespace PetMate.Api.Configuration;

/// <summary>
/// Valida la configuración crítica necesaria para arrancar la API (T004).
/// Solo comprueba presencia; el formato y la conectividad se validan en tareas posteriores (T006, T010).
/// </summary>
public static class StartupConfigurationValidator
{
    private static readonly string[] RequiredKeys =
    [
        "MONGODB_URI",
        "MONGODB_DATABASE",
        "REDIS_AUTH_CONNECTION",
        "REDIS_CACHE_CONNECTION",
        "JWT_ISSUER",
        "JWT_AUDIENCE",
        "JWT_SIGNING_KEY",
    ];

    /// <summary>
    /// Devuelve los nombres de las variables requeridas ausentes o vacías, en el orden
    /// declarado. Nunca devuelve ni expone valores.
    /// </summary>
    public static IReadOnlyList<string> GetMissingKeys(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var missing = new List<string>();

        foreach (var key in RequiredKeys)
        {
            if (string.IsNullOrWhiteSpace(configuration[key]))
            {
                missing.Add(key);
            }
        }

        return missing;
    }

    /// <summary>
    /// Lanza <see cref="InvalidOperationException"/> si falta configuración crítica.
    /// El mensaje enumera únicamente los nombres de las variables faltantes:
    /// no incluye valores, cadenas de conexión ni contenido de <c>.env</c>.
    /// </summary>
    public static void EnsureValid(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var missing = GetMissingKeys(configuration);

        if (missing.Count == 0)
        {
            return;
        }

        throw new InvalidOperationException(
            "Configuración crítica incompleta. Faltan las siguientes variables de entorno: "
            + string.Join(", ", missing)
            + ". Defínalas en el entorno de ejecución o en el archivo .env local (ver .env.example). "
            + "No se muestran valores por seguridad.");
    }
}
