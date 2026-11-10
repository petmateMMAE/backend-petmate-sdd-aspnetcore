using System.Net;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PetMate.Application.Security;
using PetMate.ContractTests.Support;
using PetMate.TestDoubles;

namespace PetMate.ContractTests.Authentication;

/// <summary>
/// Evidencia directa (pm-017 CA-04): un cliente no confiable no puede falsificar su IP con <c>X-Forwarded-For</c> para evadir el
/// límite por IP, ni sin <c>TRUSTED_PROXIES</c> (las cabeceras se ignoran) ni con una lista que no incluye al cliente. El control
/// positivo demuestra que la cabecera solo vale cuando la conexión viene de un proxy listado. Las solicitudes se hacen con una IP de
/// origen real (el servidor de pruebas no trae una), como ocurre con Kestrel.
/// </summary>
[Collection(ApiEnvironmentGroup.Name)]
public sealed class ForwardedHeadersSpoofingTests
{
    private const string UntrustedClient = "192.0.2.50";
    private const string TrustedProxy = "203.0.113.9";

    [Trait("Endpoint", "auth-login")]
    [Fact]
    public async Task Without_trusted_proxies_a_forged_x_forwarded_for_does_not_evade_the_ip_limit()
    {
        using var factory = new RemoteIpApiFactory(UntrustedClient, new Dictionary<string, string> { ["AUTH_RATE_LIMIT_PER_MINUTE"] = "3" });
        using var client = factory.CreateClient();

        var statuses = await Spoof(client, 8);

        Assert.Equal(3, statuses.Count(s => s != HttpStatusCode.TooManyRequests));
        Assert.Equal(5, statuses.Count(s => s == HttpStatusCode.TooManyRequests));
    }

    [Trait("Endpoint", "auth-login")]
    [Fact]
    public async Task With_a_trusted_proxy_list_that_does_not_include_the_client_forged_headers_are_still_ignored()
    {
        using var factory = new RemoteIpApiFactory(UntrustedClient, new Dictionary<string, string>
        {
            ["AUTH_RATE_LIMIT_PER_MINUTE"] = "3",
            ["TRUSTED_PROXIES"] = TrustedProxy,
        });
        using var client = factory.CreateClient();

        var statuses = await Spoof(client, 8);

        Assert.Equal(3, statuses.Count(s => s != HttpStatusCode.TooManyRequests));
        Assert.Equal(5, statuses.Count(s => s == HttpStatusCode.TooManyRequests));
    }

    [Trait("Endpoint", "auth-login")]
    [Fact]
    public async Task Control_the_header_is_honored_only_when_the_connection_comes_from_a_listed_proxy()
    {
        using var factory = new RemoteIpApiFactory(TrustedProxy, new Dictionary<string, string>
        {
            ["AUTH_RATE_LIMIT_PER_MINUTE"] = "3",
            ["TRUSTED_PROXIES"] = TrustedProxy,
        });
        using var client = factory.CreateClient();

        // El proxy listado anuncia a cada cliente por separado: cada uno tiene su propio cupo.
        var statuses = await Spoof(client, 8);

        Assert.DoesNotContain(HttpStatusCode.TooManyRequests, statuses);
    }

    [Trait("Endpoint", "auth-login")]
    [Fact]
    public async Task Forged_forwarded_proto_and_host_are_ignored_and_never_echoed()
    {
        using var factory = new RemoteIpApiFactory(UntrustedClient, new Dictionary<string, string> { ["TRUSTED_PROXIES"] = TrustedProxy });
        using var client = factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/auth/login")
        {
            Content = new StringContent("{\"email\":\"no-es-correo\",\"password\":\"x\"}", Encoding.UTF8, "application/json"),
        };
        request.Headers.TryAddWithoutValidation("X-Forwarded-For", "198.51.100.77");
        request.Headers.TryAddWithoutValidation("X-Forwarded-Proto", "https");
        request.Headers.TryAddWithoutValidation("X-Forwarded-Host", "evil.example.com");

        using var response = await client.SendAsync(request);
        var raw = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.DoesNotContain("evil.example.com", raw, StringComparison.Ordinal);
        Assert.DoesNotContain("198.51.100.77", raw, StringComparison.Ordinal);
    }

    // Cada solicitud declara una IP de origen distinta para intentar caer en una partición nueva del limitador.
    private static async Task<List<HttpStatusCode>> Spoof(HttpClient client, int attempts)
    {
        var statuses = new List<HttpStatusCode>();
        for (var i = 0; i < attempts; i++)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/auth/login")
            {
                Content = new StringContent("{\"email\":\"no-es-correo\",\"password\":\"x\"}", Encoding.UTF8, "application/json"),
            };
            request.Headers.TryAddWithoutValidation("X-Forwarded-For", $"198.51.100.{i + 1}");
            using var response = await client.SendAsync(request);
            statuses.Add(response.StatusCode);
        }

        return statuses;
    }

    /// <summary>Fábrica que fija la IP de origen de la conexión antes de que actúen las cabeceras reenviadas.</summary>
    private sealed class RemoteIpApiFactory(string remoteIp, IDictionary<string, string> overrides) : IsolatedApiFactory("Development", overrides)
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            base.ConfigureWebHost(builder);
            builder.ConfigureServices(services =>
            {
                services.AddTransient<IStartupFilter>(_ => new SetRemoteIpFilter(IPAddress.Parse(remoteIp)));
                // El límite por IP ahora cuenta en Redis Auth (H6): se sustituye por el doble en memoria con la misma semántica.
                services.RemoveAll<IAuthAttemptThrottle>();
                services.AddSingleton<IAuthAttemptThrottle>(new InMemoryAuthAttemptThrottle(new ManualClock(DateTimeOffset.UtcNow)));
            });
        }
    }

    private sealed class SetRemoteIpFilter(IPAddress address) : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
            => app =>
            {
                app.Use((HttpContext context, RequestDelegate chain) =>
                {
                    context.Connection.RemoteIpAddress = address;
                    return chain(context);
                });
                next(app);
            };
    }
}
