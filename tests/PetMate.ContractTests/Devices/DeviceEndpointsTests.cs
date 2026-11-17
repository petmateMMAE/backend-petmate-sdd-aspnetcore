using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using PetMate.ContractTests.Authentication;
using PetMate.ContractTests.Support;

namespace PetMate.ContractTests.Devices;

/// <summary>Contrato HTTP del registro de dispositivo (pm-151). El usuario sale del token, nunca del cuerpo.</summary>
[Collection(ApiEnvironmentGroup.Name)]
public sealed class DeviceEndpointsTests
{
    private static readonly string Token = new('a', 40);

    [Trait("Endpoint", "devices-register")]
    [Fact]
    public async Task Register_stores_the_token_and_repeating_it_does_not_duplicate_it()
    {
        using var factory = new AuthFlowFactory();
        using var client = await Owner(factory, "ana@example.com");

        await AssertWrapper(await Post(client, $"{{\"token\":\"{Token}\"}}"), HttpStatusCode.OK);
        await AssertWrapper(await Post(client, $"{{\"token\":\"{Token}\"}}"), HttpStatusCode.OK);

        Assert.Equal(1, factory.Devices.CountFor(factory.Devices.OwnerOf(Token)!.Value));
    }

    [Trait("Endpoint", "devices-register")]
    [Fact]
    public async Task A_token_registered_by_another_account_moves_to_the_current_one()
    {
        using var factory = new AuthFlowFactory();
        using var ana = await Owner(factory, "ana@example.com");
        using var luis = await Owner(factory, "luis@example.com");

        await AssertWrapper(await Post(ana, $"{{\"token\":\"{Token}\"}}"), HttpStatusCode.OK);
        var anaId = factory.Devices.OwnerOf(Token)!.Value;
        await AssertWrapper(await Post(luis, $"{{\"token\":\"{Token}\"}}"), HttpStatusCode.OK);

        Assert.NotEqual(anaId, factory.Devices.OwnerOf(Token));
        Assert.Equal(0, factory.Devices.CountFor(anaId));
    }

    [Trait("Endpoint", "devices-register")]
    [Fact]
    public async Task The_sixth_distinct_device_returns_409()
    {
        using var factory = new AuthFlowFactory();
        using var client = await Owner(factory, "ana@example.com");

        for (var i = 0; i < 5; i++)
        {
            await AssertWrapper(await Post(client, $"{{\"token\":\"{new string((char)('a' + i), 40)}\"}}"), HttpStatusCode.OK);
        }

        await AssertWrapper(await Post(client, $"{{\"token\":\"{new string('z', 40)}\"}}"), HttpStatusCode.Conflict);
        await AssertWrapper(await Post(client, $"{{\"token\":\"{new string('a', 40)}\"}}"), HttpStatusCode.OK);
    }

    [Trait("Endpoint", "devices-register")]
    [Theory]
    [InlineData("{\"token\":\"corto\"}")]
    [InlineData("{\"token\":\"con espacios en medio del token del dispositivo\"}")]
    [InlineData("{}")]
    [InlineData("{\"token\":\"aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa\",\"userId\":\"00000000-0000-0000-0000-000000000001\"}")]
    [InlineData("no es json")]
    public async Task Register_with_an_invalid_token_or_body_returns_400(string body)
    {
        using var factory = new AuthFlowFactory();
        using var client = await Owner(factory, "ana@example.com");

        await AssertWrapper(await Post(client, body), HttpStatusCode.BadRequest);
    }

    [Trait("Endpoint", "devices-register")]
    [Fact]
    public async Task Register_requires_authentication()
    {
        using var factory = new AuthFlowFactory();
        using var client = factory.CreateClient();

        await AssertWrapper(await Post(client, $"{{\"token\":\"{Token}\"}}"), HttpStatusCode.Unauthorized);
    }

    [Trait("Endpoint", "devices-unregister")]
    [Fact]
    public async Task Unregister_removes_only_the_own_token_and_is_idempotent()
    {
        using var factory = new AuthFlowFactory();
        using var ana = await Owner(factory, "ana@example.com");
        using var luis = await Owner(factory, "luis@example.com");
        await AssertWrapper(await Post(ana, $"{{\"token\":\"{Token}\"}}"), HttpStatusCode.OK);

        await AssertWrapper(await Post(luis, $"{{\"token\":\"{Token}\"}}", "unregister"), HttpStatusCode.OK);
        Assert.NotNull(factory.Devices.OwnerOf(Token));

        await AssertWrapper(await Post(ana, $"{{\"token\":\"{Token}\"}}", "unregister"), HttpStatusCode.OK);
        await AssertWrapper(await Post(ana, $"{{\"token\":\"{Token}\"}}", "unregister"), HttpStatusCode.OK);
        Assert.Null(factory.Devices.OwnerOf(Token));
    }

    [Trait("Endpoint", "devices-unregister")]
    [Theory]
    [InlineData("{\"token\":\"corto\"}")]
    [InlineData("{}")]
    [InlineData("{\"token\":\"aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa\",\"userId\":\"x\"}")]
    public async Task Unregister_with_an_invalid_body_returns_400(string body)
    {
        using var factory = new AuthFlowFactory();
        using var client = await Owner(factory, "ana@example.com");

        await AssertWrapper(await Post(client, body, "unregister"), HttpStatusCode.BadRequest);
    }

    [Trait("Endpoint", "devices-unregister")]
    [Fact]
    public async Task Unregister_requires_authentication()
    {
        using var factory = new AuthFlowFactory();
        using var client = factory.CreateClient();

        await AssertWrapper(await Post(client, $"{{\"token\":\"{Token}\"}}", "unregister"), HttpStatusCode.Unauthorized);
    }

    private static async Task<HttpResponseMessage> Post(HttpClient client, string body, string action = "register")
    {
        using var content = new StringContent(body, Encoding.UTF8, "application/json");
        return await client.PostAsync($"/api/v1/devices/{action}", content);
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

    private static async Task AssertWrapper(HttpResponseMessage response, HttpStatusCode expected)
    {
        using (response)
        {
            using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            Assert.Equal(expected, response.StatusCode);
            Assert.Equal(["data", "message", "statusCode"], document.RootElement.EnumerateObject().Select(p => p.Name).Order(StringComparer.Ordinal));
            Assert.Equal((int)expected, document.RootElement.GetProperty("statusCode").GetInt32());
        }
    }
}
