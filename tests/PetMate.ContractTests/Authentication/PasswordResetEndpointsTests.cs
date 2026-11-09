using System.Net;
using System.Text;
using System.Text.Json;
using PetMate.ContractTests.Support;

namespace PetMate.ContractTests.Authentication;

/// <summary>Contrato HTTP de la recuperación de contraseña (pm-089, pm-090) con correo y almacenamiento simulados.</summary>
[Collection(ApiEnvironmentGroup.Name)]
public sealed class PasswordResetEndpointsTests
{
    private const string OldPassword = "contrasena-vieja-123";
    private const string NewPassword = "contrasena-nueva-456";

    [Trait("Endpoint", "auth-forgot-password")]
    [Fact]
    public async Task Forgot_answers_200_with_the_same_message_for_known_and_unknown_emails()
    {
        using var factory = new AuthFlowFactory();
        using var client = factory.CreateClient();
        await Post(client, "register", new { displayName = "Ana", email = "ana@example.com", password = OldPassword });

        using var known = await Post(client, "forgot-password", new { email = "ana@example.com" });
        using var unknown = await Post(client, "forgot-password", new { email = "nadie@example.com" });
        var knownBody = await known.Content.ReadAsStringAsync();
        var unknownBody = await unknown.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, known.StatusCode);
        Assert.Equal(knownBody, unknownBody);
        Assert.Equal("ana@example.com", Assert.Single(factory.Email.Sent).To);
        Assert.DoesNotContain(factory.Email.Sent[0].Token, knownBody, StringComparison.Ordinal);
    }

    [Trait("Endpoint", "auth-forgot-password")]
    [Theory]
    [InlineData("{\"email\":\"malo\"}")]
    [InlineData("{}")]
    [InlineData("{\"email\":\"ana@example.com\",\"role\":\"User\"}")]
    public async Task Forgot_with_an_invalid_body_returns_400_and_sends_nothing(string body)
    {
        using var factory = new AuthFlowFactory();
        using var client = factory.CreateClient();

        using var response = await PostRaw(client, "forgot-password", body);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Empty(factory.Email.Sent);
    }

    [Trait("Endpoint", "auth-forgot-password")]
    [Fact]
    public async Task Forgot_is_limited_per_email_with_429_and_retry_after()
    {
        using var factory = new AuthFlowFactory();
        using var client = factory.CreateClient();

        for (var i = 0; i < 3; i++)
        {
            using var ok = await Post(client, "forgot-password", new { email = "ana@example.com" });
            Assert.Equal(HttpStatusCode.OK, ok.StatusCode);
        }

        using var limited = await Post(client, "forgot-password", new { email = "ana@example.com" });

        Assert.Equal(HttpStatusCode.TooManyRequests, limited.StatusCode);
        Assert.True(limited.Headers.Contains("Retry-After"));
    }

    [Trait("Endpoint", "auth-forgot-password")]
    [Fact]
    public async Task A_provider_outage_does_not_change_the_response()
    {
        using var factory = new AuthFlowFactory();
        using var client = factory.CreateClient();
        await Post(client, "register", new { displayName = "Ana", email = "ana@example.com", password = OldPassword });
        factory.Email.Succeeds = false;

        using var response = await Post(client, "forgot-password", new { email = "ana@example.com" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Trait("Endpoint", "auth-reset-password")]
    [Fact]
    public async Task Reset_sets_the_new_password_once_and_closes_every_session()
    {
        using var factory = new AuthFlowFactory();
        using var client = factory.CreateClient();
        using var registered = await Post(client, "register", new { displayName = "Ana", email = "ana@example.com", password = OldPassword });
        using var registeredDoc = JsonDocument.Parse(await registered.Content.ReadAsStringAsync());
        var refresh = registeredDoc.RootElement.GetProperty("data").GetProperty("refreshToken").GetString();
        await Post(client, "forgot-password", new { email = "ana@example.com" });
        var token = factory.Email.Sent.Single().Token;

        using var reset = await Post(client, "reset-password", new { token, newPassword = NewPassword });
        using var reused = await Post(client, "reset-password", new { token, newPassword = NewPassword });
        using var oldLogin = await Post(client, "login", new { email = "ana@example.com", password = OldPassword });
        using var newLogin = await Post(client, "login", new { email = "ana@example.com", password = NewPassword });
        using var oldRefresh = await Post(client, "refresh", new { refreshToken = refresh });

        Assert.Equal(HttpStatusCode.OK, reset.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, reused.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, oldLogin.StatusCode);
        Assert.Equal(HttpStatusCode.OK, newLogin.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, oldRefresh.StatusCode);
    }

    [Trait("Endpoint", "auth-reset-password")]
    [Theory]
    [InlineData("{\"token\":\"no-existe\",\"newPassword\":\"contrasena-nueva-456\"}")]
    [InlineData("{\"token\":\"abc\",\"newPassword\":\"corta\"}")]
    [InlineData("{\"token\":\"\",\"newPassword\":\"contrasena-nueva-456\"}")]
    [InlineData("{\"token\":\"abc\",\"newPassword\":\"contrasena-nueva-456\",\"email\":\"a@b.co\"}")]
    public async Task Reset_with_an_invalid_token_or_body_returns_400_with_the_wrapper(string body)
    {
        using var factory = new AuthFlowFactory();
        using var client = factory.CreateClient();

        using var response = await PostRaw(client, "reset-password", body);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(400, document.RootElement.GetProperty("statusCode").GetInt32());
    }

    private static Task<HttpResponseMessage> Post(HttpClient client, string action, object body)
        => PostRaw(client, action, JsonSerializer.Serialize(body));

    private static async Task<HttpResponseMessage> PostRaw(HttpClient client, string action, string body)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/auth/{action}")
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json"),
        };
        return await client.SendAsync(request);
    }
}
