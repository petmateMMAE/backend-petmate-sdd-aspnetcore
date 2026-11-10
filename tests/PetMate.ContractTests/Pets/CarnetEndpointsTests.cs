using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using PetMate.Application.Security;
using PetMate.ContractTests.Authentication;
using PetMate.ContractTests.Support;
using PetMate.Domain.Carnet;
using PetMate.Domain.Users;

namespace PetMate.ContractTests.Pets;

/// <summary>
/// Contrato HTTP del carnet de salud (pm-097 a pm-103) con JWT real y repositorios en memoria. Solo el rol Owner; el dueño sale
/// del token; mascota o registro ajenos responden 404 por igual; los registros de negocio son de solo lectura (403).
/// </summary>
[Collection(ApiEnvironmentGroup.Name)]
public sealed class CarnetEndpointsTests
{
    private const string PetBody = "{\"name\":\"Firulais\",\"species\":\"Perro\"}";

    // ---------- Vacunas ----------

    [Trait("Endpoint", "vaccines-create")]
    [Fact]
    public async Task Vaccine_create_returns_201_with_owner_origin_and_list_shows_it_most_recent_first()
    {
        using var factory = new AuthFlowFactory();
        var owner = Guid.NewGuid();
        using var client = Client(factory, owner, UserRole.User);
        var pet = await CreatePet(client);

        using var first = await Send(client, HttpMethod.Post, "vaccines/create", $"{{\"petId\":\"{pet}\",\"appliedOn\":\"{Days(-30)}\",\"name\":\"  Rabia \",\"batch\":\"L1\",\"nextDoseOn\":\"{Days(335)}\"}}");
        using var second = await Send(client, HttpMethod.Post, "vaccines/create", $"{{\"petId\":\"{pet}\",\"appliedOn\":\"{Days(-1)}\",\"name\":\"Parvovirus\"}}");
        using var firstDoc = JsonDocument.Parse(await first.Content.ReadAsStringAsync());
        using var list = await Send(client, HttpMethod.Get, $"vaccines/get-all?petId={pet}");
        using var listDoc = JsonDocument.Parse(await list.Content.ReadAsStringAsync());
        var data = listDoc.RootElement.GetProperty("data");

        Assert.Equal(HttpStatusCode.Created, first.StatusCode);
        Assert.Equal(HttpStatusCode.Created, second.StatusCode);
        Assert.Equal("Rabia", firstDoc.RootElement.GetProperty("data").GetProperty("name").GetString());
        Assert.Equal("Owner", firstDoc.RootElement.GetProperty("data").GetProperty("origin").GetProperty("type").GetString());
        Assert.Equal(2, data.GetProperty("totalCount").GetInt32());
        Assert.Equal("Parvovirus", data.GetProperty("items")[0].GetProperty("name").GetString());
        Assert.Equal(Days(335), data.GetProperty("items")[1].GetProperty("nextDoseOn").GetString());
    }

    [Trait("Endpoint", "vaccines-create")]
    [Theory]
    [InlineData("appliedOn", "\"appliedOn\":\"2999-01-01\",\"name\":\"X\"")]
    [InlineData("appliedOn", "\"name\":\"X\"")]
    [InlineData("name", "\"appliedOn\":\"2026-01-01\",\"name\":\" \"")]
    [InlineData("nextDoseOn", "\"appliedOn\":\"2026-01-10\",\"name\":\"X\",\"nextDoseOn\":\"2026-01-10\"")]
    public async Task Vaccine_create_with_an_invalid_field_returns_400_naming_it(string field, string members)
    {
        using var factory = new AuthFlowFactory();
        using var client = Client(factory, Guid.NewGuid(), UserRole.User);
        var pet = await CreatePet(client);

        using var response = await Send(client, HttpMethod.Post, "vaccines/create", $"{{\"petId\":\"{pet}\",{members}}}");

        await AssertFieldError(response, field);
        Assert.Empty(factory.Vaccines.All);
    }

    [Trait("Endpoint", "vaccines-create")]
    [Fact]
    public async Task Vaccine_create_for_another_owner_pet_returns_404_and_stores_nothing()
    {
        using var factory = new AuthFlowFactory();
        using var ana = Client(factory, Guid.NewGuid(), UserRole.User);
        using var luis = Client(factory, Guid.NewGuid(), UserRole.User);
        var pet = await CreatePet(ana);

        using var response = await Send(luis, HttpMethod.Post, "vaccines/create", $"{{\"petId\":\"{pet}\",\"appliedOn\":\"{Days(-1)}\",\"name\":\"X\"}}");

        await AssertWrapper(response, HttpStatusCode.NotFound);
        Assert.Empty(factory.Vaccines.All);
    }

    [Trait("Endpoint", "vaccines-create")]
    [Fact]
    public async Task Vaccine_create_stops_at_200_records_per_pet_with_409()
    {
        using var factory = new AuthFlowFactory();
        var owner = Guid.NewGuid();
        using var client = Client(factory, owner, UserRole.User);
        var pet = Guid.Parse(await CreatePet(client));
        for (var i = 0; i < 200; i++)
        {
            await factory.Vaccines.Add(NewVaccine(pet, owner, RecordProvenance.ByOwner), CancellationToken.None);
        }

        using var response = await Send(client, HttpMethod.Post, "vaccines/create", $"{{\"petId\":\"{pet}\",\"appliedOn\":\"{Days(-1)}\",\"name\":\"X\"}}");

        await AssertWrapper(response, HttpStatusCode.Conflict);
        Assert.Equal(200, factory.Vaccines.All.Count);
    }

    [Trait("Endpoint", "vaccines-create")]
    [Fact]
    public async Task Vaccine_create_requires_an_owner_token()
    {
        using var factory = new AuthFlowFactory();
        using var anonymous = factory.CreateClient();
        const string body = "{\"petId\":\"00000000-0000-0000-0000-000000000001\",\"appliedOn\":\"2026-01-01\",\"name\":\"X\"}";

        await AssertWrapper(await Send(anonymous, HttpMethod.Post, "vaccines/create", body), HttpStatusCode.Unauthorized);
    }

    [Trait("Endpoint", "vaccines-get-all")]
    [Fact]
    public async Task Vaccine_list_requires_pet_id_validates_paging_and_hides_foreign_pets()
    {
        using var factory = new AuthFlowFactory();
        using var ana = Client(factory, Guid.NewGuid(), UserRole.User);
        using var luis = Client(factory, Guid.NewGuid(), UserRole.User);
        var pet = await CreatePet(ana);

        await AssertFieldError(await Send(ana, HttpMethod.Get, "vaccines/get-all"), "petId");
        await AssertFieldError(await Send(ana, HttpMethod.Get, $"vaccines/get-all?petId={pet}&pageSize=51"), "pageSize");
        await AssertWrapper(await Send(luis, HttpMethod.Get, $"vaccines/get-all?petId={pet}"), HttpStatusCode.NotFound);
        await AssertWrapper(await Send(ana, HttpMethod.Get, $"vaccines/get-all?petId={Guid.NewGuid()}"), HttpStatusCode.NotFound);
    }

    [Trait("Endpoint", "vaccines-get-all")]
    [Fact]
    public async Task Vaccine_list_requires_an_owner_token()
    {
        using var factory = new AuthFlowFactory();
        using var anonymous = factory.CreateClient();

        await AssertWrapper(await Send(anonymous, HttpMethod.Get, $"vaccines/get-all?petId={Guid.NewGuid()}"), HttpStatusCode.Unauthorized);
    }

    [Trait("Endpoint", "vaccines-delete")]
    [Fact]
    public async Task Vaccine_delete_removes_own_records_but_not_business_ones_nor_foreign_ones()
    {
        using var factory = new AuthFlowFactory();
        var owner = Guid.NewGuid();
        using var client = Client(factory, owner, UserRole.User);
        using var other = Client(factory, Guid.NewGuid(), UserRole.User);
        var pet = Guid.Parse(await CreatePet(client));
        var own = NewVaccine(pet, owner, RecordProvenance.ByOwner);
        var fromBusiness = NewVaccine(pet, owner, RecordProvenance.ByBusiness(Guid.NewGuid(), "Veterinaria Sol"));
        await factory.Vaccines.Add(own, CancellationToken.None);
        await factory.Vaccines.Add(fromBusiness, CancellationToken.None);

        await AssertWrapper(await Send(other, HttpMethod.Delete, $"vaccines/delete/{own.Id}"), HttpStatusCode.NotFound);
        using var blocked = await Send(client, HttpMethod.Delete, $"vaccines/delete/{fromBusiness.Id}");
        var blockedBody = await blocked.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.Forbidden, blocked.StatusCode);
        Assert.Contains("lo registró el negocio", blockedBody, StringComparison.Ordinal);
        Assert.Equal(2, factory.Vaccines.All.Count);

        await AssertWrapper(await Send(client, HttpMethod.Delete, $"vaccines/delete/{own.Id}"), HttpStatusCode.OK);
        Assert.Equal(fromBusiness.Id, Assert.Single(factory.Vaccines.All).Id);
        await AssertWrapper(await Send(client, HttpMethod.Delete, $"vaccines/delete/{own.Id}"), HttpStatusCode.NotFound);
    }

    [Trait("Endpoint", "vaccines-delete")]
    [Fact]
    public async Task Vaccine_delete_requires_an_owner_token()
    {
        using var factory = new AuthFlowFactory();
        using var anonymous = factory.CreateClient();

        await AssertWrapper(await Send(anonymous, HttpMethod.Delete, $"vaccines/delete/{Guid.NewGuid()}"), HttpStatusCode.Unauthorized);
    }

    // ---------- Desparasitaciones ----------

    [Trait("Endpoint", "dewormings-create")]
    [Fact]
    public async Task Deworming_create_accepts_type_and_future_next_date_and_list_returns_it()
    {
        using var factory = new AuthFlowFactory();
        using var client = Client(factory, Guid.NewGuid(), UserRole.User);
        var pet = await CreatePet(client);

        using var created = await Send(client, HttpMethod.Post, "dewormings/create", $"{{\"petId\":\"{pet}\",\"appliedOn\":\"{Days(-2)}\",\"product\":\"Drontal\",\"type\":\"Internal\",\"nextOn\":\"{Days(88)}\"}}");
        using var list = await Send(client, HttpMethod.Get, $"dewormings/get-all?petId={pet}");
        using var listDoc = JsonDocument.Parse(await list.Content.ReadAsStringAsync());
        var item = listDoc.RootElement.GetProperty("data").GetProperty("items")[0];

        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        Assert.Equal("Internal", item.GetProperty("type").GetString());
        Assert.Equal(Days(88), item.GetProperty("nextOn").GetString());
    }

    [Trait("Endpoint", "dewormings-create")]
    [Theory]
    [InlineData("nextOn", "\"appliedOn\":\"2026-01-10\",\"product\":\"X\",\"nextOn\":\"2000-01-01\"")]
    [InlineData("type", "\"appliedOn\":\"2026-01-10\",\"product\":\"X\",\"type\":\"Otro\"")]
    [InlineData("product", "\"appliedOn\":\"2026-01-10\"")]
    [InlineData("appliedOn", "\"appliedOn\":\"2999-01-01\",\"product\":\"X\"")]
    public async Task Deworming_create_with_an_invalid_field_returns_400_naming_it(string field, string members)
    {
        using var factory = new AuthFlowFactory();
        using var client = Client(factory, Guid.NewGuid(), UserRole.User);
        var pet = await CreatePet(client);

        await AssertFieldError(await Send(client, HttpMethod.Post, "dewormings/create", $"{{\"petId\":\"{pet}\",{members}}}"), field);
        Assert.Empty(factory.Dewormings.All);
    }

    [Trait("Endpoint", "dewormings-get-all")]
    [Fact]
    public async Task Deworming_list_hides_foreign_pets()
    {
        using var factory = new AuthFlowFactory();
        using var ana = Client(factory, Guid.NewGuid(), UserRole.User);
        using var luis = Client(factory, Guid.NewGuid(), UserRole.User);
        var pet = await CreatePet(ana);

        await AssertWrapper(await Send(luis, HttpMethod.Get, $"dewormings/get-all?petId={pet}"), HttpStatusCode.NotFound);
        await AssertWrapper(await Send(factory.CreateClient(), HttpMethod.Get, $"dewormings/get-all?petId={pet}"), HttpStatusCode.Unauthorized);
    }

    [Trait("Endpoint", "dewormings-delete")]
    [Fact]
    public async Task Deworming_delete_removes_own_records_but_not_business_ones()
    {
        using var factory = new AuthFlowFactory();
        var owner = Guid.NewGuid();
        using var client = Client(factory, owner, UserRole.User);
        var pet = Guid.Parse(await CreatePet(client));
        var details = new DewormingDetails(DateOnly.Parse(Days(-3), System.Globalization.CultureInfo.InvariantCulture), "Drontal", null, null, null);
        var own = Deworming.Create(pet, owner, RecordProvenance.ByOwner, details, Today(factory), factory.Clock.GetUtcNow().UtcDateTime);
        var fromBusiness = Deworming.Create(pet, owner, RecordProvenance.ByBusiness(Guid.NewGuid(), "Veterinaria Sol"), details, Today(factory), factory.Clock.GetUtcNow().UtcDateTime);
        await factory.Dewormings.Add(own, CancellationToken.None);
        await factory.Dewormings.Add(fromBusiness, CancellationToken.None);

        await AssertWrapper(await Send(client, HttpMethod.Delete, $"dewormings/delete/{fromBusiness.Id}"), HttpStatusCode.Forbidden);
        await AssertWrapper(await Send(client, HttpMethod.Delete, $"dewormings/delete/{own.Id}"), HttpStatusCode.OK);
        await AssertWrapper(await Send(factory.CreateClient(), HttpMethod.Delete, $"dewormings/delete/{own.Id}"), HttpStatusCode.Unauthorized);
        Assert.Equal(fromBusiness.Id, Assert.Single(factory.Dewormings.All).Id);
    }

    // ---------- Pesos ----------

    [Trait("Endpoint", "weights-create")]
    [Fact]
    public async Task Weight_create_returns_201_and_list_goes_from_newest_to_oldest()
    {
        using var factory = new AuthFlowFactory();
        using var client = Client(factory, Guid.NewGuid(), UserRole.User);
        var pet = await CreatePet(client);

        using var old = await Send(client, HttpMethod.Post, "weights/create", $"{{\"petId\":\"{pet}\",\"measuredOn\":\"{Days(-60)}\",\"kilograms\":8.5}}");
        using var recent = await Send(client, HttpMethod.Post, "weights/create", $"{{\"petId\":\"{pet}\",\"measuredOn\":\"{Days(-1)}\",\"kilograms\":9.2}}");
        using var list = await Send(client, HttpMethod.Get, $"weights/get-all?petId={pet}");
        using var listDoc = JsonDocument.Parse(await list.Content.ReadAsStringAsync());
        var items = listDoc.RootElement.GetProperty("data").GetProperty("items");

        Assert.Equal(HttpStatusCode.Created, old.StatusCode);
        Assert.Equal(HttpStatusCode.Created, recent.StatusCode);
        Assert.Equal(9.2m, items[0].GetProperty("kilograms").GetDecimal());
        Assert.Equal(8.5m, items[1].GetProperty("kilograms").GetDecimal());
    }

    [Trait("Endpoint", "weights-create")]
    [Theory]
    [InlineData("kilograms", "\"measuredOn\":\"2026-01-10\",\"kilograms\":0")]
    [InlineData("kilograms", "\"measuredOn\":\"2026-01-10\",\"kilograms\":-3")]
    [InlineData("kilograms", "\"measuredOn\":\"2026-01-10\",\"kilograms\":8.55")]
    [InlineData("kilograms", "\"measuredOn\":\"2026-01-10\"")]
    [InlineData("measuredOn", "\"measuredOn\":\"2999-01-01\",\"kilograms\":8.5")]
    public async Task Weight_create_with_an_invalid_field_returns_400_naming_it(string field, string members)
    {
        using var factory = new AuthFlowFactory();
        using var client = Client(factory, Guid.NewGuid(), UserRole.User);
        var pet = await CreatePet(client);

        await AssertFieldError(await Send(client, HttpMethod.Post, "weights/create", $"{{\"petId\":\"{pet}\",{members}}}"), field);
        Assert.Empty(factory.Weights.All);
    }

    [Trait("Endpoint", "weights-get-all")]
    [Fact]
    public async Task Weight_list_hides_foreign_pets_and_requires_an_owner_token()
    {
        using var factory = new AuthFlowFactory();
        using var ana = Client(factory, Guid.NewGuid(), UserRole.User);
        using var luis = Client(factory, Guid.NewGuid(), UserRole.User);
        var pet = await CreatePet(ana);

        await AssertWrapper(await Send(luis, HttpMethod.Get, $"weights/get-all?petId={pet}"), HttpStatusCode.NotFound);
        await AssertWrapper(await Send(luis, HttpMethod.Post, "weights/create", $"{{\"petId\":\"{pet}\",\"measuredOn\":\"{Days(-1)}\",\"kilograms\":5}}"), HttpStatusCode.NotFound);
    }

    // ---------- Alergias y condiciones ----------

    [Trait("Endpoint", "allergies-create")]
    [Fact]
    public async Task Allergy_create_starts_active_and_the_list_puts_active_before_resolved()
    {
        using var factory = new AuthFlowFactory();
        using var client = Client(factory, Guid.NewGuid(), UserRole.User);
        var pet = await CreatePet(client);

        using var created = await Send(client, HttpMethod.Post, "allergies/create", $"{{\"petId\":\"{pet}\",\"name\":\"Pollo\",\"kind\":\"Allergy\",\"notes\":\"Urticaria\"}}");
        using var createdDoc = JsonDocument.Parse(await created.Content.ReadAsStringAsync());
        var id = createdDoc.RootElement.GetProperty("data").GetProperty("id").GetString();
        await Send(client, HttpMethod.Post, "allergies/create", $"{{\"petId\":\"{pet}\",\"name\":\"Displasia\",\"kind\":\"Condition\"}}");
        await Send(client, HttpMethod.Put, $"allergies/update/{id}", "{\"name\":\"Pollo\",\"kind\":\"Allergy\",\"status\":\"Resolved\"}");
        using var list = await Send(client, HttpMethod.Get, $"allergies/get-all?petId={pet}");
        using var listDoc = JsonDocument.Parse(await list.Content.ReadAsStringAsync());
        var items = listDoc.RootElement.GetProperty("data").GetProperty("items");

        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        Assert.Equal("Active", createdDoc.RootElement.GetProperty("data").GetProperty("status").GetString());
        Assert.Equal("Displasia", items[0].GetProperty("name").GetString());
        Assert.Equal("Resolved", items[1].GetProperty("status").GetString());
    }

    [Trait("Endpoint", "allergies-create")]
    [Theory]
    [InlineData("name", "\"kind\":\"Allergy\"")]
    [InlineData("kind", "\"name\":\"X\",\"kind\":\"Otra\"")]
    [InlineData("kind", "\"name\":\"X\"")]
    public async Task Allergy_create_with_an_invalid_field_returns_400_naming_it(string field, string members)
    {
        using var factory = new AuthFlowFactory();
        using var client = Client(factory, Guid.NewGuid(), UserRole.User);
        var pet = await CreatePet(client);

        await AssertFieldError(await Send(client, HttpMethod.Post, "allergies/create", $"{{\"petId\":\"{pet}\",{members}}}"), field);
        Assert.Empty(factory.Allergies.All);
    }

    [Trait("Endpoint", "allergies-get-all")]
    [Fact]
    public async Task Allergy_list_hides_foreign_pets_and_requires_an_owner_token()
    {
        using var factory = new AuthFlowFactory();
        using var ana = Client(factory, Guid.NewGuid(), UserRole.User);
        using var luis = Client(factory, Guid.NewGuid(), UserRole.User);
        var pet = await CreatePet(ana);

        await AssertWrapper(await Send(luis, HttpMethod.Get, $"allergies/get-all?petId={pet}"), HttpStatusCode.NotFound);
        await AssertWrapper(await Send(factory.CreateClient(), HttpMethod.Get, $"allergies/get-all?petId={pet}"), HttpStatusCode.Unauthorized);
    }

    [Trait("Endpoint", "allergies-update")]
    [Fact]
    public async Task Allergy_update_resolves_and_reactivates_and_rejects_foreign_business_and_incomplete_requests()
    {
        using var factory = new AuthFlowFactory();
        var owner = Guid.NewGuid();
        using var client = Client(factory, owner, UserRole.User);
        using var other = Client(factory, Guid.NewGuid(), UserRole.User);
        var pet = Guid.Parse(await CreatePet(client));
        using var created = await Send(client, HttpMethod.Post, "allergies/create", $"{{\"petId\":\"{pet}\",\"name\":\"Pollo\",\"kind\":\"Allergy\"}}");
        using var createdDoc = JsonDocument.Parse(await created.Content.ReadAsStringAsync());
        var id = createdDoc.RootElement.GetProperty("data").GetProperty("id").GetString();
        var fromBusiness = Allergy.Create(pet, owner, RecordProvenance.ByBusiness(Guid.NewGuid(), "Veterinaria Sol"), new AllergyDetails("Penicilina", AllergyKind.Allergy, null, null), factory.Clock.GetUtcNow().UtcDateTime);
        await factory.Allergies.Add(fromBusiness, CancellationToken.None);

        using var resolved = await Send(client, HttpMethod.Put, $"allergies/update/{id}", "{\"name\":\"Pollo crudo\",\"kind\":\"Condition\",\"notes\":\"Ya no\",\"status\":\"Resolved\"}");
        using var resolvedDoc = JsonDocument.Parse(await resolved.Content.ReadAsStringAsync());
        using var reactivated = await Send(client, HttpMethod.Put, $"allergies/update/{id}", "{\"name\":\"Pollo crudo\",\"kind\":\"Condition\",\"status\":\"Active\"}");
        using var reactivatedDoc = JsonDocument.Parse(await reactivated.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, resolved.StatusCode);
        Assert.Equal("Resolved", resolvedDoc.RootElement.GetProperty("data").GetProperty("status").GetString());
        Assert.Equal("Condition", resolvedDoc.RootElement.GetProperty("data").GetProperty("kind").GetString());
        Assert.Equal("Active", reactivatedDoc.RootElement.GetProperty("data").GetProperty("status").GetString());
        await AssertFieldError(await Send(client, HttpMethod.Put, $"allergies/update/{id}", "{\"name\":\"X\",\"kind\":\"Allergy\"}"), "status");
        await AssertWrapper(await Send(other, HttpMethod.Put, $"allergies/update/{id}", "{\"name\":\"X\",\"kind\":\"Allergy\",\"status\":\"Active\"}"), HttpStatusCode.NotFound);
        await AssertWrapper(await Send(client, HttpMethod.Put, $"allergies/update/{fromBusiness.Id}", "{\"name\":\"X\",\"kind\":\"Allergy\",\"status\":\"Resolved\"}"), HttpStatusCode.Forbidden);
        await AssertWrapper(await Send(factory.CreateClient(), HttpMethod.Put, $"allergies/update/{id}", "{\"name\":\"X\",\"kind\":\"Allergy\",\"status\":\"Active\"}"), HttpStatusCode.Unauthorized);
    }

    // ---------- Transversales ----------

    [Trait("Endpoint", "vaccines-create")]
    [Theory]
    [InlineData("ownerId")]
    [InlineData("origin")]
    [InlineData("createdAt")]
    public async Task Carnet_requests_reject_members_the_client_must_not_send(string member)
    {
        using var factory = new AuthFlowFactory();
        using var client = Client(factory, Guid.NewGuid(), UserRole.User);
        var pet = await CreatePet(client);

        using var response = await Send(client, HttpMethod.Post, "vaccines/create", $"{{\"petId\":\"{pet}\",\"appliedOn\":\"{Days(-1)}\",\"name\":\"X\",\"{member}\":\"x\"}}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Empty(factory.Vaccines.All);
    }

    [Trait("Endpoint", "vaccines-get-all")]
    [Theory]
    [InlineData("vaccines/get-all")]
    [InlineData("dewormings/get-all")]
    [InlineData("weights/get-all")]
    [InlineData("allergies/get-all")]
    public async Task Every_carnet_controller_applies_the_per_user_rate_limit_with_429(string route)
    {
        using var factory = new AuthFlowFactory(carnetRateLimitPerMinute: 2);
        using var client = Client(factory, Guid.NewGuid(), UserRole.User);
        var pet = await CreatePet(client);

        await Send(client, HttpMethod.Get, $"{route}?petId={pet}");
        await Send(client, HttpMethod.Get, $"{route}?petId={pet}");
        using var limited = await Send(client, HttpMethod.Get, $"{route}?petId={pet}");

        Assert.Equal(HttpStatusCode.TooManyRequests, limited.StatusCode);
        Assert.True(limited.Headers.Contains("Retry-After"));
    }

    // ---------- helpers ----------

    private static string Days(int offset) => DateTime.UtcNow.AddDays(offset).ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);

    private static DateOnly Today(AuthFlowFactory factory) => DateOnly.FromDateTime(factory.Clock.GetUtcNow().UtcDateTime);

    private static Vaccine NewVaccine(Guid pet, Guid owner, RecordProvenance provenance)
        => Vaccine.Create(pet, owner, provenance, new VaccineDetails(DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-5), "Rabia", null, null, null), DateOnly.FromDateTime(DateTime.UtcNow), DateTime.UtcNow);

    private static HttpClient Client(AuthFlowFactory factory, Guid userId, UserRole role)
    {
        var token = factory.Services.GetRequiredService<IAccessTokenService>().Issue(userId, role);
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Value);
        return client;
    }

    private static async Task<string> CreatePet(HttpClient client)
    {
        using var response = await Send(client, HttpMethod.Post, "pets/create", PetBody);
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

    private static async Task AssertWrapper(HttpResponseMessage response, HttpStatusCode expected)
    {
        using (response)
        {
            var raw = await response.Content.ReadAsStringAsync();
            Assert.Equal(expected, response.StatusCode);
            using var document = JsonDocument.Parse(raw);
            Assert.Equal(["data", "message", "statusCode"], document.RootElement.EnumerateObject().Select(p => p.Name).Order(StringComparer.Ordinal));
            Assert.Equal((int)expected, document.RootElement.GetProperty("statusCode").GetInt32());
        }
    }

    private static async Task AssertFieldError(HttpResponseMessage response, string field)
    {
        using (response)
        {
            var raw = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            using var document = JsonDocument.Parse(raw);
            var errors = document.RootElement.GetProperty("data").GetProperty("errors");
            Assert.Contains(errors.EnumerateArray(), error => error.GetProperty("field").GetString() == field);
        }
    }
}
