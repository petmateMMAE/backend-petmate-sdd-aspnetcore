using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using PetMate.Application.Images;
using PetMate.ContractTests.Authentication;
using PetMate.ContractTests.Support;

namespace PetMate.ContractTests.Images;

/// <summary>
/// Contrato HTTP de las fotos (pm-080, pm-081, pm-111) con JWT real y almacenamiento en memoria. El propietario y el
/// negocio salen siempre del token; el tipo de la imagen se deduce de sus bytes, nunca del nombre del archivo.
/// </summary>
[Collection(ApiEnvironmentGroup.Name)]
public sealed class PhotoEndpointsTests
{
    private const string Pet = "{\"name\":\"Firulais\",\"species\":\"Perro\",\"sex\":\"Male\"}";
    private const string Store = "{\"type\":\"Store\",\"name\":\"Pet Shop Sol\",\"phone\":\"5512345678\",\"address\":\"Av. Reforma 10\",\"latitude\":19.4326,\"longitude\":-99.1332}";

    private static readonly byte[] Jpeg = [0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, 0x49, 0x46, 0x00, 0x01, 0x02, 0x03];
    private static readonly byte[] Png = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00, 0x00, 0x0D, 0x01];
    private static readonly byte[] NotAnImage = Encoding.UTF8.GetBytes("MZ esto es un ejecutable disfrazado de foto");

    // ---------- Foto de una mascota (pm-080, pm-081) ----------

    [Trait("Endpoint", "pets-upload-photo")]
    [Fact]
    public async Task Upload_pet_photo_stores_it_and_returns_a_temporary_url()
    {
        using var factory = new AuthFlowFactory();
        using var client = await Owner(factory, "foto-alta@petmate.mx");
        var petId = await CreatePet(client);

        using var response = await Upload(client, $"pets/upload-photo/{petId}", Jpeg, "firulais.jpg");
        var data = await Data(response, HttpStatusCode.OK);

        Assert.Equal(petId, data.GetProperty("petId").GetString());
        Assert.StartsWith("https://", data.GetProperty("url").GetString(), StringComparison.Ordinal);
        Assert.True(data.GetProperty("expiresAt").GetDateTimeOffset() > DateTimeOffset.UtcNow);
        Assert.Single(factory.Images.Keys);
        Assert.Equal(Jpeg, factory.Images.Content(ImageContainer.PetPhotos, Key(factory, petId)));
    }

    [Trait("Endpoint", "pets-upload-photo")]
    [Fact]
    public async Task Upload_pet_photo_rejects_a_file_that_is_not_an_image_even_with_an_image_name()
    {
        using var factory = new AuthFlowFactory();
        using var client = await Owner(factory, "foto-falsa@petmate.mx");
        var petId = await CreatePet(client);

        using var response = await Upload(client, $"pets/upload-photo/{petId}", NotAnImage, "firulais.jpg", "image/jpeg");

        await AssertWrapper(response, HttpStatusCode.BadRequest);
        Assert.Empty(factory.Images.Keys);
    }

    [Trait("Endpoint", "pets-upload-photo")]
    [Fact]
    public async Task Upload_pet_photo_rejects_a_file_over_the_limit()
    {
        using var factory = new AuthFlowFactory();
        using var client = await Owner(factory, "foto-grande@petmate.mx");
        var petId = await CreatePet(client);

        var big = new byte[ImageRules.MaxPetPhotoBytes + 1];
        Jpeg.CopyTo(big.AsSpan());

        using var response = await Upload(client, $"pets/upload-photo/{petId}", big, "grande.jpg");

        await AssertWrapper(response, HttpStatusCode.RequestEntityTooLarge);
        Assert.Empty(factory.Images.Keys);
    }

    [Trait("Endpoint", "pets-upload-photo")]
    [Fact]
    public async Task Upload_pet_photo_of_another_owner_responds_404_and_stores_nothing()
    {
        using var factory = new AuthFlowFactory();
        using var mine = await Owner(factory, "foto-mia@petmate.mx");
        using var theirs = await Owner(factory, "foto-ajena@petmate.mx");
        var petId = await CreatePet(mine);

        using var response = await Upload(theirs, $"pets/upload-photo/{petId}", Jpeg, "ajena.jpg");

        await AssertWrapper(response, HttpStatusCode.NotFound);
        Assert.Empty(factory.Images.Keys);
    }

    [Trait("Endpoint", "pets-upload-photo")]
    [Fact]
    public async Task Upload_pet_photo_without_a_file_responds_400()
    {
        using var factory = new AuthFlowFactory();
        using var client = await Owner(factory, "foto-sin-archivo@petmate.mx");
        var petId = await CreatePet(client);

        using var content = new MultipartFormDataContent("limite-de-prueba");
        content.Add(new StringContent("hola"), "comentario");
        using var response = await client.PostAsync(new Uri($"/api/v1/pets/upload-photo/{petId}", UriKind.Relative), content);

        await AssertWrapper(response, HttpStatusCode.BadRequest);
    }

    [Trait("Endpoint", "pets-get-photo-url")]
    [Fact]
    public async Task Get_pet_photo_url_only_signs_after_checking_the_owner()
    {
        using var factory = new AuthFlowFactory();
        using var mine = await Owner(factory, "url-mia@petmate.mx");
        using var theirs = await Owner(factory, "url-ajena@petmate.mx");
        var petId = await CreatePet(mine);
        using (var upload = await Upload(mine, $"pets/upload-photo/{petId}", Png, "firulais.png"))
        {
            Assert.Equal(HttpStatusCode.OK, upload.StatusCode);
        }

        using var ajena = await theirs.GetAsync(new Uri($"/api/v1/pets/get-photo-url/{petId}", UriKind.Relative));
        await AssertWrapper(ajena, HttpStatusCode.NotFound);

        using var propia = await mine.GetAsync(new Uri($"/api/v1/pets/get-photo-url/{petId}", UriKind.Relative));
        var data = await Data(propia, HttpStatusCode.OK);
        Assert.StartsWith("https://", data.GetProperty("url").GetString(), StringComparison.Ordinal);
    }

    [Trait("Endpoint", "pets-get-photo-url")]
    [Fact]
    public async Task Get_pet_photo_url_responds_404_when_the_pet_has_no_uploaded_photo()
    {
        using var factory = new AuthFlowFactory();
        using var client = await Owner(factory, "url-sin-foto@petmate.mx");
        var petId = await CreatePet(client);

        using var response = await client.GetAsync(new Uri($"/api/v1/pets/get-photo-url/{petId}", UriKind.Relative));

        await AssertWrapper(response, HttpStatusCode.NotFound);
    }

    [Trait("Endpoint", "pets-delete-photo")]
    [Fact]
    public async Task Delete_pet_photo_removes_the_object_and_the_url_stops_being_issued()
    {
        using var factory = new AuthFlowFactory();
        using var client = await Owner(factory, "foto-borrar@petmate.mx");
        var petId = await CreatePet(client);
        using (var upload = await Upload(client, $"pets/upload-photo/{petId}", Jpeg, "firulais.jpg"))
        {
            Assert.Equal(HttpStatusCode.OK, upload.StatusCode);
        }

        using var deleted = await client.DeleteAsync(new Uri($"/api/v1/pets/delete-photo/{petId}", UriKind.Relative));
        await AssertWrapper(deleted, HttpStatusCode.OK);

        Assert.Empty(factory.Images.Keys);
        using var after = await client.GetAsync(new Uri($"/api/v1/pets/get-photo-url/{petId}", UriKind.Relative));
        await AssertWrapper(after, HttpStatusCode.NotFound);
    }

    // ---------- Foto de un negocio (pm-111) ----------

    [Trait("Endpoint", "businesses-upload-photo")]
    [Fact]
    public async Task Upload_business_photo_returns_a_public_url_and_shows_it_in_the_public_profile()
    {
        using var factory = new AuthFlowFactory();
        using var client = await Owner(factory, "negocio-foto@petmate.mx");
        var businessId = await CreateBusiness(client);

        using var response = await Upload(client, "businesses/upload-photo", Png, "local.png");
        var data = await Data(response, HttpStatusCode.OK);
        var photoUrl = data.GetProperty("photoUrl").GetString();

        Assert.StartsWith("https://", photoUrl, StringComparison.Ordinal);
        Assert.Contains("business-photos", photoUrl, StringComparison.Ordinal);

        using var anonymous = factory.CreateClient();
        using var profile = await anonymous.GetAsync(new Uri($"/api/v1/businesses/get-by-id/{businessId}", UriKind.Relative));
        var publicData = await Data(profile, HttpStatusCode.OK);
        Assert.Equal(photoUrl, publicData.GetProperty("photoUrl").GetString());
    }

    [Trait("Endpoint", "businesses-upload-photo")]
    [Fact]
    public async Task Upload_business_photo_rejects_over_one_megabyte()
    {
        using var factory = new AuthFlowFactory();
        using var client = await Owner(factory, "negocio-grande@petmate.mx");
        await CreateBusiness(client);

        var big = new byte[ImageRules.MaxBusinessPhotoBytes + 1];
        Png.CopyTo(big.AsSpan());

        using var response = await Upload(client, "businesses/upload-photo", big, "local.png");

        await AssertWrapper(response, HttpStatusCode.RequestEntityTooLarge);
        Assert.Empty(factory.Images.Keys);
    }

    [Trait("Endpoint", "businesses-upload-photo")]
    [Fact]
    public async Task Upload_business_photo_without_a_business_responds_403()
    {
        using var factory = new AuthFlowFactory();
        using var client = await Owner(factory, "sin-negocio-foto@petmate.mx");

        using var response = await Upload(client, "businesses/upload-photo", Jpeg, "local.jpg");

        await AssertWrapper(response, HttpStatusCode.Forbidden);
        Assert.Empty(factory.Images.Keys);
    }

    [Trait("Endpoint", "businesses-delete-photo")]
    [Fact]
    public async Task Delete_business_photo_clears_it_from_the_public_profile()
    {
        using var factory = new AuthFlowFactory();
        using var client = await Owner(factory, "negocio-borrar-foto@petmate.mx");
        var businessId = await CreateBusiness(client);
        using (var upload = await Upload(client, "businesses/upload-photo", Png, "local.png"))
        {
            Assert.Equal(HttpStatusCode.OK, upload.StatusCode);
        }

        using var deleted = await client.DeleteAsync(new Uri("/api/v1/businesses/delete-photo", UriKind.Relative));
        await AssertWrapper(deleted, HttpStatusCode.OK);

        Assert.Empty(factory.Images.Keys);
        using var anonymous = factory.CreateClient();
        using var profile = await anonymous.GetAsync(new Uri($"/api/v1/businesses/get-by-id/{businessId}", UriKind.Relative));
        var publicData = await Data(profile, HttpStatusCode.OK);
        Assert.Equal(JsonValueKind.Null, publicData.GetProperty("photoUrl").ValueKind);
    }

    [Trait("Endpoint", "businesses-delete-photo")]
    [Fact]
    public async Task Delete_business_photo_without_a_business_responds_403()
    {
        using var factory = new AuthFlowFactory();
        using var client = await Owner(factory, "sin-negocio-borrar@petmate.mx");

        using var response = await client.DeleteAsync(new Uri("/api/v1/businesses/delete-photo", UriKind.Relative));

        await AssertWrapper(response, HttpStatusCode.Forbidden);
    }

    // ---------- Apoyo ----------

    private static string Key(AuthFlowFactory factory, string petId)
    {
        var pet = factory.Pets.All.Single(p => p.Id.ToString() == petId);
        return PhotoKeys.ForPet(pet.OwnerId, pet.Id);
    }

    private static async Task<HttpResponseMessage> Upload(HttpClient client, string route, byte[] bytes, string fileName, string? contentType = null)
    {
        using var content = new MultipartFormDataContent("limite-de-prueba");
        var file = new ByteArrayContent(bytes);
        file.Headers.ContentType = new MediaTypeHeaderValue(contentType ?? "application/octet-stream");
        content.Add(file, "file", fileName);
        return await client.PostAsync(new Uri($"/api/v1/{route}", UriKind.Relative), content);
    }

    private static async Task<string> CreatePet(HttpClient client)
    {
        using var content = new StringContent(Pet, Encoding.UTF8, "application/json");
        using var response = await client.PostAsync(new Uri("/api/v1/pets/create", UriKind.Relative), content);
        var data = await Data(response, HttpStatusCode.Created);
        return data.GetProperty("id").GetString()!;
    }

    private static async Task<string> CreateBusiness(HttpClient client)
    {
        using var content = new StringContent(Store, Encoding.UTF8, "application/json");
        using var response = await client.PostAsync(new Uri("/api/v1/businesses/create", UriKind.Relative), content);
        var data = await Data(response, HttpStatusCode.Created);
        return data.GetProperty("id").GetString()!;
    }

    private static async Task<HttpClient> Owner(AuthFlowFactory factory, string email)
    {
        using var anonymous = factory.CreateClient();
        using var content = new StringContent(
            $"{{\"displayName\":\"Ana\",\"email\":\"{email}\",\"password\":\"contrasena-segura-123\"}}",
            Encoding.UTF8,
            "application/json");
        using var response = await anonymous.PostAsync(new Uri("/api/v1/auth/register", UriKind.Relative), content);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var access = document.RootElement.GetProperty("data").GetProperty("accessToken").GetString();

        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", access);
        return client;
    }

    /// <summary>Comprueba el código y devuelve <c>data</c> ya materializado (el documento se libera aquí).</summary>
    private static async Task<JsonElement> Data(HttpResponseMessage response, HttpStatusCode expected)
    {
        var raw = await response.Content.ReadAsStringAsync();
        Assert.Equal(expected, response.StatusCode);
        using var document = JsonDocument.Parse(raw);
        return document.RootElement.GetProperty("data").Clone();
    }

    private static async Task AssertWrapper(HttpResponseMessage response, HttpStatusCode expected)
    {
        var raw = await response.Content.ReadAsStringAsync();
        Assert.Equal(expected, response.StatusCode);
        using var document = JsonDocument.Parse(raw);
        Assert.Equal(["data", "message", "statusCode"], document.RootElement.EnumerateObject().Select(p => p.Name).Order(StringComparer.Ordinal));
        Assert.Equal((int)expected, document.RootElement.GetProperty("statusCode").GetInt32());
    }
}
