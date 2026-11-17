using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using PetMate.ContractTests.Authentication;
using PetMate.ContractTests.Support;

namespace PetMate.ContractTests.Businesses;

/// <summary>
/// Contrato HTTP de las consultas, el carnet aplicado por la veterinaria, el historial, las mascotas atendidas, el seguimiento y el
/// chat (pm-130 a pm-149) con JWT real y almacenamiento en memoria.
/// </summary>
[Collection(ApiEnvironmentGroup.Name)]
public sealed class VeterinaryEndpointsTests
{
    private const string Password = "contrasena-segura-123";

    private sealed record Setup(AuthFlowFactory Factory, HttpClient Owner, HttpClient Vet, string PetId, Guid VetBusinessId);

    private static async Task<Setup> Arrange(bool withGrant = true)
    {
        var factory = new AuthFlowFactory();
        var owner = await Account(factory, "dueno@example.com");
        var vet = await Account(factory, "vet@example.com");
        using var created = await Send(vet, HttpMethod.Post, "businesses/create", "{\"type\":\"Veterinary\",\"name\":\"Vet Sol\",\"phone\":\"5512345678\",\"address\":\"A\",\"latitude\":19.4,\"longitude\":-99.1,\"licenseNumber\":\"C-1\"}");
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var petId = await CreatePet(owner);
        var businessId = factory.Businesses.All.Single().Id;
        if (withGrant)
        {
            await Grant(owner, petId, businessId);
        }

        return new Setup(factory, owner, vet, petId, businessId);
    }

    private static async Task Grant(HttpClient owner, string petId, Guid? businessId, int minutes = 10080)
    {
        var target = businessId is null ? string.Empty : $"\"businessId\":\"{businessId}\",";
        using var response = await Send(owner, HttpMethod.Post, "access-grants/create", $"{{\"petId\":\"{petId}\",{target}\"data\":[\"Vaccines\"],\"durationMinutes\":{minutes}}}");
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    private static async Task<string> NewConsultation(HttpClient vet, string petId, string extra = "")
    {
        using var response = await Send(vet, HttpMethod.Post, "consultations/create", $"{{\"petId\":\"{petId}\",\"reason\":\"Vómito\",\"diagnosis\":\"Gastritis\"{extra}}}");
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return document.RootElement.GetProperty("data").GetProperty("id").GetString()!;
    }

    // ---------- Consultas ----------

    [Trait("Endpoint", "consultations-create")]
    [Trait("Endpoint", "consultations-get-by-id")]
    [Fact]
    public async Task A_veterinary_with_a_grant_records_a_consultation_with_a_prescription_that_the_owner_reads_but_cannot_change()
    {
        var s = await Arrange();
        using var factory = s.Factory;
        using var owner = s.Owner;
        using var vet = s.Vet;

        using var created = await Send(vet, HttpMethod.Post, "consultations/create", $"{{\"petId\":\"{s.PetId}\",\"reason\":\"  Vómito  \",\"diagnosis\":\"Gastritis\",\"weightKg\":8.4,\"medications\":[{{\"name\":\"Omeprazol\",\"dose\":\"10 mg\",\"frequency\":\"cada 12 h\",\"duration\":\"7 días\",\"instructions\":\"En ayunas\"}},{{\"name\":\"Probiótico\",\"dose\":\"1 sobre\",\"frequency\":\"diaria\",\"duration\":\"5 días\"}}]}}");
        using var createdDoc = JsonDocument.Parse(await created.Content.ReadAsStringAsync());
        var data = createdDoc.RootElement.GetProperty("data");
        var id = data.GetProperty("id").GetString();
        using var detail = await Send(owner, HttpMethod.Get, $"consultations/get-by-id/{id}");
        using var detailDoc = JsonDocument.Parse(await detail.Content.ReadAsStringAsync());
        using var list = await Send(owner, HttpMethod.Get, $"consultations/get-all?petId={s.PetId}");
        using var listDoc = JsonDocument.Parse(await list.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        Assert.Equal("Vómito", data.GetProperty("reason").GetString());
        Assert.Equal("Business", data.GetProperty("origin").GetProperty("type").GetString());
        Assert.Equal("Vet Sol", data.GetProperty("origin").GetProperty("businessName").GetString());
        Assert.Equal(2, detailDoc.RootElement.GetProperty("data").GetProperty("medications").GetArrayLength());
        var item = listDoc.RootElement.GetProperty("data").GetProperty("items")[0];
        Assert.Equal(["businessName", "consultedOn", "diagnosis", "id", "reason"], item.EnumerateObject().Select(p => p.Name).Order(StringComparer.Ordinal));
        // Solo lectura para el dueño: no existe ninguna ruta de escritura para él.
        using var write = await Send(owner, HttpMethod.Put, $"consultations/update/{id}", "{}");
        Assert.True(write.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.MethodNotAllowed);
        using var ownerCreate = await Send(owner, HttpMethod.Post, "consultations/create", $"{{\"petId\":\"{s.PetId}\",\"reason\":\"X\",\"diagnosis\":\"Y\"}}");
        Assert.Equal(HttpStatusCode.Forbidden, ownerCreate.StatusCode);
    }

    [Trait("Endpoint", "consultations-create")]
    [Fact]
    public async Task Only_a_veterinary_with_a_current_grant_can_record_and_store_or_grooming_get_403()
    {
        var s = await Arrange(withGrant: false);
        using var factory = s.Factory;
        using var owner = s.Owner;
        using var vet = s.Vet;
        using var store = await Account(factory, "tienda@example.com");
        await Send(store, HttpMethod.Post, "businesses/create", "{\"type\":\"Store\",\"name\":\"Tienda\",\"phone\":\"5512345678\",\"address\":\"A\",\"latitude\":19.4,\"longitude\":-99.1}");
        var body = $"{{\"petId\":\"{s.PetId}\",\"reason\":\"X\",\"diagnosis\":\"Y\"}}";

        await AssertWrapper(await Send(vet, HttpMethod.Post, "consultations/create", body), HttpStatusCode.Forbidden);
        await Grant(owner, s.PetId, null);
        await AssertWrapper(await Send(store, HttpMethod.Post, "consultations/create", body), HttpStatusCode.Forbidden);
        await AssertWrapper(await Send(vet, HttpMethod.Post, "consultations/create", body), HttpStatusCode.Created);
        await AssertWrapper(await Send(factory.CreateClient(), HttpMethod.Post, "consultations/create", body), HttpStatusCode.Unauthorized);
        Assert.Single(factory.Consultations.All);
    }

    [Trait("Endpoint", "consultations-create")]
    [Fact]
    public async Task A_grant_that_expired_no_longer_allows_recording()
    {
        var s = await Arrange(withGrant: false);
        using var factory = s.Factory;
        using var owner = s.Owner;
        using var vet = s.Vet;
        await Grant(owner, s.PetId, s.VetBusinessId, 30);
        factory.Clock.Advance(TimeSpan.FromMinutes(31));
        using var again = await Login(factory, "vet@example.com");

        await AssertWrapper(await Send(again, HttpMethod.Post, "consultations/create", $"{{\"petId\":\"{s.PetId}\",\"reason\":\"X\",\"diagnosis\":\"Y\"}}"), HttpStatusCode.Forbidden);
    }

    [Trait("Endpoint", "consultations-create")]
    [Theory]
    [InlineData("reason", "{\"diagnosis\":\"Y\"}")]
    [InlineData("diagnosis", "{\"reason\":\"X\"}")]
    [InlineData("weightKg", "{\"reason\":\"X\",\"diagnosis\":\"Y\",\"weightKg\":0}")]
    [InlineData("weightKg", "{\"reason\":\"X\",\"diagnosis\":\"Y\",\"weightKg\":8.55}")]
    [InlineData("medications", "{\"reason\":\"X\",\"diagnosis\":\"Y\",\"medications\":[{\"name\":\"A\"}]}")]
    public async Task Consultation_with_an_invalid_field_returns_400_naming_it(string field, string members)
    {
        var s = await Arrange();
        using var factory = s.Factory;
        using var owner = s.Owner;
        using var vet = s.Vet;

        await AssertFieldError(await Send(vet, HttpMethod.Post, "consultations/create", $"{{\"petId\":\"{s.PetId}\",{members[1..]}"), field);
        Assert.Empty(factory.Consultations.All);
    }

    [Trait("Endpoint", "consultations-add-medication")]
    [Fact]
    public async Task The_prescription_admits_up_to_ten_medications_and_only_the_authoring_veterinary_adds_them()
    {
        var s = await Arrange();
        using var factory = s.Factory;
        using var owner = s.Owner;
        using var vet = s.Vet;
        using var other = await Account(factory, "otra@example.com");
        await Send(other, HttpMethod.Post, "businesses/create", "{\"type\":\"Veterinary\",\"name\":\"Otra\",\"phone\":\"5512345678\",\"address\":\"A\",\"latitude\":19.4,\"longitude\":-99.1,\"licenseNumber\":\"C-2\"}");
        var id = await NewConsultation(vet, s.PetId);
        const string med = "\"name\":\"M\",\"dose\":\"1\",\"frequency\":\"2\",\"duration\":\"3\"";

        for (var i = 0; i < 10; i++)
        {
            await AssertWrapper(await Send(vet, HttpMethod.Post, "consultations/add-medication", $"{{\"consultationId\":\"{id}\",{med}}}"), HttpStatusCode.OK);
        }

        await AssertWrapper(await Send(vet, HttpMethod.Post, "consultations/add-medication", $"{{\"consultationId\":\"{id}\",{med}}}"), HttpStatusCode.Conflict);
        await AssertWrapper(await Send(other, HttpMethod.Post, "consultations/add-medication", $"{{\"consultationId\":\"{id}\",{med}}}"), HttpStatusCode.NotFound);
        await AssertWrapper(await Send(owner, HttpMethod.Post, "consultations/add-medication", $"{{\"consultationId\":\"{id}\",{med}}}"), HttpStatusCode.Forbidden);
        await AssertFieldError(await Send(vet, HttpMethod.Post, "consultations/add-medication", $"{{\"consultationId\":\"{id}\",\"name\":\"M\"}}"), "dose");
        Assert.Equal(10, factory.Consultations.All.Single().Medications.Count);
    }

    [Trait("Endpoint", "consultations-get-all")]
    [Fact]
    public async Task Consultation_reads_hide_foreign_pets_and_foreign_consultations_with_404()
    {
        var s = await Arrange();
        using var factory = s.Factory;
        using var owner = s.Owner;
        using var vet = s.Vet;
        using var intruder = await Account(factory, "intruso@example.com");
        var id = await NewConsultation(vet, s.PetId);

        await AssertWrapper(await Send(intruder, HttpMethod.Get, $"consultations/get-all?petId={s.PetId}"), HttpStatusCode.NotFound);
        await AssertWrapper(await Send(intruder, HttpMethod.Get, $"consultations/get-by-id/{id}"), HttpStatusCode.NotFound);
        await AssertFieldError(await Send(owner, HttpMethod.Get, "consultations/get-all"), "petId");
        await AssertWrapper(await Send(factory.CreateClient(), HttpMethod.Get, $"consultations/get-by-id/{id}"), HttpStatusCode.Unauthorized);
    }

    // ---------- Vacunas y desparasitaciones aplicadas por la veterinaria ----------

    [Trait("Endpoint", "vaccines-create-by-business")]
    [Fact]
    public async Task A_vaccine_applied_by_the_veterinary_has_business_origin_the_business_name_and_the_owner_cannot_delete_it()
    {
        var s = await Arrange();
        using var factory = s.Factory;
        using var owner = s.Owner;
        using var vet = s.Vet;
        var today = DateTime.UtcNow.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);

        using var created = await Send(vet, HttpMethod.Post, "vaccines/create-by-business", $"{{\"petId\":\"{s.PetId}\",\"appliedOn\":\"{today}\",\"name\":\"Rabia\",\"batch\":\"L9\"}}");
        using var document = JsonDocument.Parse(await created.Content.ReadAsStringAsync());
        var data = document.RootElement.GetProperty("data");
        var id = data.GetProperty("id").GetString();

        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        Assert.Equal("Vet Sol", data.GetProperty("appliedBy").GetString());
        Assert.Equal("Business", data.GetProperty("origin").GetProperty("type").GetString());
        using var blocked = await Send(owner, HttpMethod.Delete, $"vaccines/delete/{id}");
        Assert.Equal(HttpStatusCode.Forbidden, blocked.StatusCode);
        Assert.Contains("lo registró el negocio", await blocked.Content.ReadAsStringAsync(), StringComparison.Ordinal);
        using var withAppliedBy = await Send(vet, HttpMethod.Post, "vaccines/create-by-business", $"{{\"petId\":\"{s.PetId}\",\"appliedOn\":\"{today}\",\"name\":\"X\",\"appliedBy\":\"Otro\"}}");
        Assert.Equal(HttpStatusCode.BadRequest, withAppliedBy.StatusCode);
        await AssertFieldError(await Send(vet, HttpMethod.Post, "vaccines/create-by-business", $"{{\"petId\":\"{s.PetId}\",\"appliedOn\":\"{today}\",\"name\":\"X\",\"nextDoseOn\":\"{today}\"}}"), "nextDoseOn");
    }

    [Trait("Endpoint", "vaccines-create-by-business")]
    [Fact]
    public async Task Vaccine_by_business_requires_a_veterinary_with_a_grant()
    {
        var s = await Arrange(withGrant: false);
        using var factory = s.Factory;
        using var owner = s.Owner;
        using var vet = s.Vet;
        var body = $"{{\"petId\":\"{s.PetId}\",\"appliedOn\":\"{DateTime.UtcNow:yyyy-MM-dd}\",\"name\":\"Rabia\"}}";

        await AssertWrapper(await Send(vet, HttpMethod.Post, "vaccines/create-by-business", body), HttpStatusCode.Forbidden);
        await AssertWrapper(await Send(owner, HttpMethod.Post, "vaccines/create-by-business", body), HttpStatusCode.Forbidden);
        await AssertWrapper(await Send(factory.CreateClient(), HttpMethod.Post, "vaccines/create-by-business", body), HttpStatusCode.Unauthorized);
        Assert.Empty(factory.Vaccines.All);
    }

    [Trait("Endpoint", "dewormings-create-by-business")]
    [Fact]
    public async Task A_deworming_applied_by_the_veterinary_has_business_origin_and_the_next_date_must_be_future()
    {
        var s = await Arrange();
        using var factory = s.Factory;
        using var owner = s.Owner;
        using var vet = s.Vet;
        var today = DateTime.UtcNow.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
        var future = DateTime.UtcNow.AddDays(90).ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);

        using var created = await Send(vet, HttpMethod.Post, "dewormings/create-by-business", $"{{\"petId\":\"{s.PetId}\",\"appliedOn\":\"{today}\",\"product\":\"Drontal\",\"type\":\"Internal\",\"nextOn\":\"{future}\"}}");
        using var document = JsonDocument.Parse(await created.Content.ReadAsStringAsync());
        var id = document.RootElement.GetProperty("data").GetProperty("id").GetString();

        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        Assert.Equal("Vet Sol", document.RootElement.GetProperty("data").GetProperty("appliedBy").GetString());
        await AssertFieldError(await Send(vet, HttpMethod.Post, "dewormings/create-by-business", $"{{\"petId\":\"{s.PetId}\",\"appliedOn\":\"{today}\",\"product\":\"X\",\"nextOn\":\"{today}\"}}"), "nextOn");
        await AssertWrapper(await Send(owner, HttpMethod.Delete, $"dewormings/delete/{id}"), HttpStatusCode.Forbidden);
        await AssertWrapper(await Send(owner, HttpMethod.Post, "dewormings/create-by-business", $"{{\"petId\":\"{s.PetId}\",\"appliedOn\":\"{today}\",\"product\":\"X\"}}"), HttpStatusCode.Forbidden);
    }

    // ---------- Historial ----------

    [Trait("Endpoint", "history-get-all")]
    [Fact]
    public async Task History_mixes_consultations_vaccines_dewormings_and_weights_newest_first_and_filters_by_type()
    {
        var s = await Arrange();
        using var factory = s.Factory;
        using var owner = s.Owner;
        using var vet = s.Vet;
        var day = (int offset) => DateTime.UtcNow.AddDays(offset).ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
        await Send(owner, HttpMethod.Post, "weights/create", $"{{\"petId\":\"{s.PetId}\",\"measuredOn\":\"{day(-10)}\",\"kilograms\":8.0}}");
        await Send(owner, HttpMethod.Post, "vaccines/create", $"{{\"petId\":\"{s.PetId}\",\"appliedOn\":\"{day(-5)}\",\"name\":\"Rabia\"}}");
        await Send(vet, HttpMethod.Post, "dewormings/create-by-business", $"{{\"petId\":\"{s.PetId}\",\"appliedOn\":\"{day(-3)}\",\"product\":\"Drontal\"}}");
        await NewConsultation(vet, s.PetId);

        var all = await History(owner, $"petId={s.PetId}");
        var onlyWeights = await History(owner, $"petId={s.PetId}&types=Weight");
        var two = await History(owner, $"petId={s.PetId}&types=Vaccine,Deworming");

        Assert.Equal(["Consultation", "Deworming", "Vaccine", "Weight"], all);
        Assert.Equal(["Weight"], onlyWeights);
        Assert.Equal(["Deworming", "Vaccine"], two);
        await AssertFieldError(await Send(owner, HttpMethod.Get, $"history/get-all?petId={s.PetId}&types=Foto"), "types");
        await AssertFieldError(await Send(owner, HttpMethod.Get, "history/get-all"), "petId");
    }

    [Trait("Endpoint", "history-get-all")]
    [Fact]
    public async Task History_of_a_foreign_pet_returns_404()
    {
        var s = await Arrange();
        using var factory = s.Factory;
        using var owner = s.Owner;
        using var vet = s.Vet;

        await AssertWrapper(await Send(vet, HttpMethod.Get, $"history/get-all?petId={s.PetId}"), HttpStatusCode.NotFound);
        await AssertWrapper(await Send(factory.CreateClient(), HttpMethod.Get, $"history/get-all?petId={s.PetId}"), HttpStatusCode.Unauthorized);
    }

    // ---------- Mascotas atendidas ----------

    [Trait("Endpoint", "scans-get-attended")]
    [Fact]
    public async Task Attended_pets_list_name_photo_and_date_only_newest_first_with_day_and_week_filters()
    {
        var s = await Arrange();
        using var factory = s.Factory;
        using var owner = s.Owner;
        using var vet = s.Vet;
        var code = factory.Pets.All.Single().Qr.Code;
        await Send(vet, HttpMethod.Post, "scans/resolve", $"{{\"code\":\"{code}\"}}");
        factory.Clock.Advance(TimeSpan.FromDays(3));
        using var again = await Login(factory, "vet@example.com");
        await Send(again, HttpMethod.Post, "scans/resolve", $"{{\"code\":\"{code}\"}}");

        using var all = await Send(again, HttpMethod.Get, "scans/get-attended");
        using var allDoc = JsonDocument.Parse(await all.Content.ReadAsStringAsync());
        var items = allDoc.RootElement.GetProperty("data").GetProperty("items").EnumerateArray().ToArray();
        using var day = await Send(again, HttpMethod.Get, "scans/get-attended?period=day");
        using var dayDoc = JsonDocument.Parse(await day.Content.ReadAsStringAsync());
        using var week = await Send(again, HttpMethod.Get, "scans/get-attended?period=week");
        using var weekDoc = JsonDocument.Parse(await week.Content.ReadAsStringAsync());

        Assert.Equal(2, items.Length);
        Assert.Equal(["petName", "photoUrl", "scannedAt"], items[0].EnumerateObject().Select(p => p.Name).Order(StringComparer.Ordinal));
        Assert.True(items[0].GetProperty("scannedAt").GetDateTime() > items[1].GetProperty("scannedAt").GetDateTime());
        Assert.Equal("Firulais", items[0].GetProperty("petName").GetString());
        Assert.Equal(1, dayDoc.RootElement.GetProperty("data").GetProperty("totalCount").GetInt32());
        Assert.Equal(2, weekDoc.RootElement.GetProperty("data").GetProperty("totalCount").GetInt32());
        await AssertFieldError(await Send(again, HttpMethod.Get, "scans/get-attended?period=year"), "period");
        using var ownerAgain = await Login(factory, "dueno@example.com");
        await AssertWrapper(await Send(ownerAgain, HttpMethod.Get, "scans/get-attended"), HttpStatusCode.Forbidden);
        await AssertWrapper(await Send(factory.CreateClient(), HttpMethod.Get, "scans/get-attended"), HttpStatusCode.Unauthorized);
    }

    // ---------- Seguimiento ----------

    [Trait("Endpoint", "followups-create")]
    [Fact]
    public async Task The_authoring_veterinary_opens_one_followup_per_consultation_and_it_creates_the_conversation()
    {
        var s = await Arrange();
        using var factory = s.Factory;
        using var owner = s.Owner;
        using var vet = s.Vet;
        using var other = await Account(factory, "otra@example.com");
        await Send(other, HttpMethod.Post, "businesses/create", "{\"type\":\"Veterinary\",\"name\":\"Otra\",\"phone\":\"5512345678\",\"address\":\"A\",\"latitude\":19.4,\"longitude\":-99.1,\"licenseNumber\":\"C-2\"}");
        var consultation = await NewConsultation(vet, s.PetId);

        await AssertWrapper(await Send(other, HttpMethod.Post, "followups/create", $"{{\"consultationId\":\"{consultation}\"}}"), HttpStatusCode.NotFound);
        await AssertFieldError(await Send(vet, HttpMethod.Post, "followups/create", $"{{\"consultationId\":\"{consultation}\",\"nextReviewInDays\":5}}"), "nextReviewInDays");
        using var created = await Send(vet, HttpMethod.Post, "followups/create", $"{{\"consultationId\":\"{consultation}\",\"nextReviewInDays\":7}}");
        using var document = JsonDocument.Parse(await created.Content.ReadAsStringAsync());
        var data = document.RootElement.GetProperty("data");
        await AssertWrapper(await Send(vet, HttpMethod.Post, "followups/create", $"{{\"consultationId\":\"{consultation}\"}}"), HttpStatusCode.Conflict);

        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        Assert.Equal("Treating", data.GetProperty("status").GetString());
        Assert.Equal(DateOnly.FromDateTime(factory.Clock.GetUtcNow().UtcDateTime).AddDays(7).ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture), data.GetProperty("nextReviewOn").GetString());
        Assert.NotEqual(JsonValueKind.Null, data.GetProperty("conversationId").ValueKind);
        Assert.Equal(1, factory.Conversations.Count);
        Assert.Single(factory.Messages.All);
        await AssertWrapper(await Send(owner, HttpMethod.Post, "followups/create", $"{{\"consultationId\":\"{consultation}\"}}"), HttpStatusCode.Forbidden);
    }

    [Trait("Endpoint", "followups-update")]
    [Fact]
    public async Task Update_changes_status_and_review_and_discharging_removes_the_review_and_closes_the_conversation()
    {
        var s = await Arrange();
        using var factory = s.Factory;
        using var owner = s.Owner;
        using var vet = s.Vet;
        using var other = await Account(factory, "otra@example.com");
        await Send(other, HttpMethod.Post, "businesses/create", "{\"type\":\"Veterinary\",\"name\":\"Otra\",\"phone\":\"5512345678\",\"address\":\"A\",\"latitude\":19.4,\"longitude\":-99.1,\"licenseNumber\":\"C-2\"}");
        var consultation = await NewConsultation(vet, s.PetId);
        var followup = await Followup(vet, consultation, 7);

        using var improving = await Send(vet, HttpMethod.Put, $"followups/update/{followup}", "{\"status\":\"Improving\",\"nextReviewInDays\":3}");
        using var improvingDoc = JsonDocument.Parse(await improving.Content.ReadAsStringAsync());
        using var discharged = await Send(vet, HttpMethod.Put, $"followups/update/{followup}", "{\"status\":\"Discharged\",\"nextReviewInDays\":3}");
        using var dischargedDoc = JsonDocument.Parse(await discharged.Content.ReadAsStringAsync());

        Assert.Equal("Improving", improvingDoc.RootElement.GetProperty("data").GetProperty("status").GetString());
        Assert.NotEqual(JsonValueKind.Null, improvingDoc.RootElement.GetProperty("data").GetProperty("nextReviewOn").ValueKind);
        Assert.Equal("Discharged", dischargedDoc.RootElement.GetProperty("data").GetProperty("status").GetString());
        Assert.Equal(JsonValueKind.Null, dischargedDoc.RootElement.GetProperty("data").GetProperty("nextReviewOn").ValueKind);
        var conversationId = dischargedDoc.RootElement.GetProperty("data").GetProperty("conversationId").GetString();
        await AssertWrapper(await Send(vet, HttpMethod.Post, "messages/send", $"{{\"conversationId\":\"{conversationId}\",\"text\":\"Hola\"}}"), HttpStatusCode.Conflict);
        await AssertFieldError(await Send(vet, HttpMethod.Put, $"followups/update/{followup}", "{\"status\":\"Curado\"}"), "status");
        await AssertWrapper(await Send(other, HttpMethod.Put, $"followups/update/{followup}", "{\"status\":\"Treating\"}"), HttpStatusCode.NotFound);
        await AssertWrapper(await Send(owner, HttpMethod.Put, $"followups/update/{followup}", "{\"status\":\"Treating\"}"), HttpStatusCode.Forbidden);
    }

    [Trait("Endpoint", "followups-add-note")]
    [Fact]
    public async Task Notes_keep_their_order_and_only_the_authoring_veterinary_adds_them_while_the_owner_reads()
    {
        var s = await Arrange();
        using var factory = s.Factory;
        using var owner = s.Owner;
        using var vet = s.Vet;
        using var intruder = await Account(factory, "intruso@example.com");
        var consultation = await NewConsultation(vet, s.PetId);
        var followup = await Followup(vet, consultation, null);

        await AssertWrapper(await Send(vet, HttpMethod.Post, "followups/add-note", $"{{\"followupId\":\"{followup}\",\"text\":\"Primera\"}}"), HttpStatusCode.OK);
        factory.Clock.Advance(TimeSpan.FromMinutes(5));
        using var vetAgain = await Login(factory, "vet@example.com");
        await AssertWrapper(await Send(vetAgain, HttpMethod.Post, "followups/add-note", $"{{\"followupId\":\"{followup}\",\"text\":\"Segunda\"}}"), HttpStatusCode.OK);
        await AssertFieldError(await Send(vetAgain, HttpMethod.Post, "followups/add-note", $"{{\"followupId\":\"{followup}\",\"text\":\"  \"}}"), "text");
        await AssertWrapper(await Send(owner, HttpMethod.Post, "followups/add-note", $"{{\"followupId\":\"{followup}\",\"text\":\"Yo\"}}"), HttpStatusCode.Forbidden);
        using var read = await Send(await Login(factory, "dueno@example.com"), HttpMethod.Get, $"followups/get-by-consultation/{consultation}");
        using var readDoc = JsonDocument.Parse(await read.Content.ReadAsStringAsync());
        var notes = readDoc.RootElement.GetProperty("data").GetProperty("notes").EnumerateArray().Select(n => n.GetProperty("text").GetString()!).ToArray();

        Assert.Equal(["Primera", "Segunda"], notes);
        await AssertWrapper(await Send(intruder, HttpMethod.Get, $"followups/get-by-consultation/{consultation}"), HttpStatusCode.NotFound);
    }

    [Trait("Endpoint", "followups-get-by-consultation")]
    [Fact]
    public async Task The_owner_sees_state_review_and_notes_of_the_own_consultation_and_a_missing_followup_is_404()
    {
        var s = await Arrange();
        using var factory = s.Factory;
        using var owner = s.Owner;
        using var vet = s.Vet;
        var consultation = await NewConsultation(vet, s.PetId);

        await AssertWrapper(await Send(owner, HttpMethod.Get, $"followups/get-by-consultation/{consultation}"), HttpStatusCode.NotFound);
        await Followup(vet, consultation, 3);
        using var read = await Send(owner, HttpMethod.Get, $"followups/get-by-consultation/{consultation}");
        using var readDoc = JsonDocument.Parse(await read.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, read.StatusCode);
        Assert.Equal("Treating", readDoc.RootElement.GetProperty("data").GetProperty("status").GetString());
        await AssertWrapper(await Send(factory.CreateClient(), HttpMethod.Get, $"followups/get-by-consultation/{consultation}"), HttpStatusCode.Unauthorized);
    }

    [Trait("Endpoint", "followups-get-all")]
    [Fact]
    public async Task The_veterinary_lists_only_its_followups_marks_overdue_reviews_and_filters_by_day_and_active()
    {
        var s = await Arrange();
        using var factory = s.Factory;
        using var owner = s.Owner;
        using var vet = s.Vet;
        using var other = await Account(factory, "otra@example.com");
        await Send(other, HttpMethod.Post, "businesses/create", "{\"type\":\"Veterinary\",\"name\":\"Otra\",\"phone\":\"5512345678\",\"address\":\"A\",\"latitude\":19.4,\"longitude\":-99.1,\"licenseNumber\":\"C-2\"}");
        var first = await NewConsultation(vet, s.PetId);
        var second = await NewConsultation(vet, s.PetId);
        var treating = await Followup(vet, first, 3);
        var discharged = await Followup(vet, second, 7);
        await Send(vet, HttpMethod.Put, $"followups/update/{discharged}", "{\"status\":\"Discharged\"}");
        factory.Clock.Advance(TimeSpan.FromDays(4));
        using var vetAgain = await Login(factory, "vet@example.com");
        using var otherAgain = await Login(factory, "otra@example.com");
        var reviewDay = DateOnly.FromDateTime(factory.Followups.All.Single(f => f.Id.ToString() == treating).NextReviewOn!.Value.ToDateTime(TimeOnly.MinValue)).ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);

        using var all = await Send(vetAgain, HttpMethod.Get, "followups/get-all");
        using var allDoc = JsonDocument.Parse(await all.Content.ReadAsStringAsync());
        using var active = await Send(vetAgain, HttpMethod.Get, "followups/get-all?onlyActive=true");
        using var activeDoc = JsonDocument.Parse(await active.Content.ReadAsStringAsync());
        using var byDay = await Send(vetAgain, HttpMethod.Get, $"followups/get-all?reviewDate={reviewDay}");
        using var byDayDoc = JsonDocument.Parse(await byDay.Content.ReadAsStringAsync());
        using var foreign = await Send(otherAgain, HttpMethod.Get, "followups/get-all");
        using var foreignDoc = JsonDocument.Parse(await foreign.Content.ReadAsStringAsync());

        Assert.Equal(2, allDoc.RootElement.GetProperty("data").GetProperty("totalCount").GetInt32());
        var activeItem = Assert.Single(activeDoc.RootElement.GetProperty("data").GetProperty("items").EnumerateArray());
        Assert.True(activeItem.GetProperty("isOverdue").GetBoolean());
        Assert.Equal("Firulais", activeItem.GetProperty("petName").GetString());
        Assert.Equal(1, byDayDoc.RootElement.GetProperty("data").GetProperty("totalCount").GetInt32());
        Assert.Equal(0, foreignDoc.RootElement.GetProperty("data").GetProperty("totalCount").GetInt32());
        using var ownerAgain = await Login(factory, "dueno@example.com");
        await AssertWrapper(await Send(ownerAgain, HttpMethod.Get, "followups/get-all"), HttpStatusCode.Forbidden);
    }

    // ---------- Chat ----------

    [Trait("Endpoint", "messages-send")]
    [Fact]
    public async Task Only_the_two_participants_exchange_messages_and_anyone_else_gets_404()
    {
        var s = await Arrange();
        using var factory = s.Factory;
        using var owner = s.Owner;
        using var vet = s.Vet;
        using var intruder = await Account(factory, "intruso@example.com");
        var conversation = await Conversation(vet, s.PetId);

        using var fromOwner = await Send(owner, HttpMethod.Post, "messages/send", $"{{\"conversationId\":\"{conversation}\",\"text\":\"  Hola doctora  \"}}");
        using var fromOwnerDoc = JsonDocument.Parse(await fromOwner.Content.ReadAsStringAsync());
        await AssertWrapper(await Send(vet, HttpMethod.Post, "messages/send", $"{{\"conversationId\":\"{conversation}\",\"text\":\"Hola\"}}"), HttpStatusCode.Created);

        Assert.Equal(HttpStatusCode.Created, fromOwner.StatusCode);
        Assert.Equal("Hola doctora", fromOwnerDoc.RootElement.GetProperty("data").GetProperty("text").GetString());
        Assert.True(fromOwnerDoc.RootElement.GetProperty("data").GetProperty("isMine").GetBoolean());
        await AssertWrapper(await Send(intruder, HttpMethod.Post, "messages/send", $"{{\"conversationId\":\"{conversation}\",\"text\":\"Hola\"}}"), HttpStatusCode.NotFound);
        await AssertWrapper(await Send(owner, HttpMethod.Post, "messages/send", $"{{\"conversationId\":\"{Guid.NewGuid()}\",\"text\":\"Hola\"}}"), HttpStatusCode.NotFound);
        await AssertFieldError(await Send(owner, HttpMethod.Post, "messages/send", $"{{\"conversationId\":\"{conversation}\",\"text\":\"  \"}}"), "text");
        await AssertFieldError(await Send(owner, HttpMethod.Post, "messages/send", $"{{\"conversationId\":\"{conversation}\",\"text\":\"{new string('a', 1001)}\"}}"), "text");
        await AssertWrapper(await Send(factory.CreateClient(), HttpMethod.Post, "messages/send", $"{{\"conversationId\":\"{conversation}\",\"text\":\"Hola\"}}"), HttpStatusCode.Unauthorized);
        Assert.Equal(3, factory.Messages.All.Count);
    }

    [Trait("Endpoint", "messages-get-all")]
    [Fact]
    public async Task Messages_come_in_order_with_system_notices_and_after_returns_only_the_later_ones()
    {
        var s = await Arrange();
        using var factory = s.Factory;
        using var owner = s.Owner;
        using var vet = s.Vet;
        using var intruder = await Account(factory, "intruso@example.com");
        var conversation = await Conversation(vet, s.PetId);
        for (var i = 1; i <= 3; i++)
        {
            await Send(owner, HttpMethod.Post, "messages/send", $"{{\"conversationId\":\"{conversation}\",\"text\":\"m{i}\"}}");
        }

        using var all = await Send(vet, HttpMethod.Get, $"messages/get-all?conversationId={conversation}");
        using var allDoc = JsonDocument.Parse(await all.Content.ReadAsStringAsync());
        var items = allDoc.RootElement.GetProperty("data").GetProperty("items").EnumerateArray().ToArray();
        var secondId = items[2].GetProperty("id").GetString();
        using var later = await Send(vet, HttpMethod.Get, $"messages/get-all?conversationId={conversation}&after={secondId}");
        using var laterDoc = JsonDocument.Parse(await later.Content.ReadAsStringAsync());
        using var paged = await Send(vet, HttpMethod.Get, $"messages/get-all?conversationId={conversation}&pageSize=2");
        using var pagedDoc = JsonDocument.Parse(await paged.Content.ReadAsStringAsync());

        Assert.Equal(["System", "Text", "Text", "Text"], items.Select(i => i.GetProperty("kind").GetString()!));
        Assert.Equal([1L, 2L, 3L, 4L], items.Select(i => i.GetProperty("seq").GetInt64()));
        Assert.Equal(["m3"], laterDoc.RootElement.GetProperty("data").GetProperty("items").EnumerateArray().Select(i => i.GetProperty("text").GetString()!));
        Assert.True(pagedDoc.RootElement.GetProperty("data").GetProperty("hasMore").GetBoolean());
        await AssertWrapper(await Send(intruder, HttpMethod.Get, $"messages/get-all?conversationId={conversation}"), HttpStatusCode.NotFound);
        await AssertWrapper(await Send(vet, HttpMethod.Get, $"messages/get-all?conversationId={conversation}&after={Guid.NewGuid()}"), HttpStatusCode.NotFound);
        await AssertFieldError(await Send(vet, HttpMethod.Get, "messages/get-all"), "conversationId");
    }

    [Trait("Endpoint", "conversations-get-all")]
    [Fact]
    public async Task Each_participant_lists_only_the_own_conversations_with_the_last_message_and_the_unread_count()
    {
        var s = await Arrange();
        using var factory = s.Factory;
        using var owner = s.Owner;
        using var vet = s.Vet;
        using var intruder = await Account(factory, "intruso@example.com");
        var conversation = await Conversation(vet, s.PetId);
        await Send(owner, HttpMethod.Post, "messages/send", $"{{\"conversationId\":\"{conversation}\",\"text\":\"Hola\"}}");
        await Send(owner, HttpMethod.Post, "messages/send", $"{{\"conversationId\":\"{conversation}\",\"text\":\"¿Sigue igual?\"}}");

        var vetSide = await Summary(vet);
        var ownerSide = await Summary(owner);
        using var none = await Send(intruder, HttpMethod.Get, "conversations/get-all");
        using var noneDoc = JsonDocument.Parse(await none.Content.ReadAsStringAsync());

        Assert.Equal("¿Sigue igual?", vetSide.GetProperty("lastMessage").GetProperty("text").GetString());
        Assert.Equal(3, vetSide.GetProperty("unreadCount").GetInt64());
        Assert.Equal(0, ownerSide.GetProperty("unreadCount").GetInt64());
        Assert.Equal(0, noneDoc.RootElement.GetProperty("data").GetProperty("totalCount").GetInt32());
        await AssertFieldError(await Send(vet, HttpMethod.Get, "conversations/get-all?pageSize=51"), "pageSize");
    }

    [Trait("Endpoint", "conversations-mark-read")]
    [Fact]
    public async Task Marking_read_only_changes_the_state_of_the_participant_who_asks()
    {
        var s = await Arrange();
        using var factory = s.Factory;
        using var owner = s.Owner;
        using var vet = s.Vet;
        using var intruder = await Account(factory, "intruso@example.com");
        var conversation = await Conversation(vet, s.PetId);
        await Send(owner, HttpMethod.Post, "messages/send", $"{{\"conversationId\":\"{conversation}\",\"text\":\"Hola\"}}");
        await Send(vet, HttpMethod.Post, "messages/send", $"{{\"conversationId\":\"{conversation}\",\"text\":\"Buenas\"}}");

        using var read = await Send(vet, HttpMethod.Post, "conversations/mark-read", $"{{\"conversationId\":\"{conversation}\"}}");
        using var readDoc = JsonDocument.Parse(await read.Content.ReadAsStringAsync());
        var ownerSide = await Summary(owner);

        Assert.Equal(HttpStatusCode.OK, read.StatusCode);
        Assert.Equal(0, readDoc.RootElement.GetProperty("data").GetProperty("unreadCount").GetInt64());
        Assert.Equal(1, ownerSide.GetProperty("unreadCount").GetInt64());
        using var ownerRead = await Send(owner, HttpMethod.Post, "conversations/mark-read", $"{{\"conversationId\":\"{conversation}\"}}");
        Assert.Equal(0, (await Summary(owner)).GetProperty("unreadCount").GetInt64());
        await AssertWrapper(await Send(intruder, HttpMethod.Post, "conversations/mark-read", $"{{\"conversationId\":\"{conversation}\"}}"), HttpStatusCode.NotFound);
        await AssertWrapper(await Send(vet, HttpMethod.Post, "conversations/mark-read", $"{{\"conversationId\":\"{conversation}\",\"upToMessageId\":\"{Guid.NewGuid()}\"}}"), HttpStatusCode.NotFound);
    }

    [Trait("Endpoint", "conversations-reopen")]
    [Fact]
    public async Task The_veterinary_reopens_a_closed_conversation_and_the_followup_goes_back_to_treating_but_the_owner_cannot()
    {
        var s = await Arrange();
        using var factory = s.Factory;
        using var owner = s.Owner;
        using var vet = s.Vet;
        var consultation = await NewConsultation(vet, s.PetId);
        var followup = await Followup(vet, consultation, 7);
        using var discharged = await Send(vet, HttpMethod.Put, $"followups/update/{followup}", "{\"status\":\"Discharged\"}");
        using var dischargedDoc = JsonDocument.Parse(await discharged.Content.ReadAsStringAsync());
        var conversation = dischargedDoc.RootElement.GetProperty("data").GetProperty("conversationId").GetString();

        await AssertWrapper(await Send(owner, HttpMethod.Post, "conversations/reopen", $"{{\"conversationId\":\"{conversation}\"}}"), HttpStatusCode.Forbidden);
        await AssertWrapper(await Send(owner, HttpMethod.Post, "messages/send", $"{{\"conversationId\":\"{conversation}\",\"text\":\"Hola\"}}"), HttpStatusCode.Conflict);
        using var reopened = await Send(vet, HttpMethod.Post, "conversations/reopen", $"{{\"conversationId\":\"{conversation}\"}}");
        using var reopenedDoc = JsonDocument.Parse(await reopened.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, reopened.StatusCode);
        Assert.Equal("Treating", reopenedDoc.RootElement.GetProperty("data").GetProperty("status").GetString());
        await AssertWrapper(await Send(owner, HttpMethod.Post, "messages/send", $"{{\"conversationId\":\"{conversation}\",\"text\":\"Hola\"}}"), HttpStatusCode.Created);
        await AssertWrapper(await Send(vet, HttpMethod.Post, "conversations/reopen", $"{{\"conversationId\":\"{Guid.NewGuid()}\"}}"), HttpStatusCode.NotFound);
    }

    [Trait("Endpoint", "conversations-reopen")]
    [Fact]
    public async Task Deleting_the_pet_removes_consultations_followups_conversations_and_messages()
    {
        var s = await Arrange();
        using var factory = s.Factory;
        using var owner = s.Owner;
        using var vet = s.Vet;
        var conversation = await Conversation(vet, s.PetId);
        await Send(owner, HttpMethod.Post, "messages/send", $"{{\"conversationId\":\"{conversation}\",\"text\":\"Hola\"}}");
        Assert.NotEmpty(factory.Messages.All);

        await AssertWrapper(await Send(owner, HttpMethod.Delete, $"pets/delete/{s.PetId}"), HttpStatusCode.OK);

        Assert.Empty(factory.Consultations.All);
        Assert.Empty(factory.Followups.All);
        Assert.Equal(0, factory.Conversations.Count);
        Assert.Empty(factory.Messages.All);
    }

    // ---------- Ficha completa de la veterinaria (pm-127) ----------

    [Trait("Endpoint", "scans-resolve")]
    [Fact]
    public async Task A_veterinary_receives_only_the_data_groups_the_owner_authorized_and_the_ficha_is_read_only()
    {
        var s = await Arrange(withGrant: false);
        using var factory = s.Factory;
        using var owner = s.Owner;
        using var vet = s.Vet;
        var today = DateTime.UtcNow.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
        await Send(owner, HttpMethod.Post, "vaccines/create", $"{{\"petId\":\"{s.PetId}\",\"appliedOn\":\"{today}\",\"name\":\"Rabia\"}}");
        await Send(owner, HttpMethod.Post, "dewormings/create", $"{{\"petId\":\"{s.PetId}\",\"appliedOn\":\"{today}\",\"product\":\"Drontal\"}}");
        await Send(owner, HttpMethod.Post, "weights/create", $"{{\"petId\":\"{s.PetId}\",\"measuredOn\":\"{today}\",\"kilograms\":8.5}}");
        await Send(owner, HttpMethod.Post, "allergies/create", $"{{\"petId\":\"{s.PetId}\",\"name\":\"Pollo\",\"kind\":\"Allergy\"}}");
        using var vaccinesOnly = await Send(owner, HttpMethod.Post, "access-grants/create", $"{{\"petId\":\"{s.PetId}\",\"businessId\":\"{s.VetBusinessId}\",\"data\":[\"Vaccines\"],\"durationMinutes\":30}}");
        Assert.Equal(HttpStatusCode.Created, vaccinesOnly.StatusCode);
        var code = factory.Pets.All.Single().Qr.Code;

        var first = await Resolve(vet, code);
        Assert.Equal(["Rabia"], first.GetProperty("vaccines").EnumerateArray().Select(v => v.GetProperty("name").GetString()!));
        Assert.Equal("Pollo", Assert.Single(first.GetProperty("allergies").EnumerateArray()).GetProperty("name").GetString());
        foreach (var missing in new[] { "dewormings", "weights", "consultations" })
        {
            Assert.False(first.TryGetProperty(missing, out _), $"{missing} no debe llegar sin autorización");
        }

        await Send(vet, HttpMethod.Post, "consultations/create", $"{{\"petId\":\"{s.PetId}\",\"reason\":\"Vómito\",\"diagnosis\":\"Gastritis\",\"medications\":[{{\"name\":\"Omeprazol\",\"dose\":\"10 mg\",\"frequency\":\"12 h\",\"duration\":\"7 días\"}}]}}");
        await Send(owner, HttpMethod.Post, "access-grants/create", $"{{\"petId\":\"{s.PetId}\",\"data\":[\"Dewormings\",\"Weights\",\"Consultations\"],\"durationMinutes\":30}}");
        var full = await Resolve(vet, code);

        Assert.Equal(["Rabia"], full.GetProperty("vaccines").EnumerateArray().Select(v => v.GetProperty("name").GetString()!));
        Assert.Equal("Drontal", Assert.Single(full.GetProperty("dewormings").EnumerateArray()).GetProperty("product").GetString());
        Assert.Equal(8.5m, Assert.Single(full.GetProperty("weights").EnumerateArray()).GetProperty("kilograms").GetDecimal());
        var consultation = Assert.Single(full.GetProperty("consultations").EnumerateArray());
        Assert.Equal("Gastritis", consultation.GetProperty("diagnosis").GetString());
        Assert.Equal("Omeprazol", Assert.Single(consultation.GetProperty("medications").EnumerateArray()).GetProperty("name").GetString());
        Assert.Equal(s.PetId, full.GetProperty("petId").GetString());
        // Solo lectura: la ficha no ofrece ninguna operación de escritura para ese resultado.
        using var write = await Send(vet, HttpMethod.Put, "scans/resolve", "{}");
        Assert.True(write.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.MethodNotAllowed);
    }

    [Trait("Endpoint", "scans-resolve")]
    [Fact]
    public async Task A_store_never_receives_the_full_groups_even_if_the_owner_authorized_everything_for_any_business()
    {
        var s = await Arrange(withGrant: false);
        using var factory = s.Factory;
        using var owner = s.Owner;
        using var vet = s.Vet;
        using var store = await Account(factory, "tienda@example.com");
        await Send(store, HttpMethod.Post, "businesses/create", "{\"type\":\"Store\",\"name\":\"Tienda\",\"phone\":\"5512345678\",\"address\":\"A\",\"latitude\":19.4,\"longitude\":-99.1}");
        await Send(owner, HttpMethod.Post, "vaccines/create", $"{{\"petId\":\"{s.PetId}\",\"appliedOn\":\"{DateTime.UtcNow:yyyy-MM-dd}\",\"name\":\"Rabia\"}}");
        await Send(owner, HttpMethod.Post, "access-grants/create", $"{{\"petId\":\"{s.PetId}\",\"data\":[\"Vaccines\",\"Dewormings\",\"Weights\",\"Consultations\"],\"durationMinutes\":30}}");

        var ficha = await Resolve(store, factory.Pets.All.Single().Qr.Code);

        Assert.Equal(["ageMonths", "ageYears", "allergies", "breed", "name", "photoUrl", "species"], ficha.EnumerateObject().Select(p => p.Name).Order(StringComparer.Ordinal));
    }

    // ---------- helpers ----------

    private static async Task<JsonElement> Resolve(HttpClient client, string code)
    {
        using var response = await Send(client, HttpMethod.Post, "scans/resolve", $"{{\"code\":\"{code}\"}}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return document.RootElement.GetProperty("data").Clone();
    }

    private static async Task<string> Followup(HttpClient vet, string consultationId, int? days)
    {
        var review = days is null ? string.Empty : $",\"nextReviewInDays\":{days}";
        using var response = await Send(vet, HttpMethod.Post, "followups/create", $"{{\"consultationId\":\"{consultationId}\"{review}}}");
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return document.RootElement.GetProperty("data").GetProperty("id").GetString()!;
    }

    private static async Task<string> Conversation(HttpClient vet, string petId)
    {
        var consultation = await NewConsultation(vet, petId);
        using var response = await Send(vet, HttpMethod.Post, "followups/create", $"{{\"consultationId\":\"{consultation}\"}}");
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return document.RootElement.GetProperty("data").GetProperty("conversationId").GetString()!;
    }

    private static async Task<JsonElement> Summary(HttpClient client)
    {
        using var response = await Send(client, HttpMethod.Get, "conversations/get-all");
        var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return document.RootElement.GetProperty("data").GetProperty("items")[0].Clone();
    }

    private static async Task<string[]> History(HttpClient client, string query)
    {
        using var response = await Send(client, HttpMethod.Get, $"history/get-all?{query}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return document.RootElement.GetProperty("data").GetProperty("items").EnumerateArray().Select(i => i.GetProperty("kind").GetString()!).ToArray();
    }

    private static async Task<HttpClient> Account(AuthFlowFactory factory, string email)
    {
        using var anonymous = factory.CreateClient();
        using var response = await Send(anonymous, HttpMethod.Post, "auth/register", $"{{\"displayName\":\"Ana\",\"email\":\"{email}\",\"password\":\"{Password}\"}}");
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return Bearer(factory, document);
    }

    private static async Task<HttpClient> Login(AuthFlowFactory factory, string email)
    {
        using var anonymous = factory.CreateClient();
        using var response = await Send(anonymous, HttpMethod.Post, "auth/login", $"{{\"email\":\"{email}\",\"password\":\"{Password}\"}}");
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return Bearer(factory, document);
    }

    private static HttpClient Bearer(AuthFlowFactory factory, JsonDocument document)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", document.RootElement.GetProperty("data").GetProperty("accessToken").GetString());
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

    private static async Task AssertWrapper(HttpResponseMessage response, HttpStatusCode expected)
    {
        using (response)
        {
            var raw = await response.Content.ReadAsStringAsync();
            Assert.Equal(expected, response.StatusCode);
            using var document = JsonDocument.Parse(raw);
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
