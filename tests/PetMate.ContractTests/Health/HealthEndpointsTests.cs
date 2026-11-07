using PetMate.ContractTests.Support;
using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace PetMate.ContractTests.Health;

[Collection(ApiEnvironmentGroup.Name)]
public sealed class HealthEndpointsTests
{
    [Trait("Endpoint", "health-live")]
    [Fact]
    public async Task Live_returns_200_wrapper_without_checking_dependencies()
    {
        using var factory = new HealthApiFactory();
        using var client = factory.CreateClient();

        using var response = await client.GetAsync("/api/v1/health/live");
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(200, document.RootElement.GetProperty("statusCode").GetInt32());
        Assert.Equal("Healthy", document.RootElement.GetProperty("data").GetProperty("status").GetString());
    }

    [Trait("Endpoint", "health-ready")]
    [Fact]
    public async Task Ready_with_unreachable_dependencies_returns_503_naming_each_unhealthy_dependency_without_secrets()
    {
        using var factory = new HealthApiFactory();
        using var client = factory.CreateClient();

        using var response = await client.GetAsync("/api/v1/health/ready");
        var raw = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(raw);

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        Assert.Equal(503, document.RootElement.GetProperty("statusCode").GetInt32());
        var data = document.RootElement.GetProperty("data");
        Assert.Equal("Unhealthy", data.GetProperty("status").GetString());
        var dependencies = data.GetProperty("dependencies").EnumerateArray()
            .ToDictionary(item => item.GetProperty("name").GetString()!, item => item.GetProperty("status").GetString());
        Assert.Equal(["mongodb", "redis-auth", "redis-cache"], dependencies.Keys.Order(StringComparer.Ordinal));
        Assert.All(dependencies.Values, status => Assert.Equal("Unhealthy", status));

        Assert.DoesNotContain("test-auth-pw", raw, StringComparison.Ordinal);
        Assert.DoesNotContain("test-cache-pw", raw, StringComparison.Ordinal);
        Assert.DoesNotContain("test.invalid", raw, StringComparison.Ordinal);
        Assert.DoesNotContain("mongodb://", raw, StringComparison.Ordinal);
    }

    /// <summary>Dependencias inalcanzables a propósito, con el entorno aislado de cualquier .env real.</summary>
    private sealed class HealthApiFactory() : IsolatedApiFactory(
        "Development",
        new Dictionary<string, string>
        {
            ["REDIS_AUTH_CONNECTION"] = "127.0.0.1:1,password=test-auth-pw,ssl=True,connectTimeout=500",
            ["REDIS_CACHE_CONNECTION"] = "127.0.0.1:2,password=test-cache-pw,ssl=True,connectTimeout=500",
        });
}
