using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using PetMate.Application.Security;
using PetMate.ContractTests.Authentication;
using PetMate.ContractTests.Support;
using PetMate.Domain.Users;

namespace PetMate.ContractTests.Users;

/// <summary>
/// El perfil propio solo existe para cuentas reales: un access token válido de una cuenta que no existe (por ejemplo,
/// eliminada) responde 404 con el wrapper y nunca crea ni expone datos. La identidad viene únicamente del claim <c>sub</c>.
/// </summary>
[Collection(ApiEnvironmentGroup.Name)]
public sealed class UserProfileAccountTests
{
    [Trait("Endpoint", "users-me-get")]
    [Fact]
    public async Task Get_for_a_valid_token_of_a_missing_account_returns_404_wrapper()
    {
        using var factory = new AuthFlowFactory();
        using var client = Authenticated(factory, Guid.NewGuid());

        using var response = await client.GetAsync("/api/v1/users/me/get");

        await AssertNotFoundWrapper(response);
    }

    [Trait("Endpoint", "users-me-update")]
    [Fact]
    public async Task Update_for_a_valid_token_of_a_missing_account_returns_404_wrapper_and_creates_nothing()
    {
        using var factory = new AuthFlowFactory();
        using var client = Authenticated(factory, Guid.NewGuid());
        using var body = new StringContent("{\"displayName\":\"Ana\"}", System.Text.Encoding.UTF8, "application/json");

        using var response = await client.PutAsync("/api/v1/users/me/update", body);

        await AssertNotFoundWrapper(response);
        Assert.Empty(factory.Users.All);
    }

    [Trait("Endpoint", "users-me-update")]
    [Theory]
    [InlineData("")]
    [InlineData("no-es-json")]
    public async Task Update_with_an_empty_or_malformed_body_returns_400_wrapper(string raw)
    {
        using var factory = new AuthFlowFactory();
        using var client = Authenticated(factory, Guid.NewGuid());
        using var body = new StringContent(raw, System.Text.Encoding.UTF8, "application/json");

        using var response = await client.PutAsync("/api/v1/users/me/update", body);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(400, document.RootElement.GetProperty("statusCode").GetInt32());
    }

    private static HttpClient Authenticated(AuthFlowFactory factory, Guid userId)
    {
        var token = factory.Services.GetRequiredService<IAccessTokenService>().Issue(userId, UserRole.User);
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Value);
        return client;
    }

    private static async Task AssertNotFoundWrapper(HttpResponseMessage response)
    {
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal(404, document.RootElement.GetProperty("statusCode").GetInt32());
        Assert.Equal(JsonValueKind.Null, document.RootElement.GetProperty("data").ValueKind);
    }
}
