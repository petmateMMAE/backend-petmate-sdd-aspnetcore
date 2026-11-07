using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PetMate.Application.Persistence;
using PetMate.Application.Security;
using PetMate.ContractTests.Support;
using PetMate.TestDoubles;

namespace PetMate.ContractTests.Authentication;

/// <summary>
/// Contrato HTTP de Register, Login, Refresh y Logout con Argon2id, JWT y fábrica de tokens reales y
/// repositorios/lista negra en memoria (la persistencia real se prueba en IntegrationTests).
/// </summary>
[Collection(ApiEnvironmentGroup.Name)]
public sealed class AuthEndpointsTests
{
    private const string Password = "contraseña-muy-segura-123";

    // ---------- Register ----------

    [Trait("Endpoint", "auth-register")]
    [Fact]
    public async Task Register_returns_201_wrapper_with_owner_role_and_tokens_and_never_echoes_the_password()
    {
        using var factory = new AuthFlowFactory();
        using var client = factory.CreateClient();

        using var response = await Post(client, "register", new { email = "Ana@Example.com", password = Password });
        var raw = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        using var document = JsonDocument.Parse(raw);
        AssertWrapper(document, 201);
        var data = document.RootElement.GetProperty("data");
        Assert.Equal("Bearer", data.GetProperty("tokenType").GetString());
        Assert.Equal("Owner", data.GetProperty("user").GetProperty("role").GetString());
        Assert.Equal("ana@example.com", data.GetProperty("user").GetProperty("email").GetString());
        Assert.Equal(43, data.GetProperty("refreshToken").GetString()!.Length);
        Assert.DoesNotContain(Password, raw, StringComparison.Ordinal);
        Assert.DoesNotContain("passwordHash", raw, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("argon2id", raw, StringComparison.OrdinalIgnoreCase);
    }

    [Trait("Endpoint", "auth-register")]
    [Theory]
    [InlineData("{\"email\":\"ana@example.com\",\"password\":\"contraseña-muy-segura-123\",\"role\":\"Business\"}")]
    [InlineData("{\"email\":\"ana@example.com\",\"password\":\"contraseña-muy-segura-123\",\"role\":\"Admin\"}")]
    [InlineData("{\"email\":\"ana@example.com\",\"password\":\"contraseña-muy-segura-123\",\"isAdmin\":true}")]
    public async Task Register_rejects_any_attempt_to_send_a_role_or_extra_members_and_creates_nothing(string body)
    {
        using var factory = new AuthFlowFactory();
        using var client = factory.CreateClient();

        using var response = await PostRaw(client, "register", body);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        AssertWrapper(document, 400);
        Assert.Empty(factory.Users.All);
    }

    [Trait("Endpoint", "auth-register")]
    [Fact]
    public async Task Register_returns_400_with_all_validation_errors_in_spanish()
    {
        using var factory = new AuthFlowFactory();
        using var client = factory.CreateClient();

        using var response = await Post(client, "register", new { email = "malo", password = "corta" });
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errors = document.RootElement.GetProperty("data").GetProperty("errors");
        Assert.Equal(2, errors.GetArrayLength());
        Assert.Contains("correo", errors[0].GetProperty("message").GetString(), StringComparison.Ordinal);
        Assert.Empty(factory.Users.All);
    }

    [Trait("Endpoint", "auth-register")]
    [Theory]
    [InlineData("")]
    [InlineData("{}")]
    [InlineData("{\"email\":null,\"password\":null}")]
    [InlineData("no-es-json")]
    public async Task Register_with_empty_or_malformed_body_returns_400_wrapper(string body)
    {
        using var factory = new AuthFlowFactory();
        using var client = factory.CreateClient();

        using var response = await PostRaw(client, "register", body);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        AssertWrapper(document, 400);
    }

    [Trait("Endpoint", "auth-register")]
    [Fact]
    public async Task Register_duplicate_email_returns_409_with_a_generic_message_and_does_not_create_a_second_account()
    {
        using var factory = new AuthFlowFactory();
        using var client = factory.CreateClient();
        await Post(client, "register", new { email = "ana@example.com", password = Password });

        using var response = await Post(client, "register", new { email = "ANA@example.com", password = Password });
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        AssertWrapper(document, 409);
        Assert.Equal(JsonValueKind.Null, document.RootElement.GetProperty("data").ValueKind);
        Assert.Single(factory.Users.All);
    }

    // ---------- Login ----------

    [Trait("Endpoint", "auth-login")]
    [Fact]
    public async Task Login_succeeds_and_returns_a_working_access_token_with_the_owner_role()
    {
        using var factory = new AuthFlowFactory();
        using var client = factory.CreateClient();
        await Post(client, "register", new { email = "ana@example.com", password = Password });

        var tokens = await LoginAndRead(client, "ana@example.com", Password);

        var claims = await factory.Services.GetRequiredService<IAccessTokenService>().Validate(tokens.Access, CancellationToken.None);
        Assert.NotNull(claims);
        Assert.Equal(PetMate.Domain.Users.UserRole.Owner, claims.Role);
    }

    [Trait("Endpoint", "auth-login")]
    [Fact]
    public async Task Login_failures_for_unknown_email_and_wrong_password_return_the_same_401_body()
    {
        using var factory = new AuthFlowFactory();
        using var client = factory.CreateClient();
        await Post(client, "register", new { email = "ana@example.com", password = Password });

        using var wrongPassword = await Post(client, "login", new { email = "ana@example.com", password = "otra-contraseña-123" });
        using var unknown = await Post(client, "login", new { email = "nadie@example.com", password = Password });

        Assert.Equal(HttpStatusCode.Unauthorized, wrongPassword.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, unknown.StatusCode);
        Assert.Equal(await wrongPassword.Content.ReadAsStringAsync(), await unknown.Content.ReadAsStringAsync());
    }

    [Trait("Endpoint", "auth-login")]
    [Fact]
    public async Task Login_is_throttled_with_429_wrapper_and_retry_after_after_the_configured_limit()
    {
        using var factory = new AuthFlowFactory(rateLimitPerMinute: 3);
        using var client = factory.CreateClient();

        var statuses = new List<HttpStatusCode>();
        HttpResponseMessage? last = null;
        for (var attempt = 0; attempt < 5; attempt++)
        {
            last?.Dispose();
            last = await Post(client, "login", new { email = "nadie@example.com", password = Password });
            statuses.Add(last.StatusCode);
        }

        Assert.Equal(3, statuses.Count(status => status == HttpStatusCode.Unauthorized));
        Assert.Equal(2, statuses.Count(status => status == HttpStatusCode.TooManyRequests));
        Assert.True(last!.Headers.Contains("Retry-After"));
        using var document = JsonDocument.Parse(await last.Content.ReadAsStringAsync());
        AssertWrapper(document, 429);
        last.Dispose();
    }

    // ---------- Refresh ----------

    [Trait("Endpoint", "auth-refresh")]
    [Fact]
    public async Task Refresh_rotates_and_the_old_token_cannot_be_reused_which_revokes_the_whole_session()
    {
        using var factory = new AuthFlowFactory();
        using var client = factory.CreateClient();
        var first = await RegisterAndRead(client, "ana@example.com");

        using var refreshed = await Post(client, "refresh", new { refreshToken = first.Refresh });
        Assert.Equal(HttpStatusCode.OK, refreshed.StatusCode);
        var second = ReadTokens(await refreshed.Content.ReadAsStringAsync());
        Assert.NotEqual(first.Refresh, second.Refresh);

        using var replay = await Post(client, "refresh", new { refreshToken = first.Refresh });
        using var newestAfterReplay = await Post(client, "refresh", new { refreshToken = second.Refresh });

        Assert.Equal(HttpStatusCode.Unauthorized, replay.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, newestAfterReplay.StatusCode);
        using var document = JsonDocument.Parse(await replay.Content.ReadAsStringAsync());
        AssertWrapper(document, 401);
    }

    [Trait("Endpoint", "auth-refresh")]
    [Fact]
    public async Task Expired_access_token_is_rejected_but_the_session_renews_with_the_refresh_token()
    {
        using var factory = new AuthFlowFactory();
        using var client = factory.CreateClient();
        var tokens = await RegisterAndRead(client, "ana@example.com");
        factory.Clock.Advance(TimeSpan.FromMinutes(20));

        using var expired = await Logout(client, tokens.Access, tokens.Refresh);
        using var renewed = await Post(client, "refresh", new { refreshToken = tokens.Refresh });

        Assert.Equal(HttpStatusCode.Unauthorized, expired.StatusCode);
        Assert.Equal(HttpStatusCode.OK, renewed.StatusCode);
    }

    [Trait("Endpoint", "auth-refresh")]
    [Fact]
    public async Task Refresh_with_garbage_returns_401_wrapper()
    {
        using var factory = new AuthFlowFactory();
        using var client = factory.CreateClient();

        using var response = await Post(client, "refresh", new { refreshToken = "no-valido" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // ---------- Logout ----------

    [Trait("Endpoint", "auth-logout")]
    [Fact]
    public async Task Logout_requires_a_valid_access_token()
    {
        using var factory = new AuthFlowFactory();
        using var client = factory.CreateClient();

        using var response = await Post(client, "logout", new { refreshToken = new string('a', 43) });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Trait("Endpoint", "auth-logout")]
    [Fact]
    public async Task Logout_returns_200_with_null_data_then_both_tokens_stop_working()
    {
        using var factory = new AuthFlowFactory();
        using var client = factory.CreateClient();
        var tokens = await RegisterAndRead(client, "ana@example.com");

        using var logout = await Logout(client, tokens.Access, tokens.Refresh);
        using var document = JsonDocument.Parse(await logout.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, logout.StatusCode);
        AssertWrapper(document, 200);
        Assert.Equal(JsonValueKind.Null, document.RootElement.GetProperty("data").ValueKind);

        using var reuseAccess = await Logout(client, tokens.Access, tokens.Refresh);
        using var reuseRefresh = await Post(client, "refresh", new { refreshToken = tokens.Refresh });
        Assert.Equal(HttpStatusCode.Unauthorized, reuseAccess.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, reuseRefresh.StatusCode);
    }

    [Trait("Endpoint", "auth-logout")]
    [Fact]
    public async Task A_user_cannot_log_out_another_users_session_and_the_victim_keeps_working()
    {
        using var factory = new AuthFlowFactory();
        using var client = factory.CreateClient();
        var victim = await RegisterAndRead(client, "victima@example.com");
        var attacker = await RegisterAndRead(client, "intruso@example.com");

        using var attack = await Logout(client, attacker.Access, victim.Refresh);

        Assert.Equal(HttpStatusCode.Forbidden, attack.StatusCode);
        using var victimRefresh = await Post(client, "refresh", new { refreshToken = victim.Refresh });
        Assert.Equal(HttpStatusCode.OK, victimRefresh.StatusCode);
    }

    [Trait("Endpoint", "auth-logout")]
    [Fact]
    public async Task Logout_without_refresh_token_returns_400_and_with_redis_down_returns_503()
    {
        using var factory = new AuthFlowFactory();
        using var client = factory.CreateClient();
        var tokens = await RegisterAndRead(client, "ana@example.com");

        using var missing = await PostRaw(client, "logout", "{}", tokens.Access);
        Assert.Equal(HttpStatusCode.BadRequest, missing.StatusCode);

        factory.Revocations.Fail = true;
        using var down = await Logout(client, tokens.Access, tokens.Refresh);
        var raw = await down.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.ServiceUnavailable, down.StatusCode);
        Assert.DoesNotContain("detalle-interno", raw, StringComparison.Ordinal);
    }

    // ---------- helpers ----------

    private static void AssertWrapper(JsonDocument document, int statusCode)
    {
        Assert.Equal(["data", "message", "statusCode"], document.RootElement.EnumerateObject().Select(p => p.Name).Order(StringComparer.Ordinal));
        Assert.Equal(statusCode, document.RootElement.GetProperty("statusCode").GetInt32());
    }

    private static Task<HttpResponseMessage> Post(HttpClient client, string action, object body, string? bearer = null)
        => PostRaw(client, action, JsonSerializer.Serialize(body), bearer);

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

    private static Task<HttpResponseMessage> Logout(HttpClient client, string access, string refresh)
        => Post(client, "logout", new { refreshToken = refresh }, access);

    private static async Task<(string Access, string Refresh)> RegisterAndRead(HttpClient client, string email)
    {
        using var response = await Post(client, "register", new { email, password = Password });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return ReadTokens(await response.Content.ReadAsStringAsync());
    }

    private static async Task<(string Access, string Refresh)> LoginAndRead(HttpClient client, string email, string password)
    {
        using var response = await Post(client, "login", new { email, password });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return ReadTokens(await response.Content.ReadAsStringAsync());
    }

    private static (string Access, string Refresh) ReadTokens(string raw)
    {
        using var document = JsonDocument.Parse(raw);
        var data = document.RootElement.GetProperty("data");
        return (data.GetProperty("accessToken").GetString()!, data.GetProperty("refreshToken").GetString()!);
    }
}

public sealed class AuthFlowFactory : IsolatedApiFactory
{
    public AuthFlowFactory(int? rateLimitPerMinute = null)
        : base("Development", rateLimitPerMinute is null ? null : new Dictionary<string, string> { ["AUTH_RATE_LIMIT_PER_MINUTE"] = rateLimitPerMinute.Value.ToString(System.Globalization.CultureInfo.InvariantCulture) })
    {
        Clock = new ManualClock(DateTimeOffset.UtcNow);
        Users = new InMemoryUserRepository();
        Sessions = new InMemorySessionRepository();
        Revocations = new InMemoryRevocationStore(Clock);
    }

    public ManualClock Clock { get; }

    public InMemoryUserRepository Users { get; }

    public InMemorySessionRepository Sessions { get; }

    public InMemoryRevocationStore Revocations { get; }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<TimeProvider>();
            services.AddSingleton<TimeProvider>(Clock);
            services.RemoveAll<IUserRepository>();
            services.AddSingleton<IUserRepository>(Users);
            services.RemoveAll<ISessionRepository>();
            services.AddSingleton<ISessionRepository>(Sessions);
            services.RemoveAll<ITokenRevocationStore>();
            services.AddSingleton<ITokenRevocationStore>(Revocations);
        });
    }
}
