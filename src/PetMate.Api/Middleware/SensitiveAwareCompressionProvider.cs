using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Options;

namespace PetMate.Api.Middleware;

/// <summary>
/// Proveedor de compresión (pm-048) que evita comprimir respuestas con material sensible. Comprimir sobre HTTPS puede exponer a
/// ataques de tipo BREACH cuando un secreto y texto controlado por el atacante viajan en la misma respuesta; las respuestas de
/// autenticación contienen tokens, así que nunca se comprimen. Ver <c>docs/security/compression-risk.md</c>.
/// </summary>
public sealed class SensitiveAwareCompressionProvider(IServiceProvider services, IOptions<ResponseCompressionOptions> options)
    : ResponseCompressionProvider(services, options)
{
    private const string AuthPrefix = "/api/v1/auth/";

    public override bool ShouldCompressResponse(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        return !context.Request.Path.StartsWithSegments(AuthPrefix.TrimEnd('/'), StringComparison.OrdinalIgnoreCase)
            && base.ShouldCompressResponse(context);
    }
}
