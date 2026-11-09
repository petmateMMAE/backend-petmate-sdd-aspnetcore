using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PetMate.Application.Security;
using PetMate.ContractTests.Support;
using PetMate.Domain.Users;
using PetMate.TestDoubles;

namespace PetMate.ContractTests.Authentication;

/// <summary>Endurecimiento de la autenticación: límite por IP compartido entre réplicas (H6), arranque con proxies (H6) y tokens ligados a la sesión (H7).</summary>
[Collection(ApiEnvironmentGroup.Name)]
public sealed class AuthHardeningTests
{
    private const string Body = "{\"email\":\"alguien@example.com\",\"password\":\"contraseña-muy-segura-123\"}";

    [Trait("Endpoint", "auth-login")]
    [Fact]
    public async Task The_ip_limit_is_shared_across_replicas_through_redis_auth()
    {
        var shared = new InMemoryAuthAttemptThrottle(new ManualClock(DateTimeOffset.UtcNow));
        using var replicaA = new ReplicaFactory(shared);
        using var replicaB = new ReplicaFactory(shared);
        using var a = replicaA.CreateClient();
        using var b = replicaB.CreateClient();

        // Límite de 3 por minuto: cada réplica por separado vería 2 solicitudes, pero la IP ya hizo 3 en total.
        Assert.Equal(HttpStatusCode.Unauthorized, (await Login(a)).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await Login(a)).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await Login(b)).StatusCode);

        using var blocked = await Login(b);
        using var document = JsonDocument.Parse(await blocked.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.TooManyRequests, blocked.StatusCode);
        Assert.True(blocked.Headers.Contains("Retry-After"));
        Assert.Equal(429, document.RootElement.GetProperty("statusCode").GetInt32());
        Assert.Equal(JsonValueKind.Null, document.RootElement.GetProperty("data").ValueKind);
    }

    [Trait("Endpoint", "auth-login")]
    [Fact]
    public async Task Authentication_routes_fail_closed_with_503_when_the_shared_limiter_cannot_reach_redis()
    {
        using var factory = new IsolatedApiFactory("Development");
        using var client = factory.CreateClient();

        using var response = await client.PostAsync("/api/v1/auth/login", new StringContent(Body, Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }

    [Trait("Endpoint", "transversal")]
    [Fact]
    public void Production_can_require_trusted_proxies_and_starts_when_they_are_configured()
    {
        using var strict = new IsolatedApiFactory("Production", new Dictionary<string, string> { ["REQUIRE_TRUSTED_PROXIES"] = "true", ["TRUSTED_PROXIES"] = string.Empty });
        Assert.ThrowsAny<InvalidOperationException>(() => strict.CreateClient());

        using var configured = new IsolatedApiFactory("Production", new Dictionary<string, string> { ["REQUIRE_TRUSTED_PROXIES"] = "true", ["TRUSTED_PROXIES"] = "10.0.0.5" });
        using var client = configured.CreateClient();
        Assert.NotNull(client);
    }

    [Trait("Endpoint", "pets-get-all")]
    [Fact]
    public async Task An_access_token_bound_to_a_session_stops_working_when_the_session_is_revoked_but_legacy_tokens_keep_working()
    {
        using var factory = new AuthFlowFactory();
        var tokens = factory.Services.GetRequiredService<IAccessTokenService>();
        var store = factory.Services.GetRequiredService<ITokenRevocationStore>();
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var bound = tokens.Issue(userId, UserRole.User, sessionId);
        var legacy = tokens.Issue(userId, UserRole.User);
        using var client = factory.CreateClient();

        Assert.Equal(HttpStatusCode.OK, await Get(client, bound.Value));
        Assert.Equal(HttpStatusCode.OK, await Get(client, legacy.Value));

        await store.RevokeSession(sessionId, DateTime.UtcNow.AddMinutes(15), CancellationToken.None);

        Assert.Equal(HttpStatusCode.Unauthorized, await Get(client, bound.Value));
        Assert.Equal(HttpStatusCode.OK, await Get(client, legacy.Value)); // sin sid: vale hasta expirar (compatibilidad)
    }

    private static async Task<HttpStatusCode> Get(HttpClient client, string token)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/pets/get-all");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        using var response = await client.SendAsync(request);
        return response.StatusCode;
    }

    private static Task<HttpResponseMessage> Login(HttpClient client)
        => client.PostAsync("/api/v1/auth/login", new StringContent(Body, Encoding.UTF8, "application/json"));

    private sealed class ReplicaFactory(IAuthAttemptThrottle sharedThrottle) : AuthFlowFactory(rateLimitPerMinute: 3)
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            base.ConfigureWebHost(builder);
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IAuthAttemptThrottle>();
                services.AddSingleton(sharedThrottle);
            });
        }
    }
}
