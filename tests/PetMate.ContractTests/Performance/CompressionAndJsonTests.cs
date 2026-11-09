using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using PetMate.Api.Contracts.Auth;
using PetMate.Api.Contracts.Pets;
using PetMate.Api.Contracts.Responses;
using PetMate.Api.Contracts.Users;
using PetMate.Api.Controllers;
using PetMate.Application.Security;
using PetMate.ContractTests.Authentication;
using PetMate.ContractTests.Support;
using PetMate.Domain.Users;
using Xunit.Abstractions;

namespace PetMate.ContractTests.Performance;

/// <summary>
/// Compresión HTTP (pm-048) y serialización JSON eficiente (pm-049): el efecto se mide antes y después, la compresión excluye las
/// respuestas con tokens y el JSON generado en compilación es byte a byte idéntico al de reflexión (el contrato no cambia).
/// </summary>
[Collection(ApiEnvironmentGroup.Name)]
public sealed class CompressionAndJsonTests(ITestOutputHelper output)
{
    [Trait("Endpoint", "pets-get-all")]
    [Fact]
    public async Task A_large_list_is_compressed_when_the_client_asks_and_the_effect_is_measured()
    {
        using var factory = new AuthFlowFactory();
        using var client = Client(factory);
        for (var i = 0; i < 50; i++)
        {
            await CreatePet(client, $"Mascota número {i}");
        }

        var plain = await Fetch(client, "/api/v1/pets/get-all?pageSize=50", null);
        var gzip = await Fetch(client, "/api/v1/pets/get-all?pageSize=50", "gzip");
        var brotli = await Fetch(client, "/api/v1/pets/get-all?pageSize=50", "br");

        Assert.Null(plain.Encoding);
        Assert.Equal("gzip", gzip.Encoding);
        Assert.Equal("br", brotli.Encoding);
        Assert.Contains("Accept-Encoding", gzip.Vary);
        Assert.True(gzip.Bytes.Length < plain.Bytes.Length * 0.5, "La compresión gzip debe reducir el tamaño al menos a la mitad.");
        output.WriteLine($"MEDICION pets/get-all (50 mascotas): sin compresión {plain.Bytes.Length} B; gzip {gzip.Bytes.Length} B ({100.0 * gzip.Bytes.Length / plain.Bytes.Length:F1} %); brotli {brotli.Bytes.Length} B ({100.0 * brotli.Bytes.Length / plain.Bytes.Length:F1} %).");

        // El contenido descomprimido es idéntico al original.
        using var unzipped = new System.IO.Compression.GZipStream(new MemoryStream(gzip.Bytes), System.IO.Compression.CompressionMode.Decompress);
        using var reader = new StreamReader(unzipped, Encoding.UTF8);
        Assert.Equal(Encoding.UTF8.GetString(plain.Bytes), await reader.ReadToEndAsync());
    }

    [Trait("Endpoint", "auth-login")]
    [Fact]
    public async Task Authentication_responses_with_tokens_are_never_compressed()
    {
        using var factory = new AuthFlowFactory();
        using var client = factory.CreateClient();
        var body = new { email = "compresion@example.com", password = "contraseña-muy-segura-123" };

        var register = await Post(client, "register", body);
        var login = await Post(client, "login", body);
        var failed = await Post(client, "login", new { email = "compresion@example.com", password = "otra-contraseña-123" });

        Assert.Null(register.Encoding);
        Assert.Null(login.Encoding);
        Assert.Null(failed.Encoding);
        Assert.Contains("accessToken", Encoding.UTF8.GetString(login.Bytes), StringComparison.Ordinal);
    }

    [Trait("Endpoint", "auth-login")]
    [Theory]
    [InlineData("/API/V1/AUTH/login")]
    [InlineData("/api/v1/Auth/Login")]
    public async Task The_compression_exclusion_ignores_the_case_of_the_route(string url)
    {
        using var factory = new AuthFlowFactory();
        using var client = factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent("{\"email\":\"mayus@example.com\",\"password\":\"otra-contraseña-123\"}", Encoding.UTF8, "application/json"),
        };
        request.Headers.TryAddWithoutValidation("Accept-Encoding", "gzip, br");

        using var response = await client.SendAsync(request);

        Assert.Empty(response.Content.Headers.ContentEncoding);
    }

    [Trait("Endpoint", "transversal")]
    [Fact]
    public void Source_generated_json_is_identical_to_reflection_json_and_is_not_slower()
    {
        var reflection = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        var pet = new PetResponse(Guid.NewGuid(), "Firulais", "Perro", null, new DateOnly(2020, 5, 17), "Male", "https://ejemplo.test/f.png", DateTime.UtcNow, DateTime.UtcNow);
        var samples = new object[]
        {
            new ApiResponse<object>(404, "No encontrado.", null),
            new ApiResponse<string>(200, "ok", "mediated"),
            new ApiResponse<ValidationErrorsData>(400, "inválido", new ValidationErrorsData([new ValidationErrorItem("email", "obligatorio")])),
            new ApiResponse<AuthResponse>(200, "ok", new AuthResponse("a", "Bearer", DateTime.UtcNow, "r", DateTime.UtcNow, new AuthUserResponse(Guid.NewGuid(), "a@b.test", "Owner"))),
            new ApiResponse<UserProfileResponse>(200, "ok", new UserProfileResponse(Guid.NewGuid(), "a@b.test", "Owner", null, DateTime.UtcNow)),
            new ApiResponse<PetResponse>(201, "ok", pet),
            new ApiResponse<PagedResponse<PetResponse>>(200, "ok", new PagedResponse<PetResponse>([pet, pet], 1, 20, 2, 1)),
            new ApiResponse<HealthData>(200, "ok", new HealthData("Healthy", [new DependencyHealth("redis", "Healthy")])),
        };

        foreach (var sample in samples)
        {
            Assert.Equal(
                JsonSerializer.Serialize(sample, sample.GetType(), reflection),
                JsonSerializer.Serialize(sample, sample.GetType(), ApiJsonContext.Default));
        }

        // Medición antes/después (informativa): el aserto solo impide una regresión grande.
        var page = new ApiResponse<PagedResponse<PetResponse>>(200, "ok", new PagedResponse<PetResponse>(Enumerable.Repeat(pet, 50).ToList(), 1, 50, 50, 1));
        var before = Measure(() => JsonSerializer.Serialize(page, reflection));
        var after = Measure(() => JsonSerializer.Serialize(page, ApiJsonContext.Default.ApiResponsePagedResponsePetResponse));
        output.WriteLine($"MEDICION serialización de 50 mascotas x 2000: reflexión {before:F1} ms; generado {after:F1} ms.");
        Assert.True(after < before * 3, "El serializador generado no debe ser mucho más lento que el de reflexión.");
    }

    private static double Measure(Action action)
    {
        for (var i = 0; i < 200; i++)
        {
            action();
        }

        var started = Stopwatch.GetTimestamp();
        for (var i = 0; i < 2000; i++)
        {
            action();
        }

        return Stopwatch.GetElapsedTime(started).TotalMilliseconds;
    }

    private static HttpClient Client(AuthFlowFactory factory)
    {
        var token = factory.Services.GetRequiredService<IAccessTokenService>().Issue(Guid.NewGuid(), UserRole.Owner);
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Value);
        return client;
    }

    private static async Task CreatePet(HttpClient client, string name)
    {
        using var response = await client.PostAsync(
            "/api/v1/pets/create",
            new StringContent($"{{\"name\":\"{name}\",\"species\":\"Perro\",\"sex\":\"Male\",\"breed\":\"Mestizo de talla mediana\"}}", Encoding.UTF8, "application/json"));
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    private static async Task<(byte[] Bytes, string? Encoding, string Vary)> Fetch(HttpClient client, string url, string? acceptEncoding)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        if (acceptEncoding is not null)
        {
            request.Headers.TryAddWithoutValidation("Accept-Encoding", acceptEncoding);
        }

        using var response = await client.SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return (await response.Content.ReadAsByteArrayAsync(), response.Content.Headers.ContentEncoding.SingleOrDefault(), string.Join(",", response.Headers.Vary));
    }

    private static async Task<(byte[] Bytes, string? Encoding)> Post(HttpClient client, string action, object body)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/auth/{action}")
        {
            Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json"),
        };
        request.Headers.TryAddWithoutValidation("Accept-Encoding", "gzip, br");
        using var response = await client.SendAsync(request);
        return (await response.Content.ReadAsByteArrayAsync(), response.Content.Headers.ContentEncoding.SingleOrDefault());
    }
}
