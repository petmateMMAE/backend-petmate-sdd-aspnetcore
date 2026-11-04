using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PetMate.Api.Contracts.Responses;
using PetMate.Application.Security;
using PetMate.ContractTests.Support;
using PetMate.Domain.Users;
using PetMate.Infrastructure.Security;

namespace PetMate.ContractTests.Authentication;

[Collection(ApiEnvironmentGroup.Name)]
public sealed class JwtPipelineTests
{
    [Fact]
    public async Task Request_without_token_returns_wrapped_401()
    {
        using var factory = new AuthApiFactory();
        using var client = factory.CreateClient();

        using var response = await client.GetAsync("/api/v1/test-auth/whoami");

        await AssertWrapped(response, HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Valid_token_reaches_the_protected_endpoint_with_the_expected_identity()
    {
        using var factory = new AuthApiFactory();
        var userId = Guid.NewGuid();
        var token = factory.Services.GetRequiredService<IAccessTokenService>().Issue(userId, UserRole.Owner);
        using var client = Bearer(factory, token.Value);

        using var response = await client.GetAsync("/api/v1/test-auth/whoami");
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal($"{userId:D}|Owner", document.RootElement.GetProperty("data").GetString());
    }

    [Fact]
    public async Task Revoked_jti_is_rejected_with_wrapped_401_even_though_the_token_has_not_expired()
    {
        using var factory = new AuthApiFactory();
        var token = factory.Services.GetRequiredService<IAccessTokenService>().Issue(Guid.NewGuid(), UserRole.Owner);
        await factory.Store.Revoke(token.Jti, token.ExpiresAtUtc, CancellationToken.None);
        using var client = Bearer(factory, token.Value);

        using var response = await client.GetAsync("/api/v1/test-auth/whoami");

        await AssertWrapped(response, HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Redis_outage_fails_closed_with_wrapped_503_not_a_false_401_and_never_200()
    {
        using var factory = new AuthApiFactory();
        factory.Store.Fail = true;
        var token = factory.Services.GetRequiredService<IAccessTokenService>().Issue(Guid.NewGuid(), UserRole.Owner);
        using var client = Bearer(factory, token.Value);

        using var response = await client.GetAsync("/api/v1/test-auth/whoami");
        var raw = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        Assert.DoesNotContain("InvalidOperation", raw, StringComparison.Ordinal);
        await AssertWrapped(response, HttpStatusCode.ServiceUnavailable, raw);
    }

    [Fact]
    public async Task Token_signed_with_another_key_or_for_another_audience_is_rejected()
    {
        using var factory = new AuthApiFactory();
        var key = System.Text.Encoding.UTF8.GetBytes(IsolatedApiFactory.JwtKey);
        var otherKey = System.Text.Encoding.UTF8.GetBytes("otra-clave-distinta-0123456789-abcdefghi");
        var wrongAudience = new JwtAccessTokenService(new JwtOptions(IsolatedApiFactory.JwtIssuer, "otra-audiencia", key, TimeSpan.FromMinutes(5)), TimeProvider.System)
            .Issue(Guid.NewGuid(), UserRole.Owner);
        var wrongKey = new JwtAccessTokenService(new JwtOptions(IsolatedApiFactory.JwtIssuer, IsolatedApiFactory.JwtAudience, otherKey, TimeSpan.FromMinutes(5)), TimeProvider.System)
            .Issue(Guid.NewGuid(), UserRole.Owner);

        foreach (var token in new[] { wrongAudience.Value, wrongKey.Value })
        {
            using var client = Bearer(factory, token);
            using var response = await client.GetAsync("/api/v1/test-auth/whoami");
            await AssertWrapped(response, HttpStatusCode.Unauthorized);
        }
    }

    [Fact]
    public async Task Expired_token_is_rejected()
    {
        using var factory = new AuthApiFactory();
        var token = factory.Services.GetRequiredService<IAccessTokenService>().Issue(Guid.NewGuid(), UserRole.Owner);
        factory.Clock.Advance(TimeSpan.FromMinutes(16));
        using var client = Bearer(factory, token.Value);

        using var response = await client.GetAsync("/api/v1/test-auth/whoami");

        await AssertWrapped(response, HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData("Bearer not-a-jwt")]
    [InlineData("Basic dXNlcjpwYXNz")]
    public async Task Malformed_authorization_headers_are_rejected(string header)
    {
        using var factory = new AuthApiFactory();
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", header);

        using var response = await client.GetAsync("/api/v1/test-auth/whoami");

        await AssertWrapped(response, HttpStatusCode.Unauthorized);
    }

    private static HttpClient Bearer(AuthApiFactory factory, string token)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private static async Task AssertWrapped(HttpResponseMessage response, HttpStatusCode expected, string? raw = null)
    {
        raw ??= await response.Content.ReadAsStringAsync();
        Assert.Equal(expected, response.StatusCode);
        using var document = JsonDocument.Parse(raw);
        Assert.Equal(["data", "message", "statusCode"], document.RootElement.EnumerateObject().Select(p => p.Name).Order(StringComparer.Ordinal));
        Assert.Equal((int)expected, document.RootElement.GetProperty("statusCode").GetInt32());
    }
}

/// <summary>Almacén de revocación en memoria para probar el hook de JwtBearer sin Redis.</summary>
public sealed class FakeRevocationStore : ITokenRevocationStore
{
    private readonly HashSet<string> revoked = [];

    public bool Fail { get; set; }

    public Task Revoke(string jti, DateTime expiresAtUtc, CancellationToken cancellationToken)
    {
        ThrowIfFailing();
        revoked.Add(jti);
        return Task.CompletedTask;
    }

    public Task<bool> IsRevoked(string jti, CancellationToken cancellationToken)
    {
        ThrowIfFailing();
        return Task.FromResult(revoked.Contains(jti));
    }

    private void ThrowIfFailing()
    {
        if (Fail)
        {
            throw new AuthDependencyUnavailableException(new InvalidOperationException("detalle-interno"));
        }
    }
}

public sealed class TestClock(DateTimeOffset start) : TimeProvider
{
    private DateTimeOffset now = start;

    public override DateTimeOffset GetUtcNow() => now;

    public void Advance(TimeSpan delta) => now += delta;
}

public sealed class AuthApiFactory : IsolatedApiFactory
{
    public AuthApiFactory(string? issuer = null, string? audience = null)
        : base("Development", Overrides(issuer, audience))
    {
    }

    public FakeRevocationStore Store { get; } = new();

    public TestClock Clock { get; } = new(DateTimeOffset.UtcNow);

    private static Dictionary<string, string> Overrides(string? issuer, string? audience)
    {
        var overrides = new Dictionary<string, string>();
        if (issuer is not null)
        {
            overrides["JWT_ISSUER"] = issuer;
        }

        if (audience is not null)
        {
            overrides["JWT_AUDIENCE"] = audience;
        }

        return overrides;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        builder.ConfigureServices(services =>
        {
            services.AddControllers().AddApplicationPart(typeof(ProtectedProbeController).Assembly);
            services.RemoveAll<ITokenRevocationStore>();
            services.AddSingleton<ITokenRevocationStore>(Store);
            services.RemoveAll<TimeProvider>();
            services.AddSingleton<TimeProvider>(Clock);
        });
    }
}

[ApiController]
[Route("api/v1/test-auth")]
public sealed class ProtectedProbeController : ControllerBase
{
#pragma warning disable CA1822 // MVC no enruta acciones static.
    [Authorize]
    [HttpGet("whoami")]
    public ObjectResult WhoAmI()
        => ApiResponseFactory.Ok(
            "Identidad verificada.",
            $"{User.FindFirst("sub")?.Value}|{User.FindFirst("role")?.Value}");
#pragma warning restore CA1822
}
