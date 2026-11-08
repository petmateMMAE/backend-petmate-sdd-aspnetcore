using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using PetMate.Application.Security;
using PetMate.ContractTests.Authentication;
using PetMate.ContractTests.Support;
using PetMate.Domain.Users;

namespace PetMate.ContractTests.Pets;

/// <summary>
/// Contrato HTTP del listado de mascotas (pm-041) y de la convención de paginación (pm-047): solo las mascotas del Owner del
/// token, <c>page</c> desde 1, <c>pageSize</c> 20 por defecto y 50 como máximo, 400 con el parámetro inválido.
/// </summary>
[Collection(ApiEnvironmentGroup.Name)]
public sealed class PetListEndpointsTests
{
    [Trait("Endpoint", "pets-get-all")]
    [Fact]
    public async Task Get_all_returns_the_standard_page_shape_with_defaults_and_only_the_own_pets()
    {
        using var factory = new AuthFlowFactory();
        using var ana = Client(factory, Guid.NewGuid(), UserRole.Owner);
        using var luis = Client(factory, Guid.NewGuid(), UserRole.Owner);
        await CreatePet(ana, "Firulais");
        await CreatePet(ana, "Michi");
        await CreatePet(luis, "Ajena");

        using var response = await Get(ana, "");
        var raw = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(raw);
        var data = document.RootElement.GetProperty("data");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(200, document.RootElement.GetProperty("statusCode").GetInt32());
        Assert.Equal(["items", "page", "pageSize", "totalCount", "totalPages"], data.EnumerateObject().Select(p => p.Name).Order(StringComparer.Ordinal));
        Assert.Equal(1, data.GetProperty("page").GetInt32());
        Assert.Equal(20, data.GetProperty("pageSize").GetInt32());
        Assert.Equal(2, data.GetProperty("totalCount").GetInt32());
        Assert.Equal(1, data.GetProperty("totalPages").GetInt32());
        Assert.Equal(["Firulais", "Michi"], data.GetProperty("items").EnumerateArray().Select(i => i.GetProperty("name").GetString()).Order(StringComparer.Ordinal));
        Assert.DoesNotContain("Ajena", raw, StringComparison.Ordinal);
        Assert.DoesNotContain("ownerId", raw, StringComparison.OrdinalIgnoreCase);
    }

    [Trait("Endpoint", "pets-get-all")]
    [Fact]
    public async Task Get_all_with_no_pets_returns_an_empty_page()
    {
        using var factory = new AuthFlowFactory();
        using var client = Client(factory, Guid.NewGuid(), UserRole.Owner);

        using var response = await Get(client, "");
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var data = document.RootElement.GetProperty("data");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Empty(data.GetProperty("items").EnumerateArray());
        Assert.Equal(0, data.GetProperty("totalCount").GetInt32());
        Assert.Equal(0, data.GetProperty("totalPages").GetInt32());
    }

    [Trait("Endpoint", "pets-get-all")]
    [Fact]
    public async Task Get_all_pages_through_the_pets_and_an_out_of_range_page_is_empty()
    {
        using var factory = new AuthFlowFactory();
        using var client = Client(factory, Guid.NewGuid(), UserRole.Owner);
        for (var i = 0; i < 3; i++)
        {
            await CreatePet(client, $"Mascota {i}");
        }

        var first = await ReadPage(client, "?page=1&pageSize=2");
        var second = await ReadPage(client, "?page=2&pageSize=2");
        var beyond = await ReadPage(client, "?page=3&pageSize=2");
        var huge = await ReadPage(client, "?page=2147483647&pageSize=50");

        Assert.Equal(2, first.Items);
        Assert.Equal(1, second.Items);
        Assert.Equal((3, 2), (first.TotalCount, first.TotalPages));
        Assert.Equal(0, beyond.Items);
        Assert.Equal(3, beyond.TotalCount);
        Assert.Equal(0, huge.Items);
    }

    [Trait("Endpoint", "pets-get-all")]
    [Fact]
    public async Task Get_all_accepts_the_maximum_page_size_of_50()
    {
        using var factory = new AuthFlowFactory();
        using var client = Client(factory, Guid.NewGuid(), UserRole.Owner);

        using var response = await Get(client, "?pageSize=50");
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(50, document.RootElement.GetProperty("data").GetProperty("pageSize").GetInt32());
    }

    [Trait("Endpoint", "pets-get-all")]
    [Theory]
    [InlineData("?page=0", "page")]
    [InlineData("?page=-1", "page")]
    [InlineData("?page=abc", "page")]
    [InlineData("?page=1.5", "page")]
    [InlineData("?pageSize=0", "pageSize")]
    [InlineData("?pageSize=51", "pageSize")]
    [InlineData("?pageSize=-5", "pageSize")]
    [InlineData("?pageSize=abc", "pageSize")]
    [InlineData("?pageSize=99999999999", "pageSize")]
    public async Task Get_all_with_an_invalid_parameter_returns_400_naming_it(string query, string field)
    {
        using var factory = new AuthFlowFactory();
        using var client = Client(factory, Guid.NewGuid(), UserRole.Owner);

        using var response = await Get(client, query);
        var raw = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(raw);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(400, document.RootElement.GetProperty("statusCode").GetInt32());
        Assert.Contains(document.RootElement.GetProperty("data").GetProperty("errors").EnumerateArray(), e => e.GetProperty("field").GetString() == field);
    }

    [Trait("Endpoint", "pets-get-all")]
    [Fact]
    public async Task Get_all_ignores_an_owner_id_sent_in_the_query_string()
    {
        using var factory = new AuthFlowFactory();
        var victim = Guid.NewGuid();
        using var attacker = Client(factory, Guid.NewGuid(), UserRole.Owner);
        using var owner = Client(factory, victim, UserRole.Owner);
        await CreatePet(owner, "Privada");

        using var response = await Get(attacker, $"?ownerId={victim}&userId={victim}");
        var raw = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.DoesNotContain("Privada", raw, StringComparison.Ordinal);
    }

    [Trait("Endpoint", "pets-get-all")]
    [Fact]
    public async Task Get_all_requires_an_owner_token()
    {
        using var factory = new AuthFlowFactory();
        using var anonymous = factory.CreateClient();
        using var business = Client(factory, Guid.NewGuid(), UserRole.Business);

        using var unauthenticated = await Get(anonymous, "");
        using var forbidden = await Get(business, "");

        Assert.Equal(HttpStatusCode.Unauthorized, unauthenticated.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, forbidden.StatusCode);
        Assert.Equal(401, JsonDocument.Parse(await unauthenticated.Content.ReadAsStringAsync()).RootElement.GetProperty("statusCode").GetInt32());
    }

    private static HttpClient Client(AuthFlowFactory factory, Guid userId, UserRole role)
    {
        var token = factory.Services.GetRequiredService<IAccessTokenService>().Issue(userId, role);
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Value);
        return client;
    }

    private static async Task CreatePet(HttpClient client, string name)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/pets/create")
        {
            Content = new StringContent($"{{\"name\":\"{name}\",\"species\":\"Perro\",\"sex\":\"Male\"}}", Encoding.UTF8, "application/json"),
        };
        using var response = await client.SendAsync(request);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    private static Task<HttpResponseMessage> Get(HttpClient client, string query) => client.GetAsync($"/api/v1/pets/get-all{query}");

    private static async Task<(int Items, int TotalCount, int TotalPages)> ReadPage(HttpClient client, string query)
    {
        using var response = await Get(client, query);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var data = document.RootElement.GetProperty("data");
        return (data.GetProperty("items").GetArrayLength(), data.GetProperty("totalCount").GetInt32(), data.GetProperty("totalPages").GetInt32());
    }
}
