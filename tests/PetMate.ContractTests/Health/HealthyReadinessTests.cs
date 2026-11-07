using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using PetMate.ContractTests.Support;
using PetMate.Infrastructure.Health;

namespace PetMate.ContractTests.Health;

/// <summary>Camino feliz de readiness: con las tres dependencias sanas responde 200 sin exponer detalles internos.</summary>
[Collection(ApiEnvironmentGroup.Name)]
public sealed class HealthyReadinessTests
{
    [Trait("Endpoint", "health-ready")]
    [Fact]
    public async Task Ready_with_healthy_dependencies_returns_200_wrapper_listing_each_one_as_healthy()
    {
        using var factory = new HealthyDependenciesFactory();
        using var client = factory.CreateClient();

        using var response = await client.GetAsync("/api/v1/health/ready");
        var raw = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(raw);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(200, document.RootElement.GetProperty("statusCode").GetInt32());
        var data = document.RootElement.GetProperty("data");
        Assert.Equal("Healthy", data.GetProperty("status").GetString());
        var dependencies = data.GetProperty("dependencies").EnumerateArray()
            .ToDictionary(item => item.GetProperty("name").GetString()!, item => item.GetProperty("status").GetString());
        Assert.Equal(["mongodb", "redis-auth", "redis-cache"], dependencies.Keys.Order(StringComparer.Ordinal));
        Assert.All(dependencies.Values, status => Assert.Equal("Healthy", status));
        Assert.DoesNotContain("mongodb://", raw, StringComparison.Ordinal);
        Assert.DoesNotContain("test.invalid", raw, StringComparison.Ordinal);
    }

    [Trait("Endpoint", "health-ready")]
    [Fact]
    public async Task Ready_rejects_non_get_methods_with_405_wrapper()
    {
        using var factory = new HealthyDependenciesFactory();
        using var client = factory.CreateClient();

        using var response = await client.PostAsync("/api/v1/health/ready", content: null);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        Assert.Equal(405, document.RootElement.GetProperty("statusCode").GetInt32());
    }

    [Trait("Endpoint", "health-live")]
    [Fact]
    public async Task Live_rejects_non_get_methods_with_405_wrapper()
    {
        using var factory = new HealthyDependenciesFactory();
        using var client = factory.CreateClient();

        using var response = await client.PostAsync("/api/v1/health/live", content: null);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        Assert.Equal(405, document.RootElement.GetProperty("statusCode").GetInt32());
    }

    /// <summary>Sustituye las verificaciones reales por verificaciones sanas con los mismos nombres y la misma etiqueta.</summary>
    private sealed class HealthyDependenciesFactory : IsolatedApiFactory
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            base.ConfigureWebHost(builder);
            builder.ConfigureServices(services => services.PostConfigure<HealthCheckServiceOptions>(options =>
            {
                options.Registrations.Clear();
                foreach (var name in new[] { HealthCheckNames.Mongo, HealthCheckNames.RedisAuth, HealthCheckNames.RedisCache })
                {
                    options.Registrations.Add(new HealthCheckRegistration(name, _ => new AlwaysHealthy(), failureStatus: null, tags: [HealthCheckNames.ReadyTag]));
                }
            }));
        }
    }

    private sealed class AlwaysHealthy : IHealthCheck
    {
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
            => Task.FromResult(HealthCheckResult.Healthy());
    }
}
