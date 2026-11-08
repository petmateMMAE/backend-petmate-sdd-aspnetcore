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

    public async Task InvokeAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (context.Request.ContentLength > MaxRequestBodyBytes)
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
            feature.MaxRequestBodySize = MaxRequestBodyBytes;
        }

        await next(context);
    }
}
