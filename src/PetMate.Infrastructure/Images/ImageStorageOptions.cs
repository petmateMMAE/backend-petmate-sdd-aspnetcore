using System.Globalization;

namespace PetMate.Infrastructure.Images;

/// <summary>
/// Configuración del almacenamiento de imágenes (pm-079). Dos formas de autenticarse, en este orden:
/// identidad administrada con <see cref="AccountKey"/> (como corre en Azure) o cadena de conexión con
/// <see cref="ConnectionKey"/> (Azurite en local). Sin ninguna de las dos no hay almacenamiento y la API
/// arranca igual, con el adaptador deshabilitado.
/// </summary>
public sealed record ImageStorageOptions(string? AccountName, string? ConnectionString, string PetContainer, string BusinessContainer)
{
    public const string AccountKey = "IMAGES_STORAGE_ACCOUNT";
    public const string ConnectionKey = "IMAGES_STORAGE_CONNECTION";
    public const string PetContainerKey = "IMAGES_PET_CONTAINER";
    public const string BusinessContainerKey = "IMAGES_BUSINESS_CONTAINER";

    public const string DefaultPetContainer = "pet-photos";
    public const string DefaultBusinessContainer = "business-photos";

    /// <summary>Punto de entrada del servicio Blob cuando se usa identidad administrada.</summary>
    public Uri ServiceUri => new(string.Create(CultureInfo.InvariantCulture, $"https://{AccountName}.blob.core.windows.net"));

    /// <summary>
    /// Lee la configuración. Devuelve <c>null</c> cuando no hay cuenta ni cadena de conexión. Lanza si lo que hay
    /// es inválido: vale más no arrancar que subir fotos a un sitio equivocado. Los errores nombran la clave, nunca el valor.
    /// </summary>
    public static ImageStorageOptions? TryRead(Func<string, string?> read)
    {
        ArgumentNullException.ThrowIfNull(read);

        var account = Clean(read(AccountKey));
        var connection = Clean(read(ConnectionKey));
        if (account is null && connection is null)
        {
            return null;
        }

        var errors = new List<string>();
        if (account is not null && !IsValidAccountName(account))
        {
            errors.Add($"{AccountKey}: nombre de cuenta de almacenamiento no válido.");
        }

        var petContainer = Clean(read(PetContainerKey)) ?? DefaultPetContainer;
        var businessContainer = Clean(read(BusinessContainerKey)) ?? DefaultBusinessContainer;
        if (!IsValidContainerName(petContainer))
        {
            errors.Add($"{PetContainerKey}: nombre de contenedor no válido.");
        }

        if (!IsValidContainerName(businessContainer))
        {
            errors.Add($"{BusinessContainerKey}: nombre de contenedor no válido.");
        }

        // Mezclarlos publicaría las fotos de las mascotas (pm-079#CA-01, pm-111#CA-01).
        if (string.Equals(petContainer, businessContainer, StringComparison.OrdinalIgnoreCase))
        {
            errors.Add($"{PetContainerKey}/{BusinessContainerKey}: el contenedor privado y el público deben ser distintos.");
        }

        if (errors.Count > 0)
        {
            throw new InvalidOperationException(
                "Configuración de almacenamiento de imágenes inválida: " + string.Join(" ", errors)
                + " No se muestran valores por seguridad.");
        }

        return new ImageStorageOptions(account, connection, petContainer, businessContainer);
    }

    /// <summary>Reglas de Azure: 3 a 24 caracteres, minúsculas y dígitos.</summary>
    private static bool IsValidAccountName(string value)
        => value.Length is >= 3 and <= 24 && value.All(c => char.IsAsciiLetterLower(c) || char.IsAsciiDigit(c));

    /// <summary>Reglas de Azure: 3 a 63 caracteres, minúsculas, dígitos y guiones; ni empieza ni termina en guion.</summary>
    private static bool IsValidContainerName(string value)
        => value.Length is >= 3 and <= 63
            && value[0] != '-' && value[^1] != '-'
            && !value.Contains("--", StringComparison.Ordinal)
            && value.All(c => char.IsAsciiLetterLower(c) || char.IsAsciiDigit(c) || c == '-');

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
