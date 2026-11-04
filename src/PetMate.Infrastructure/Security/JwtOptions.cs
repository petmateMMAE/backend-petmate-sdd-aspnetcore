using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace PetMate.Infrastructure.Security;

/// <summary>
/// Configuración de JWT (HS256). Variables: <c>JWT_ISSUER</c>, <c>JWT_AUDIENCE</c>, <c>JWT_SIGNING_KEY</c> y
/// <c>JWT_ACCESS_TOKEN_MINUTES</c> (opcional, 1–60, por defecto 15). Los errores nombran claves, nunca valores.
/// </summary>
public sealed class JwtOptions
{
    public const string IssuerKey = "JWT_ISSUER";
    public const string AudienceKey = "JWT_AUDIENCE";
    public const string SigningKeyKey = "JWT_SIGNING_KEY";
    public const string LifetimeKey = "JWT_ACCESS_TOKEN_MINUTES";
    public const int MinSigningKeyBytes = 32;
    public const int DefaultLifetimeMinutes = 15;
    public const int MaxLifetimeMinutes = 60;
    public const string Algorithm = SecurityAlgorithms.HmacSha256;

    private static readonly string[] PlaceholderMarkers = ["placeholder", "ficticio", "no_usar", "changeme", "cambiar"];

    private readonly byte[] signingKey;

    public JwtOptions(string issuer, string audience, byte[] signingKey, TimeSpan accessTokenLifetime)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(issuer);
        ArgumentException.ThrowIfNullOrWhiteSpace(audience);
        ArgumentNullException.ThrowIfNull(signingKey);
        if (signingKey.Length < MinSigningKeyBytes)
        {
            throw new ArgumentException("La clave de firma debe tener al menos 256 bits.", nameof(signingKey));
        }

        if (accessTokenLifetime <= TimeSpan.Zero || accessTokenLifetime > TimeSpan.FromMinutes(MaxLifetimeMinutes))
        {
            throw new ArgumentOutOfRangeException(nameof(accessTokenLifetime));
        }

        Issuer = issuer;
        Audience = audience;
        this.signingKey = [.. signingKey];
        AccessTokenLifetime = accessTokenLifetime;
    }

    public string Issuer { get; }

    public string Audience { get; }

    public TimeSpan AccessTokenLifetime { get; }

    /// <summary>Verdadero si, en Development, la clave configurada es una plantilla conocida (la API lo advierte al arrancar).</summary>
    public bool UsesPlaceholderKey { get; init; }

    /// <summary>Clave simétrica para firmar y validar. No se expone el material ni se imprime.</summary>
    public SymmetricSecurityKey CreateKey() => new(signingKey);

    public override string ToString() => "JwtOptions";

    /// <summary>
    /// Lee y valida la configuración. Fuera de Development se rechaza una clave que contenga marcas de plantilla;
    /// en Development se admite (solo local) y la API lo advierte al arrancar.
    /// </summary>
    public static JwtOptions Parse(IConfiguration configuration, bool isDevelopment)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var errors = new List<string>();
        var issuer = configuration[IssuerKey];
        var audience = configuration[AudienceKey];
        var key = configuration[SigningKeyKey];

        if (string.IsNullOrWhiteSpace(issuer))
        {
            errors.Add($"{IssuerKey}: obligatoria.");
        }

        if (string.IsNullOrWhiteSpace(audience))
        {
            errors.Add($"{AudienceKey}: obligatoria.");
        }

        if (string.IsNullOrEmpty(key) || Encoding.UTF8.GetByteCount(key) < MinSigningKeyBytes)
        {
            errors.Add($"{SigningKeyKey}: debe tener al menos {MinSigningKeyBytes} bytes (256 bits) de entropía.");
        }
        else if (!isDevelopment && HasPlaceholderMarker(key))
        {
            errors.Add($"{SigningKeyKey}: no puede ser una clave de plantilla fuera de Development.");
        }

        var minutes = DefaultLifetimeMinutes;
        var rawLifetime = configuration[LifetimeKey];
        if (!string.IsNullOrWhiteSpace(rawLifetime)
            && (!int.TryParse(rawLifetime, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out minutes)
                || minutes is < 1 or > MaxLifetimeMinutes))
        {
            errors.Add($"{LifetimeKey}: debe ser un entero entre 1 y {MaxLifetimeMinutes}.");
        }

        if (errors.Count > 0)
        {
            throw new InvalidOperationException("Configuración JWT inválida: " + string.Join(" ", errors) + " No se muestran valores por seguridad.");
        }

        return new JwtOptions(issuer!, audience!, Encoding.UTF8.GetBytes(key!), TimeSpan.FromMinutes(minutes))
        {
            UsesPlaceholderKey = HasPlaceholderMarker(key!),
        };
    }

    /// <summary>Indica si la clave configurada es una plantilla conocida (para advertir en Development).</summary>
    public static bool HasPlaceholderMarker(string key)
        => PlaceholderMarkers.Any(marker => key.Contains(marker, StringComparison.OrdinalIgnoreCase));
}
