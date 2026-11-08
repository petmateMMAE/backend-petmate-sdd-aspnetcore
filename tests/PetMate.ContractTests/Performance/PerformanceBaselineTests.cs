using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using PetMate.Application.Security;
using PetMate.ContractTests.Authentication;
using PetMate.ContractTests.Support;
using PetMate.Domain.Users;
using Xunit.Abstractions;

namespace PetMate.ContractTests.Performance;

/// <summary>
/// Línea base de rendimiento (pm-053): prueba de carga en proceso (costo $0, sin red ni servicios externos, datos sintéticos sin PII)
/// con concurrencia fija. Mide la latencia de la aplicación (middleware, validación, casos de uso y serialización) con
/// repositorios en memoria; la latencia con Atlas y red real se mide en el despliegue (pm-084). Los topes de latencia son
/// deliberadamente holgados para no ser frágiles en CI; los valores medidos se imprimen y se registran en
/// <c>docs/quality/performance-baseline.md</c>.
/// </summary>
[Collection(ApiEnvironmentGroup.Name)]
public sealed class PerformanceBaselineTests(ITestOutputHelper output)
{
    private const int Concurrency = 16;
    private const int RequestsPerEndpoint = 600;

    [Trait("Endpoint", "transversal")]
    [Fact]
    public async Task Read_endpoints_meet_the_in_process_latency_objectives_under_concurrent_load()
    {
        using var factory = new AuthFlowFactory(petsRateLimitPerMinute: 10_000, usersRateLimitPerMinute: 10_000);
        var token = factory.Services.GetRequiredService<IAccessTokenService>().Issue(Guid.NewGuid(), UserRole.Owner);
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Value);
        for (var i = 0; i < 20; i++)
        {
            using var create = await client.PostAsync("/api/v1/pets/create", new StringContent($"{{\"name\":\"Mascota {i}\",\"species\":\"Perro\",\"sex\":\"Male\"}}", Encoding.UTF8, "application/json"));
            Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        }

        var targets = new (string Url, double P95Ms)[]
        {
            ("/api/v1/health/live", 250),
            ("/api/v1/pets/get-all?page=2&pageSize=10", 250),
            ("/api/v1/pets/get-all?pageSize=20", 250),
        };
        foreach (var (url, p95Target) in targets)
        {
            var (p50, p95, p99, rps, errors) = await RunLoad(client, url);
            output.WriteLine($"MEDICION {url}: {RequestsPerEndpoint} solicitudes, concurrencia {Concurrency}: p50 {p50:F1} ms, p95 {p95:F1} ms, p99 {p99:F1} ms, {rps:F0} req/s, errores {errors}.");
            Assert.Equal(0, errors);
            Assert.True(p95 <= p95Target, $"{url}: p95 {p95:F1} ms supera el objetivo de {p95Target} ms.");
        }
    }

    private static async Task<(double P50, double P95, double P99, double Rps, int Errors)> RunLoad(HttpClient client, string url)
    {
        for (var i = 0; i < 50; i++)
        {
            using var warm = await client.GetAsync(url);
        }

        var latencies = new double[RequestsPerEndpoint];
        var errors = 0;
        var next = -1;
        var started = Stopwatch.GetTimestamp();
        await Task.WhenAll(Enumerable.Range(0, Concurrency).Select(worker => Task.Run(async () =>
        {
            int index;
            while ((index = Interlocked.Increment(ref next)) < RequestsPerEndpoint)
            {
                var begin = Stopwatch.GetTimestamp();
                using var response = await client.GetAsync(url);
                await response.Content.ReadAsByteArrayAsync();
                latencies[index] = Stopwatch.GetElapsedTime(begin).TotalMilliseconds;
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    Interlocked.Increment(ref errors);
                }
            }
        })));
        var elapsed = Stopwatch.GetElapsedTime(started).TotalSeconds;
        Array.Sort(latencies);
        double Percentile(double p) => latencies[(int)Math.Ceiling(p * latencies.Length) - 1];
        return (Percentile(0.50), Percentile(0.95), Percentile(0.99), RequestsPerEndpoint / elapsed, errors);
    }
}
