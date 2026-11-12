using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using PetMate.Application.Security;
using PetMate.ContractTests.Authentication;
using PetMate.ContractTests.Support;
using PetMate.Domain.Users;

namespace PetMate.ContractTests.Businesses;

/// <summary>
/// Contrato HTTP del negocio afiliado y su catálogo (pm-107 a pm-115) con JWT real y repositorios en memoria. El negocio es
/// siempre el de la cuenta del token: no hay identificador de negocio en el cliente.
/// </summary>
[Collection(ApiEnvironmentGroup.Name)]
public sealed class BusinessEndpointsTests
{
    private const string Store = "{\"type\":\"Store\",\"name\":\"Pet Shop Sol\",\"phone\":\"55 1234 5678\",\"address\":\"Av. Reforma 10\",\"latitude\":19.4326,\"longitude\":-99.1332}";
    private const string Vet = "{\"type\":\"Veterinary\",\"name\":\"Vet Sol\",\"phone\":\"5512345678\",\"whatsApp\":\"5598765432\",\"address\":\"Calle 5\",\"description\":\"Urgencias\",\"latitude\":19.4,\"longitude\":-99.1,\"licenseNumber\":\"CED-1\",\"schedule\":{\"monday\":[{\"open\":\"09:00\",\"close\":\"18:00\"}]}}";

    // ---------- Negocio: alta (pm-107) ----------

    [Trait("Endpoint", "businesses-create")]
    [Fact]
    public async Task Create_returns_201_without_approval_and_the_session_reports_the_business_type()
    {
        using var factory = new AuthFlowFactory();
        using var client = factory.CreateClient();
        var (access, refresh) = await Register(client, "ana@example.com");
        using var authed = Authed(factory, access);

        using var response = await Send(authed, HttpMethod.Post, "businesses/create", Vet);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var data = document.RootElement.GetProperty("data");
        using var refreshed = await Send(client, HttpMethod.Post, "auth/refresh", $"{{\"refreshToken\":\"{refresh}\"}}");
        using var refreshedDoc = JsonDocument.Parse(await refreshed.Content.ReadAsStringAsync());
        using var login = await Send(client, HttpMethod.Post, "auth/login", "{\"email\":\"ana@example.com\",\"password\":\"contrasena-segura-123\"}");
        using var loginDoc = JsonDocument.Parse(await login.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal("Veterinary", data.GetProperty("type").GetString());
        Assert.Equal("CED-1", data.GetProperty("licenseNumber").GetString());
        Assert.Equal("09:00", data.GetProperty("schedule").GetProperty("monday")[0].GetProperty("open").GetString());
        Assert.Equal("Veterinary", refreshedDoc.RootElement.GetProperty("data").GetProperty("user").GetProperty("business").GetProperty("type").GetString());
        Assert.Equal("Veterinary", loginDoc.RootElement.GetProperty("data").GetProperty("user").GetProperty("business").GetProperty("type").GetString());
        Assert.Equal("User", loginDoc.RootElement.GetProperty("data").GetProperty("user").GetProperty("role").GetString());
        Assert.Single(factory.Businesses.All);
    }

    [Trait("Endpoint", "businesses-create")]
    [Fact]
    public async Task A_second_business_for_the_same_account_returns_409()
    {
        using var factory = new AuthFlowFactory();
        using var client = await Owner(factory, "ana@example.com");

        await AssertWrapper(await Send(client, HttpMethod.Post, "businesses/create", Store), HttpStatusCode.Created);
        await AssertWrapper(await Send(client, HttpMethod.Post, "businesses/create", Vet), HttpStatusCode.Conflict);

        Assert.Single(factory.Businesses.All);
    }

    [Trait("Endpoint", "businesses-create")]
    [Theory]
    [InlineData("type", "{\"type\":\"Hospital\",\"name\":\"X\",\"phone\":\"5512345678\",\"address\":\"A\",\"latitude\":1,\"longitude\":1}")]
    [InlineData("licenseNumber", "{\"type\":\"Veterinary\",\"name\":\"X\",\"phone\":\"5512345678\",\"address\":\"A\",\"latitude\":1,\"longitude\":1}")]
    [InlineData("name", "{\"type\":\"Store\",\"phone\":\"5512345678\",\"address\":\"A\",\"latitude\":1,\"longitude\":1}")]
    [InlineData("phone", "{\"type\":\"Store\",\"name\":\"X\",\"phone\":\"123\",\"address\":\"A\",\"latitude\":1,\"longitude\":1}")]
    [InlineData("latitude", "{\"type\":\"Store\",\"name\":\"X\",\"phone\":\"5512345678\",\"address\":\"A\",\"latitude\":91,\"longitude\":1}")]
    [InlineData("longitude", "{\"type\":\"Store\",\"name\":\"X\",\"phone\":\"5512345678\",\"address\":\"A\",\"latitude\":1}")]
    [InlineData("schedule", "{\"type\":\"Store\",\"name\":\"X\",\"phone\":\"5512345678\",\"address\":\"A\",\"latitude\":1,\"longitude\":1,\"schedule\":{\"monday\":[{\"open\":\"18:00\",\"close\":\"09:00\"}]}}")]
    public async Task Create_with_an_invalid_field_returns_400_naming_it(string field, string body)
    {
        using var factory = new AuthFlowFactory();
        using var client = await Owner(factory, "ana@example.com");

        await AssertFieldError(await Send(client, HttpMethod.Post, "businesses/create", body), field);
        Assert.Empty(factory.Businesses.All);
    }

    [Trait("Endpoint", "businesses-create")]
    [Fact]
    public async Task Create_rejects_members_the_client_must_not_send_and_requires_a_token()
    {
        using var factory = new AuthFlowFactory();
        using var client = await Owner(factory, "ana@example.com");
        using var anonymous = factory.CreateClient();

        using var withOwner = await Send(client, HttpMethod.Post, "businesses/create", Store.Replace("{", "{\"ownerId\":\"" + Guid.NewGuid() + "\",", StringComparison.Ordinal));

        Assert.Equal(HttpStatusCode.BadRequest, withOwner.StatusCode);
        await AssertWrapper(await Send(anonymous, HttpMethod.Post, "businesses/create", Store), HttpStatusCode.Unauthorized);
        Assert.Empty(factory.Businesses.All);
    }

    [Trait("Endpoint", "businesses-create")]
    [Fact]
    public async Task Creating_a_business_keeps_the_account_role_and_its_pets()
    {
        using var factory = new AuthFlowFactory();
        using var client = await Owner(factory, "ana@example.com");
        using var pet = await Send(client, HttpMethod.Post, "pets/create", "{\"name\":\"Firulais\",\"species\":\"Perro\"}");

        await Send(client, HttpMethod.Post, "businesses/create", Store);
        using var list = await Send(client, HttpMethod.Get, "pets/get-all");
        using var profile = await Send(client, HttpMethod.Get, "users/me/get");
        using var profileDoc = JsonDocument.Parse(await profile.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.Created, pet.StatusCode);
        Assert.Contains("Firulais", await list.Content.ReadAsStringAsync(), StringComparison.Ordinal);
        Assert.Equal("User", profileDoc.RootElement.GetProperty("data").GetProperty("role").GetString());
    }

    // ---------- Ubicación (pm-108) ----------

    [Trait("Endpoint", "businesses-update-location")]
    [Fact]
    public async Task Update_location_changes_the_coordinates_and_rejects_invalid_ones()
    {
        using var factory = new AuthFlowFactory();
        using var client = await Owner(factory, "ana@example.com");
        await Send(client, HttpMethod.Post, "businesses/create", Store);

        using var moved = await Send(client, HttpMethod.Put, "businesses/update-location", "{\"latitude\":20.5,\"longitude\":-100.4}");
        using var movedDoc = JsonDocument.Parse(await moved.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, moved.StatusCode);
        Assert.Equal(20.5, movedDoc.RootElement.GetProperty("data").GetProperty("latitude").GetDouble());
        await AssertFieldError(await Send(client, HttpMethod.Put, "businesses/update-location", "{\"latitude\":120,\"longitude\":0}"), "latitude");
        await AssertFieldError(await Send(client, HttpMethod.Put, "businesses/update-location", "{\"latitude\":0}"), "longitude");
        Assert.Equal(20.5, factory.Businesses.All.Single().Latitude);
    }

    [Trait("Endpoint", "businesses-update-location")]
    [Fact]
    public async Task Only_the_owner_account_can_move_its_business_and_an_account_without_business_gets_403()
    {
        using var factory = new AuthFlowFactory();
        using var ana = await Owner(factory, "ana@example.com");
        using var luis = await Owner(factory, "luis@example.com");
        await Send(ana, HttpMethod.Post, "businesses/create", Store);

        await AssertWrapper(await Send(luis, HttpMethod.Put, "businesses/update-location", "{\"latitude\":1,\"longitude\":1}"), HttpStatusCode.Forbidden);
        await AssertWrapper(await Send(factory.CreateClient(), HttpMethod.Put, "businesses/update-location", "{\"latitude\":1,\"longitude\":1}"), HttpStatusCode.Unauthorized);
        Assert.Equal(19.4326, factory.Businesses.All.Single().Latitude);
    }

    // ---------- Perfil (pm-109) ----------

    [Trait("Endpoint", "businesses-update")]
    [Fact]
    public async Task Update_edits_the_profile_but_a_request_that_tries_to_change_the_type_returns_400()
    {
        using var factory = new AuthFlowFactory();
        using var client = await Owner(factory, "ana@example.com");
        await Send(client, HttpMethod.Post, "businesses/create", Vet);

        using var updated = await Send(client, HttpMethod.Put, "businesses/update", "{\"name\":\"Vet Luna\",\"phone\":\"5511112222\",\"description\":\"Nueva\",\"schedule\":{\"friday\":[{\"open\":\"10:00\",\"close\":\"14:00\"}]}}");
        using var updatedDoc = JsonDocument.Parse(await updated.Content.ReadAsStringAsync());
        using var typeChange = await Send(client, HttpMethod.Put, "businesses/update", "{\"type\":\"Store\",\"name\":\"X\",\"phone\":\"5511112222\"}");

        Assert.Equal(HttpStatusCode.OK, updated.StatusCode);
        Assert.Equal("Vet Luna", updatedDoc.RootElement.GetProperty("data").GetProperty("name").GetString());
        Assert.Equal("Veterinary", updatedDoc.RootElement.GetProperty("data").GetProperty("type").GetString());
        Assert.False(updatedDoc.RootElement.GetProperty("data").GetProperty("schedule").TryGetProperty("monday", out _));
        Assert.Equal(HttpStatusCode.BadRequest, typeChange.StatusCode);
        Assert.Equal("Vet Luna", factory.Businesses.All.Single().Name);
    }

    [Trait("Endpoint", "businesses-update")]
    [Theory]
    [InlineData("name", "{\"name\":\"\",\"phone\":\"5511112222\"}")]
    [InlineData("phone", "{\"name\":\"X\",\"phone\":\"12\"}")]
    [InlineData("schedule", "{\"name\":\"X\",\"phone\":\"5511112222\",\"schedule\":{\"monday\":[{\"open\":\"09:00\",\"close\":\"14:00\"},{\"open\":\"13:00\",\"close\":\"18:00\"}]}}")]
    public async Task Update_with_an_invalid_field_returns_400_and_without_business_403(string field, string body)
    {
        using var factory = new AuthFlowFactory();
        using var client = await Owner(factory, "ana@example.com");
        using var luis = await Owner(factory, "luis@example.com");
        await Send(client, HttpMethod.Post, "businesses/create", Store);

        await AssertFieldError(await Send(client, HttpMethod.Put, "businesses/update", body), field);
        await AssertWrapper(await Send(luis, HttpMethod.Put, "businesses/update", "{\"name\":\"X\",\"phone\":\"5511112222\"}"), HttpStatusCode.Forbidden);
    }

    // ---------- Baja (pm-110) ----------

    [Trait("Endpoint", "businesses-delete")]
    [Fact]
    public async Task Delete_removes_the_business_and_its_catalog_but_not_the_account_or_the_pets()
    {
        using var factory = new AuthFlowFactory();
        using var client = await Owner(factory, "ana@example.com");
        await Send(client, HttpMethod.Post, "pets/create", "{\"name\":\"Firulais\",\"species\":\"Perro\"}");
        await Send(client, HttpMethod.Post, "businesses/create", Store);
        await Send(client, HttpMethod.Post, "services/create", "{\"name\":\"Baño\"}");

        await AssertWrapper(await Send(client, HttpMethod.Delete, "businesses/delete"), HttpStatusCode.OK);
        using var login = await Send(client, HttpMethod.Post, "auth/login", "{\"email\":\"ana@example.com\",\"password\":\"contrasena-segura-123\"}");
        using var loginDoc = JsonDocument.Parse(await login.Content.ReadAsStringAsync());
        using var pets = await Send(client, HttpMethod.Get, "pets/get-all");

        Assert.Empty(factory.Businesses.All);
        Assert.Empty(factory.BusinessServices.All);
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        Assert.Equal(JsonValueKind.Null, loginDoc.RootElement.GetProperty("data").GetProperty("user").GetProperty("business").ValueKind);
        Assert.Contains("Firulais", await pets.Content.ReadAsStringAsync(), StringComparison.Ordinal);
        await AssertWrapper(await Send(client, HttpMethod.Delete, "businesses/delete"), HttpStatusCode.Forbidden);
        await AssertWrapper(await Send(client, HttpMethod.Post, "businesses/create", Vet), HttpStatusCode.Created);
    }

    [Trait("Endpoint", "businesses-delete")]
    [Fact]
    public async Task Delete_only_touches_the_business_of_the_token_account()
    {
        using var factory = new AuthFlowFactory();
        using var ana = await Owner(factory, "ana@example.com");
        using var luis = await Owner(factory, "luis@example.com");
        await Send(ana, HttpMethod.Post, "businesses/create", Store);

        await AssertWrapper(await Send(luis, HttpMethod.Delete, "businesses/delete"), HttpStatusCode.Forbidden);
        await AssertWrapper(await Send(factory.CreateClient(), HttpMethod.Delete, "businesses/delete"), HttpStatusCode.Unauthorized);

        Assert.Single(factory.Businesses.All);
    }

    // ---------- Servicios (pm-112 a pm-115) ----------

    [Trait("Endpoint", "services-create")]
    [Fact]
    public async Task Service_create_returns_201_and_without_price_the_price_is_null_never_zero()
    {
        using var factory = new AuthFlowFactory();
        using var client = await Owner(factory, "ana@example.com");
        await Send(client, HttpMethod.Post, "businesses/create", Store);

        using var free = await Send(client, HttpMethod.Post, "services/create", "{\"name\":\"  Consulta  \"}");
        using var freeDoc = JsonDocument.Parse(await free.Content.ReadAsStringAsync());
        using var priced = await Send(client, HttpMethod.Post, "services/create", "{\"name\":\"Baño\",\"description\":\"Con secado\",\"price\":250.5}");
        using var zero = await Send(client, HttpMethod.Post, "services/create", "{\"name\":\"Gratis\",\"price\":0}");
        using var zeroDoc = JsonDocument.Parse(await zero.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.Created, free.StatusCode);
        Assert.Equal("Consulta", freeDoc.RootElement.GetProperty("data").GetProperty("name").GetString());
        Assert.Equal(JsonValueKind.Null, freeDoc.RootElement.GetProperty("data").GetProperty("price").ValueKind);
        Assert.Equal(HttpStatusCode.Created, priced.StatusCode);
        Assert.Equal(0m, zeroDoc.RootElement.GetProperty("data").GetProperty("price").GetDecimal());
        Assert.Equal(3, factory.BusinessServices.All.Count);
    }

    [Trait("Endpoint", "services-create")]
    [Theory]
    [InlineData("name", "{\"price\":10}")]
    [InlineData("name", "{\"name\":\"  \"}")]
    [InlineData("price", "{\"name\":\"X\",\"price\":-1}")]
    [InlineData("price", "{\"name\":\"X\",\"price\":10.999}")]
    [InlineData("description", "{\"name\":\"X\",\"description\":\"ppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppppp\"}")]
    public async Task Service_create_with_an_invalid_field_returns_400_naming_it(string field, string body)
    {
        using var factory = new AuthFlowFactory();
        using var client = await Owner(factory, "ana@example.com");
        await Send(client, HttpMethod.Post, "businesses/create", Store);

        await AssertFieldError(await Send(client, HttpMethod.Post, "services/create", body), field);
        Assert.Empty(factory.BusinessServices.All);
    }

    [Trait("Endpoint", "services-create")]
    [Fact]
    public async Task Only_an_account_with_a_business_can_create_services_and_no_stock_or_sales_members_exist()
    {
        using var factory = new AuthFlowFactory();
        using var client = await Owner(factory, "ana@example.com");
        using var anonymous = factory.CreateClient();

        await AssertWrapper(await Send(client, HttpMethod.Post, "services/create", "{\"name\":\"Baño\"}"), HttpStatusCode.Forbidden);
        await AssertWrapper(await Send(anonymous, HttpMethod.Post, "services/create", "{\"name\":\"Baño\"}"), HttpStatusCode.Unauthorized);
        await Send(client, HttpMethod.Post, "businesses/create", Store);
        using var withStock = await Send(client, HttpMethod.Post, "services/create", "{\"name\":\"Baño\",\"stock\":5}");

        Assert.Equal(HttpStatusCode.BadRequest, withStock.StatusCode);
        Assert.Empty(factory.BusinessServices.All);
    }

    [Trait("Endpoint", "services-get-all")]
    [Fact]
    public async Task Service_list_returns_only_the_own_catalog_in_a_stable_order_with_paging()
    {
        using var factory = new AuthFlowFactory();
        using var ana = await Owner(factory, "ana@example.com");
        using var luis = await Owner(factory, "luis@example.com");
        await Send(ana, HttpMethod.Post, "businesses/create", Store);
        await Send(luis, HttpMethod.Post, "businesses/create", Vet);
        foreach (var name in new[] { "A", "B", "C" })
        {
            await Send(ana, HttpMethod.Post, "services/create", $"{{\"name\":\"{name}\"}}");
        }

        await Send(luis, HttpMethod.Post, "services/create", "{\"name\":\"Ajeno\"}");

        using var first = await Send(ana, HttpMethod.Get, "services/get-all?pageSize=2");
        using var firstDoc = JsonDocument.Parse(await first.Content.ReadAsStringAsync());
        using var second = await Send(ana, HttpMethod.Get, "services/get-all?pageSize=2&page=2");
        using var secondDoc = JsonDocument.Parse(await second.Content.ReadAsStringAsync());
        var firstData = firstDoc.RootElement.GetProperty("data");

        Assert.Equal(3, firstData.GetProperty("totalCount").GetInt32());
        var firstNames = firstData.GetProperty("items").EnumerateArray().Select(i => i.GetProperty("name").GetString()).ToArray();
        var secondNames = secondDoc.RootElement.GetProperty("data").GetProperty("items").EnumerateArray().Select(i => i.GetProperty("name").GetString()).ToArray();
        using var again = await Send(ana, HttpMethod.Get, "services/get-all?pageSize=2");
        using var againDoc = JsonDocument.Parse(await again.Content.ReadAsStringAsync());

        // Orden estable: las dos páginas no se repiten, suman el catálogo propio y una segunda consulta devuelve lo mismo.
        Assert.Equal(2, firstNames.Length);
        Assert.Equal(["A", "B", "C"], firstNames.Concat(secondNames).Order(StringComparer.Ordinal));
        Assert.Equal(firstNames, againDoc.RootElement.GetProperty("data").GetProperty("items").EnumerateArray().Select(i => i.GetProperty("name").GetString()));
        await AssertFieldError(await Send(ana, HttpMethod.Get, "services/get-all?pageSize=51"), "pageSize");
    }

    [Trait("Endpoint", "services-get-all")]
    [Fact]
    public async Task Service_list_for_an_account_without_business_returns_403_without_data()
    {
        using var factory = new AuthFlowFactory();
        using var client = await Owner(factory, "ana@example.com");

        using var response = await Send(client, HttpMethod.Get, "services/get-all");
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Equal(JsonValueKind.Null, document.RootElement.GetProperty("data").ValueKind);
        await AssertWrapper(await Send(factory.CreateClient(), HttpMethod.Get, "services/get-all"), HttpStatusCode.Unauthorized);
    }

    [Trait("Endpoint", "services-update")]
    [Fact]
    public async Task Service_update_edits_an_own_service_and_hides_foreign_or_missing_ones_with_404()
    {
        using var factory = new AuthFlowFactory();
        using var ana = await Owner(factory, "ana@example.com");
        using var luis = await Owner(factory, "luis@example.com");
        await Send(ana, HttpMethod.Post, "businesses/create", Store);
        await Send(luis, HttpMethod.Post, "businesses/create", Vet);
        using var created = await Send(ana, HttpMethod.Post, "services/create", "{\"name\":\"Baño\",\"price\":200}");
        using var createdDoc = JsonDocument.Parse(await created.Content.ReadAsStringAsync());
        var id = createdDoc.RootElement.GetProperty("data").GetProperty("id").GetString();

        using var updated = await Send(ana, HttpMethod.Put, $"services/update/{id}", "{\"name\":\"Baño y corte\",\"description\":\"Completo\"}");
        using var updatedDoc = JsonDocument.Parse(await updated.Content.ReadAsStringAsync());
        using var foreign = await Send(luis, HttpMethod.Put, $"services/update/{id}", "{\"name\":\"Robado\"}");
        using var missing = await Send(luis, HttpMethod.Put, $"services/update/{Guid.NewGuid()}", "{\"name\":\"X\"}");
        var foreignBody = await foreign.Content.ReadAsStringAsync();
        var missingBody = await missing.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, updated.StatusCode);
        Assert.Equal("Baño y corte", updatedDoc.RootElement.GetProperty("data").GetProperty("name").GetString());
        Assert.Equal(JsonValueKind.Null, updatedDoc.RootElement.GetProperty("data").GetProperty("price").ValueKind);
        Assert.Equal(HttpStatusCode.NotFound, foreign.StatusCode);
        Assert.Equal(missingBody, foreignBody);
        Assert.Equal("Baño y corte", factory.BusinessServices.All.Single().Name);
        await AssertFieldError(await Send(ana, HttpMethod.Put, $"services/update/{id}", "{\"name\":\"\"}"), "name");
        await AssertWrapper(await Send(factory.CreateClient(), HttpMethod.Put, $"services/update/{id}", "{\"name\":\"X\"}"), HttpStatusCode.Unauthorized);
    }

    [Trait("Endpoint", "services-delete")]
    [Fact]
    public async Task Service_delete_removes_an_own_service_and_returns_404_for_foreign_or_missing_ones()
    {
        using var factory = new AuthFlowFactory();
        using var ana = await Owner(factory, "ana@example.com");
        using var luis = await Owner(factory, "luis@example.com");
        await Send(ana, HttpMethod.Post, "businesses/create", Store);
        await Send(luis, HttpMethod.Post, "businesses/create", Vet);
        using var created = await Send(ana, HttpMethod.Post, "services/create", "{\"name\":\"Baño\"}");
        using var createdDoc = JsonDocument.Parse(await created.Content.ReadAsStringAsync());
        var id = createdDoc.RootElement.GetProperty("data").GetProperty("id").GetString();

        await AssertWrapper(await Send(luis, HttpMethod.Delete, $"services/delete/{id}"), HttpStatusCode.NotFound);
        Assert.Single(factory.BusinessServices.All);
        await AssertWrapper(await Send(ana, HttpMethod.Delete, $"services/delete/{id}"), HttpStatusCode.OK);
        await AssertWrapper(await Send(ana, HttpMethod.Delete, $"services/delete/{id}"), HttpStatusCode.NotFound);
        await AssertWrapper(await Send(factory.CreateClient(), HttpMethod.Delete, $"services/delete/{id}"), HttpStatusCode.Unauthorized);
        Assert.Empty(factory.BusinessServices.All);
    }

    [Trait("Endpoint", "services-create")]
    [Fact]
    public async Task Business_routes_apply_the_per_user_rate_limit_with_429()
    {
        using var factory = new AuthFlowFactory(businessRateLimitPerMinute: 2);
        using var client = await Owner(factory, "ana@example.com");

        await Send(client, HttpMethod.Get, "services/get-all");
        await Send(client, HttpMethod.Get, "services/get-all");
        using var limited = await Send(client, HttpMethod.Get, "services/get-all");

        Assert.Equal(HttpStatusCode.TooManyRequests, limited.StatusCode);
        Assert.True(limited.Headers.Contains("Retry-After"));
    }

    // ---------- helpers ----------

    private static async Task<(string Access, string Refresh)> Register(HttpClient client, string email)
    {
        using var response = await Send(client, HttpMethod.Post, "auth/register", $"{{\"displayName\":\"Ana\",\"email\":\"{email}\",\"password\":\"contrasena-segura-123\"}}");
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var data = document.RootElement.GetProperty("data");
        return (data.GetProperty("accessToken").GetString()!, data.GetProperty("refreshToken").GetString()!);
    }

    private static HttpClient Authed(AuthFlowFactory factory, string access)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", access);
        return client;
    }

    private static async Task<HttpClient> Owner(AuthFlowFactory factory, string email)
    {
        using var anonymous = factory.CreateClient();
        var (access, _) = await Register(anonymous, email);
        return Authed(factory, access);
    }

    private static async Task<HttpResponseMessage> Send(HttpClient client, HttpMethod method, string route, string? body = null)
    {
        using var request = new HttpRequestMessage(method, $"/api/v1/{route}");
        if (body is not null)
        {
            request.Content = new StringContent(body, Encoding.UTF8, "application/json");
        }

        return await client.SendAsync(request);
    }

    private static async Task AssertWrapper(HttpResponseMessage response, HttpStatusCode expected)
    {
        using (response)
        {
            var raw = await response.Content.ReadAsStringAsync();
            Assert.Equal(expected, response.StatusCode);
            using var document = JsonDocument.Parse(raw);
            Assert.Equal(["data", "message", "statusCode"], document.RootElement.EnumerateObject().Select(p => p.Name).Order(StringComparer.Ordinal));
            Assert.Equal((int)expected, document.RootElement.GetProperty("statusCode").GetInt32());
        }
    }

    private static async Task AssertFieldError(HttpResponseMessage response, string field)
    {
        using (response)
        {
            var raw = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            using var document = JsonDocument.Parse(raw);
            Assert.Contains(document.RootElement.GetProperty("data").GetProperty("errors").EnumerateArray(), e => e.GetProperty("field").GetString() == field);
        }
    }
}
