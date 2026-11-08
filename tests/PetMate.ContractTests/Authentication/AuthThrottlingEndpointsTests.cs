using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using PetMate.Application.Security;
using PetMate.ContractTests.Support;
using PetMate.Domain.Users;

namespace PetMate.ContractTests.Authentication;

/// <summary>
/// Cuota por correo y bloqueo temporal en Login (pm-034) y límite por usuario del perfil (pm-036 CA-03): respuestas 429 con
/// el Response Wrapper y <c>Retry-After</c>, sin revelar si el correo existe.
/// </summary>
[Collection(ApiEnvironmentGroup.Name)]
public sealed class AuthThrottlingEndpointsTests
{
    private const string Password = "contraseña-muy-segura-123";

    [Trait("Endpoint", "auth-login")]
    [Fact]
    public async Task Login_locks_an_email_after_five_failures_with_429_wrapper_and_retry_after()
    {
        using var factory = new AuthFlowFactory();
        using var client = factory.CreateClient();
        using var registered = await Post(client, "register", new { email = "ana@example.com", password = Password });
        Assert.Equal(HttpStatusCode.Created, registered.StatusCode);

        for (var i = 0; i < 5; i++)
        {
            using var failure = await Post(client, "login", new { email = "ana@example.com", password = "otra-contraseña-123" });
            Assert.Equal(HttpStatusCode.Unauthorized, failure.StatusCode);
        }

        using var locked = await Post(client, "login", new { email = "ana@example.com", password = "otra-contraseña-123" });
        using var rightPassword = await Post(client, "login", new { email = "ana@example.com", password = Password });
        using var document = JsonDocument.Parse(await rightPassword.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.TooManyRequests, locked.StatusCode);
        Assert.Equal(HttpStatusCode.TooManyRequests, rightPassword.StatusCode);
        Assert.True(int.Parse(rightPassword.Headers.GetValues("Retry-After").Single(), System.Globalization.CultureInfo.InvariantCulture) > 14 * 60);
        Assert.Equal(429, document.RootElement.GetProperty("statusCode").GetInt32());
        Assert.Equal(JsonValueKind.Null, document.RootElement.GetProperty("data").ValueKind);
        Assert.Equal(["data", "message", "statusCode"], document.RootElement.EnumerateObject().Select(p => p.Name).Order(StringComparer.Ordinal));
    }

    [Trait("Endpoint", "auth-login")]
    [Fact]
    public async Task Login_lock_looks_the_same_for_an_unknown_email()
    {
        using var factory = new AuthFlowFactory();
        using var client = factory.CreateClient();
        using var registered = await Post(client, "register", new { email = "ana@example.com", password = Password });

        var known = new List<HttpStatusCode>();
        var unknown = new List<HttpStatusCode>();
        for (var i = 0; i < 8; i++)
        {
            using var a = await Post(client, "login", new { email = "ana@example.com", password = "otra-contraseña-123" });
            using var b = await Post(client, "login", new { email = "nadie@example.com", password = "otra-contraseña-123" });
            known.Add(a.StatusCode);
            unknown.Add(b.StatusCode);
        }

        Assert.Equal(known, unknown);
        Assert.Equal(HttpStatusCode.TooManyRequests, known[^1]);
    }

    [Trait("Endpoint", "auth-login")]
    [Fact]
    public async Task Login_lock_of_one_email_does_not_affect_another_account()
    {
        using var factory = new AuthFlowFactory();
        using var client = factory.CreateClient();
        using var ana = await Post(client, "register", new { email = "ana@example.com", password = Password });
        using var luis = await Post(client, "register", new { email = "luis@example.com", password = Password });
        for (var i = 0; i < 6; i++)
        {
            using var failure = await Post(client, "login", new { email = "ana@example.com", password = "otra-contraseña-123" });
        }

        using var ok = await Post(client, "login", new { email = "luis@example.com", password = Password });

        Assert.Equal(HttpStatusCode.OK, ok.StatusCode);
    }

    [Trait("Endpoint", "auth-login")]
    [Fact]
    public async Task Login_fails_closed_with_503_when_the_attempt_store_is_down()
    {
        using var factory = new AuthFlowFactory();
        using var client = factory.CreateClient();
        using var registered = await Post(client, "register", new { email = "ana@example.com", password = Password });
        factory.Throttle.Fail = true;

        using var response = await Post(client, "login", new { email = "ana@example.com", password = Password });

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }

    [Trait("Endpoint", "users-me-get")]
    [Fact]
    public async Task Profile_endpoints_are_throttled_per_user_with_429_wrapper_and_retry_after()
    {
        using var factory = new AuthFlowFactory(usersRateLimitPerMinute: 3);
        using var ana = Client(factory, Guid.NewGuid());
        using var luis = Client(factory, Guid.NewGuid());

        for (var i = 0; i < 3; i++)
        {
            using var response = await ana.GetAsync("/api/v1/users/me/get");
            Assert.NotEqual(HttpStatusCode.TooManyRequests, response.StatusCode);
        }

        using var throttled = await ana.GetAsync("/api/v1/users/me/get");
        using var document = JsonDocument.Parse(await throttled.Content.ReadAsStringAsync());
        using var other = await luis.GetAsync("/api/v1/users/me/get");

        Assert.Equal(HttpStatusCode.TooManyRequests, throttled.StatusCode);
        Assert.Equal(429, document.RootElement.GetProperty("statusCode").GetInt32());
        Assert.True(throttled.Headers.Contains("Retry-After"));
        Assert.NotEqual(HttpStatusCode.TooManyRequests, other.StatusCode);
    }

    private static HttpClient Client(AuthFlowFactory factory, Guid userId)
    {
        var token = factory.Services.GetRequiredService<IAccessTokenService>().Issue(userId, UserRole.Owner);
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Value);
        return client;
    }

    private static Task<HttpResponseMessage> Post(HttpClient client, string action, object body)
        => client.PostAsync($"/api/v1/auth/{action}", new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json"));
}
