using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using PetMate.Application.Security;
using PetMate.Domain.Users;

namespace PetMate.Infrastructure.Security;

/// <summary>
/// JWT HS256 de corta vida. Claims: <c>sub</c>, <c>jti</c>, <c>role</c>, <c>iss</c>, <c>aud</c>, <c>iat</c>, <c>nbf</c>, <c>exp</c>.
/// Nunca incluye contraseñas, hashes, correo ni datos de mascotas. El algoritmo permitido es fijo (HS256).
/// </summary>
public sealed class JwtAccessTokenService : IAccessTokenService
{
    private const int MaxTokenLength = 4096;

    private readonly JwtOptions options;
    private readonly TimeProvider timeProvider;
    private readonly JsonWebTokenHandler handler = new() { MapInboundClaims = false };
    private readonly TokenValidationParameters validationParameters;

    public JwtAccessTokenService(JwtOptions options, TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(timeProvider);
        this.options = options;
        this.timeProvider = timeProvider;
        validationParameters = CreateValidationParameters(options, timeProvider);
    }

    /// <summary>Parámetros de validación compartidos con el middleware JwtBearer: mismo contrato en ambos caminos.</summary>
    public static TokenValidationParameters CreateValidationParameters(JwtOptions options, TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(timeProvider);

        return new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = options.Issuer,
            ValidateAudience = true,
            ValidAudience = options.Audience,
            ValidateLifetime = true,
            RequireExpirationTime = true,
            RequireSignedTokens = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = options.CreateKey(),
            ValidAlgorithms = [JwtOptions.Algorithm],
            ClockSkew = TimeSpan.Zero,
            // Vigencia evaluada con el reloj inyectado (pruebas deterministas); nbf <= ahora < exp, sin tolerancia.
            LifetimeValidator = (notBefore, expires, _, _) =>
            {
                var now = timeProvider.GetUtcNow().UtcDateTime;
                return expires is not null && now < expires && (notBefore is null || now >= notBefore);
            },
            NameClaimType = JwtRegisteredClaimNames.Sub,
            RoleClaimType = "role",
        };
    }

    public TimeSpan Lifetime => options.AccessTokenLifetime;

    public AccessToken Issue(Guid userId, UserRole role, Guid? sessionId = null)
    {
        if (userId == Guid.Empty || !Enum.IsDefined(role))
        {
            throw new ArgumentException("Usuario o rol no válido para emitir un token.");
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var expires = now + options.AccessTokenLifetime;
        var jti = Guid.NewGuid().ToString("N");
        var tokenClaims = new Dictionary<string, object>
        {
            [JwtRegisteredClaimNames.Sub] = userId.ToString("D"),
            [JwtRegisteredClaimNames.Jti] = jti,
            ["role"] = role.ToString(),
        };
        if (sessionId is { } sid && sid != Guid.Empty)
        {
            tokenClaims["sid"] = sid.ToString("D");
        }

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = options.Issuer,
            Audience = options.Audience,
            IssuedAt = now,
            NotBefore = now,
            Expires = expires,
            SigningCredentials = new SigningCredentials(options.CreateKey(), JwtOptions.Algorithm),
            Claims = tokenClaims,
        };

        return new AccessToken(handler.CreateToken(descriptor), jti, expires);
    }

    public async Task<AccessTokenClaims?> Validate(string token, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(token) || token.Length > MaxTokenLength)
        {
            return null;
        }

        var result = await handler.ValidateTokenAsync(token, validationParameters).ConfigureAwait(false);
        if (!result.IsValid || result.SecurityToken is not JsonWebToken jwt)
        {
            return null;
        }

        return TryRead(jwt, out var claims) ? claims : null;
    }

    /// <summary>Extrae y valida los claims obligatorios de un JWT ya verificado criptográficamente.</summary>
    public static bool TryRead(JsonWebToken jwt, out AccessTokenClaims? claims)
    {
        ArgumentNullException.ThrowIfNull(jwt);
        claims = null;

        if (!jwt.TryGetPayloadValue<string>(JwtRegisteredClaimNames.Sub, out var subject) || !Guid.TryParseExact(subject, "D", out var userId)
            || userId == Guid.Empty
            || !jwt.TryGetPayloadValue<string>(JwtRegisteredClaimNames.Jti, out var jti) || string.IsNullOrWhiteSpace(jti)
            || !jwt.TryGetPayloadValue<string>("role", out var roleName)
            || !Enum.TryParse<UserRole>(roleName, ignoreCase: false, out var role) || Enum.GetName(role) != roleName)
        {
            return false;
        }

        // `sid` es opcional (tokens anteriores a H7 siguen vigentes hasta expirar); si existe debe ser un GUID válido.
        Guid? sessionId = null;
        if (jwt.TryGetPayloadValue<string>("sid", out var sidText))
        {
            if (!Guid.TryParseExact(sidText, "D", out var sid) || sid == Guid.Empty)
            {
                return false;
            }

            sessionId = sid;
        }

        claims = new AccessTokenClaims(userId, role, jti, DateTime.SpecifyKind(jwt.ValidTo, DateTimeKind.Utc), sessionId);
        return true;
    }
}
