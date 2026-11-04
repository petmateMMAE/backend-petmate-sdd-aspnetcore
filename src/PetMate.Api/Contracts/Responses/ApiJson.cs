using System.Text.Json;

namespace PetMate.Api.Contracts.Responses;

internal static class ApiJson
{
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);
}
