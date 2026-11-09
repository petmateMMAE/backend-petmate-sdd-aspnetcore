using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using PetMate.ContractTests.Support;

namespace PetMate.ContractTests.Authentication;

/// <summary>
/// Escenarios de ciclo de vida de sesión entre Register, Login, Refresh y Logout con el JWT real y los dobles en memoria
/// (pm-035 CA-01): varias sesiones del mismo usuario son independientes, la rotación y el replay afectan solo a su sesión
/// y el cierre de sesión invalida el access token y el refresh token de esa sesión.
/// </summary>
[Collection(ApiEnvironmentGroup.Name)]
public sealed class SessionLifecycleTests
{
    private const string Password = "contraseña-muy-segura-123";

    [Trait("Endpoint", "transversal")]
    [Fact]
    public async Task Sessions_of_one_user_are_independent_across_register_login_refresh_replay_and_logout()
    {
        using var factory = new AuthFlowFactory();
        using var client = factory.CreateClient();

        // Dos sesiones del mismo usuario: la del registro (A) y un segundo inicio de sesión (B).
        var a = Tokens(await Post(client, "register", new { displayName = "Ana", email = "ana@example.com", password = Password }, HttpStatusCode.Created));
        var b = Tokens(await Post(client, "login", new { email = "ana@example.com", password = Password }, HttpStatusCode.OK));
        Assert.NotEqual(a.Refresh, b.Refresh);
        Assert.Equal(HttpStatusCode.OK, await GetProfile(client, a.Access));
        Assert.Equal(HttpStatusCode.OK, await GetProfile(client, b.Access));

        // A rota su refresh token; reutilizar el anterior (replay) revoca la sesión A pero no la B.
        var a2 = Tokens(await Post(client, "refresh", new { refreshToken = a.Refresh }, HttpStatusCode.OK));
        Assert.NotEqual(a.Refresh, a2.Refresh);
        Assert.Equal(HttpStatusCode.OK, await GetProfile(client, a.Access));

        // H8: reintentar el token ya consumido dentro de la ventana de gracia se rechaza SIN revocar la sesión.
        await Post(client, "refresh", new { refreshToken = a.Refresh }, HttpStatusCode.Unauthorized);
        Assert.Equal(HttpStatusCode.OK, await GetProfile(client, a2.Access));

        // Pasada la ventana, el mismo token es un replay: revoca la sesión A y, de inmediato (H7), todos sus access tokens.
        factory.Clock.Advance(PetMate.Application.Authentication.RefreshHandler.ReplayGrace + TimeSpan.FromSeconds(1));
        await Post(client, "refresh", new { refreshToken = a.Refresh }, HttpStatusCode.Unauthorized);
        await Post(client, "refresh", new { refreshToken = a2.Refresh }, HttpStatusCode.Unauthorized);
        Assert.Equal(HttpStatusCode.Unauthorized, await GetProfile(client, a.Access));
        Assert.Equal(HttpStatusCode.Unauthorized, await GetProfile(client, a2.Access));
        Assert.Equal(HttpStatusCode.OK, await GetProfile(client, b.Access));

        // B sigue renovándose con normalidad y cierra su sesión: su access token y su refresh token dejan de servir.
        var b2 = Tokens(await Post(client, "refresh", new { refreshToken = b.Refresh }, HttpStatusCode.OK));
        await Post(client, "logout", new { refreshToken = b2.Refresh }, HttpStatusCode.OK, b2.Access);
        Assert.Equal(HttpStatusCode.Unauthorized, await GetProfile(client, b2.Access));
        await Post(client, "refresh", new { refreshToken = b2.Refresh }, HttpStatusCode.Unauthorized);
        await Post(client, "logout", new { refreshToken = b2.Refresh }, HttpStatusCode.Unauthorized, b2.Access);

        // Tras todo eso la cuenta sigue usable: un nuevo inicio de sesión crea una sesión nueva.
        var c = Tokens(await Post(client, "login", new { email = "ana@example.com", password = Password }, HttpStatusCode.OK));
        Assert.Equal(HttpStatusCode.OK, await GetProfile(client, c.Access));
    }

    [Trait("Endpoint", "transversal")]
    [Fact]
    public async Task Two_users_never_share_sessions_even_with_the_same_flow()
    {
        using var factory = new AuthFlowFactory();
        using var client = factory.CreateClient();
        var ana = Tokens(await Post(client, "register", new { displayName = "Ana", email = "ana@example.com", password = Password }, HttpStatusCode.Created));
        var luis = Tokens(await Post(client, "register", new { displayName = "Ana", email = "luis@example.com", password = Password }, HttpStatusCode.Created));

        // Ana no puede cerrar la sesión de Luis con su propio token ni con el refresh token de Luis.
        await Post(client, "logout", new { refreshToken = luis.Refresh }, HttpStatusCode.Forbidden, ana.Access);

        Assert.Equal(HttpStatusCode.OK, await GetProfile(client, luis.Access));
        await Post(client, "refresh", new { refreshToken = luis.Refresh }, HttpStatusCode.OK);
    }

    private static async Task<HttpStatusCode> GetProfile(HttpClient client, string access)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/users/me/get");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", access);
        using var response = await client.SendAsync(request);
        return response.StatusCode;
    }

    private static async Task<string> Post(HttpClient client, string action, object body, HttpStatusCode expected, string? access = null)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/auth/{action}")
        {
            Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json"),
        };
        if (access is not null)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", access);
        }

        using var response = await client.SendAsync(request);
        var raw = await response.Content.ReadAsStringAsync();
        Assert.True(expected == response.StatusCode, $"POST {action}: se esperaba {expected} y se obtuvo {response.StatusCode}.");
        return raw;
    }

    private static (string Access, string Refresh) Tokens(string raw)
    {
        using var document = JsonDocument.Parse(raw);
        var data = document.RootElement.GetProperty("data");
        return (data.GetProperty("accessToken").GetString()!, data.GetProperty("refreshToken").GetString()!);
    }
}
