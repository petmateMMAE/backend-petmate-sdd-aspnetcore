using System.Net;
using System.Text;
using System.Text.Json;
using PetMate.ContractTests.Authentication;
using PetMate.ContractTests.Support;

namespace PetMate.ContractTests.Accounts;

/// <summary>Contrato HTTP de la eliminación de cuenta por la web (pm-164, pm-165): enlace por correo de un solo uso, sin contraseña.</summary>
[Collection(ApiEnvironmentGroup.Name)]
public sealed class WebAccountDeletionEndpointsTests
{
    private const string Password = "contrasena-segura-123";

    [Trait("Endpoint", "account-deletion-request")]
    [Fact]
    public async Task Request_answers_the_same_for_known_and_unknown_emails_and_only_the_known_one_gets_a_link()
    {
        using var factory = new AuthFlowFactory();
        using var client = factory.CreateClient();
        await Post(client, "auth/register", "{\"displayName\":\"Ana\",\"email\":\"ana@example.com\",\"password\":\"" + Password + "\"}");

        using var known = await Post(client, "account-deletion/request", "{\"email\":\"ana@example.com\"}");
        using var unknown = await Post(client, "account-deletion/request", "{\"email\":\"nadie@example.com\"}");
        var knownBody = await known.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, known.StatusCode);
        Assert.Equal(knownBody, await unknown.Content.ReadAsStringAsync());
        Assert.Equal("ana@example.com", Assert.Single(factory.Email.DeletionSent).To);
        Assert.Empty(factory.Email.Sent);
        Assert.DoesNotContain(factory.Email.DeletionSent[0].Token, knownBody, StringComparison.Ordinal);
        Assert.Single(factory.Users.All);
    }

    [Trait("Endpoint", "account-deletion-request")]
    [Theory]
    [InlineData("{\"email\":\"malo\"}")]
    [InlineData("{}")]
    [InlineData("{\"email\":\"ana@example.com\",\"role\":\"User\"}")]
    public async Task Request_with_an_invalid_body_returns_400_and_sends_nothing(string body)
    {
        using var factory = new AuthFlowFactory();
        using var client = factory.CreateClient();

        using var response = await Post(client, "account-deletion/request", body);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Empty(factory.Email.DeletionSent);
    }

    [Trait("Endpoint", "account-deletion-request")]
    [Fact]
    public async Task Request_is_limited_per_email_with_429_and_retry_after()
    {
        using var factory = new AuthFlowFactory();
        using var client = factory.CreateClient();

        for (var i = 0; i < 3; i++)
        {
            using var ok = await Post(client, "account-deletion/request", "{\"email\":\"ana@example.com\"}");
            Assert.Equal(HttpStatusCode.OK, ok.StatusCode);
        }

        using var limited = await Post(client, "account-deletion/request", "{\"email\":\"ana@example.com\"}");

        Assert.Equal(HttpStatusCode.TooManyRequests, limited.StatusCode);
        Assert.True(limited.Headers.Contains("Retry-After"));
    }

    [Trait("Endpoint", "account-deletion-request")]
    [Fact]
    public async Task A_provider_failure_or_a_database_failure_is_not_revealed()
    {
        using var factory = new AuthFlowFactory();
        using var client = factory.CreateClient();
        await Post(client, "auth/register", "{\"displayName\":\"Ana\",\"email\":\"ana@example.com\",\"password\":\"" + Password + "\"}");
        factory.Email.Succeeds = false;

        using var response = await Post(client, "account-deletion/request", "{\"email\":\"ana@example.com\"}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Trait("Endpoint", "account-deletion-confirm")]
    [Fact]
    public async Task Confirm_deletes_the_account_with_everything_once_and_closes_every_session()
    {
        using var factory = new AuthFlowFactory();
        using var client = factory.CreateClient();
        var refresh = await Register(client, "ana@example.com");
        using var pet = await Authorized(factory, client, "ana@example.com");
        await Post(client, "account-deletion/request", "{\"email\":\"ana@example.com\"}");
        var token = factory.Email.DeletionSent.Single().Token;

        using var first = await Post(client, "account-deletion/confirm", $"{{\"token\":\"{token}\"}}");
        using var replay = await Post(client, "account-deletion/confirm", $"{{\"token\":\"{token}\"}}");
        using var refreshed = await Post(client, "auth/refresh", $"{{\"refreshToken\":\"{refresh}\"}}");
        using var login = await Post(client, "auth/login", $"{{\"email\":\"ana@example.com\",\"password\":\"{Password}\"}}");

        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Empty(factory.Users.All);
        Assert.Empty(factory.Pets.All);
        Assert.Equal(HttpStatusCode.BadRequest, replay.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, refreshed.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, login.StatusCode);
        Assert.Equal(0, factory.AccountDeletions.Count);
    }

    [Trait("Endpoint", "account-deletion-confirm")]
    [Theory]
    [InlineData("token-inexistente-que-no-existe-0123456789")]
    [InlineData("corto")]
    public async Task Confirm_with_an_unknown_token_returns_the_same_400_and_deletes_nothing(string token)
    {
        using var factory = new AuthFlowFactory();
        using var client = factory.CreateClient();
        await Register(client, "ana@example.com");

        using var response = await Post(client, "account-deletion/confirm", $"{{\"token\":\"{token}\"}}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Single(factory.Users.All);
    }

    [Trait("Endpoint", "account-deletion-confirm")]
    [Theory]
    [InlineData("{}")]
    [InlineData("{\"token\":\"\"}")]
    [InlineData("{\"token\":\"x\",\"userId\":\"1\"}")]
    public async Task Confirm_with_an_invalid_body_returns_400(string body)
    {
        using var factory = new AuthFlowFactory();
        using var client = factory.CreateClient();

        using var response = await Post(client, "account-deletion/confirm", body);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Trait("Endpoint", "account-deletion-confirm")]
    [Fact]
    public async Task A_password_reset_token_does_not_delete_the_account()
    {
        using var factory = new AuthFlowFactory();
        using var client = factory.CreateClient();
        await Register(client, "ana@example.com");
        await Post(client, "auth/forgot-password", "{\"email\":\"ana@example.com\"}");
        var resetToken = factory.Email.Sent.Single().Token;

        using var response = await Post(client, "account-deletion/confirm", $"{{\"token\":\"{resetToken}\"}}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Single(factory.Users.All);
    }

    private static async Task<string> Register(HttpClient client, string email)
    {
        using var response = await Post(client, "auth/register", $"{{\"displayName\":\"Ana\",\"email\":\"{email}\",\"password\":\"{Password}\"}}");
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return document.RootElement.GetProperty("data").GetProperty("refreshToken").GetString()!;
    }

    // Crea una mascota con la sesión de la cuenta para comprobar que la eliminación también la borra.
    private static async Task<HttpResponseMessage> Authorized(AuthFlowFactory factory, HttpClient anonymous, string email)
    {
        using var login = await Post(anonymous, "auth/login", $"{{\"email\":\"{email}\",\"password\":\"{Password}\"}}");
        using var document = JsonDocument.Parse(await login.Content.ReadAsStringAsync());
        using var authed = factory.CreateClient();
        authed.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", document.RootElement.GetProperty("data").GetProperty("accessToken").GetString());
        return await Post(authed, "pets/create", "{\"name\":\"Firulais\",\"species\":\"Perro\"}");
    }

    private static async Task<HttpResponseMessage> Post(HttpClient client, string route, string body)
    {
        using var content = new StringContent(body, Encoding.UTF8, "application/json");
        return await client.PostAsync($"/api/v1/{route}", content);
    }
}
