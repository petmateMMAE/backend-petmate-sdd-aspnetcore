using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using PetMate.ContractTests.Authentication;
using PetMate.ContractTests.Support;

namespace PetMate.ContractTests.Users;

/// <summary>
/// pm-037 / pm-038: contrato HTTP del perfil propio con JWT real y repositorios en memoria.
/// La identidad sale del token; nunca se puede leer ni cambiar el perfil de otra cuenta.
/// </summary>
[Collection(ApiEnvironmentGroup.Name)]
public sealed class UserProfileEndpointsTests
{
    private const string Password = "contrasena-segura-1234";

    [Trait("Endpoint", "users-me-get")]
    [Trait("Endpoint", "users-me-update")]
    [Theory]
    [InlineData("GET", "me/get", null)]
    [InlineData("PUT", "me/update", "{\"displayName\":\"Ana\"}")]
    public async Task Anonymous_requests_get_401_wrapper(string method, string action, string? body)
    {
        using var factory = new AuthFlowFactory();
        using var client = factory.CreateClient();

        using var response = await Send(client, method, action, body, bearer: null);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal(401, document.RootElement.GetProperty("statusCode").GetInt32());
        Assert.Equal(JsonValueKind.Null, document.RootElement.GetProperty("data").ValueKind);
    }

    [Trait("Endpoint", "users-me-get")]
    [Fact]
    public async Task Get_returns_own_profile_without_secrets_and_with_the_registration_name()
    {
        using var factory = new AuthFlowFactory();
        using var client = factory.CreateClient();
        var access = await RegisterAndGetAccess(client, "Ana@Example.com");

        using var response = await Send(client, "GET", "me/get", null, access);
        var raw = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(raw);
        var data = document.RootElement.GetProperty("data");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("ana@example.com", data.GetProperty("email").GetString());
        Assert.Equal("User", data.GetProperty("role").GetString());
        Assert.Equal("Ana", data.GetProperty("displayName").GetString());
        Assert.DoesNotContain("passwordHash", raw, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(Password, raw, StringComparison.Ordinal);
    }

    [Trait("Endpoint", "users-me-update")]
    [Fact]
    public async Task Update_sets_trimmed_name_and_get_returns_it()
    {
        using var factory = new AuthFlowFactory();
        using var client = factory.CreateClient();
        var access = await RegisterAndGetAccess(client, "ana@example.com");

        using var update = await Send(client, "PUT", "me/update", "{\"displayName\":\"  Ana Pérez  \"}", access);
        using var updated = JsonDocument.Parse(await update.Content.ReadAsStringAsync());
        using var read = await Send(client, "GET", "me/get", null, access);
        using var current = JsonDocument.Parse(await read.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, update.StatusCode);
        Assert.Equal("Ana Pérez", updated.RootElement.GetProperty("data").GetProperty("displayName").GetString());
        Assert.Equal("Ana Pérez", current.RootElement.GetProperty("data").GetProperty("displayName").GetString());
    }

    [Trait("Endpoint", "users-me-update")]
    [Fact]
    public async Task Phone_is_optional_added_changed_removed_and_only_visible_to_its_owner()
    {
        using var factory = new AuthFlowFactory();
        using var client = factory.CreateClient();
        var ana = await RegisterAndGetAccess(client, "ana@example.com");
        var luis = await RegisterAndGetAccess(client, "luis@example.com");

        using var initial = await Send(client, "GET", "me/get", null, ana);
        using var added = await Send(client, "PUT", "me/update", "{\"displayName\":\"Ana\",\"phone\":\"+52 55 1234 5678\"}", ana);
        using var addedDoc = JsonDocument.Parse(await added.Content.ReadAsStringAsync());
        using var luisRead = await Send(client, "GET", "me/get", null, luis);
        using var removed = await Send(client, "PUT", "me/update", "{\"displayName\":\"Ana\"}", ana);
        using var removedDoc = JsonDocument.Parse(await removed.Content.ReadAsStringAsync());
        using var initialDoc = JsonDocument.Parse(await initial.Content.ReadAsStringAsync());
        using var luisDoc = JsonDocument.Parse(await luisRead.Content.ReadAsStringAsync());

        Assert.Equal(JsonValueKind.Null, initialDoc.RootElement.GetProperty("data").GetProperty("phone").ValueKind);
        Assert.Equal("5512345678", addedDoc.RootElement.GetProperty("data").GetProperty("phone").GetString());
        Assert.Equal(JsonValueKind.Null, luisDoc.RootElement.GetProperty("data").GetProperty("phone").ValueKind);
        Assert.Equal(JsonValueKind.Null, removedDoc.RootElement.GetProperty("data").GetProperty("phone").ValueKind);
    }

    [Trait("Endpoint", "users-me-update")]
    [Theory]
    [InlineData("12345")]
    [InlineData("55123456ab")]
    [InlineData("+34 612345678")]
    public async Task Update_with_an_invalid_phone_returns_400_naming_phone(string phone)
    {
        using var factory = new AuthFlowFactory();
        using var client = factory.CreateClient();
        var access = await RegisterAndGetAccess(client, "ana@example.com");

        using var response = await Send(client, "PUT", "me/update", $"{{\"displayName\":\"Ana\",\"phone\":\"{phone}\"}}", access);
        var raw = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("\"field\":\"phone\"", raw, StringComparison.Ordinal);
        Assert.DoesNotContain(phone, raw.Replace("\u002B", "+", StringComparison.Ordinal), StringComparison.Ordinal);
    }

    [Trait("Endpoint", "users-me-update")]
    [Theory]
    [InlineData("{\"displayName\":\"\"}")]
    [InlineData("{\"displayName\":\"   \"}")]
    [InlineData("{}")]
    public async Task Update_with_missing_or_blank_name_returns_400_with_the_field(string body)
    {
        using var factory = new AuthFlowFactory();
        using var client = factory.CreateClient();
        var access = await RegisterAndGetAccess(client, "ana@example.com");

        using var response = await Send(client, "PUT", "me/update", body, access);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errors = document.RootElement.GetProperty("data").GetProperty("errors");
        Assert.Equal("displayName", errors[0].GetProperty("field").GetString());
    }

    [Trait("Endpoint", "users-me-update")]
    [Fact]
    public async Task Update_with_a_name_longer_than_80_returns_400()
    {
        using var factory = new AuthFlowFactory();
        using var client = factory.CreateClient();
        var access = await RegisterAndGetAccess(client, "ana@example.com");

        using var response = await Send(client, "PUT", "me/update", $"{{\"displayName\":\"{new string('a', 81)}\"}}", access);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Trait("Endpoint", "users-me-update")]
    [Theory]
    [InlineData("{\"displayName\":\"Ana\",\"role\":\"Business\"}")]
    [InlineData("{\"displayName\":\"Ana\",\"email\":\"otra@example.com\"}")]
    public async Task Update_rejects_attempts_to_change_role_or_email(string body)
    {
        using var factory = new AuthFlowFactory();
        using var client = factory.CreateClient();
        var access = await RegisterAndGetAccess(client, "ana@example.com");

        using var response = await Send(client, "PUT", "me/update", body, access);
        var user = Assert.Single(factory.Users.All);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("ana@example.com", user.Email.Value);
        Assert.Equal("Ana", user.DisplayName);
    }

    [Trait("Endpoint", "users-me-get")]
    [Fact]
    public async Task Each_account_only_sees_its_own_profile()
    {
        using var factory = new AuthFlowFactory();
        using var client = factory.CreateClient();
        var ana = await RegisterAndGetAccess(client, "ana@example.com");
        var luis = await RegisterAndGetAccess(client, "luis@example.com");
        await Send(client, "PUT", "me/update", "{\"displayName\":\"Ana\"}", ana);

        using var response = await Send(client, "GET", "me/get", null, luis);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var data = document.RootElement.GetProperty("data");

        Assert.Equal("luis@example.com", data.GetProperty("email").GetString());
        Assert.Equal("Ana", data.GetProperty("displayName").GetString());
    }

    private static async Task<string> RegisterAndGetAccess(HttpClient client, string email)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/auth/register")
        {
            Content = new StringContent(JsonSerializer.Serialize(new { displayName = "Ana", email, password = Password }), System.Text.Encoding.UTF8, "application/json"),
        };
        using var response = await client.SendAsync(request);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return document.RootElement.GetProperty("data").GetProperty("accessToken").GetString()!;
    }

    private static async Task<HttpResponseMessage> Send(HttpClient client, string method, string action, string? body, string? bearer)
    {
        using var request = new HttpRequestMessage(new HttpMethod(method), $"/api/v1/users/{action}");
        if (body is not null)
        {
            request.Content = new StringContent(body, System.Text.Encoding.UTF8, "application/json");
        }

        if (bearer is not null)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearer);
        }

        return await client.SendAsync(request);
    }
}
