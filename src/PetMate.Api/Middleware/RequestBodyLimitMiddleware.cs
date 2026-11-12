using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using PetMate.Api.Contracts.Responses;

namespace PetMate.Api.Middleware;

/// <summary>
/// Limita el tamaño del cuerpo de toda petición (pm-045 CA-04): todos los cuerpos de la API son JSON pequeños, así que 16 KB
/// bastan. Rechaza con 413 antes de leer cuando <c>Content-Length</c> ya lo excede y, para cuerpos sin longitud conocida,
/// fija el límite en Kestrel, que corta la lectura y responde 413 (lo traduce <see cref="ExceptionHandlingMiddleware"/>).
/// </summary>
public sealed class RequestBodyLimitMiddleware(RequestDelegate next)
{
    public const long MaxRequestBodyBytes = 16 * 1024;

    /// <summary>
    /// Las rutas de carga de imágenes (pm-080) son la excepción: llevan un archivo, no JSON. El tope cubre la foto
    /// más grande permitida (2 MB) más el envoltorio multipart; el tamaño real de la imagen lo revisa el endpoint.
    /// </summary>
    public const long MaxImageBodyBytes = (2 * 1024 * 1024) + (64 * 1024);

    private const string UploadPhotoSegment = "/upload-photo";

    public async Task InvokeAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var limit = IsImageUpload(context.Request.Path) ? MaxImageBodyBytes : MaxRequestBodyBytes;
        if (context.Request.ContentLength > limit)
        {
            await ApiResponseFactory.WriteAsync<object?>(
                context,
                StatusCodes.Status413PayloadTooLarge,
                ApiResponseMessages.ForStatusCode(StatusCodes.Status413PayloadTooLarge),
                null);
            return;
        }

        var feature = context.Features.Get<IHttpMaxRequestBodySizeFeature>();
        if (feature is { IsReadOnly: false })
        {
            feature.MaxRequestBodySize = limit;
        }

        await next(context);
    }

    private static bool IsImageUpload(PathString path)
        => path.HasValue && path.Value.Contains(UploadPhotoSegment, StringComparison.OrdinalIgnoreCase);
}
