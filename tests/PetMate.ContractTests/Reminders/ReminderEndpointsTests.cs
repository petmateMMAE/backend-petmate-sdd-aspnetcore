using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using PetMate.Application.Security;
using PetMate.ContractTests.Authentication;
using PetMate.ContractTests.Support;
using PetMate.Domain.Users;

namespace PetMate.ContractTests.Reminders;

/// <summary>Contrato HTTP de los recordatorios (pm-154, pm-156): se generan al registrar una próxima fecha y solo los ve su dueño.</summary>
[Collection(ApiEnvironmentGroup.Name)]
public sealed class ReminderEndpointsTests
{
    [Trait("Endpoint", "reminders-get-all")]
    [Fact]
    public async Task Next_dates_become_reminders_ordered_by_closeness_with_the_pet_name_and_three_days_of_notice()
    {
        using var factory = new AuthFlowFactory();
        using var client = Client(factory, Guid.NewGuid());
        var pet = await CreatePet(client);
        await Send(client, HttpMethod.Post, "vaccines/create", $"{{\"petId\":\"{pet}\",\"appliedOn\":\"{Days(-30)}\",\"name\":\"Rabia\",\"nextDoseOn\":\"{Days(40)}\"}}");
        await Send(client, HttpMethod.Post, "dewormings/create", $"{{\"petId\":\"{pet}\",\"appliedOn\":\"{Days(-10)}\",\"product\":\"Drontal\",\"nextOn\":\"{Days(10)}\"}}");
        await Send(client, HttpMethod.Post, "vaccines/create", $"{{\"petId\":\"{pet}\",\"appliedOn\":\"{Days(-1)}\",\"name\":\"Sin próxima\"}}");

        using var response = await Send(client, HttpMethod.Get, "reminders/get-all");
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var items = document.RootElement.GetProperty("data").GetProperty("items").EnumerateArray().ToArray();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(["Drontal", "Rabia"], items.Select(i => i.GetProperty("title").GetString()));
        Assert.Equal(["Deworming", "Vaccine"], items.Select(i => i.GetProperty("kind").GetString()));
        Assert.All(items, i => Assert.Equal("Firulais", i.GetProperty("petName").GetString()));
        Assert.All(items, i => Assert.False(i.GetProperty("isRead").GetBoolean()));
        Assert.Equal(Days(7), items[0].GetProperty("remindOn").GetString());
    }

    [Trait("Endpoint", "reminders-get-all")]
    [Fact]
    public async Task Deleting_the_record_or_the_pet_removes_the_reminder_and_each_owner_only_sees_their_own()
    {
        using var factory = new AuthFlowFactory();
        using var ana = Client(factory, Guid.NewGuid());
        using var luis = Client(factory, Guid.NewGuid());
        var pet = await CreatePet(ana);
        using var created = await Send(ana, HttpMethod.Post, "vaccines/create", $"{{\"petId\":\"{pet}\",\"appliedOn\":\"{Days(-30)}\",\"name\":\"Rabia\",\"nextDoseOn\":\"{Days(40)}\"}}");
        using var createdDoc = JsonDocument.Parse(await created.Content.ReadAsStringAsync());
        var vaccineId = createdDoc.RootElement.GetProperty("data").GetProperty("id").GetString();
        await Send(ana, HttpMethod.Post, "dewormings/create", $"{{\"petId\":\"{pet}\",\"appliedOn\":\"{Days(-10)}\",\"product\":\"Drontal\",\"nextOn\":\"{Days(10)}\"}}");

        Assert.Equal(0, await Count(luis));
        Assert.Equal(2, await Count(ana));

        await Send(ana, HttpMethod.Delete, $"vaccines/delete/{vaccineId}");
        Assert.Equal(1, await Count(ana));

        await Send(ana, HttpMethod.Delete, $"pets/delete/{pet}");
        Assert.Equal(0, await Count(ana));
        Assert.Empty(factory.Reminders.All);
    }

    [Trait("Endpoint", "reminders-get-all")]
    [Theory]
    [InlineData("page=0")]
    [InlineData("pageSize=1000")]
    public async Task Invalid_paging_returns_400(string query)
    {
        using var factory = new AuthFlowFactory();
        using var client = Client(factory, Guid.NewGuid());

        using var response = await Send(client, HttpMethod.Get, $"reminders/get-all?{query}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Trait("Endpoint", "reminders-get-all")]
    [Fact]
    public async Task Reminders_require_authentication()
    {
        using var factory = new AuthFlowFactory();
        using var client = factory.CreateClient();

        using var response = await Send(client, HttpMethod.Get, "reminders/get-all");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private static async Task<int> Count(HttpClient client)
    {
        using var response = await Send(client, HttpMethod.Get, "reminders/get-all");
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return document.RootElement.GetProperty("data").GetProperty("items").GetArrayLength();
    }

    [Trait("Endpoint", "reminders-mark-read")]
    [Fact]
    public async Task Marking_a_reminder_read_shows_it_read_is_idempotent_and_a_stranger_gets_404()
    {
        using var factory = new AuthFlowFactory();
        using var ana = Client(factory, Guid.NewGuid());
        using var luis = Client(factory, Guid.NewGuid());
        var pet = await CreatePet(ana);
        await Send(ana, HttpMethod.Post, "dewormings/create", $"{{\"petId\":\"{pet}\",\"appliedOn\":\"{Days(-10)}\",\"product\":\"Drontal\",\"nextOn\":\"{Days(10)}\"}}");
        var id = factory.Reminders.All.Single().SourceId;

        using var stranger = await Send(luis, HttpMethod.Post, "reminders/mark-read", $"{{\"id\":\"{id}\"}}");
        using var first = await Send(ana, HttpMethod.Post, "reminders/mark-read", $"{{\"id\":\"{id}\"}}");
        using var second = await Send(ana, HttpMethod.Post, "reminders/mark-read", $"{{\"id\":\"{id}\"}}");
        using var list = await Send(ana, HttpMethod.Get, "reminders/get-all");
        using var document = JsonDocument.Parse(await list.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.NotFound, stranger.StatusCode);
        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Equal(HttpStatusCode.OK, second.StatusCode);
        Assert.True(document.RootElement.GetProperty("data").GetProperty("items")[0].GetProperty("isRead").GetBoolean());
    }

    [Trait("Endpoint", "reminders-mark-read")]
    [Theory]
    [InlineData("{}")]
    [InlineData("{\"id\":\"00000000-0000-0000-0000-000000000000\"}")]
    [InlineData("{\"id\":\"6f1c1b0e-6c3b-4a55-9a52-0d5a9d2b7c11\",\"extra\":1}")]
    public async Task Marking_read_with_a_missing_empty_or_unknown_member_returns_400(string body)
    {
        using var factory = new AuthFlowFactory();
        using var client = Client(factory, Guid.NewGuid());

        using var response = await Send(client, HttpMethod.Post, "reminders/mark-read", body);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private static string Days(int offset) => DateTime.UtcNow.AddDays(offset).ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);

    private static HttpClient Client(AuthFlowFactory factory, Guid userId)
    {
        var token = factory.Services.GetRequiredService<IAccessTokenService>().Issue(userId, UserRole.User);
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Value);
        return client;
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
