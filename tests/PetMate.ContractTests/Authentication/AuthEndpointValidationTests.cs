using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using PetMate.ContractTests.Support;

namespace PetMate.ContractTests.Authentication;

/// <summary>
/// Validación de entrada de los endpoints de autenticación (RegisterCommandValidator, LoginCommandValidator, RefreshCommandValidator
/// y LogoutCommandValidator): cada uno responde 400 con el Response Wrapper y el campo inválido. Complementa a AuthEndpointsTests.
/// </summary>
[Collection(ApiEnvironmentGroup.Name)]
public sealed class AuthEndpointValidationTests
{
    private const string StrongPhrase = "contrasena-segura-1234";

    [Trait("Endpoint", "auth-login")]
    [Theory]
    [InlineData("{\"email\":\"malo\",\"password\":\"x\"}", "email")]
    [InlineData("{\"email\":\"ana@example.com\",\"password\":\"\"}", "password")]
    [InlineData("{\"email\":\"ana@example.com\",\"password\":\"   \"}", "password")]
    public async Task Login_with_an_invalid_field_returns_400_wrapper_naming_the_field(string body, string field)
    {
        using var factory = new AuthFlowFactory();
        using var client = factory.CreateClient();

        using var response = await PostRaw(client, "login", body);

        await AssertValidationError(response, field);
    }

    [Trait("Endpoint", "auth-login")]
    [Theory]
    [InlineData("")]
    [InlineData("{}")]
    [InlineData("no-es-json")]
    [InlineData("{\"email\":\"ana@example.com\",\"password\":\"x\",\"role\":\"Admin\"}")]
    public async Task Login_with_an_empty_malformed_or_extended_body_returns_400_wrapper(string body)
    {
        using var factory = new AuthFlowFactory();
        using var client = factory.CreateClient();

        using var response = await PostRaw(client, "login", body);

        await AssertBadRequestWrapper(response);
    }

    [Trait("Endpoint", "auth-refresh")]
    [Theory]
    [InlineData("{}")]
    [InlineData("{\"refreshToken\":\"\"}")]
    [InlineData("{\"refreshToken\":\"   \"}")]
    public async Task Refresh_without_a_usable_token_returns_400_wrapper_naming_the_field(string body)
    {
        using var factory = new AuthFlowFactory();
        using var client = factory.CreateClient();

        using var response = await PostRaw(client, "refresh", body);

        await AssertValidationError(response, "refreshToken");
    }

    [Trait("Endpoint", "auth-refresh")]
    [Theory]
    [InlineData("")]
    [InlineData("no-es-json")]
    [InlineData("{\"refreshToken\":\"x\",\"role\":\"Admin\"}")]
    public async Task Refresh_with_an_empty_malformed_or_extended_body_returns_400_wrapper(string body)
    {
        using var factory = new AuthFlowFactory();
        using var client = factory.CreateClient();

        using var response = await PostRaw(client, "refresh", body);

        await AssertBadRequestWrapper(response);
    }

    [Trait("Endpoint", "auth-logout")]
    [Theory]
    [InlineData("{\"refreshToken\":\"\"}")]
    [InlineData("{\"refreshToken\":\"   \"}")]
    public async Task Logout_with_a_blank_refresh_token_returns_400_wrapper_naming_the_field(string body)
    {
        using var factory = new AuthFlowFactory();
        using var client = factory.CreateClient();
        var access = await RegisterAndGetAccess(client, "ana@example.com");

        using var response = await PostRaw(client, "logout", body, access);

        await AssertValidationError(response, "refreshToken");
    }

    [Trait("Endpoint", "auth-logout")]
    [Theory]
    [InlineData("")]
    [InlineData("no-es-json")]
    public async Task Logout_with_an_empty_or_malformed_body_returns_400_wrapper(string body)
    {
        using var factory = new AuthFlowFactory();
        using var client = factory.CreateClient();
        var access = await RegisterAndGetAccess(client, "ana@example.com");

        using var response = await PostRaw(client, "logout", body, access);

        await AssertBadRequestWrapper(response);
    }

    [Trait("Endpoint", "auth-register")]
    [Theory]
    [InlineData(11, HttpStatusCode.BadRequest)]
    [InlineData(12, HttpStatusCode.Created)]
    [InlineData(128, HttpStatusCode.Created)]
    [InlineData(129, HttpStatusCode.BadRequest)]
    public async Task Register_enforces_the_password_length_boundaries_of_12_to_128(int length, HttpStatusCode expected)
    {
        using var factory = new AuthFlowFactory();
        using var client = factory.CreateClient();
        var body = JsonSerializer.Serialize(new { email = "ana@example.com", password = new string('a', length) });

        using var response = await PostRaw(client, "register", body);

        Assert.Equal(expected, response.StatusCode);
        if (expected == HttpStatusCode.BadRequest)
        {
            await AssertValidationError(response, "password");
            Assert.Empty(factory.Users.All);
        }
    }

    private static async Task AssertBadRequestWrapper(HttpResponseMessage response)
    {
        var raw = await response.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        using var document = JsonDocument.Parse(raw);
        Assert.Equal(["data", "message", "statusCode"], document.RootElement.EnumerateObject().Select(p => p.Name).Order(StringComparer.Ordinal));
        Assert.Equal(400, document.RootElement.GetProperty("statusCode").GetInt32());
    }

    private static async Task AssertValidationError(HttpResponseMessage response, string field)
    {
        var raw = await response.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        using var document = JsonDocument.Parse(raw);
        Assert.Equal(400, document.RootElement.GetProperty("statusCode").GetInt32());
        var errors = document.RootElement.GetProperty("data").GetProperty("errors");
        Assert.Contains(errors.EnumerateArray(), error => error.GetProperty("field").GetString() == field);
        Assert.DoesNotContain(StrongPhrase, raw, StringComparison.Ordinal);
    }

    private static async Task<HttpResponseMessage> PostRaw(HttpClient client, string action, string body, string? bearer = null)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/auth/{action}")
        {
            Content = new StringContent(body, System.Text.Encoding.UTF8, "application/json"),
        };
        if (bearer is not null)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearer);
        }

        return await client.SendAsync(request);
    }

    private static async Task<string> RegisterAndGetAccess(HttpClient client, string email)
    {
        using var response = await PostRaw(client, "register", JsonSerializer.Serialize(new { email, password = StrongPhrase }));
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return document.RootElement.GetProperty("data").GetProperty("accessToken").GetString()!;
    }
}
