using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using PetMate.Application.Pets;
using PetMate.Application.Security;
using PetMate.ContractTests.Authentication;
using PetMate.ContractTests.Support;
using PetMate.Domain.Users;

namespace PetMate.ContractTests.Pets;

/// <summary>
/// Límites contra el abuso del módulo de mascotas (pm-045 CA-04): máximo por propietario, peticiones por minuto por usuario
/// y tamaño máximo del cuerpo.
/// </summary>
[Collection(ApiEnvironmentGroup.Name)]
public sealed class PetLimitsTests
{
    private const string ValidBody = "{\"name\":\"Firulais\",\"species\":\"Perro\",\"sex\":\"Male\"}";

    [Trait("Endpoint", "pets-create")]
    [Fact]
    public async Task Create_at_the_maximum_per_owner_returns_409_wrapper_and_creates_nothing_more()
    {
        using var factory = new AuthFlowFactory();
        using var ana = Client(factory, Guid.NewGuid(), UserRole.User);
        using var luis = Client(factory, Guid.NewGuid(), UserRole.User);
        for (var i = 0; i < PetLimits.MaxPetsPerOwner; i++)
        {
            using var created = await Post(ana, ValidBody);
            Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        }

        using var blocked = await Post(ana, ValidBody);
        var raw = await blocked.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(raw);

        Assert.Equal(HttpStatusCode.Conflict, blocked.StatusCode);
        Assert.Equal(409, document.RootElement.GetProperty("statusCode").GetInt32());
        Assert.Equal(JsonValueKind.Null, document.RootElement.GetProperty("data").ValueKind);
        Assert.Contains("100", document.RootElement.GetProperty("message").GetString(), StringComparison.Ordinal);
        Assert.Equal(PetLimits.MaxPetsPerOwner, factory.Pets.All.Count);

        using var other = await Post(luis, ValidBody);
        Assert.Equal(HttpStatusCode.Created, other.StatusCode);
    }

    [Trait("Endpoint", "pets-create")]
    [Fact]
    public async Task Pets_endpoints_are_throttled_per_user_with_429_wrapper_and_retry_after()
    {
        using var factory = new AuthFlowFactory(petsRateLimitPerMinute: 3);
        using var ana = Client(factory, Guid.NewGuid(), UserRole.User);
        using var luis = Client(factory, Guid.NewGuid(), UserRole.User);

        for (var i = 0; i < 3; i++)
        {
            using var ok = await ana.GetAsync("/api/v1/pets/get-all");
            Assert.Equal(HttpStatusCode.OK, ok.StatusCode);
        }

        using var throttled = await ana.GetAsync("/api/v1/pets/get-all");
        using var document = JsonDocument.Parse(await throttled.Content.ReadAsStringAsync());
        using var untouched = await luis.GetAsync("/api/v1/pets/get-all");

        Assert.Equal(HttpStatusCode.TooManyRequests, throttled.StatusCode);
        Assert.Equal(429, document.RootElement.GetProperty("statusCode").GetInt32());
        Assert.True(throttled.Headers.Contains("Retry-After"));
        Assert.Equal(HttpStatusCode.OK, untouched.StatusCode);
    }

    [Trait("Endpoint", "pets-create")]
    [Theory]
    [InlineData(16 * 1024 + 1)]
    [InlineData(1024 * 1024)]
    public async Task A_body_over_16_KB_is_rejected_with_413_wrapper_before_it_is_read(int size)
    {
        using var factory = new AuthFlowFactory();
        using var client = Client(factory, Guid.NewGuid(), UserRole.User);
        var body = "{\"name\":\"" + new string('a', size) + "\",\"species\":\"Perro\",\"sex\":\"Male\"}";

        using var response = await Post(client, body);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.RequestEntityTooLarge, response.StatusCode);
        Assert.Equal(413, document.RootElement.GetProperty("statusCode").GetInt32());
        Assert.Empty(factory.Pets.All);
    }

    [Trait("Endpoint", "pets-update")]
    [Fact]
    public async Task An_oversized_update_body_is_rejected_with_413_and_changes_nothing()
    {
        using var factory = new AuthFlowFactory();
        using var client = Client(factory, Guid.NewGuid(), UserRole.User);
        using var created = await Post(client, ValidBody);
        using var createdDoc = JsonDocument.Parse(await created.Content.ReadAsStringAsync());
        var id = createdDoc.RootElement.GetProperty("data").GetProperty("id").GetString();
        var body = "{\"name\":\"" + new string('a', 20_000) + "\",\"species\":\"Perro\",\"sex\":\"Male\"}";

        using var request = new HttpRequestMessage(HttpMethod.Put, $"/api/v1/pets/update/{id}") { Content = new StringContent(body, Encoding.UTF8, "application/json") };
        using var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.RequestEntityTooLarge, response.StatusCode);
        Assert.Equal("Firulais", Assert.Single(factory.Pets.All).Name);
    }

    [Trait("Endpoint", "pets-create")]
    [Fact]
    public async Task A_body_just_under_16_KB_still_reaches_validation()
    {
        using var factory = new AuthFlowFactory();
        using var client = Client(factory, Guid.NewGuid(), UserRole.User);
        var body = "{\"name\":\"" + new string('a', 16 * 1024 - 100) + "\",\"species\":\"Perro\",\"sex\":\"Male\"}";

        using var response = await Post(client, body);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private static HttpClient Client(AuthFlowFactory factory, Guid userId, UserRole role)
    {
        var token = factory.Services.GetRequiredService<IAccessTokenService>().Issue(userId, role);
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Value);
        return client;
    }

    private static Task<HttpResponseMessage> Post(HttpClient client, string body)
        => client.PostAsync("/api/v1/pets/create", new StringContent(body, Encoding.UTF8, "application/json"));
}
