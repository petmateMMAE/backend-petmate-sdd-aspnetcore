using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using PetMate.ContractTests.Authentication;
using PetMate.ContractTests.Support;

namespace PetMate.ContractTests.Devices;

/// <summary>Contrato HTTP de las preferencias de aviso (pm-157). Son propias: la cuenta sale del token.</summary>
[Collection(ApiEnvironmentGroup.Name)]
public sealed class NotificationSettingsEndpointsTests
{
    private const string Valid = "{\"vaccines\":false,\"reviews\":true,\"chat\":true,\"followups\":false,\"sound\":\"soft\",\"reminderDaysBefore\":7}";

    [Trait("Endpoint", "notification-settings-get")]
    [Fact]
    public async Task Get_returns_the_defaults_when_nothing_was_saved()
    {
        using var factory = new AuthFlowFactory();
        using var client = await Owner(factory, "ana@example.com");

        using var response = await client.GetAsync("/api/v1/notification-settings/get");
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var data = document.RootElement.GetProperty("data");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(data.GetProperty("vaccines").GetBoolean());
        Assert.Equal("default", data.GetProperty("sound").GetString());
        Assert.Equal(3, data.GetProperty("reminderDaysBefore").GetInt32());
    }

    [Trait("Endpoint", "notification-settings-update")]
    [Fact]
    public async Task Update_saves_the_preferences_and_each_account_only_sees_its_own()
    {
        using var factory = new AuthFlowFactory();
        using var ana = await Owner(factory, "ana@example.com");
        using var luis = await Owner(factory, "luis@example.com");

        using var put = await Put(ana, Valid);
        using var anaGet = await ana.GetAsync("/api/v1/notification-settings/get");
        using var luisGet = await luis.GetAsync("/api/v1/notification-settings/get");
        using var anaDoc = JsonDocument.Parse(await anaGet.Content.ReadAsStringAsync());
        using var luisDoc = JsonDocument.Parse(await luisGet.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, put.StatusCode);
        Assert.False(anaDoc.RootElement.GetProperty("data").GetProperty("vaccines").GetBoolean());
        Assert.Equal(7, anaDoc.RootElement.GetProperty("data").GetProperty("reminderDaysBefore").GetInt32());
        Assert.True(luisDoc.RootElement.GetProperty("data").GetProperty("vaccines").GetBoolean());
    }

    [Trait("Endpoint", "notification-settings-update")]
    [Theory]
    [InlineData("{\"vaccines\":true,\"reviews\":true,\"chat\":true,\"followups\":true,\"sound\":\"soft\",\"reminderDaysBefore\":0}")]
    [InlineData("{\"vaccines\":true,\"reviews\":true,\"chat\":true,\"followups\":true,\"sound\":\"soft\",\"reminderDaysBefore\":15}")]
    [InlineData("{\"vaccines\":true,\"reviews\":true,\"chat\":true,\"followups\":true,\"sound\":\"trompeta\",\"reminderDaysBefore\":3}")]
    [InlineData("{\"vaccines\":true}")]
    [InlineData("{\"vaccines\":true,\"reviews\":true,\"chat\":true,\"followups\":true,\"sound\":\"soft\",\"reminderDaysBefore\":3,\"promotions\":true}")]
    [InlineData("no es json")]
    public async Task Update_with_invalid_values_returns_400(string body)
    {
        using var factory = new AuthFlowFactory();
        using var client = await Owner(factory, "ana@example.com");

        using var response = await Put(client, body);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Trait("Endpoint", "notification-settings-get")]
    [Fact]
    public async Task Get_requires_authentication()
    {
        using var factory = new AuthFlowFactory();
        using var client = factory.CreateClient();

        using var response = await client.GetAsync("/api/v1/notification-settings/get");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Trait("Endpoint", "notification-settings-update")]
    [Fact]
    public async Task Update_requires_authentication()
    {
        using var factory = new AuthFlowFactory();
        using var client = factory.CreateClient();

        using var response = await Put(client, Valid);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private static async Task<HttpResponseMessage> Put(HttpClient client, string body)
    {
        using var content = new StringContent(body, Encoding.UTF8, "application/json");
        return await client.PutAsync("/api/v1/notification-settings/update", content);
    }

    private static async Task<HttpClient> Owner(AuthFlowFactory factory, string email)
    {
        using var anonymous = factory.CreateClient();
        using var response = await anonymous.PostAsync("/api/v1/auth/register", new StringContent(
            $"{{\"displayName\":\"Ana\",\"email\":\"{email}\",\"password\":\"contrasena-segura-123\"}}", Encoding.UTF8, "application/json"));
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", document.RootElement.GetProperty("data").GetProperty("accessToken").GetString());
        return client;
    }
}
