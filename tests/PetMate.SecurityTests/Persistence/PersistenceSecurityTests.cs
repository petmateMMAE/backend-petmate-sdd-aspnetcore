using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using PetMate.Api.Middleware;
using PetMate.Application.Persistence;
using PetMate.Infrastructure.DependencyInjection;

namespace PetMate.SecurityTests.Persistence;

public sealed class PersistenceSecurityTests
{
    [Fact]
    public async Task Middleware_maps_persistence_failure_to_503_without_provider_details()
    {
        var context = new DefaultHttpContext { Response = { Body = new MemoryStream() } };
        var middleware = new ExceptionHandlingMiddleware(
            _ => throw new PersistenceUnavailableException(new InvalidOperationException("mongodb://usuario:clave-secreta@host")),
            NullLogger<ExceptionHandlingMiddleware>.Instance);

        await middleware.InvokeAsync(context);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var payload = await new StreamReader(context.Response.Body, Encoding.UTF8).ReadToEndAsync();
        Assert.Equal(StatusCodes.Status503ServiceUnavailable, context.Response.StatusCode);
        Assert.Contains("\"statusCode\":503", payload, StringComparison.Ordinal);
        Assert.DoesNotContain("clave-secreta", payload, StringComparison.Ordinal);
        Assert.DoesNotContain("mongodb", payload, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Infrastructure_registers_repositories_behind_application_abstractions_without_connecting()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["MONGODB_URI"] = "mongodb+srv://usuario_ficticio:password_ficticio@cluster-ficticio.mongodb.net/?retryWrites=true&w=majority",
            ["MONGODB_DATABASE"] = "petmate_test_ficticio",
            ["REDIS_AUTH_CONNECTION"] = "redis-auth-ficticio:6380,password=pw-a,ssl=True",
            ["REDIS_CACHE_CONNECTION"] = "redis-cache-ficticio:6381,password=pw-b,ssl=True",
            ["JWT_ISSUER"] = "https://issuer-prueba.local",
            ["JWT_AUDIENCE"] = "petmate-api-prueba",
            ["JWT_SIGNING_KEY"] = "clave-de-prueba-0123456789-abcdefghijkl",
        }).Build();
        var services = new ServiceCollection().AddLogging().AddInfrastructure(configuration);

        using var provider = services.BuildServiceProvider();

        Assert.NotNull(provider.GetRequiredService<IUserRepository>());
        Assert.NotNull(provider.GetRequiredService<ISessionRepository>());
        Assert.DoesNotContain(
            typeof(IUserRepository).GetMethods().Concat(typeof(ISessionRepository).GetMethods()).SelectMany(method => method.GetParameters()),
            parameter => parameter.ParameterType.Namespace?.StartsWith("MongoDB", StringComparison.Ordinal) == true);
    }

    [Fact]
    public void Persistence_exception_message_is_generic_even_with_a_sensitive_inner_exception()
    {
        var exception = new PersistenceUnavailableException(new InvalidOperationException("clave-secreta"));

        Assert.DoesNotContain("clave-secreta", exception.Message, StringComparison.Ordinal);
    }
}
