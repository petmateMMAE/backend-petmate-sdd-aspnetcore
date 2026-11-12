namespace PetMate.Application.Images;

/// <summary>
/// Contenedores de imágenes. Las fotos de mascotas son privadas (pm-079#CA-01) y las de negocios públicas de solo
/// lectura (pm-111#CA-01): nunca comparten contenedor.
/// </summary>
public enum ImageContainer
{
    /// <summary>Privado: solo se lee con una URL temporal firmada (pm-081).</summary>
    PetPhotos = 1,

    /// <summary>Público de solo lectura: la foto del negocio se ve en el mapa (pm-111).</summary>
    BusinessPhotos = 2,
}

/// <summary>Imagen ya guardada. <see cref="PublicUrl"/> solo existe en los contenedores públicos.</summary>
public sealed record StoredImage(string ObjectKey, Uri? PublicUrl);

/// <summary>URL temporal de lectura y el instante en que deja de servir.</summary>
public sealed record TemporaryImageUrl(Uri Url, DateTimeOffset ExpiresAtUtc);

/// <summary>
/// Puerto de almacenamiento de imágenes (pm-079). La aplicación no sabe de Azure: guarda, borra y pide una URL
/// temporal. Ninguna implementación escribe en el disco del contenedor de la API (pm-079#CA-02).
/// </summary>
public interface IImageStorage
{
    /// <summary>Falso cuando no hay almacenamiento configurado; los endpoints de imágenes responden 503.</summary>
    bool IsEnabled { get; }

    /// <summary>Guarda o reemplaza el objeto y devuelve su clave; la URL pública solo llega en contenedores públicos.</summary>
    Task<StoredImage> Save(ImageContainer container, string objectKey, string contentType, Stream content, CancellationToken cancellationToken);

    /// <summary>Borra el objeto. Es idempotente: que no exista no es un error.</summary>
    Task Delete(ImageContainer container, string objectKey, CancellationToken cancellationToken);

    /// <summary>URL temporal de solo lectura (pm-081). Devuelve <c>null</c> si el objeto ya no existe.</summary>
    Task<TemporaryImageUrl?> CreateTemporaryUrl(ImageContainer container, string objectKey, TimeSpan lifetime, CancellationToken cancellationToken);
}

/// <summary>Motivo por el que se rechaza una imagen (pm-080#CA-01, pm-080#CA-02).</summary>
public enum ImageRejection
{
    /// <summary>La imagen es válida.</summary>
    None = 0,

    /// <summary>No llegó archivo o venía vacío.</summary>
    Missing = 1,

    /// <summary>Pesa más de lo permitido.</summary>
    TooLarge = 2,

    /// <summary>El contenido no es una imagen de un tipo permitido.</summary>
    UnsupportedType = 3,
}

/// <summary>Tipos permitidos y límites de tamaño de las imágenes (pm-080#CA-01, pm-111#CA-02).</summary>
public static class ImageRules
{
    public const string Jpeg = "image/jpeg";
    public const string Png = "image/png";
    public const string WebP = "image/webp";

    /// <summary>Foto de una mascota: 2 MB.</summary>
    public const long MaxPetPhotoBytes = 2 * 1024 * 1024;

    /// <summary>Foto de un negocio: 1 MB (pm-111#CA-02).</summary>
    public const long MaxBusinessPhotoBytes = 1024 * 1024;

    /// <summary>Bytes que hay que leer del inicio para reconocer cualquiera de los tipos permitidos.</summary>
    public const int HeaderLength = 12;

    /// <summary>Tipos permitidos, para documentar el contrato y los mensajes de error.</summary>
    public static IReadOnlyList<string> AllowedContentTypes { get; } = [Jpeg, Png, WebP];

    /// <summary>Tamaño máximo del contenedor indicado.</summary>
    public static long MaxBytes(ImageContainer container)
        => container == ImageContainer.BusinessPhotos ? MaxBusinessPhotoBytes : MaxPetPhotoBytes;

    /// <summary>
    /// Reconoce el tipo real por los bytes mágicos del archivo (pm-080#CA-02). El nombre y el <c>Content-Type</c> que
    /// manda el cliente se ignoran: un archivo con extensión mentirosa se rechaza. Devuelve <c>null</c> si no es
    /// ninguno de los tipos permitidos.
    /// </summary>
    public static string? Detect(ReadOnlySpan<byte> header)
    {
        if (header.Length >= 3 && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF)
        {
            return Jpeg;
        }

        if (header.Length >= 8 && header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47
            && header[4] == 0x0D && header[5] == 0x0A && header[6] == 0x1A && header[7] == 0x0A)
        {
            return Png;
        }

        // WebP: "RIFF" + 4 bytes de tamaño + "WEBP".
        if (header.Length >= 12
            && header[0] == 0x52 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x46
            && header[8] == 0x57 && header[9] == 0x45 && header[10] == 0x42 && header[11] == 0x50)
        {
            return WebP;
        }

        return null;
    }

    /// <summary>Extensión que decide el servidor a partir del tipo detectado; nunca la del nombre que subió el cliente.</summary>
    public static string ExtensionFor(string contentType) => contentType switch
    {
        Png => ".png",
        WebP => ".webp",
        _ => ".jpg",
    };

    /// <summary>Revisa tamaño y contenido. <paramref name="header"/> son los primeros <see cref="HeaderLength"/> bytes.</summary>
    public static ImageRejection Check(ImageContainer container, long length, ReadOnlySpan<byte> header, out string? contentType)
    {
        contentType = null;
        if (length <= 0)
        {
            return ImageRejection.Missing;
        }

        if (length > MaxBytes(container))
        {
            return ImageRejection.TooLarge;
        }

        contentType = Detect(header);
        return contentType is null ? ImageRejection.UnsupportedType : ImageRejection.None;
    }
}
