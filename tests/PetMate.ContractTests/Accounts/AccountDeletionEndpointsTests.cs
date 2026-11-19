using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using PetMate.ContractTests.Authentication;
using PetMate.ContractTests.Support;

namespace PetMate.ContractTests.Accounts;

/// <summary>Contrato HTTP de la eliminación de cuenta (pm-161 a pm-163): contraseña, sesiones y borrado de todo lo asociado.</summary>
[Collection(ApiEnvironmentGroup.Name)]
public sealed class AccountDeletionEndpointsTests
{
    private const string Password = "contrasena-segura-123";

    [Trait("Endpoint", "users-me-delete")]
    [Fact]
    public async Task A_wrong_password_deletes_nothing_and_a_correct_one_removes_the_account_with_everything_attached()
    {
        using var factory = new AuthFlowFactory();
        var (client, refresh) = await Register(factory, "ana@example.com");
        using var _ = client;
        var pet = await CreatePet(client);
        var userId = factory.Users.All.Single().Id;
        var tomorrow = DateTime.UtcNow.AddDays(30).ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
        await Send(client, HttpMethod.Post, "vaccines/create", $"{{\"petId\":\"{pet}\",\"appliedOn\":\"{DateTime.UtcNow.AddDays(-5):yyyy-MM-dd}\",\"name\":\"Rabia\",\"nextDoseOn\":\"{tomorrow}\"}}");
        await Send(client, HttpMethod.Post, "devices/register", $"{{\"token\":\"{new string('t', 40)}\"}}");
        await Send(client, HttpMethod.Put, "notification-settings/update", "{\"vaccines\":true,\"reviews\":true,\"chat\":true,\"followups\":true,\"sound\":\"soft\",\"reminderDaysBefore\":5}");
        await Send(client, HttpMethod.Post, "businesses/create", "{\"type\":\"Store\",\"name\":\"Pet Shop\",\"phone\":\"5512345678\",\"address\":\"A\",\"latitude\":19.4,\"longitude\":-99.1}");

        using var wrong = await Send(client, HttpMethod.Delete, "users/me/delete", "{\"password\":\"otra-contrasena-123\"}");
        Assert.Equal(HttpStatusCode.Forbidden, wrong.StatusCode);
        Assert.Single(factory.Users.All);
        Assert.Single(factory.Pets.All);

        using var ok = await Send(client, HttpMethod.Delete, "users/me/delete", $"{{\"password\":\"{Password}\"}}");
        using var okDoc = JsonDocument.Parse(await ok.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, ok.StatusCode);
        Assert.Equal(["data", "message", "statusCode"], okDoc.RootElement.EnumerateObject().Select(p => p.Name).Order(StringComparer.Ordinal));
        Assert.Empty(factory.Users.All);
        Assert.Empty(factory.Pets.All);
        Assert.Empty(factory.Vaccines.All);
        Assert.Empty(factory.Reminders.All);
        Assert.Empty(factory.Businesses.All);
        Assert.Equal(0, factory.Sessions.SessionCount);
        Assert.Null(factory.Devices.OwnerOf(new string('t', 40)));
        Assert.False(factory.NotificationSettings.Has(userId));
        // La sesión que hizo la petición y el refresh token ya no sirven; la cuenta no puede iniciar sesión.
        using var after = await Send(client, HttpMethod.Get, "users/me/get");
        Assert.Equal(HttpStatusCode.Unauthorized, after.StatusCode);
        using var refreshed = await Send(factory.CreateClient(), HttpMethod.Post, "auth/refresh", $"{{\"refreshToken\":\"{refresh}\"}}");
        Assert.Equal(HttpStatusCode.Unauthorized, refreshed.StatusCode);
        using var login = await Send(factory.CreateClient(), HttpMethod.Post, "auth/login", $"{{\"email\":\"ana@example.com\",\"password\":\"{Password}\"}}");
        Assert.Equal(HttpStatusCode.Unauthorized, login.StatusCode);
    }

    [Trait("Endpoint", "users-me-delete")]
    [Fact]
    public async Task Other_accounts_keep_their_data()
    {
        using var factory = new AuthFlowFactory();
        var (ana, _) = await Register(factory, "ana@example.com");
        var (luis, _) = await Register(factory, "luis@example.com");
        using var anaClient = ana;
        using var luisClient = luis;
        await CreatePet(anaClient);
        await CreatePet(luisClient);

        using var response = await Send(anaClient, HttpMethod.Delete, "users/me/delete", $"{{\"password\":\"{Password}\"}}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Single(factory.Users.All);
        Assert.Single(factory.Pets.All);
        using var luisProfile = await Send(luisClient, HttpMethod.Get, "users/me/get");
        Assert.Equal(HttpStatusCode.OK, luisProfile.StatusCode);
    }

    [Trait("Endpoint", "users-me-delete")]
    [Fact]
    public async Task Repeated_wrong_passwords_lock_the_endpoint_with_429_and_retry_after()
    {
        using var factory = new AuthFlowFactory();
        var (client, _) = await Register(factory, "ana@example.com");
        using var _ = client;

        for (var i = 0; i < 5; i++)
        {
            using var wrong = await Send(client, HttpMethod.Delete, "users/me/delete", "{\"password\":\"incorrecta-incorrecta\"}");
            Assert.Equal(HttpStatusCode.Forbidden, wrong.StatusCode);
        }

        using var locked = await Send(client, HttpMethod.Delete, "users/me/delete", $"{{\"password\":\"{Password}\"}}");

        Assert.Equal(HttpStatusCode.TooManyRequests, locked.StatusCode);
        Assert.True(locked.Headers.Contains("Retry-After"));
        Assert.Single(factory.Users.All);
    }

    [Trait("Endpoint", "users-me-delete")]
    [Theory]
    [InlineData("{}")]
    [InlineData("{\"password\":\"\"}")]
    [InlineData("{\"password\":\"x\",\"userId\":\"00000000-0000-0000-0000-000000000001\"}")]
    [InlineData("no es json")]
    public async Task An_invalid_body_returns_400_and_deletes_nothing(string body)
    {
        using var factory = new AuthFlowFactory();
        var (client, _) = await Register(factory, "ana@example.com");
        using var _ = client;

        using var response = await Send(client, HttpMethod.Delete, "users/me/delete", body);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Single(factory.Users.All);
    }

    [Trait("Endpoint", "users-me-delete")]
    [Fact]
    public async Task Deleting_the_account_requires_authentication()
    {
        using var factory = new AuthFlowFactory();
        using var client = factory.CreateClient();

        using var response = await Send(client, HttpMethod.Delete, "users/me/delete", $"{{\"password\":\"{Password}\"}}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private static async Task<(HttpClient Client, string Refresh)> Register(AuthFlowFactory factory, string email)
    {
        using var anonymous = factory.CreateClient();
        using var response = await Send(anonymous, HttpMethod.Post, "auth/register", $"{{\"displayName\":\"Ana\",\"email\":\"{email}\",\"password\":\"{Password}\"}}");
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var data = document.RootElement.GetProperty("data");
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", data.GetProperty("accessToken").GetString());
        return (client, data.GetProperty("refreshToken").GetString()!);
    }

    private static async Task<string> CreatePet(HttpClient client)
    {
        using var response = await Send(client, HttpMethod.Post, "pets/create", "{\"name\":\"Firulais\",\"species\":\"Perro\"}");
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return document.RootElement.GetProperty("data").GetProperty("id").GetString()!;
    }

    private static async Task<HttpResponseMessage> Send(HttpClient client, HttpMethod method, string route, string? body = null)
    {
        using var request = new HttpRequestMessage(method, $"/api/v1/{route}");
        if (body is not null)
        {
            request.Content = new StringContent(body, Encoding.UTF8, "application/json");
        }

        return await client.SendAsync(request);
    }
}
