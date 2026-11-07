using System.Net;
using System.Text.Json;
using PetMate.ContractTests.Support;

namespace PetMate.ContractTests.Technical;

/// <summary>
/// GET /api/v1/technical/mediator-probe: ruta técnica sin cuerpo ni parámetros; no hay entrada que validar, por lo que la
/// validación equivalente es el método HTTP y el contrato del wrapper.
/// </summary>
[Collection(ApiEnvironmentGroup.Name)]
public sealed class MediatorProbeEndpointTests
{
    [Trait("Endpoint", "technical-mediator-probe")]
    [Fact]
    public async Task Get_returns_200_wrapper_with_the_mediated_result()
    {
        using var factory = new IsolatedApiFactory("Development");
        using var client = factory.CreateClient();

        using var response = await client.GetAsync("/api/v1/technical/mediator-probe");
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(["data", "message", "statusCode"], document.RootElement.EnumerateObject().Select(p => p.Name).Order(StringComparer.Ordinal));
        Assert.Equal(200, document.RootElement.GetProperty("statusCode").GetInt32());
        Assert.Equal("mediated", document.RootElement.GetProperty("data").GetString());
    }

    [Trait("Endpoint", "technical-mediator-probe")]
    [Fact]
    public async Task Query_string_parameters_are_ignored()
    {
        using var factory = new IsolatedApiFactory("Development");
        using var client = factory.CreateClient();

        using var response = await client.GetAsync("/api/v1/technical/mediator-probe?valor=%27%3Cscript%3E");
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("mediated", document.RootElement.GetProperty("data").GetString());
    }

    [Trait("Endpoint", "technical-mediator-probe")]
    [Theory]
    [InlineData("POST")]
    [InlineData("PUT")]
    [InlineData("DELETE")]
    public async Task Other_http_methods_return_405_wrapper(string method)
    {
        using var factory = new IsolatedApiFactory("Development");
        using var client = factory.CreateClient();
        using var request = new HttpRequestMessage(new HttpMethod(method), "/api/v1/technical/mediator-probe");

        using var response = await client.SendAsync(request);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        Assert.Equal(405, document.RootElement.GetProperty("statusCode").GetInt32());
        Assert.Equal(JsonValueKind.Null, document.RootElement.GetProperty("data").ValueKind);
    }
}
