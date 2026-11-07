using PetMate.ContractTests.Support;
using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace PetMate.ContractTests.OpenApi;

[Collection(ApiEnvironmentGroup.Name)]
public sealed class OpenApiEndpointsTests
{
    [Trait("Endpoint", "transversal")]
    [Fact]
    public async Task Development_openapi_document_is_valid_and_contains_technical_route()
    {
        using var factory = new PetMateApiFactory("Development");
        using var client = factory.CreateClient();

        using var response = await client.GetAsync("/openapi/v1.json");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var document = await response.Content.ReadFromJsonAsync<OpenApiDocument>();
        Assert.NotNull(document);
        Assert.Equal("PetMate API", document.Info.Title);
        Assert.Equal("v1", document.Info.Version);
        Assert.Contains("/api/v1/technical/mediator-probe", document.Paths.Keys);
    }

    [Trait("Endpoint", "transversal")]
    [Fact]
    public async Task Development_scalar_loads_the_openapi_document()
    {
        using var factory = new PetMateApiFactory("Development");
        using var client = factory.CreateClient();

        using var response = await client.GetAsync("/scalar/v1");
        var content = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("openapi", content, StringComparison.OrdinalIgnoreCase);
    }

    [Trait("Endpoint", "transversal")]
    [Fact]
    public async Task Production_hides_openapi_and_scalar_by_default()
    {
        using var factory = new PetMateApiFactory("Production");
        using var client = factory.CreateClient();

        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/openapi/v1.json")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/scalar/v1")).StatusCode);
    }

    [Trait("Endpoint", "technical-mediator-probe")]
    [Fact]
    public async Task Only_the_versioned_technical_route_is_mapped()
    {
        using var factory = new PetMateApiFactory("Development");
        using var client = factory.CreateClient();

        using var versionedResponse = await client.GetAsync("/api/v1/technical/mediator-probe");

        Assert.Equal(HttpStatusCode.OK, versionedResponse.StatusCode);
        var body = await versionedResponse.Content.ReadFromJsonAsync<ApiResponseDocument>();
        Assert.NotNull(body);
        Assert.Equal(200, body.StatusCode);
        Assert.Equal("Consulta técnica procesada correctamente.", body.Message);
        Assert.Equal("mediated", body.Data);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/technical/mediator-probe")).StatusCode);
    }

    private sealed class PetMateApiFactory(string environmentName) : IsolatedApiFactory(environmentName);

    private sealed class OpenApiDocument
    {
        public OpenApiInfo Info { get; init; } = new();

        public Dictionary<string, object> Paths { get; init; } = [];
    }

    private sealed class OpenApiInfo
    {
        public string? Title { get; init; }

        public string? Version { get; init; }
    }

    private sealed class ApiResponseDocument
    {
        public int StatusCode { get; init; }

        public string? Message { get; init; }

        public string? Data { get; init; }
    }
}
