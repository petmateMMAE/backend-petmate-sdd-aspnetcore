using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace PetMate.Api.Contracts.Responses;

internal static class ApiJson
{
    // Metadatos generados primero (sin reflexión) y, para cualquier otro tipo, el resolvedor por reflexión de respaldo.
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        TypeInfoResolver = JsonTypeInfoResolver.Combine(ApiJsonContext.Default, new DefaultJsonTypeInfoResolver()),
    };
}
