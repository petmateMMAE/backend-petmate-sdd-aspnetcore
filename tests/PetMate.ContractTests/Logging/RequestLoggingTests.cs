using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;
using PetMate.ContractTests.Authentication;
using PetMate.ContractTests.Support;

namespace PetMate.ContractTests.Logging;

/// <summary>
/// Logging estructurado con correlación y sin datos sensibles (pm-054 CA-01 a CA-03). Captura todos los registros de la API
/// (mensaje, estado estructurado, ámbitos y excepción) durante flujos reales y comprueba que ningún secreto ni dato personal aparece.
/// </summary>
[Collection(ApiEnvironmentGroup.Name)]
public sealed class RequestLoggingTests
{
    private const string Email = "ana.logs@example.com";
    private const string Password = "contraseña-muy-segura-123";
    private const string PetName = "NombreSecretoDeMascota";

    [Trait("Endpoint", "transversal")]
    [Fact]
    public async Task Every_response_carries_a_correlation_id_and_a_safe_supplied_one_is_kept()
    {
        using var factory = new LoggingFactory();
        using var client = factory.CreateClient();

        using var generated = await client.GetAsync("/api/v1/technical/mediator-probe");
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/technical/mediator-probe");
        request.Headers.Add("X-Correlation-Id", "cliente-1234-abcd");
        using var kept = await client.SendAsync(request);

        var id = generated.Headers.GetValues("X-Correlation-Id").Single();
        Assert.Matches("^[0-9a-f]{32}$", id);
        Assert.Equal("cliente-1234-abcd", kept.Headers.GetValues("X-Correlation-Id").Single());
    }

    [Trait("Endpoint", "transversal")]
    [Theory]
    [InlineData("corto")]
    [InlineData("tiene espacios y símbolos <script>")]
    [InlineData("a-valid-prefix-but-way-too-long-0123456789-0123456789-0123456789-0123456789")]
    public async Task An_unsafe_supplied_correlation_id_is_replaced(string supplied)
    {
        using var factory = new LoggingFactory();
        using var client = factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/technical/mediator-probe");
        request.Headers.TryAddWithoutValidation("X-Correlation-Id", supplied);

        using var response = await client.SendAsync(request);

        var id = response.Headers.GetValues("X-Correlation-Id").Single();
        Assert.NotEqual(supplied, id);
        Assert.Matches("^[0-9a-f]{32}$", id);
    }

    [Trait("Endpoint", "transversal")]
    [Fact]
    public async Task The_request_log_is_one_event_per_request_with_route_template_status_and_correlation_scope()
    {
        using var factory = new LoggingFactory();
        using var client = factory.CreateClient();
        var realId = Guid.NewGuid();

        using var response = await client.GetAsync($"/api/v1/pets/get-by-id/{realId}?telefono=5551234567");
        var correlation = response.Headers.GetValues("X-Correlation-Id").Single();

        var entry = Assert.Single(factory.Entries, e => e.Category.EndsWith("RequestLoggingMiddleware", StringComparison.Ordinal));
        Assert.Equal(LogLevel.Information, entry.Level);
        Assert.Equal("401", entry.State["StatusCode"]);
        Assert.Equal("GET", entry.State["Method"]);
        Assert.Equal("api/v1/pets/get-by-id/{id:guid}", entry.State["Route"]);
        Assert.Contains(entry.Scopes, scope => scope.TryGetValue("CorrelationId", out var value) && value == correlation);
        Assert.DoesNotContain(realId.ToString(), entry.AllText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("5551234567", entry.AllText, StringComparison.Ordinal);
    }

    [Trait("Endpoint", "transversal")]
    [Fact]
    public void Console_logs_are_json_with_scopes()
    {
        using var factory = new LoggingFactory();
        using var _ = factory.CreateClient();

        var formatters = factory.Services.GetServices<ConsoleFormatter>().Select(f => f.Name).ToList();
        var options = factory.Services.GetRequiredService<IOptionsMonitor<JsonConsoleFormatterOptions>>().CurrentValue;
        var console = factory.Services.GetRequiredService<IOptionsMonitor<ConsoleLoggerOptions>>().CurrentValue;

        Assert.Contains(ConsoleFormatterNames.Json, formatters);
        Assert.Equal(ConsoleFormatterNames.Json, console.FormatterName);
        Assert.True(options.IncludeScopes);
        Assert.True(options.UseUtcTimestamp);
    }

    [Trait("Endpoint", "transversal")]
    [Fact]
    public async Task Logs_never_contain_credentials_tokens_emails_names_or_request_bodies()
    {
        using var factory = new LoggingFactory();
        using var client = factory.CreateClient();

        using var register = await Post(client, "/api/v1/auth/register", new { email = Email, password = Password }, null);
        Assert.Equal(HttpStatusCode.Created, register.StatusCode);
        using var wrong = await Post(client, "/api/v1/auth/login", new { email = Email, password = "otra-contraseña-123" }, null);
        using var login = await Post(client, "/api/v1/auth/login", new { email = Email, password = Password }, null);
        var tokens = ReadTokens(await login.Content.ReadAsStringAsync());
        using var create = await Post(client, "/api/v1/pets/create", new { name = PetName, species = "Perro", sex = "Male" }, tokens.Access);
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        using var profile = await Send(client, HttpMethod.Get, "/api/v1/users/me/get", tokens.Access);
        using var forged = await Send(client, HttpMethod.Get, "/api/v1/users/me/get", string.Join('.', "eyJhbGciOiJIUzI1NiJ9", "eyJzdWIiOiJ4In0", "firma-falsa-secreta"));
        using var badJson = await client.PostAsync("/api/v1/auth/login", new StringContent("{\"email\":\"" + Email + "\",\"password\":", Encoding.UTF8, "application/json"));
        using var oversized = await Post(client, "/api/v1/pets/create", new { name = new string('x', 20_000), species = "Perro", sex = "Male" }, tokens.Access);
        using var refresh = await Post(client, "/api/v1/auth/refresh", new { refreshToken = tokens.Refresh }, null);
        var refreshed = ReadTokens(await refresh.Content.ReadAsStringAsync());
        using var replay = await Post(client, "/api/v1/auth/refresh", new { refreshToken = tokens.Refresh }, null);

        Assert.Equal(HttpStatusCode.Unauthorized, forged.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, replay.StatusCode);
        Assert.NotEmpty(factory.Entries);
        var forbidden = new[]
        {
            Email, "ana.logs", Password, "otra-contraseña-123", PetName, tokens.Access, tokens.Refresh, refreshed.Access, refreshed.Refresh,
            "firma-falsa-secreta", IsolatedApiFactory.JwtKey, "Bearer ", "Authorization", "password", "refreshToken", "RequestPath",
        };
        foreach (var entry in factory.Entries)
        {
            foreach (var secret in forbidden)
            {
                Assert.False(
                    entry.AllText.Contains(secret, StringComparison.OrdinalIgnoreCase),
                    $"Un registro del logger ({entry.Category}) contiene un dato sensible.");
            }
        }
    }

    private static (string Access, string Refresh) ReadTokens(string json)
    {
        using var document = JsonDocument.Parse(json);
        var data = document.RootElement.GetProperty("data");
        return (data.GetProperty("accessToken").GetString()!, data.GetProperty("refreshToken").GetString()!);
    }

    private static Task<HttpResponseMessage> Post(HttpClient client, string url, object body, string? bearer)
    {
        var message = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json"),
        };
        if (bearer is not null)
        {
            message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearer);
        }

        return client.SendAsync(message);
    }

    private static Task<HttpResponseMessage> Send(HttpClient client, HttpMethod method, string url, string bearer)
    {
        var message = new HttpRequestMessage(method, url);
        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearer);
        return client.SendAsync(message);
    }

    private sealed class LoggingFactory : AuthFlowFactory
    {
        private readonly CapturingLoggerProvider provider = new();

        public IReadOnlyCollection<CapturedEntry> Entries => provider.Entries;

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            base.ConfigureWebHost(builder);
            builder.ConfigureLogging(logging => logging.AddProvider(provider));
        }
    }

    private sealed record CapturedEntry(
        string Category,
        LogLevel Level,
        string Message,
        Dictionary<string, string> State,
        List<Dictionary<string, string>> Scopes,
        string? Exception)
    {
        public string AllText => string.Join(
            '\n',
            new[] { Category, Message, Exception ?? string.Empty }
                .Concat(State.SelectMany(p => new[] { p.Key, p.Value }))
                .Concat(Scopes.SelectMany(s => s.SelectMany(p => new[] { p.Key, p.Value }))));
    }

    private sealed class CapturingLoggerProvider : ILoggerProvider, ISupportExternalScope
    {
        private IExternalScopeProvider? scopes;

        public ConcurrentQueue<CapturedEntry> Queue { get; } = new();

        public IReadOnlyCollection<CapturedEntry> Entries => Queue;

        public ILogger CreateLogger(string categoryName) => new CapturingLogger(categoryName, this);

        public void SetScopeProvider(IExternalScopeProvider scopeProvider) => scopes = scopeProvider;

        public void Dispose()
        {
        }

        private sealed class CapturingLogger(string category, CapturingLoggerProvider owner) : ILogger
        {
            public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
                var values = new Dictionary<string, string>();
                if (state is IEnumerable<KeyValuePair<string, object?>> pairs)
                {
                    foreach (var (key, value) in pairs)
                    {
                        values[key] = Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;
                    }
                }

                var captured = new List<Dictionary<string, string>>();
                owner.scopes?.ForEachScope(
                    static (scope, list) =>
                    {
                        var item = new Dictionary<string, string>();
                        if (scope is IEnumerable<KeyValuePair<string, object>> scopePairs)
                        {
                            foreach (var (key, value) in scopePairs)
                            {
                                item[key] = Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;
                            }
                        }
                        else
                        {
                            item["scope"] = scope?.ToString() ?? string.Empty;
                        }

                        list.Add(item);
                    },
                    captured);
                owner.Queue.Enqueue(new CapturedEntry(category, logLevel, formatter(state, exception), values, captured, exception?.ToString()));
            }
        }
    }
}
