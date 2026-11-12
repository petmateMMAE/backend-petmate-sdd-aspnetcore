using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using PetMate.ContractTests.Authentication;
using PetMate.ContractTests.Support;

namespace PetMate.ContractTests.Businesses;

/// <summary>
/// Contrato HTTP del mapa público, las autorizaciones de acceso y el escaneo (pm-116 a pm-129) con repositorios, caché y
/// throttle en memoria. Un negocio solo ve a una mascota con una autorización vigente que lo incluya.
/// </summary>
[Collection(ApiEnvironmentGroup.Name)]
public sealed class MapAndAccessEndpointsTests
{
    private const string Password = "contrasena-segura-123";

    private static string Biz(string type, string name, double lat, double lng, string extra = "")
        => $"{{\"type\":\"{type}\",\"name\":\"{name}\",\"phone\":\"5512345678\",\"address\":\"Calle 1\",\"latitude\":{lat.ToString(System.Globalization.CultureInfo.InvariantCulture)},\"longitude\":{lng.ToString(System.Globalization.CultureInfo.InvariantCulture)}{(type == "Veterinary" ? ",\"licenseNumber\":\"CED-SECRETA-9\"" : string.Empty)}{extra}}}";

    // ---------- Mapa público ----------

    [Trait("Endpoint", "businesses-get-nearby")]
    [Fact]
    public async Task Nearby_returns_active_businesses_inside_the_radius_nearest_first_for_an_anonymous_visitor()
    {
        using var factory = new AuthFlowFactory();
        using var near = await Account(factory, "near@example.com");
        using var mid = await Account(factory, "mid@example.com");
        using var far = await Account(factory, "far@example.com");
        await Send(near, HttpMethod.Post, "businesses/create", Biz("Store", "Cerca", 19.4330, -99.1330));
        await Send(mid, HttpMethod.Post, "businesses/create", Biz("Veterinary", "Media", 19.4500, -99.1330));
        await Send(far, HttpMethod.Post, "businesses/create", Biz("Grooming", "Lejos", 25.6866, -100.3161));
        using var visitor = factory.CreateClient();

        using var response = await Send(visitor, HttpMethod.Get, "businesses/get-nearby?latitude=19.4326&longitude=-99.1332");
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var data = document.RootElement.GetProperty("data");
        var items = data.GetProperty("items").EnumerateArray().ToArray();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(["Cerca", "Media"], items.Select(i => i.GetProperty("name").GetString()!).ToArray());
        Assert.True(items[0].GetProperty("distanceMeters").GetDouble() < items[1].GetProperty("distanceMeters").GetDouble());
        Assert.Equal(["distanceMeters", "id", "isOpenNow", "name", "photoUrl", "todayHours", "type"], items[0].EnumerateObject().Select(p => p.Name).Order(StringComparer.Ordinal));
        Assert.DoesNotContain("CED-SECRETA", await response.Content.ReadAsStringAsync(), StringComparison.Ordinal);
        Assert.Equal(2, data.GetProperty("totalCount").GetInt32());
    }

    [Trait("Endpoint", "businesses-get-nearby")]
    [Fact]
    public async Task Nearby_filters_by_one_or_several_types_and_by_name_ignoring_case_and_accents_combined_with_proximity()
    {
        using var factory = new AuthFlowFactory();
        using var a = await Account(factory, "a@example.com");
        using var b = await Account(factory, "b@example.com");
        using var c = await Account(factory, "c@example.com");
        await Send(a, HttpMethod.Post, "businesses/create", Biz("Veterinary", "Veterinaria Peñalosa", 19.43, -99.13));
        await Send(b, HttpMethod.Post, "businesses/create", Biz("Store", "Tienda Peñalosa", 19.431, -99.13));
        await Send(c, HttpMethod.Post, "businesses/create", Biz("Grooming", "Estética Luna", 19.432, -99.13));
        using var visitor = factory.CreateClient();
        const string at = "latitude=19.43&longitude=-99.13";

        Assert.Equal(["Veterinaria Peñalosa"], await Names(visitor, $"businesses/get-nearby?{at}&types=Veterinary"));
        Assert.Equal(2, (await Names(visitor, $"businesses/get-nearby?{at}&types=Veterinary,Grooming")).Length);
        Assert.Equal(3, (await Names(visitor, $"businesses/get-nearby?{at}")).Length);
        Assert.Equal(2, (await Names(visitor, $"businesses/get-nearby?{at}&search=PENALOSA")).Length);
        Assert.Equal(["Tienda Peñalosa"], await Names(visitor, $"businesses/get-nearby?{at}&search=peñalosa&types=Store"));
        Assert.Equal(["Estética Luna"], await Names(visitor, $"businesses/get-nearby?{at}&search=ESTETICA%20luna"));
        Assert.Empty(await Names(visitor, $"businesses/get-nearby?{at}&search=.*"));
        Assert.Empty(await Names(visitor, $"businesses/get-nearby?{at}&radiusKm=0.01&search=luna"));
    }

    [Trait("Endpoint", "businesses-get-nearby")]
    [Theory]
    [InlineData("latitude=19&longitude=-99&types=Hospital", "types")]
    [InlineData("latitude=19&longitude=-99&types=veterinary", "types")]
    [InlineData("longitude=-99", "latitude")]
    [InlineData("latitude=19", "longitude")]
    [InlineData("latitude=91&longitude=-99", "latitude")]
    [InlineData("latitude=19&longitude=-99&radiusKm=51", "radiusKm")]
    [InlineData("latitude=19&longitude=-99&radiusKm=0", "radiusKm")]
    [InlineData("latitude=19&longitude=-99&search=a", "search")]
    [InlineData("latitude=19&longitude=-99&pageSize=51", "pageSize")]
    public async Task Nearby_with_invalid_parameters_returns_400_naming_the_field(string query, string field)
    {
        using var factory = new AuthFlowFactory();
        using var visitor = factory.CreateClient();

        using var response = await Send(visitor, HttpMethod.Get, $"businesses/get-nearby?{query}");
        var raw = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains($"\"field\":\"{field}\"", raw, StringComparison.Ordinal);
    }

    [Trait("Endpoint", "businesses-get-nearby")]
    [Fact]
    public async Task Nearby_excludes_deleted_businesses_reflects_a_new_location_and_the_cache_is_invalidated_on_change()
    {
        using var factory = new AuthFlowFactory();
        using var owner = await Account(factory, "a@example.com");
        using var visitor = factory.CreateClient();
        await Send(owner, HttpMethod.Post, "businesses/create", Biz("Store", "Mi tienda", 19.43, -99.13));
        const string at = "businesses/get-nearby?latitude=19.43&longitude=-99.13";

        Assert.Equal(["Mi tienda"], await Names(visitor, at));
        Assert.True(factory.Cache.Count > 0);
        await Send(owner, HttpMethod.Put, "businesses/update-location", "{\"latitude\":25.6,\"longitude\":-100.3}");
        Assert.Empty(await Names(visitor, at));
        await Send(owner, HttpMethod.Put, "businesses/update-location", "{\"latitude\":19.43,\"longitude\":-99.13}");
        await Send(owner, HttpMethod.Put, "businesses/update", "{\"name\":\"Nombre nuevo\",\"phone\":\"5512345678\"}");
        Assert.Equal(["Nombre nuevo"], await Names(visitor, at));
        await Send(owner, HttpMethod.Delete, "businesses/delete");
        Assert.Empty(await Names(visitor, at));
    }

    [Trait("Endpoint", "businesses-get-nearby")]
    [Fact]
    public async Task A_cache_failure_does_not_change_the_response()
    {
        using var factory = new AuthFlowFactory();
        using var owner = await Account(factory, "a@example.com");
        using var visitor = factory.CreateClient();
        await Send(owner, HttpMethod.Post, "businesses/create", Biz("Store", "Mi tienda", 19.43, -99.13));
        factory.Cache.Failing = true;

        Assert.Equal(["Mi tienda"], await Names(visitor, "businesses/get-nearby?latitude=19.43&longitude=-99.13"));
        using var profile = await Send(visitor, HttpMethod.Get, $"businesses/get-by-id/{factory.Businesses.All.Single().Id}");
        Assert.Equal(HttpStatusCode.OK, profile.StatusCode);
    }

    [Trait("Endpoint", "businesses-get-nearby")]
    [Fact]
    public async Task Public_reads_are_limited_per_ip_with_429_and_retry_after()
    {
        using var factory = new AuthFlowFactory(publicBusinessRateLimitPerMinute: 2);
        using var visitor = factory.CreateClient();

        await Send(visitor, HttpMethod.Get, "businesses/get-nearby?latitude=19&longitude=-99");
        await Send(visitor, HttpMethod.Get, "businesses/get-nearby?latitude=19&longitude=-99");
        using var limited = await Send(visitor, HttpMethod.Get, "businesses/get-nearby?latitude=19&longitude=-99");

        Assert.Equal(HttpStatusCode.TooManyRequests, limited.StatusCode);
        Assert.True(limited.Headers.Contains("Retry-After"));
    }

    [Trait("Endpoint", "businesses-get-by-id")]
    [Fact]
    public async Task Public_profile_has_services_contact_and_schedule_but_never_the_license_and_missing_contact_is_null()
    {
        using var factory = new AuthFlowFactory();
        using var owner = await Account(factory, "a@example.com");
        await Send(owner, HttpMethod.Post, "businesses/create", Biz("Veterinary", "Vet Sol", 19.4, -99.1, ",\"description\":\"Urgencias\",\"schedule\":{\"monday\":[{\"open\":\"09:00\",\"close\":\"18:00\"}]}"));
        await Send(owner, HttpMethod.Post, "services/create", "{\"name\":\"Consulta\",\"price\":350}");
        await Send(owner, HttpMethod.Post, "services/create", "{\"name\":\"Vacuna\"}");
        using var visitor = factory.CreateClient();

        using var response = await Send(visitor, HttpMethod.Get, $"businesses/get-by-id/{factory.Businesses.All.Single().Id}");
        var raw = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(raw);
        var data = document.RootElement.GetProperty("data");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Vet Sol", data.GetProperty("name").GetString());
        Assert.Equal("Urgencias", data.GetProperty("description").GetString());
        Assert.Equal(JsonValueKind.Null, data.GetProperty("whatsApp").ValueKind);
        Assert.Equal("5512345678", data.GetProperty("phone").GetString());
        Assert.Equal(2, data.GetProperty("services").GetArrayLength());
        Assert.Equal("09:00", data.GetProperty("schedule").GetProperty("monday")[0].GetProperty("open").GetString());
        Assert.DoesNotContain("CED-SECRETA", raw, StringComparison.Ordinal);
        Assert.DoesNotContain("licenseNumber", raw, StringComparison.OrdinalIgnoreCase);
    }

    [Trait("Endpoint", "businesses-get-by-id")]
    [Fact]
    public async Task Public_profile_of_a_missing_or_deleted_business_returns_404_and_reflects_service_changes()
    {
        using var factory = new AuthFlowFactory();
        using var owner = await Account(factory, "a@example.com");
        using var visitor = factory.CreateClient();
        await Send(owner, HttpMethod.Post, "businesses/create", Biz("Store", "Mi tienda", 19.4, -99.1));
        var id = factory.Businesses.All.Single().Id;
        using var created = await Send(owner, HttpMethod.Post, "services/create", "{\"name\":\"Baño\"}");
        using var createdDoc = JsonDocument.Parse(await created.Content.ReadAsStringAsync());
        var serviceId = createdDoc.RootElement.GetProperty("data").GetProperty("id").GetString();

        Assert.Equal(1, await ServiceCount(visitor, id));
        await Send(owner, HttpMethod.Put, $"services/update/{serviceId}", "{\"name\":\"Baño y corte\"}");
        using var afterUpdate = await Send(visitor, HttpMethod.Get, $"businesses/get-by-id/{id}");
        Assert.Contains("Baño y corte", await afterUpdate.Content.ReadAsStringAsync(), StringComparison.Ordinal);
        await Send(owner, HttpMethod.Delete, $"services/delete/{serviceId}");
        Assert.Equal(0, await ServiceCount(visitor, id));
        await Send(owner, HttpMethod.Delete, "businesses/delete");
        using var gone = await Send(visitor, HttpMethod.Get, $"businesses/get-by-id/{id}");
        using var missing = await Send(visitor, HttpMethod.Get, $"businesses/get-by-id/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, gone.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
    }

    // ---------- Autorizaciones de acceso ----------

    [Trait("Endpoint", "access-grants-create")]
    [Fact]
    public async Task Grant_create_accepts_only_the_three_durations_and_always_includes_allergies()
    {
        using var factory = new AuthFlowFactory();
        using var owner = await Account(factory, "dueno@example.com");
        var pet = await CreatePet(owner);

        foreach (var minutes in new[] { 30, 1440, 10080 })
        {
            using var ok = await Send(owner, HttpMethod.Post, "access-grants/create", $"{{\"petId\":\"{pet}\",\"data\":[\"Vaccines\"],\"durationMinutes\":{minutes}}}");
            using var doc = JsonDocument.Parse(await ok.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.Created, ok.StatusCode);
            Assert.True(doc.RootElement.GetProperty("data").GetProperty("includesAllergies").GetBoolean());
            Assert.Equal(JsonValueKind.Null, doc.RootElement.GetProperty("data").GetProperty("businessId").ValueKind);
        }

        foreach (var bad in new[] { 0, 29, 60, 43200 })
        {
            await AssertFieldError(await Send(owner, HttpMethod.Post, "access-grants/create", $"{{\"petId\":\"{pet}\",\"data\":[],\"durationMinutes\":{bad}}}"), "durationMinutes");
        }

        await AssertFieldError(await Send(owner, HttpMethod.Post, "access-grants/create", $"{{\"petId\":\"{pet}\",\"data\":[\"Allergies\"],\"durationMinutes\":30}}"), "data");
        Assert.Equal(3, factory.AccessGrants.All.Count);
    }

    [Trait("Endpoint", "access-grants-create")]
    [Fact]
    public async Task Grant_create_on_a_foreign_pet_returns_404_and_there_is_no_endpoint_to_revoke_or_list()
    {
        using var factory = new AuthFlowFactory();
        using var ana = await Account(factory, "ana@example.com");
        using var luis = await Account(factory, "luis@example.com");
        var pet = await CreatePet(ana);

        await AssertWrapper(await Send(luis, HttpMethod.Post, "access-grants/create", $"{{\"petId\":\"{pet}\",\"data\":[],\"durationMinutes\":30}}"), HttpStatusCode.NotFound);
        await AssertWrapper(await Send(factory.CreateClient(), HttpMethod.Post, "access-grants/create", $"{{\"petId\":\"{pet}\",\"data\":[],\"durationMinutes\":30}}"), HttpStatusCode.Unauthorized);
        Assert.Empty(factory.AccessGrants.All);
        foreach (var route in new[] { "access-grants/get-all", "access-grants/delete/" + Guid.NewGuid(), "access-grants/revoke/" + Guid.NewGuid() })
        {
            using var response = await Send(ana, HttpMethod.Get, route);
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            using var deleted = await Send(ana, HttpMethod.Delete, route);
            Assert.True(deleted.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.MethodNotAllowed);
        }
    }

    [Trait("Endpoint", "access-grants-create")]
    [Fact]
    public async Task Grant_for_a_specific_business_must_name_an_existing_one()
    {
        using var factory = new AuthFlowFactory();
        using var owner = await Account(factory, "dueno@example.com");
        var pet = await CreatePet(owner);

        await AssertFieldError(await Send(owner, HttpMethod.Post, "access-grants/create", $"{{\"petId\":\"{pet}\",\"businessId\":\"{Guid.NewGuid()}\",\"data\":[],\"durationMinutes\":30}}"), "businessId");
    }

    // ---------- Escaneo ----------

    [Trait("Endpoint", "scans-resolve")]
    [Fact]
    public async Task Scan_with_a_valid_grant_returns_exactly_the_basic_ficha_and_logs_the_scan()
    {
        using var factory = new AuthFlowFactory();
        using var owner = await Account(factory, "dueno@example.com");
        using var store = await Account(factory, "tienda@example.com");
        await Send(store, HttpMethod.Post, "businesses/create", Biz("Store", "Tienda", 19.4, -99.1));
        var pet = await CreatePet(owner, "{\"name\":\"Firulais\",\"species\":\"Perro\",\"breed\":\"Mestizo\",\"birthDate\":\"2020-05-01\"}");
        await Send(owner, HttpMethod.Post, "allergies/create", $"{{\"petId\":\"{pet}\",\"name\":\"Pollo\",\"kind\":\"Allergy\",\"notes\":\"Urticaria\"}}");
        using var resolved = await Send(owner, HttpMethod.Post, "allergies/create", $"{{\"petId\":\"{pet}\",\"name\":\"Polen\",\"kind\":\"Allergy\"}}");
        using var resolvedDoc = JsonDocument.Parse(await resolved.Content.ReadAsStringAsync());
        await Send(owner, HttpMethod.Put, $"allergies/update/{resolvedDoc.RootElement.GetProperty("data").GetProperty("id").GetString()}", "{\"name\":\"Polen\",\"kind\":\"Allergy\",\"status\":\"Resolved\"}");
        await Send(owner, HttpMethod.Post, "vaccines/create", $"{{\"petId\":\"{pet}\",\"appliedOn\":\"{DateTime.UtcNow:yyyy-MM-dd}\",\"name\":\"Rabia\"}}");
        await Send(owner, HttpMethod.Post, "access-grants/create", $"{{\"petId\":\"{pet}\",\"data\":[\"Vaccines\"],\"durationMinutes\":30}}");
        var code = factory.Pets.All.Single().Qr.Code;

        using var response = await Send(store, HttpMethod.Post, "scans/resolve", $"{{\"code\":\"{code.ToLowerInvariant()}\"}}");
        var raw = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(raw);
        var data = document.RootElement.GetProperty("data");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        // CA-02 de pm-128: el servidor no envía ningún otro campo.
        Assert.Equal(["ageMonths", "ageYears", "allergies", "breed", "name", "photoUrl", "species"], data.EnumerateObject().Select(p => p.Name).Order(StringComparer.Ordinal));
        Assert.Equal("Firulais", data.GetProperty("name").GetString());
        Assert.True(data.GetProperty("ageYears").GetInt32() >= 6);
        var allergy = Assert.Single(data.GetProperty("allergies").EnumerateArray());
        Assert.Equal("Pollo", allergy.GetProperty("name").GetString());
        Assert.DoesNotContain("Rabia", raw, StringComparison.Ordinal);
        Assert.DoesNotContain(factory.Pets.All.Single().Qr.Token, raw, StringComparison.Ordinal);
        var log = Assert.Single(factory.ScanLogs.All);
        Assert.Equal((factory.Pets.All.Single().Id, factory.Businesses.All.Single().Id), (log.PetId, log.BusinessId));
        Assert.Contains("no-store", response.Headers.CacheControl?.ToString(), StringComparison.Ordinal);
    }

    [Trait("Endpoint", "scans-resolve")]
    [Fact]
    public async Task A_veterinary_receives_the_pet_id_in_addition_and_a_token_works_like_the_short_code()
    {
        using var factory = new AuthFlowFactory();
        using var owner = await Account(factory, "dueno@example.com");
        using var vet = await Account(factory, "vet@example.com");
        await Send(vet, HttpMethod.Post, "businesses/create", Biz("Veterinary", "Vet", 19.4, -99.1));
        var pet = await CreatePet(owner);
        await Send(owner, HttpMethod.Post, "access-grants/create", $"{{\"petId\":\"{pet}\",\"data\":[],\"durationMinutes\":1440}}");

        using var byToken = await Send(vet, HttpMethod.Post, "scans/resolve", $"{{\"code\":\"{factory.Pets.All.Single().Qr.Token}\"}}");
        using var doc = JsonDocument.Parse(await byToken.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, byToken.StatusCode);
        Assert.Equal(pet, doc.RootElement.GetProperty("data").GetProperty("petId").GetString());
    }

    [Trait("Endpoint", "scans-resolve")]
    [Fact]
    public async Task Without_a_current_grant_the_scan_is_rejected_with_403_and_no_pet_data_even_knowing_the_qr()
    {
        using var factory = new AuthFlowFactory();
        using var owner = await Account(factory, "dueno@example.com");
        using var store = await Account(factory, "tienda@example.com");
        using var other = await Account(factory, "otra@example.com");
        await Send(store, HttpMethod.Post, "businesses/create", Biz("Store", "Tienda", 19.4, -99.1));
        await Send(other, HttpMethod.Post, "businesses/create", Biz("Grooming", "Otra", 19.4, -99.1));
        var pet = await CreatePet(owner, "{\"name\":\"SecretoNombre\",\"species\":\"Gato\"}");
        var code = factory.Pets.All.Single().Qr.Code;

        using var none = await Send(store, HttpMethod.Post, "scans/resolve", $"{{\"code\":\"{code}\"}}");
        var noneBody = await none.Content.ReadAsStringAsync();
        await Send(owner, HttpMethod.Post, "access-grants/create", $"{{\"petId\":\"{pet}\",\"businessId\":\"{factory.Businesses.All.Single(b => b.Name == "Otra").Id}\",\"data\":[],\"durationMinutes\":30}}");
        using var forOther = await Send(store, HttpMethod.Post, "scans/resolve", $"{{\"code\":\"{code}\"}}");

        Assert.Equal(HttpStatusCode.Forbidden, none.StatusCode);
        Assert.DoesNotContain("SecretoNombre", noneBody, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.Forbidden, forOther.StatusCode);
        Assert.Empty(factory.ScanLogs.All);
    }

    [Trait("Endpoint", "scans-resolve")]
    [Fact]
    public async Task A_grant_for_any_business_works_for_every_business_until_it_expires()
    {
        using var factory = new AuthFlowFactory();
        using var owner = await Account(factory, "dueno@example.com");
        using var one = await Account(factory, "uno@example.com");
        using var two = await Account(factory, "dos@example.com");
        await Send(one, HttpMethod.Post, "businesses/create", Biz("Store", "Uno", 19.4, -99.1));
        await Send(two, HttpMethod.Post, "businesses/create", Biz("Grooming", "Dos", 19.4, -99.1));
        var pet = await CreatePet(owner);
        await Send(owner, HttpMethod.Post, "access-grants/create", $"{{\"petId\":\"{pet}\",\"data\":[],\"durationMinutes\":30}}");
        var body = $"{{\"code\":\"{factory.Pets.All.Single().Qr.Code}\"}}";

        Assert.Equal(HttpStatusCode.OK, (await Send(one, HttpMethod.Post, "scans/resolve", body)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await Send(two, HttpMethod.Post, "scans/resolve", body)).StatusCode);
        factory.Clock.Advance(TimeSpan.FromMinutes(31));
        using var again = await Login(factory, "uno@example.com");
        Assert.Equal(HttpStatusCode.Forbidden, (await Send(again, HttpMethod.Post, "scans/resolve", body)).StatusCode);
    }

    [Trait("Endpoint", "scans-resolve")]
    [Fact]
    public async Task Regenerating_the_qr_cancels_every_active_grant_and_the_old_code_is_unknown()
    {
        using var factory = new AuthFlowFactory();
        using var owner = await Account(factory, "dueno@example.com");
        using var store = await Account(factory, "tienda@example.com");
        await Send(store, HttpMethod.Post, "businesses/create", Biz("Store", "Tienda", 19.4, -99.1));
        var pet = await CreatePet(owner);
        await Send(owner, HttpMethod.Post, "access-grants/create", $"{{\"petId\":\"{pet}\",\"data\":[],\"durationMinutes\":10080}}");
        var oldCode = factory.Pets.All.Single().Qr.Code;
        Assert.Equal(HttpStatusCode.OK, (await Send(store, HttpMethod.Post, "scans/resolve", $"{{\"code\":\"{oldCode}\"}}")).StatusCode);

        await Send(owner, HttpMethod.Post, $"pets/regenerate-qr/{pet}");
        var newCode = factory.Pets.All.Single().Qr.Code;

        Assert.Equal(HttpStatusCode.NotFound, (await Send(store, HttpMethod.Post, "scans/resolve", $"{{\"code\":\"{oldCode}\"}}")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await Send(store, HttpMethod.Post, "scans/resolve", $"{{\"code\":\"{newCode}\"}}")).StatusCode);
        Assert.All(factory.AccessGrants.All, g => Assert.False(g.IsActiveAt(factory.Clock.GetUtcNow().UtcDateTime)));
    }

    [Trait("Endpoint", "scans-resolve")]
    [Fact]
    public async Task Unknown_codes_return_the_same_404_only_an_account_with_a_business_can_scan_and_attempts_are_limited()
    {
        using var factory = new AuthFlowFactory();
        using var store = await Account(factory, "tienda@example.com");
        using var plain = await Account(factory, "sin@example.com");
        await Send(store, HttpMethod.Post, "businesses/create", Biz("Store", "Tienda", 19.4, -99.1));

        using var first = await Send(store, HttpMethod.Post, "scans/resolve", "{\"code\":\"ABCDEFGH\"}");
        using var second = await Send(store, HttpMethod.Post, "scans/resolve", $"{{\"code\":\"{new string('a', 43)}\"}}");
        var firstBody = await first.Content.ReadAsStringAsync();
        var secondBody = await second.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.NotFound, first.StatusCode);
        Assert.Equal(firstBody, secondBody);
        await AssertWrapper(await Send(plain, HttpMethod.Post, "scans/resolve", "{\"code\":\"ABCDEFGH\"}"), HttpStatusCode.Forbidden);
        await AssertWrapper(await Send(factory.CreateClient(), HttpMethod.Post, "scans/resolve", "{\"code\":\"ABCDEFGH\"}"), HttpStatusCode.Unauthorized);

        HttpResponseMessage? last = null;
        for (var i = 0; i < 25; i++)
        {
            last?.Dispose();
            last = await Send(store, HttpMethod.Post, "scans/resolve", "{\"code\":\"ABCDEFGH\"}");
        }

        Assert.Equal(HttpStatusCode.TooManyRequests, last!.StatusCode);
        Assert.True(last.Headers.Contains("Retry-After"));
        last.Dispose();
    }

    [Trait("Endpoint", "scans-resolve")]
    [Theory]
    [InlineData("")]
    [InlineData("corto")]
    [InlineData("ABCD-123")]
    [InlineData("ABCDEFG!")]
    public async Task Scan_with_a_malformed_code_returns_400(string code)
    {
        using var factory = new AuthFlowFactory();
        using var store = await Account(factory, "tienda@example.com");
        await Send(store, HttpMethod.Post, "businesses/create", Biz("Store", "Tienda", 19.4, -99.1));

        await AssertFieldError(await Send(store, HttpMethod.Post, "scans/resolve", $"{{\"code\":\"{code}\"}}"), "code");
    }

    [Trait("Endpoint", "scans-resolve")]
    [Fact]
    public async Task Deleting_the_pet_removes_its_grants_and_scan_logs()
    {
        using var factory = new AuthFlowFactory();
        using var owner = await Account(factory, "dueno@example.com");
        using var store = await Account(factory, "tienda@example.com");
        await Send(store, HttpMethod.Post, "businesses/create", Biz("Store", "Tienda", 19.4, -99.1));
        var pet = await CreatePet(owner);
        await Send(owner, HttpMethod.Post, "access-grants/create", $"{{\"petId\":\"{pet}\",\"data\":[],\"durationMinutes\":30}}");
        await Send(store, HttpMethod.Post, "scans/resolve", $"{{\"code\":\"{factory.Pets.All.Single().Qr.Code}\"}}");
        Assert.Single(factory.ScanLogs.All);

        await Send(owner, HttpMethod.Delete, $"pets/delete/{pet}");

        Assert.Empty(factory.AccessGrants.All);
        Assert.Empty(factory.ScanLogs.All);
    }

    // ---------- helpers ----------

    private static async Task<HttpClient> Account(AuthFlowFactory factory, string email)
    {
        using var anonymous = factory.CreateClient();
        using var response = await Send(anonymous, HttpMethod.Post, "auth/register", $"{{\"displayName\":\"Ana\",\"email\":\"{email}\",\"password\":\"{Password}\"}}");
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", document.RootElement.GetProperty("data").GetProperty("accessToken").GetString());
        return client;
    }

    private static async Task<HttpClient> Login(AuthFlowFactory factory, string email)
    {
        using var anonymous = factory.CreateClient();
        using var response = await Send(anonymous, HttpMethod.Post, "auth/login", $"{{\"email\":\"{email}\",\"password\":\"{Password}\"}}");
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", document.RootElement.GetProperty("data").GetProperty("accessToken").GetString());
        return client;
    }

    private static async Task<string> CreatePet(HttpClient client, string body = "{\"name\":\"Firulais\",\"species\":\"Perro\"}")
    {
        using var response = await Send(client, HttpMethod.Post, "pets/create", body);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return document.RootElement.GetProperty("data").GetProperty("id").GetString()!;
    }

    private static async Task<string[]> Names(HttpClient client, string route)
    {
        using var response = await Send(client, HttpMethod.Get, route);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return document.RootElement.GetProperty("data").GetProperty("items").EnumerateArray().Select(i => i.GetProperty("name").GetString()!).ToArray();
    }

    private static async Task<int> ServiceCount(HttpClient client, Guid id)
    {
        using var response = await Send(client, HttpMethod.Get, $"businesses/get-by-id/{id}");
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return document.RootElement.GetProperty("data").GetProperty("services").GetArrayLength();
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
            Assert.Equal(expected, response.StatusCode);
            using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            Assert.Equal(["data", "message", "statusCode"], document.RootElement.EnumerateObject().Select(p => p.Name).Order(StringComparer.Ordinal));
        }
    }

    private static async Task AssertFieldError(HttpResponseMessage response, string field)
    {
        using (response)
        {
            var raw = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            using var document = JsonDocument.Parse(raw);
            Assert.Contains(document.RootElement.GetProperty("data").GetProperty("errors").EnumerateArray(), e => e.GetProperty("field").GetString() == field);
        }
    }
}
