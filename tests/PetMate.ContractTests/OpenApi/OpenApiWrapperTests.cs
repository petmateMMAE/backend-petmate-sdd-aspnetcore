using System.Text.Json;
using PetMate.ContractTests.Support;

namespace PetMate.ContractTests.OpenApi;

/// <summary>
/// El documento OpenAPI representa el Response Wrapper (pm-061 CA-04): cada respuesta con cuerpo de cada operación referencia
/// un esquema <c>ApiResponse*</c> con exactamente las propiedades <c>statusCode</c>, <c>message</c> y <c>data</c>.
/// </summary>
[Collection(ApiEnvironmentGroup.Name)]
public sealed class OpenApiWrapperTests
{
    [Trait("Endpoint", "transversal")]
    [Fact]
    public async Task Every_response_body_in_the_openapi_document_uses_the_response_wrapper_schema()
    {
        using var factory = new WrapperFactory();
        using var client = factory.CreateClient();
        using var document = JsonDocument.Parse(await client.GetStringAsync("/openapi/v1.json"));
        var schemas = document.RootElement.GetProperty("components").GetProperty("schemas");

        var checkedResponses = 0;
        foreach (var path in document.RootElement.GetProperty("paths").EnumerateObject())
        {
            foreach (var operation in path.Value.EnumerateObject())
            {
                if (!operation.Value.TryGetProperty("responses", out var responses))
                {
                    continue;
                }

                foreach (var response in responses.EnumerateObject())
                {
                    if (!response.Value.TryGetProperty("content", out var content))
                    {
                        continue;
                    }

                    foreach (var media in content.EnumerateObject())
                    {
                        var reference = media.Value.GetProperty("schema").GetProperty("$ref").GetString()!;
                        var name = reference[(reference.LastIndexOf('/') + 1)..];
                        Assert.StartsWith("ApiResponse", name, StringComparison.Ordinal);
                        var properties = schemas.GetProperty(name).GetProperty("properties").EnumerateObject().Select(p => p.Name).Order(StringComparer.Ordinal);
                        Assert.Equal(["data", "message", "statusCode"], properties);
                        checkedResponses++;
                    }
                }
            }
        }

        Assert.True(checkedResponses >= 40, $"Se esperaban al menos 40 respuestas con cuerpo documentadas y hay {checkedResponses}.");
    }

    [Trait("Endpoint", "transversal")]
    [Fact]
    public async Task Every_business_route_documents_the_wrapper_for_success_and_unauthorized()
    {
        using var factory = new WrapperFactory();
        using var client = factory.CreateClient();
        using var document = JsonDocument.Parse(await client.GetStringAsync("/openapi/v1.json"));

        foreach (var path in document.RootElement.GetProperty("paths").EnumerateObject().Where(p => p.Name.StartsWith("/api/v1/", StringComparison.Ordinal)))
        {
            foreach (var operation in path.Value.EnumerateObject())
            {
                var codes = operation.Value.GetProperty("responses").EnumerateObject().Select(r => r.Name).ToList();
                Assert.Contains(codes, code => code.StartsWith('2'));
                var isPublic = path.Name.Contains("/auth/register", StringComparison.Ordinal)
                    || path.Name.Contains("/auth/login", StringComparison.Ordinal)
                    || path.Name.Contains("/auth/refresh", StringComparison.Ordinal)
                    || path.Name.Contains("/auth/forgot-password", StringComparison.Ordinal)
                    || path.Name.Contains("/auth/reset-password", StringComparison.Ordinal)
                    || path.Name.Contains("/health/", StringComparison.Ordinal)
                    || path.Name.Contains("/technical/", StringComparison.Ordinal);
                if (!isPublic)
                {
                    Assert.True(codes.Contains("401"), $"{path.Name} no documenta 401.");
                }
            }
        }
    }

    private sealed class WrapperFactory : IsolatedApiFactory
    {
        public WrapperFactory()
            : base("Development")
        {
        }
    }
}
