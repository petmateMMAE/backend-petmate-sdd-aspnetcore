using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PetMate.Api.Configuration;
using PetMate.Application.Mediator;

namespace PetMate.SecurityTests.Configuration;

/// <summary>
/// Ejercita el mismo flujo de creación de host que usa Program sin iniciar servicios externos.
/// Las pruebas restauran el entorno de proceso y no imprimen valores de configuración.
/// </summary>
public sealed class ApplicationBootstrapTests
{
    private const string ConfigurationKey = "PETMATE_T004_BOOTSTRAP_VALUE";

    [Fact]
    public void ConfigureCompositionRoot_BuildsHostWithoutOpeningExternalConnections()
    {
        var builder = ApplicationBootstrap.CreateBuilder(
            [],
            new WebApplicationOptions { EnvironmentName = Environments.Production });
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["MONGODB_URI"] = "mongodb://test.invalid",
            ["MONGODB_DATABASE"] = "petmate-test",
            ["REDIS_AUTH_CONNECTION"] = "redis-auth-test:6380,password=test-auth-pw,ssl=True",
            ["REDIS_CACHE_CONNECTION"] = "redis-cache-test:6381,password=test-cache-pw,ssl=True",
            ["JWT_ISSUER"] = "issuer-test",
            ["JWT_AUDIENCE"] = "audience-test",
            ["JWT_SIGNING_KEY"] = "test-key-0123456789-abcdefghijklmnop-XYZ",
        });

        ApplicationBootstrap.ConfigureCompositionRoot(builder);

        // T010 registra Infrastructure; las conexiones son perezosas y construir el host no abre ninguna.
        Assert.Contains(builder.Services, descriptor => descriptor.ServiceType == typeof(PetMate.Infrastructure.Redis.RedisAuthConnection));
        Assert.Contains(builder.Services, descriptor => descriptor.ServiceType == typeof(IMediator));
        using var app = builder.Build();
        Assert.NotNull(app.Services);
        var serviceProvider = app.Services.GetRequiredService<IServiceProvider>();
        Assert.NotNull(serviceProvider);
    }

    [Fact]
    public void CreateBuilder_LoadsDotEnvFromContentRoot_WhenEffectiveEnvironmentIsDevelopment()
    {
        using var scope = new ProcessEnvironmentScope(ConfigurationKey, "ASPNETCORE_ENVIRONMENT", "DOTNET_ENVIRONMENT");
        var contentRoot = CreateTemporaryDirectory();

        try
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", null, EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", null, EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable(ConfigurationKey, null, EnvironmentVariableTarget.Process);
            File.WriteAllText(Path.Combine(contentRoot, ".env"), $"{ConfigurationKey}=local-test-value\n");

            var builder = ApplicationBootstrap.CreateBuilder(
                [],
                new WebApplicationOptions
                {
                    ContentRootPath = contentRoot,
                    EnvironmentName = Environments.Development,
                });

            Assert.True(builder.Environment.IsDevelopment());
            Assert.Equal("local-test-value", builder.Configuration[ConfigurationKey]);
        }
        finally
        {
            Directory.Delete(contentRoot, recursive: true);
        }
    }

    [Fact]
    public void CreateBuilder_DoesNotLoadDotEnv_WhenEffectiveEnvironmentDefaultsToProduction()
    {
        using var scope = new ProcessEnvironmentScope(ConfigurationKey, "ASPNETCORE_ENVIRONMENT", "DOTNET_ENVIRONMENT");
        var contentRoot = CreateTemporaryDirectory();

        try
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", null, EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", null, EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable(ConfigurationKey, null, EnvironmentVariableTarget.Process);
            File.WriteAllText(Path.Combine(contentRoot, ".env"), $"{ConfigurationKey}=local-test-value\n");

            var builder = ApplicationBootstrap.CreateBuilder(
                [],
                new WebApplicationOptions { ContentRootPath = contentRoot });

            Assert.True(builder.Environment.IsProduction());
            Assert.Null(builder.Configuration[ConfigurationKey]);
            Assert.Null(Environment.GetEnvironmentVariable(ConfigurationKey));
        }
        finally
        {
            Directory.Delete(contentRoot, recursive: true);
        }
    }

    private static string CreateTemporaryDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), $"petmate-t004-bootstrap-{Guid.NewGuid():N}");
        Directory.CreateDirectory(path);
        return path;
    }

    private sealed class ProcessEnvironmentScope : IDisposable
    {
        private readonly Dictionary<string, string?> originalValues;

        public ProcessEnvironmentScope(params string[] names)
        {
            originalValues = names.ToDictionary(
                name => name,
                name => Environment.GetEnvironmentVariable(name),
                StringComparer.Ordinal);
        }

        public void Dispose()
        {
            foreach (var (name, value) in originalValues)
            {
                Environment.SetEnvironmentVariable(name, value, EnvironmentVariableTarget.Process);
            }
        }
    }
}
