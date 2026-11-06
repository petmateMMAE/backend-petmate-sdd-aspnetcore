using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using PetMate.ContractTests.Support;

namespace PetMate.ContractTests.Authorization;

/// <summary>
/// pm-033 / T022: la superficie pública de la API real es una lista cerrada. Un endpoint público nuevo obliga a actualizar
/// esta prueba, es decir, a tomar una decisión explícita (denegación por defecto).
/// </summary>
[Collection(ApiEnvironmentGroup.Name)]
public sealed class PublicSurfaceTests
{
    // Endpoints públicos permanentes: autenticación inicial y sondas de salud.
    private static readonly string[] PermanentPublic =
    [
        "GET /api/v1/health/live",
        "GET /api/v1/health/ready",
        "POST /api/v1/auth/login",
        "POST /api/v1/auth/refresh",
        "POST /api/v1/auth/register",
    ];

    // Público de forma temporal (decisión P-04): revisar su exposición en producción en un trabajo de seguridad posterior.
    private static readonly string[] TemporaryPublic = ["GET /api/v1/technical/mediator-probe"];

    [Fact]
    public void Development_exposes_only_the_approved_public_endpoints_plus_the_documentation_routes()
    {
        using var factory = new IsolatedApiFactory("Development");

        var anonymous = PublicEndpoints(factory);
        var business = anonymous.Where(e => !IsDocumentation(e)).Order(StringComparer.Ordinal).ToArray();

        Assert.Equal(PermanentPublic.Concat(TemporaryPublic).Order(StringComparer.Ordinal), business);
        Assert.Contains(anonymous, e => e.Contains("/openapi", StringComparison.Ordinal));
        Assert.Contains(anonymous, e => e.Contains("/scalar", StringComparison.Ordinal));
    }

    [Fact]
    public void Production_exposes_no_documentation_route_and_no_unapproved_public_endpoint()
    {
        using var factory = new IsolatedApiFactory("Production");

        var anonymous = PublicEndpoints(factory);

        Assert.DoesNotContain(anonymous, IsDocumentation);
        Assert.Equal(PermanentPublic.Concat(TemporaryPublic).Order(StringComparer.Ordinal), anonymous.Order(StringComparer.Ordinal));
    }

    [Fact]
    public void Logout_requires_authorization_and_is_not_public()
    {
        using var factory = new IsolatedApiFactory("Development");
        var endpoints = factory.Services.GetRequiredService<EndpointDataSource>().Endpoints.OfType<RouteEndpoint>().ToArray();

        var logout = Assert.Single(endpoints, e => e.RoutePattern.RawText == "api/v1/auth/logout");

        Assert.Null(logout.Metadata.GetMetadata<IAllowAnonymous>());
        Assert.NotNull(logout.Metadata.GetMetadata<IAuthorizeData>());
    }

    private static string[] PublicEndpoints(IsolatedApiFactory factory)
        => factory.Services.GetRequiredService<EndpointDataSource>().Endpoints
            .OfType<RouteEndpoint>()
            .Where(e => e.Metadata.GetMetadata<IAllowAnonymous>() is not null)
            .SelectMany(e => (e.Metadata.GetMetadata<HttpMethodMetadata>()?.HttpMethods ?? ["*"])
                .Select(method => $"{method} /{e.RoutePattern.RawText?.TrimStart('/')}"))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

    private static bool IsDocumentation(string endpoint)
        => endpoint.Contains("/openapi", StringComparison.Ordinal) || endpoint.Contains("/scalar", StringComparison.Ordinal);
}
