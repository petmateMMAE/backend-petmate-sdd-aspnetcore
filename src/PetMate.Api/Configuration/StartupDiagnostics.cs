namespace PetMate.Api.Configuration;

/// <summary>Advertencias de arranque. No incluyen valores de configuración.</summary>
public static partial class StartupDiagnostics
{
    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "JWT_SIGNING_KEY es una clave de plantilla: use una clave aleatoria de al menos 256 bits (solo se tolera en Development).")]
    public static partial void PlaceholderSigningKey(ILogger logger);
}
