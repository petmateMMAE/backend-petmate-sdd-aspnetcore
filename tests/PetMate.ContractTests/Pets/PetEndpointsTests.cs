using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using PetMate.Application.Security;
using PetMate.ContractTests.Authentication;
using PetMate.ContractTests.Support;
using PetMate.Domain.Users;

namespace PetMate.ContractTests.Pets;

/// <summary>
/// Contrato HTTP de mascotas (pm-040, pm-042, pm-043, pm-044) con JWT real y repositorio en memoria. Solo el rol Owner accede
/// y el propietario sale del token: una mascota ajena o inexistente responde 404 por igual.
/// </summary>
[Collection(ApiEnvironmentGroup.Name)]
public sealed class PetEndpointsTests
{
    private const string ValidBody = "{\"name\":\"Firulais\",\"species\":\"Perro\",\"breed\":\"Mestizo\",\"birthDate\":\"2020-05-01\",\"sex\":\"Male\",\"photoUrl\":\"https://cdn.example.com/f.jpg\"}";

    // ---------- Crear (pm-040) ----------

    [Trait("Endpoint", "pets-create")]
    [Fact]
    public async Task Create_returns_201_wrapper_with_the_pet_and_never_exposes_the_owner()
    {
        using var factory = new AuthFlowFactory();
        var owner = Guid.NewGuid();
        using var client = Client(factory, owner, UserRole.User);

        using var response = await Send(client, HttpMethod.Post, "create", ValidBody);
        var raw = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(raw);
        var data = document.RootElement.GetProperty("data");

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal(201, document.RootElement.GetProperty("statusCode").GetInt32());
        Assert.Equal("Firulais", data.GetProperty("name").GetString());
        Assert.Equal("2020-05-01", data.GetProperty("birthDate").GetString());
        Assert.Equal("Male", data.GetProperty("sex").GetString());
        Assert.True(Guid.TryParse(data.GetProperty("id").GetString(), out _));
        Assert.DoesNotContain("ownerId", raw, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(owner, Assert.Single(factory.Pets.All).OwnerId);
    }

    [Trait("Endpoint", "pets-create")]
    [Theory]
    [InlineData("{\"name\":\"Firulais\",\"species\":\"Perro\",\"sex\":\"Male\",\"ownerId\":\"7d8b1d4e-1111-4b0a-9d33-2c1f8f0c7a11\"}")]
    [InlineData("{\"name\":\"Firulais\",\"species\":\"Perro\",\"sex\":\"Male\",\"id\":\"7d8b1d4e-1111-4b0a-9d33-2c1f8f0c7a11\"}")]
    [InlineData("{\"name\":\"Firulais\",\"species\":\"Perro\",\"sex\":\"Male\",\"createdAt\":\"2020-01-01T00:00:00Z\"}")]
    public async Task Create_rejects_owner_id_or_server_fields_in_the_body_and_creates_nothing(string body)
    {
        using var factory = new AuthFlowFactory();
        using var client = Client(factory, Guid.NewGuid(), UserRole.User);

        using var response = await Send(client, HttpMethod.Post, "create", body);

        await AssertWrapper(response, HttpStatusCode.BadRequest);
        Assert.Empty(factory.Pets.All);
    }

    [Trait("Endpoint", "pets-create")]
    [Fact]
    public async Task Create_without_sex_stores_Unknown_and_accepts_Gato()
    {
        using var factory = new AuthFlowFactory();
        using var client = Client(factory, Guid.NewGuid(), UserRole.User);

        using var response = await Send(client, HttpMethod.Post, "create", "{\"name\":\"Misu\",\"species\":\"Gato\"}");
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var data = document.RootElement.GetProperty("data");

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal("Unknown", data.GetProperty("sex").GetString());
        Assert.Equal("Gato", data.GetProperty("species").GetString());
    }

    [Trait("Endpoint", "pets-create")]
    [Theory]
    [InlineData("{\"name\":\"\",\"species\":\"Perro\",\"sex\":\"Male\"}", "name")]
    [InlineData("{\"name\":\"Firulais\",\"species\":\" \",\"sex\":\"Male\"}", "species")]
    [InlineData("{\"name\":\"Firulais\",\"species\":\"Perro\",\"sex\":\"Macho\"}", "sex")]
    [InlineData("{\"name\":\"Firulais\",\"species\":\"Conejo\",\"sex\":\"Male\"}", "species")]
    [InlineData("{\"name\":\"Firulais\",\"species\":\"perro\",\"sex\":\"Male\"}", "species")]
    [InlineData("{\"name\":\"Firulais\",\"species\":\"Perro\",\"sex\":\"Male\",\"birthDate\":\"2999-01-01\"}", "birthDate")]
    [InlineData("{\"name\":\"Firulais\",\"species\":\"Perro\",\"sex\":\"Male\",\"photoUrl\":\"http://inseguro.example.com/f.jpg\"}", "photoUrl")]
    public async Task Create_with_an_invalid_field_returns_400_naming_the_field(string body, string field)
    {
        using var factory = new AuthFlowFactory();
        using var client = Client(factory, Guid.NewGuid(), UserRole.User);

        using var response = await Send(client, HttpMethod.Post, "create", body);

        await AssertFieldError(response, field);
        Assert.Empty(factory.Pets.All);
    }

    [Trait("Endpoint", "pets-create")]
    [Theory]
    [InlineData("")]
    [InlineData("no-es-json")]
    [InlineData("{\"name\":\"Firulais\",\"species\":\"Perro\",\"sex\":\"Male\",\"birthDate\":\"01/05/2020\"}")]
    public async Task Create_with_an_empty_or_malformed_body_returns_400_wrapper(string body)
    {
        using var factory = new AuthFlowFactory();
        using var client = Client(factory, Guid.NewGuid(), UserRole.User);

        using var response = await Send(client, HttpMethod.Post, "create", body);

        await AssertWrapper(response, HttpStatusCode.BadRequest);
    }

    [Trait("Endpoint", "pets-create")]
    [Fact]
    public async Task Create_requires_an_owner_token()
    {
        using var factory = new AuthFlowFactory();
        using var anonymous = factory.CreateClient();

        using var unauthenticated = await Send(anonymous, HttpMethod.Post, "create", ValidBody);

        await AssertWrapper(unauthenticated, HttpStatusCode.Unauthorized);
        Assert.Empty(factory.Pets.All);
    }

    // ---------- Detalle (pm-042) ----------

    [Trait("Endpoint", "pets-get-by-id")]
    [Fact]
    public async Task Get_by_id_returns_the_owner_pet()
    {
        using var factory = new AuthFlowFactory();
        using var client = Client(factory, Guid.NewGuid(), UserRole.User);
        var id = await CreatePet(client);

        using var response = await Send(client, HttpMethod.Get, $"get-by-id/{id}");
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(id, document.RootElement.GetProperty("data").GetProperty("id").GetString());
        Assert.Equal("Perro", document.RootElement.GetProperty("data").GetProperty("species").GetString());
    }

    [Trait("Endpoint", "pets-get-by-id")]
    [Fact]
    public async Task Get_by_id_of_another_owner_missing_or_malformed_id_returns_404()
    {
        using var factory = new AuthFlowFactory();
        using var ana = Client(factory, Guid.NewGuid(), UserRole.User);
        using var luis = Client(factory, Guid.NewGuid(), UserRole.User);
        var id = await CreatePet(ana);

        using var foreign = await Send(luis, HttpMethod.Get, $"get-by-id/{id}");
        using var missing = await Send(ana, HttpMethod.Get, $"get-by-id/{Guid.NewGuid()}");
        using var malformed = await Send(ana, HttpMethod.Get, "get-by-id/no-es-un-guid");

        await AssertWrapper(foreign, HttpStatusCode.NotFound);
        await AssertWrapper(missing, HttpStatusCode.NotFound);
        Assert.Equal(HttpStatusCode.NotFound, malformed.StatusCode);
    }

    [Trait("Endpoint", "pets-get-by-id")]
    [Fact]
    public async Task Get_by_id_requires_an_owner_token()
    {
        using var factory = new AuthFlowFactory();
        using var owner = Client(factory, Guid.NewGuid(), UserRole.User);
        var id = await CreatePet(owner);
        using var anonymous = factory.CreateClient();

        await AssertWrapper(await Send(anonymous, HttpMethod.Get, $"get-by-id/{id}"), HttpStatusCode.Unauthorized);
    }

    // ---------- Actualizar (pm-043) ----------

    [Trait("Endpoint", "pets-update")]
    [Fact]
    public async Task Update_replaces_the_editable_fields_of_the_owner_pet()
    {
        using var factory = new AuthFlowFactory();
        using var client = Client(factory, Guid.NewGuid(), UserRole.User);
        var id = await CreatePet(client);

        using var response = await Send(client, HttpMethod.Put, $"update/{id}", "{\"name\":\"  Max  \",\"species\":\"Perro\",\"sex\":\"Unknown\"}");
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var data = document.RootElement.GetProperty("data");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Max", data.GetProperty("name").GetString());
        Assert.Equal("Unknown", data.GetProperty("sex").GetString());
        Assert.Equal(JsonValueKind.Null, data.GetProperty("breed").ValueKind);
        Assert.Equal(JsonValueKind.Null, data.GetProperty("photoUrl").ValueKind);
        Assert.Equal("Max", Assert.Single(factory.Pets.All).Name);
    }

    [Trait("Endpoint", "pets-update")]
    [Theory]
    [InlineData("{\"name\":\"\",\"species\":\"Perro\",\"sex\":\"Male\"}", "name")]
    [InlineData("{\"name\":\"Max\",\"species\":\"Perro\",\"sex\":\"Male\",\"breed\":\"" + "rrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrr" + "\"}", "breed")]
    public async Task Update_with_an_invalid_field_returns_400_and_changes_nothing(string body, string field)
    {
        using var factory = new AuthFlowFactory();
        using var client = Client(factory, Guid.NewGuid(), UserRole.User);
        var id = await CreatePet(client);

        using var response = await Send(client, HttpMethod.Put, $"update/{id}", body);

        await AssertFieldError(response, field);
        Assert.Equal("Firulais", Assert.Single(factory.Pets.All).Name);
    }

    [Trait("Endpoint", "pets-update")]
    [Fact]
    public async Task Update_rejects_an_owner_id_in_the_body()
    {
        using var factory = new AuthFlowFactory();
        using var client = Client(factory, Guid.NewGuid(), UserRole.User);
        var id = await CreatePet(client);

        using var response = await Send(client, HttpMethod.Put, $"update/{id}", "{\"name\":\"Max\",\"species\":\"Perro\",\"sex\":\"Male\",\"ownerId\":\"" + Guid.NewGuid() + "\"}");

        await AssertWrapper(response, HttpStatusCode.BadRequest);
        Assert.Equal("Firulais", Assert.Single(factory.Pets.All).Name);
    }

    [Trait("Endpoint", "pets-update")]
    [Fact]
    public async Task Update_of_another_owner_or_missing_pet_returns_404_and_changes_nothing()
    {
        using var factory = new AuthFlowFactory();
        using var ana = Client(factory, Guid.NewGuid(), UserRole.User);
        using var luis = Client(factory, Guid.NewGuid(), UserRole.User);
        var id = await CreatePet(ana);
        const string body = "{\"name\":\"Robado\",\"species\":\"Perro\",\"sex\":\"Male\"}";

        await AssertWrapper(await Send(luis, HttpMethod.Put, $"update/{id}", body), HttpStatusCode.NotFound);
        await AssertWrapper(await Send(ana, HttpMethod.Put, $"update/{Guid.NewGuid()}", body), HttpStatusCode.NotFound);
        Assert.Equal("Firulais", Assert.Single(factory.Pets.All).Name);
    }

    [Trait("Endpoint", "pets-update")]
    [Fact]
    public async Task Update_requires_an_owner_token()
    {
        using var factory = new AuthFlowFactory();
        using var owner = Client(factory, Guid.NewGuid(), UserRole.User);
        var id = await CreatePet(owner);
        using var anonymous = factory.CreateClient();

        await AssertWrapper(await Send(anonymous, HttpMethod.Put, $"update/{id}", ValidBody), HttpStatusCode.Unauthorized);
    }

    // ---------- Eliminar (pm-044) ----------

    [Trait("Endpoint", "pets-delete")]
    [Fact]
    public async Task Delete_removes_the_owner_pet_physically_and_then_it_no_longer_exists()
    {
        using var factory = new AuthFlowFactory();
        using var client = Client(factory, Guid.NewGuid(), UserRole.User);
        var id = await CreatePet(client);

        using var response = await Send(client, HttpMethod.Delete, $"delete/{id}");
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(JsonValueKind.Null, document.RootElement.GetProperty("data").ValueKind);
        Assert.Empty(factory.Pets.All);
        await AssertWrapper(await Send(client, HttpMethod.Get, $"get-by-id/{id}"), HttpStatusCode.NotFound);
        await AssertWrapper(await Send(client, HttpMethod.Delete, $"delete/{id}"), HttpStatusCode.NotFound);
    }

    [Trait("Endpoint", "pets-delete")]
    [Fact]
    public async Task Delete_of_another_owner_pet_returns_404_and_deletes_nothing()
    {
        using var factory = new AuthFlowFactory();
        using var ana = Client(factory, Guid.NewGuid(), UserRole.User);
        using var luis = Client(factory, Guid.NewGuid(), UserRole.User);
        var id = await CreatePet(ana);

        await AssertWrapper(await Send(luis, HttpMethod.Delete, $"delete/{id}"), HttpStatusCode.NotFound);
        Assert.Single(factory.Pets.All);
    }

    [Trait("Endpoint", "pets-delete")]
    [Fact]
    public async Task Delete_requires_an_owner_token()
    {
        using var factory = new AuthFlowFactory();
        using var owner = Client(factory, Guid.NewGuid(), UserRole.User);
        var id = await CreatePet(owner);
        using var anonymous = factory.CreateClient();

        await AssertWrapper(await Send(anonymous, HttpMethod.Delete, $"delete/{id}"), HttpStatusCode.Unauthorized);
        Assert.Single(factory.Pets.All);
    }

    // ---------- QR (pm-094, pm-095) ----------

    [Trait("Endpoint", "pets-get-qr")]
    [Fact]
    public async Task Get_qr_returns_token_and_code_for_the_owner_pet_with_no_store()
    {
        using var factory = new AuthFlowFactory();
        using var client = Client(factory, Guid.NewGuid(), UserRole.User);
        var id = await CreatePet(client);

        using var response = await Send(client, HttpMethod.Get, $"get-qr/{id}");
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var data = document.RootElement.GetProperty("data");
        using var detail = await Send(client, HttpMethod.Get, $"get-by-id/{id}");
        var detailBody = await detail.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(id, data.GetProperty("petId").GetString());
        Assert.Equal(43, data.GetProperty("qrToken").GetString()!.Length);
        Assert.Equal(8, data.GetProperty("qrCode").GetString()!.Length);
        Assert.Contains("no-store", response.Headers.CacheControl?.ToString(), StringComparison.Ordinal);
        Assert.DoesNotContain("qrToken", detailBody, StringComparison.Ordinal);
        Assert.DoesNotContain("qrCode", detailBody, StringComparison.Ordinal);
    }

    [Trait("Endpoint", "pets-get-qr")]
    [Fact]
    public async Task Get_qr_of_another_owner_pet_or_a_missing_pet_returns_the_same_404()
    {
        using var factory = new AuthFlowFactory();
        using var ana = Client(factory, Guid.NewGuid(), UserRole.User);
        using var luis = Client(factory, Guid.NewGuid(), UserRole.User);
        var id = await CreatePet(ana);

        using var foreign = await Send(luis, HttpMethod.Get, $"get-qr/{id}");
        using var missing = await Send(luis, HttpMethod.Get, $"get-qr/{Guid.NewGuid()}");

        var foreignBody = await foreign.Content.ReadAsStringAsync();
        var missingBody = await missing.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.NotFound, foreign.StatusCode);
        Assert.Equal(missingBody, foreignBody);
        Assert.DoesNotContain(factory.Pets.All.Single().Qr.Token, foreignBody, StringComparison.Ordinal);
    }

    [Trait("Endpoint", "pets-get-qr")]
    [Fact]
    public async Task Get_qr_requires_an_owner_token()
    {
        using var factory = new AuthFlowFactory();
        using var owner = Client(factory, Guid.NewGuid(), UserRole.User);
        var id = await CreatePet(owner);
        using var anonymous = factory.CreateClient();

        await AssertWrapper(await Send(anonymous, HttpMethod.Get, $"get-qr/{id}"), HttpStatusCode.Unauthorized);
    }

    [Trait("Endpoint", "pets-regenerate-qr")]
    [Fact]
    public async Task Regenerate_qr_replaces_token_and_code_and_the_old_ones_stop_being_current()
    {
        using var factory = new AuthFlowFactory();
        using var client = Client(factory, Guid.NewGuid(), UserRole.User);
        var id = await CreatePet(client);
        var before = factory.Pets.All.Single().Qr;

        using var response = await Send(client, HttpMethod.Post, $"regenerate-qr/{id}");
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var data = document.RootElement.GetProperty("data");
        using var read = await Send(client, HttpMethod.Get, $"get-qr/{id}");
        using var readDocument = JsonDocument.Parse(await read.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("no-store", response.Headers.CacheControl?.ToString(), StringComparison.Ordinal);
        Assert.NotEqual(before.Token, data.GetProperty("qrToken").GetString());
        Assert.NotEqual(before.Code, data.GetProperty("qrCode").GetString());
        Assert.Equal(data.GetProperty("qrToken").GetString(), readDocument.RootElement.GetProperty("data").GetProperty("qrToken").GetString());
    }

    [Trait("Endpoint", "pets-regenerate-qr")]
    [Fact]
    public async Task Regenerate_qr_of_another_owner_pet_returns_404_and_keeps_the_qr()
    {
        using var factory = new AuthFlowFactory();
        using var ana = Client(factory, Guid.NewGuid(), UserRole.User);
        using var luis = Client(factory, Guid.NewGuid(), UserRole.User);
        var id = await CreatePet(ana);
        var before = factory.Pets.All.Single().Qr;

        await AssertWrapper(await Send(luis, HttpMethod.Post, $"regenerate-qr/{id}"), HttpStatusCode.NotFound);

        Assert.Equal(before, factory.Pets.All.Single().Qr);
    }

    [Trait("Endpoint", "pets-regenerate-qr")]
    [Fact]
    public async Task Regenerate_qr_requires_an_owner_token()
    {
        using var factory = new AuthFlowFactory();
        using var owner = Client(factory, Guid.NewGuid(), UserRole.User);
        var id = await CreatePet(owner);
        using var anonymous = factory.CreateClient();

        await AssertWrapper(await Send(anonymous, HttpMethod.Post, $"regenerate-qr/{id}"), HttpStatusCode.Unauthorized);
    }

    // ---------- helpers ----------

    private static HttpClient Client(AuthFlowFactory factory, Guid userId, UserRole role)
    {
        var token = factory.Services.GetRequiredService<IAccessTokenService>().Issue(userId, role);
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Value);
        return client;
    }

    private static async Task<string> CreatePet(HttpClient client)
    {
        using var response = await Send(client, HttpMethod.Post, "create", ValidBody);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return document.RootElement.GetProperty("data").GetProperty("id").GetString()!;
    }

    private static async Task<HttpResponseMessage> Send(HttpClient client, HttpMethod method, string action, string? body = null)
    {
        using var request = new HttpRequestMessage(method, $"/api/v1/pets/{action}");
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
        var raw = await response.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        using var document = JsonDocument.Parse(raw);
        var errors = document.RootElement.GetProperty("data").GetProperty("errors");
        Assert.Contains(errors.EnumerateArray(), error => error.GetProperty("field").GetString() == field);
        Assert.DoesNotContain("inseguro", raw, StringComparison.Ordinal);
    }
}
