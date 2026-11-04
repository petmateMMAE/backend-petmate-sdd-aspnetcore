using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PetMate.Application.DependencyInjection;
using PetMate.Application.Mediator;
using PetMate.Infrastructure.DependencyInjection;

namespace PetMate.SecurityTests.DependencyInjection;

public sealed class ServiceCollectionExtensionsTests
{
    private static IConfiguration ValidConfiguration() => new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["MONGODB_URI"] = "mongodb+srv://usuario_ficticio:password_ficticio@cluster-ficticio.mongodb.net/?retryWrites=true&w=majority",
            ["MONGODB_DATABASE"] = "petmate_test_ficticio",
            ["REDIS_AUTH_CONNECTION"] = "redis-auth-ficticio:6380,password=pw-ficticio-a,ssl=True",
            ["REDIS_CACHE_CONNECTION"] = "redis-cache-ficticio:6381,password=pw-ficticio-b,ssl=True",
            ["JWT_ISSUER"] = "https://issuer-prueba.local",
            ["JWT_AUDIENCE"] = "petmate-api-prueba",
            ["JWT_SIGNING_KEY"] = "clave-de-prueba-0123456789-abcdefghijkl",
        })
        .Build();

    [Fact]
    public void AddApplication_ReturnsSameServiceCollection()
    {
        ServiceCollection services = new();

        IServiceCollection result = services.AddApplication();

        Assert.Same(services, result);
    }

    [Fact]
    public void AddInfrastructure_ReturnsSameServiceCollection()
    {
        ServiceCollection services = new();

        IServiceCollection result = services.AddInfrastructure(ValidConfiguration());

        Assert.Same(services, result);
    }

    [Fact]
    public void CompositionModules_CanBeChainedWithoutOpeningConnections()
    {
        ServiceCollection services = new();

        IServiceCollection result = services.AddApplication().AddInfrastructure(ValidConfiguration());

        Assert.Same(services, result);
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IMediator));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(ISender));
    }

    [Fact]
    public void AddApplication_RejectsNullCollection()
    {
        Assert.Throws<ArgumentNullException>(() =>
            ApplicationServiceCollectionExtensions.AddApplication(null!));
    }

    [Fact]
    public void AddInfrastructure_RejectsNullArguments()
    {
        Assert.Throws<ArgumentNullException>(() =>
            InfrastructureServiceCollectionExtensions.AddInfrastructure(null!, ValidConfiguration()));
        Assert.Throws<ArgumentNullException>(() => new ServiceCollection().AddInfrastructure(null!));
    }
}
