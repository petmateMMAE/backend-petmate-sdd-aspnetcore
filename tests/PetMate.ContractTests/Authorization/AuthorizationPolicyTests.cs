using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using PetMate.Api.Authorization;
using PetMate.Api.Contracts.Responses;
using PetMate.Application.Security;
using PetMate.ContractTests.Authentication;
using PetMate.Domain.Users;

namespace PetMate.ContractTests.Authorization;

/// <summary>
/// pm-033 / T022: políticas de autorización verificadas por HTTP con controladores de prueba que viven solo en este
/// ensamblado (AuthApiFactory los descubre con AddApplicationPart); la API de producción no se amplía.
/// </summary>
[Collection(Support.ApiEnvironmentGroup.Name)]
public sealed class AuthorizationPolicyTests
{
    private static readonly string[] ProtectedPaths = ["default", "user", "unmarked"];
    private static readonly string[] UserReachable = ["user", "default", "unmarked", "public"];

    [Trait("Endpoint", "transversal")]
    [Fact]
    public async Task Anonymous_is_rejected_with_wrapped_401_everywhere_except_the_explicit_public_endpoint()
    {
        using var factory = new AuthApiFactory();
        using var client = factory.CreateClient();

        foreach (var path in ProtectedPaths)
        {
            using var response = await client.GetAsync($"/api/v1/test-authz/{path}");
            await AuthzAssert.Wrapped(response, HttpStatusCode.Unauthorized);
        }

        using var publicResponse = await client.GetAsync("/api/v1/test-authz/public");
        Assert.Equal(HttpStatusCode.OK, publicResponse.StatusCode);
    }

    [Trait("Endpoint", "transversal")]
    [Fact]
    public async Task Unknown_route_keeps_its_wrapped_404_for_anonymous_callers_instead_of_becoming_401()
    {
        using var factory = new AuthApiFactory();
        using var client = factory.CreateClient();

        using var response = await client.GetAsync("/api/v1/test-authz/does-not-exist");

        await AuthzAssert.Wrapped(response, HttpStatusCode.NotFound);
    }

    [Trait("Endpoint", "transversal")]
    [Fact]
    public async Task Endpoint_without_any_authorization_attribute_is_denied_by_default_to_anonymous()
    {
        using var factory = new AuthApiFactory();
        using var client = factory.CreateClient();

        using var response = await client.GetAsync("/api/v1/test-authz/unmarked");

        await AuthzAssert.Wrapped(response, HttpStatusCode.Unauthorized);
    }

    [Trait("Endpoint", "transversal")]
    [Fact]
    public async Task User_reaches_user_default_unmarked_and_public()
    {
        using var factory = new AuthApiFactory();
        var userId = Guid.NewGuid();
        using var client = AuthzAssert.Bearer(factory, factory.Services.GetRequiredService<IAccessTokenService>().Issue(userId, UserRole.User).Value);

        foreach (var path in UserReachable)
        {
            using var response = await client.GetAsync($"/api/v1/test-authz/{path}");
            using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(path == "public" ? "anonymous" : $"{userId:D}|User", document.RootElement.GetProperty("data").GetString());
        }
    }

    [Trait("Endpoint", "transversal")]
    [Fact]
    public async Task Revoked_owner_token_is_rejected_with_401_and_a_redis_outage_stays_fail_closed_with_503()
    {
        using var factory = new AuthApiFactory();
        var token = factory.Services.GetRequiredService<IAccessTokenService>().Issue(Guid.NewGuid(), UserRole.User);
        await factory.Store.Revoke(token.Jti, token.ExpiresAtUtc, CancellationToken.None);
        using var client = AuthzAssert.Bearer(factory, token.Value);

        using var revoked = await client.GetAsync("/api/v1/test-authz/user");
        await AuthzAssert.Wrapped(revoked, HttpStatusCode.Unauthorized);

        var other = factory.Services.GetRequiredService<IAccessTokenService>().Issue(Guid.NewGuid(), UserRole.User);
        factory.Store.Fail = true;
        using var outage = AuthzAssert.Bearer(factory, other.Value);
        using var unavailable = await outage.GetAsync("/api/v1/test-authz/user");
        await AuthzAssert.Wrapped(unavailable, HttpStatusCode.ServiceUnavailable);
    }
}

/// <summary>Aserciones y utilidades compartidas por las pruebas de autorización.</summary>
internal static class AuthzAssert
{
    public static HttpClient Bearer(AuthApiFactory factory, string token)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    public static async Task Wrapped(HttpResponseMessage response, HttpStatusCode expected)
    {
        var raw = await response.Content.ReadAsStringAsync();
        Assert.Equal(expected, response.StatusCode);
        using var document = JsonDocument.Parse(raw);
        Assert.Equal(["data", "message", "statusCode"], document.RootElement.EnumerateObject().Select(p => p.Name).Order(StringComparer.Ordinal));
        Assert.Equal((int)expected, document.RootElement.GetProperty("statusCode").GetInt32());
        Assert.Equal(JsonValueKind.Null, document.RootElement.GetProperty("data").ValueKind);
    }
}

/// <summary>Un controlador de prueba por política; solo existe en el ensamblado de pruebas.</summary>
[ApiController]
[Route("api/v1/test-authz")]
public sealed class PolicyProbeController : ControllerBase
{
#pragma warning disable CA1822 // MVC no enruta acciones static.
    [Authorize]
    [HttpGet("default")]
    public ObjectResult Default() => Identity();

    [Authorize(Policy = AuthorizationPolicies.RequireUser)]
    [HttpGet("user")]
    public ObjectResult UserOnly() => Identity();

    // Sin atributos de autorización: ejerce la política de respaldo (denegación por defecto).
    [HttpGet("unmarked")]
    public ObjectResult Unmarked() => Identity();

    [AllowAnonymous]
    [HttpGet("public")]
    public ObjectResult Public() => ApiResponseFactory.Ok("Acceso público.", "anonymous");

    private ObjectResult Identity()
        => ApiResponseFactory.Ok("Identidad verificada.", $"{User.FindFirst("sub")?.Value}|{User.FindFirst("role")?.Value}");
#pragma warning restore CA1822
}
