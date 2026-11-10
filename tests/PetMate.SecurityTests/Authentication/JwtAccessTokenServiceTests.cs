using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using PetMate.Domain.Users;
using PetMate.Infrastructure.Security;

namespace PetMate.SecurityTests.Authentication;

public sealed class JwtAccessTokenServiceTests
{
    private const string Issuer = "https://issuer-prueba.local";
    private const string Audience = "petmate-api-prueba";
    private const string Key = "clave-de-prueba-0123456789-abcdefghijkl";

    private readonly MutableTimeProvider clock = new(new DateTimeOffset(2026, 11, 3, 12, 0, 0, TimeSpan.Zero));

    private JwtAccessTokenService CreateService(string? key = null, string? issuer = null, string? audience = null, int minutes = 15)
        => new(new JwtOptions(issuer ?? Issuer, audience ?? Audience, Encoding.UTF8.GetBytes(key ?? Key), TimeSpan.FromMinutes(minutes)), clock);

    [Fact]
    public async Task Issued_token_validates_and_carries_only_the_required_claims()
    {
        var service = CreateService();
        var userId = Guid.NewGuid();

        var token = service.Issue(userId, UserRole.User);
        var claims = await service.Validate(token.Value, CancellationToken.None);

        Assert.NotNull(claims);
        Assert.Equal(userId, claims.UserId);
        Assert.Equal(UserRole.User, claims.Role);
        Assert.Equal(token.Jti, claims.Jti);
        Assert.Equal(clock.GetUtcNow().UtcDateTime.AddMinutes(15), claims.ExpiresAtUtc);

        var jwt = new JsonWebToken(token.Value);
        Assert.Equal("HS256", jwt.Alg);
        Assert.Equal(
            ["aud", "exp", "iat", "iss", "jti", "nbf", "role", "sub"],
            jwt.Claims.Select(claim => claim.Type).Order(StringComparer.Ordinal));
        Assert.Equal(Issuer, jwt.Issuer);
        Assert.Contains(Audience, jwt.Audiences);
    }

    [Fact]
    public void Every_token_has_a_unique_jti()
    {
        var service = CreateService();
        var jtis = Enumerable.Range(0, 200).Select(_ => service.Issue(Guid.NewGuid(), UserRole.User).Jti).ToArray();

        Assert.Equal(jtis.Length, jtis.Distinct().Count());
    }

    [Fact]
    public async Task User_role_round_trips()
    {
        var service = CreateService();
        var token = service.Issue(Guid.NewGuid(), UserRole.User);

        Assert.Equal(UserRole.User, (await service.Validate(token.Value, CancellationToken.None))!.Role);
    }

    [Fact]
    public async Task Token_signed_with_another_key_is_rejected()
    {
        var token = CreateService(key: "otra-clave-completamente-distinta-0123456789").Issue(Guid.NewGuid(), UserRole.User);

        Assert.Null(await CreateService().Validate(token.Value, CancellationToken.None));
    }

    [Fact]
    public async Task Tampered_payload_is_rejected()
    {
        var service = CreateService();
        var token = service.Issue(Guid.NewGuid(), UserRole.User).Value;
        var parts = token.Split('.');
        var payload = Encoding.UTF8.GetString(Base64UrlEncoder.DecodeBytes(parts[1])).Replace("\"User\"", "\"Owner\"", StringComparison.Ordinal);
        var forged = $"{parts[0]}.{Base64UrlEncoder.Encode(payload)}.{parts[2]}";

        Assert.Null(await service.Validate(forged, CancellationToken.None));
    }

    [Theory]
    [InlineData("HS384")]
    [InlineData("HS512")]
    public async Task Tokens_signed_with_a_different_hmac_algorithm_are_rejected(string algorithm)
    {
        var token = new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor
        {
            Issuer = Issuer,
            Audience = Audience,
            Expires = clock.GetUtcNow().UtcDateTime.AddMinutes(5),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(new string('k', 80))), algorithm),
            Claims = ValidClaims(),
        });

        Assert.Null(await CreateService().Validate(token, CancellationToken.None));
    }

    [Fact]
    public async Task Unsigned_alg_none_token_is_rejected()
    {
        var header = Base64UrlEncoder.Encode("{\"alg\":\"none\",\"typ\":\"JWT\"}");
        var payload = Base64UrlEncoder.Encode(JsonSerializer.Serialize(new Dictionary<string, object>
        {
            ["iss"] = Issuer,
            ["aud"] = Audience,
            ["sub"] = Guid.NewGuid().ToString("D"),
            ["jti"] = "abc12345",
            ["role"] = "Owner",
            ["exp"] = clock.GetUtcNow().AddMinutes(5).ToUnixTimeSeconds(),
        }));

        Assert.Null(await CreateService().Validate($"{header}.{payload}.", CancellationToken.None));
    }

    [Fact]
    public async Task Wrong_issuer_or_audience_is_rejected()
    {
        var token = CreateService(issuer: "https://otro-emisor.local").Issue(Guid.NewGuid(), UserRole.User);
        var otherAudience = CreateService(audience: "otra-audiencia").Issue(Guid.NewGuid(), UserRole.User);

        Assert.Null(await CreateService().Validate(token.Value, CancellationToken.None));
        Assert.Null(await CreateService().Validate(otherAudience.Value, CancellationToken.None));
    }

    [Fact]
    public async Task Token_is_rejected_exactly_at_expiration_with_zero_clock_skew()
    {
        var service = CreateService(minutes: 15);
        var token = service.Issue(Guid.NewGuid(), UserRole.User);

        clock.Advance(TimeSpan.FromMinutes(15).Subtract(TimeSpan.FromSeconds(1)));
        Assert.NotNull(await service.Validate(token.Value, CancellationToken.None));

        clock.Advance(TimeSpan.FromSeconds(1));
        Assert.Null(await service.Validate(token.Value, CancellationToken.None));
    }

    [Fact]
    public async Task Token_before_its_not_before_time_is_rejected()
    {
        var service = CreateService();
        var token = service.Issue(Guid.NewGuid(), UserRole.User);

        clock.Advance(TimeSpan.FromMinutes(-1));

        Assert.Null(await service.Validate(token.Value, CancellationToken.None));
    }

    [Theory]
    [InlineData("sub")]
    [InlineData("jti")]
    [InlineData("role")]
    public async Task Tokens_missing_a_required_claim_are_rejected(string missing)
    {
        var claims = ValidClaims();
        claims.Remove(missing);

        Assert.Null(await CreateService().Validate(Sign(claims), CancellationToken.None));
    }

    [Theory]
    [InlineData("role", "Admin")]
    [InlineData("role", "1")]
    [InlineData("role", "owner")]
    [InlineData("sub", "no-es-guid")]
    [InlineData("sub", "00000000-0000-0000-0000-000000000000")]
    public async Task Tokens_with_invalid_claim_values_are_rejected(string claim, string value)
    {
        var claims = ValidClaims();
        claims[claim] = value;

        Assert.Null(await CreateService().Validate(Sign(claims), CancellationToken.None));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("no-es-un-jwt")]
    [InlineData("a.b.c")]
    public async Task Garbage_input_is_rejected_without_throwing(string? garbage)
        => Assert.Null(await CreateService().Validate(garbage!, CancellationToken.None));

    [Fact]
    public async Task Oversized_tokens_are_rejected_before_parsing()
        => Assert.Null(await CreateService().Validate(new string('a', 10_000), CancellationToken.None));

    [Fact]
    public void Issued_token_never_contains_sensitive_material_and_tostring_hides_it()
    {
        var token = CreateService().Issue(Guid.NewGuid(), UserRole.User);
        var payload = Encoding.UTF8.GetString(Base64UrlEncoder.DecodeBytes(token.Value.Split('.')[1]));

        Assert.DoesNotContain("password", payload, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("email", payload, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(token.Value, token.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void Issue_rejects_empty_user_and_undefined_role()
    {
        var service = CreateService();

        Assert.Throws<ArgumentException>(() => service.Issue(Guid.Empty, UserRole.User));
        Assert.Throws<ArgumentException>(() => service.Issue(Guid.NewGuid(), (UserRole)42));
    }

    [Fact]
    public void Options_reject_short_keys_and_invalid_lifetimes()
    {
        Assert.Throws<ArgumentException>(() => new JwtOptions(Issuer, Audience, new byte[31], TimeSpan.FromMinutes(5)));
        Assert.Throws<ArgumentOutOfRangeException>(() => new JwtOptions(Issuer, Audience, new byte[32], TimeSpan.Zero));
        Assert.Throws<ArgumentOutOfRangeException>(() => new JwtOptions(Issuer, Audience, new byte[32], TimeSpan.FromMinutes(61)));
    }

    [Fact]
    public void Parse_applies_default_lifetime_and_reports_only_key_names()
    {
        var options = JwtOptions.Parse(Config(), isDevelopment: false);
        Assert.Equal(TimeSpan.FromMinutes(15), options.AccessTokenLifetime);
        Assert.Equal("JwtOptions", options.ToString());

        var error = Assert.Throws<InvalidOperationException>(() => JwtOptions.Parse(Config(("JWT_SIGNING_KEY", "corta-secreta"), ("JWT_ACCESS_TOKEN_MINUTES", "999")), false));
        Assert.Contains("JWT_SIGNING_KEY", error.Message, StringComparison.Ordinal);
        Assert.Contains("JWT_ACCESS_TOKEN_MINUTES", error.Message, StringComparison.Ordinal);
        Assert.DoesNotContain("corta-secreta", error.Message, StringComparison.Ordinal);
        Assert.DoesNotContain("999", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Template_keys_are_rejected_outside_development_and_flagged_inside()
    {
        var template = ("JWT_SIGNING_KEY", "clave_de_firma_placeholder_no_usar");

        Assert.Throws<InvalidOperationException>(() => JwtOptions.Parse(Config(template), isDevelopment: false));
        Assert.True(JwtOptions.Parse(Config(template), isDevelopment: true).UsesPlaceholderKey);
        Assert.False(JwtOptions.Parse(Config(), isDevelopment: false).UsesPlaceholderKey);
    }

    private static Dictionary<string, object> ValidClaims() => new()
    {
        ["sub"] = Guid.NewGuid().ToString("D"),
        ["jti"] = "jti-prueba-0001",
        ["role"] = "Owner",
    };

    private string Sign(Dictionary<string, object> claims) => new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor
    {
        Issuer = Issuer,
        Audience = Audience,
        IssuedAt = clock.GetUtcNow().UtcDateTime,
        NotBefore = clock.GetUtcNow().UtcDateTime,
        Expires = clock.GetUtcNow().UtcDateTime.AddMinutes(5),
        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Key)), SecurityAlgorithms.HmacSha256),
        Claims = claims,
    });

    private static IConfiguration Config(params (string Key, string Value)[] overrides)
    {
        var values = new Dictionary<string, string?>
        {
            ["JWT_ISSUER"] = Issuer,
            ["JWT_AUDIENCE"] = Audience,
            ["JWT_SIGNING_KEY"] = Key,
        };
        foreach (var (key, value) in overrides)
        {
            values[key] = value;
        }

        return new ConfigurationBuilder().AddInMemoryCollection(values).Build();
    }
}

/// <summary>Reloj controlable para pruebas deterministas.</summary>
internal sealed class MutableTimeProvider(DateTimeOffset start) : TimeProvider
{
    private DateTimeOffset now = start;

    public override DateTimeOffset GetUtcNow() => now;

    public void Advance(TimeSpan delta) => now += delta;
}
