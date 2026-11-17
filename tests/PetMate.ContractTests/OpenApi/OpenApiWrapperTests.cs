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
                    || path.Name.Contains("/businesses/get-nearby", StringComparison.Ordinal)
                    || path.Name.Contains("/businesses/get-by-id", StringComparison.Ordinal)
                    || path.Name.Contains("/auth/forgot-password", StringComparison.Ordinal)
                    || path.Name.Contains("/auth/reset-password", StringComparison.Ordinal)
                    || path.Name.Contains("/account-deletion/", StringComparison.Ordinal)
                    || path.Name.Contains("/health/", StringComparison.Ordinal)
                    || path.Name.Contains("/technical/", StringComparison.Ordinal);
                if (!isPublic)
                {
                    Assert.True(codes.Contains("401"), $"{path.Name} no documenta 401.");
                }
            }
        }
    }

    /// <summary>
    /// pm-166#CA-01: exporta el documento OpenAPI. Con <c>PETMATE_OPENAPI_OUT</c> definida (el CI la fija) lo guarda en esa ruta para
    /// publicarlo como artefacto; sin ella solo comprueba que el documento es válido. El documento sale del propio arranque de la
    /// API en Development, igual que lo ve el equipo móvil.
    /// </summary>
    [Trait("Endpoint", "transversal")]
    [Fact]
    public async Task The_openapi_document_is_valid_and_is_exported_when_the_ci_asks_for_it()
    {
        using var factory = new WrapperFactory();
        using var client = factory.CreateClient();
        var json = await client.GetStringAsync("/openapi/v1.json");

        using var document = JsonDocument.Parse(json);
        Assert.StartsWith("3.", document.RootElement.GetProperty("openapi").GetString(), StringComparison.Ordinal);
        Assert.True(document.RootElement.GetProperty("paths").EnumerateObject().Any());

        if (Environment.GetEnvironmentVariable("PETMATE_OPENAPI_OUT") is { Length: > 0 } output)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(output))!);
            await File.WriteAllTextAsync(output, json);
        }
    }

    [Trait("Endpoint", "transversal")]
    [Fact]
    public async Task Production_keeps_openapi_and_scalar_hidden()
    {
        using var factory = new IsolatedApiFactory("Production");
        using var client = factory.CreateClient();

        using var openapi = await client.GetAsync("/openapi/v1.json");
        using var scalar = await client.GetAsync("/scalar/v1");

        Assert.Equal(System.Net.HttpStatusCode.NotFound, openapi.StatusCode);
        Assert.Equal(System.Net.HttpStatusCode.NotFound, scalar.StatusCode);
    }

    private sealed class WrapperFactory : IsolatedApiFactory
    {
        public WrapperFactory()
            : base("Development")
        {
        }
    }
}
