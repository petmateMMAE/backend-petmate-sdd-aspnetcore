using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using PetMate.Application.Images;

namespace PetMate.Api.Contracts.Images;

/// <summary>Imagen aceptada: contenido en memoria y el tipo que se dedujo de sus bytes.</summary>
public sealed record AcceptedPhoto(string ContentType, MemoryStream Content) : IDisposable
{
    public void Dispose() => Content.Dispose();
}

/// <summary>
/// Lee la imagen del cuerpo <c>multipart/form-data</c> (pm-080). Se usa <see cref="MultipartReader"/> en vez del
/// enlace de modelos con <c>IFormFile</c> a propósito: ASP.NET escribiría los archivos grandes en un temporal del
/// disco y pm-079#CA-02 exige que ninguna imagen toque el sistema de archivos del contenedor. Como el tope es de
/// 2 MB, el archivo cabe en memoria y de ahí sale directo al Blob.
/// </summary>
public static class PhotoUploadReader
{
    /// <summary>Nombre del campo del formulario; cualquier otro se ignora.</summary>
    public const string FieldName = "file";

    private const int CopyBufferSize = 64 * 1024;

    /// <summary>
    /// Devuelve la imagen aceptada o el motivo del rechazo. Un cuerpo mal formado es un rechazo, no un error del
    /// servidor: el endpoint responde 400.
    /// </summary>
    public static async Task<(ImageRejection Rejection, AcceptedPhoto? Photo)> Read(
        HttpRequest request,
        ImageContainer container,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!TryGetBoundary(request.ContentType, out var boundary))
        {
            return (ImageRejection.Missing, null);
        }

        var maxBytes = ImageRules.MaxBytes(container);
        var reader = new MultipartReader(boundary, request.Body);

        for (var section = await Next(reader, cancellationToken).ConfigureAwait(false);
             section is not null;
             section = await Next(reader, cancellationToken).ConfigureAwait(false))
        {
            if (!IsFileField(section))
            {
                continue;
            }

            var buffer = new MemoryStream();
            var keep = false;
            try
            {
                var rejection = await CopyWithLimit(section.Body, buffer, maxBytes, cancellationToken).ConfigureAwait(false);
                if (rejection != ImageRejection.None)
                {
                    return (rejection, null);
                }

                var header = buffer.GetBuffer().AsSpan(0, (int)Math.Min(buffer.Length, ImageRules.HeaderLength));
                var check = ImageRules.Check(container, buffer.Length, header, out var contentType);
                if (check != ImageRejection.None || contentType is null)
                {
                    return (check == ImageRejection.None ? ImageRejection.UnsupportedType : check, null);
                }

                buffer.Position = 0;
                keep = true;
                return (ImageRejection.None, new AcceptedPhoto(contentType, buffer));
            }
            finally
            {
                if (!keep)
                {
                    await buffer.DisposeAsync().ConfigureAwait(false);
                }
            }
        }

        return (ImageRejection.Missing, null);
    }

    /// <summary>Copia cortando en cuanto se pasa del límite, para no cargar en memoria más de lo permitido.</summary>
    private static async Task<ImageRejection> CopyWithLimit(Stream source, Stream destination, long maxBytes, CancellationToken cancellationToken)
    {
        var buffer = new byte[CopyBufferSize];
        long total = 0;
        int read;
        while ((read = await source.ReadAsync(buffer, cancellationToken).ConfigureAwait(false)) > 0)
        {
            total += read;
            if (total > maxBytes)
            {
                return ImageRejection.TooLarge;
            }

            await destination.WriteAsync(buffer.AsMemory(0, read), cancellationToken).ConfigureAwait(false);
        }

        return total == 0 ? ImageRejection.Missing : ImageRejection.None;
    }

    /// <summary>
    /// En la cabecera el nombre y el archivo llegan entrecomillados (<c>name="file"</c>), así que hay que quitar
    /// las comillas antes de comparar.
    /// </summary>
    private static bool IsFileField(MultipartSection section)
    {
        if (!ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out var disposition)
            || !disposition.DispositionType.Equals("form-data", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var hasFile = !string.IsNullOrEmpty(HeaderUtilities.RemoveQuotes(disposition.FileName).Value)
            || !string.IsNullOrEmpty(HeaderUtilities.RemoveQuotes(disposition.FileNameStar).Value);
        if (!hasFile)
        {
            return false;
        }

        var name = HeaderUtilities.RemoveQuotes(disposition.Name).Value;
        return string.IsNullOrEmpty(name) || string.Equals(name, FieldName, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Lee la siguiente parte. Un cuerpo multipart mal formado o cortado es culpa del cliente: se trata como «no
    /// llegó archivo» (400) en vez de dejar escapar una excepción que acabaría en 500.
    /// </summary>
    private static async Task<MultipartSection?> Next(MultipartReader reader, CancellationToken cancellationToken)
    {
        try
        {
            return await reader.ReadNextSectionAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (IOException)
        {
            return null;
        }
        catch (InvalidDataException)
        {
            return null;
        }
    }

    private static bool TryGetBoundary(string? contentType, out string boundary)
    {
        boundary = string.Empty;
        if (string.IsNullOrEmpty(contentType)
            || !MediaTypeHeaderValue.TryParse(contentType, out var media)
            || !media.MediaType.HasValue
            || !media.MediaType.Value.StartsWith("multipart/", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var value = HeaderUtilities.RemoveQuotes(media.Boundary).Value;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        boundary = value;
        return true;
    }
}
