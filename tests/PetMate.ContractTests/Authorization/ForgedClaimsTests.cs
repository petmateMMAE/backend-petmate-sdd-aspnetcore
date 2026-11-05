using System.Net;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using PetMate.Application.Security;
using PetMate.ContractTests.Authentication;
using PetMate.ContractTests.Support;
using PetMate.Domain.Users;

namespace PetMate.ContractTests.Authorization;

/// <summary>
/// pm-033 / T022: tokens válidos en forma pero con rol ausente, desconocido o manipulado nunca llegan a autorizarse.
/// El rol solo se acepta desde un JWT con firma válida; cualquier otro caso produce 401 con wrapper.
/// </summary>
[Collection(ApiEnvironmentGroup.Name)]
public sealed class ForgedClaimsTests
{
    private const string LongRoleClaim = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";
    private static readonly string[] BothRoles = ["Owner", "Business"];
    private static readonly string[] AllPaths = ["default", "owner", "business", "unmarked"];

    [Theory]
    [InlineData("Admin")]
    [InlineData("owner")]
    [InlineData("OWNER")]
    [InlineData("1")]
    [InlineData("")]
    [InlineData("Owner ")]
    public async Task Signed_token_with_an_unknown_role_value_is_rejected_with_401(string role)
    {
        using var factory = new AuthApiFactory();
        var token = Forge(new Dictionary<string, object> { ["role"] = role });

        await AssertRejectedEverywhere(factory, token);
    }

    [Fact]
    public async Task Signed_token_without_a_role_claim_is_rejected_with_401()
    {
        using var factory = new AuthApiFactory();

        await AssertRejectedEverywhere(factory, Forge(new Dictionary<string, object>()));
    }

    [Fact]
    public async Task Signed_token_whose_role_is_an_array_is_rejected_with_401()
    {
        using var factory = new AuthApiFactory();
        var token = Forge(new Dictionary<string, object> { ["role"] = BothRoles });

        await AssertRejectedEverywhere(factory, token);
    }

    [Fact]
    public async Task Long_microsoft_role_claim_does_not_grant_access_when_the_role_claim_is_missing()
    {
        using var factory = new AuthApiFactory();
        var token = Forge(new Dictionary<string, object> { [LongRoleClaim] = "Owner" });

        await AssertRejectedEverywhere(factory, token);
    }

    [Fact]
    public async Task Owner_role_signed_with_another_key_is_rejected_with_401()
    {
        using var factory = new AuthApiFactory();
        var otherKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("otra-clave-distinta-0123456789-abcdefghi"));
        var token = Forge(new Dictionary<string, object> { ["role"] = "Owner" }, otherKey);

        await AssertRejectedEverywhere(factory, token);
    }

    [Fact]
    public async Task Payload_edited_from_business_to_owner_without_re_signing_is_rejected_with_401()
    {
        using var factory = new AuthApiFactory();
        var valid = factory.Services.GetRequiredService<IAccessTokenService>().Issue(Guid.NewGuid(), UserRole.Business).Value;
        var parts = valid.Split('.');
        var payload = Base64UrlEncoder.Decode(parts[1]);
        Assert.Contains("\"role\":\"Business\"", payload, StringComparison.Ordinal);
        var edited = payload.Replace("\"role\":\"Business\"", "\"role\":\"Owner\"", StringComparison.Ordinal);
        var tampered = $"{parts[0]}.{Base64UrlEncoder.Encode(edited)}.{parts[2]}";

        await AssertRejectedEverywhere(factory, tampered);
    }

    [Fact]
    public async Task Unsigned_token_with_alg_none_and_owner_role_is_rejected_with_401()
    {
        using var factory = new AuthApiFactory();
        var now = DateTimeOffset.UtcNow;
        var header = Base64UrlEncoder.Encode("{\"alg\":\"none\",\"typ\":\"JWT\"}");
        var payload = Base64UrlEncoder.Encode(
            $"{{\"sub\":\"{Guid.NewGuid():D}\",\"jti\":\"{Guid.NewGuid():N}\",\"role\":\"Owner\",\"iss\":\"{IsolatedApiFactory.JwtIssuer}\"," +
            $"\"aud\":\"{IsolatedApiFactory.JwtAudience}\",\"nbf\":{now.AddMinutes(-1).ToUnixTimeSeconds()},\"exp\":{now.AddMinutes(10).ToUnixTimeSeconds()}}}");

        await AssertRejectedEverywhere(factory, $"{header}.{payload}.");
    }

    [Fact]
    public async Task Token_that_is_valid_in_every_other_way_but_expired_is_rejected_on_the_owner_policy()
    {
        using var factory = new AuthApiFactory();
        var token = factory.Services.GetRequiredService<IAccessTokenService>().Issue(Guid.NewGuid(), UserRole.Owner);
        factory.Clock.Advance(TimeSpan.FromMinutes(16));
        using var client = AuthzAssert.Bearer(factory, token.Value);

        using var response = await client.GetAsync("/api/v1/test-authz/owner");

        await AuthzAssert.Wrapped(response, HttpStatusCode.Unauthorized);
    }

    private static async Task AssertRejectedEverywhere(AuthApiFactory factory, string token)
    {
        using var client = AuthzAssert.Bearer(factory, token);
        foreach (var path in AllPaths)
        {
            using var response = await client.GetAsync($"/api/v1/test-authz/{path}");
            await AuthzAssert.Wrapped(response, HttpStatusCode.Unauthorized);
        }
    }

    // Token firmado con la clave de prueba (o la indicada) con sub, jti y los claims adicionales; la vigencia es válida
    // respecto al reloj de AuthApiFactory (que arranca en "ahora"), por eso nbf e iat retroceden un minuto.
    private static string Forge(Dictionary<string, object> extraClaims, SecurityKey? key = null)
    {
        var now = DateTime.UtcNow;
        var claims = new Dictionary<string, object>
        {
            [JwtRegisteredClaimNames.Sub] = Guid.NewGuid().ToString("D"),
            [JwtRegisteredClaimNames.Jti] = Guid.NewGuid().ToString("N"),
        };
        foreach (var (name, value) in extraClaims)
        {
            claims[name] = value;
        }

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = IsolatedApiFactory.JwtIssuer,
            Audience = IsolatedApiFactory.JwtAudience,
            IssuedAt = now.AddMinutes(-1),
            NotBefore = now.AddMinutes(-1),
            Expires = now.AddMinutes(10),
            SigningCredentials = new SigningCredentials(
                key ?? new SymmetricSecurityKey(Encoding.UTF8.GetBytes(IsolatedApiFactory.JwtKey)),
                SecurityAlgorithms.HmacSha256),
            Claims = claims,
        };

        return new JsonWebTokenHandler { MapInboundClaims = false }.CreateToken(descriptor);
    }
}
