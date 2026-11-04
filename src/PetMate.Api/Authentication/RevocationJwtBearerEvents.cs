using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.JsonWebTokens;
using PetMate.Application.Security;
using PetMate.Infrastructure.Security;

namespace PetMate.Api.Authentication;

/// <summary>
/// Se ejecuta tras validar firma, algoritmo, issuer, audience y vigencia: exige los claims obligatorios y consulta la
/// lista negra de Redis de autenticación. Si Redis no responde, la excepción llega al middleware global (503):
/// jamás se concede acceso por omisión ni se confunde la caída con un 401.
/// </summary>
public sealed class RevocationJwtBearerEvents(ITokenRevocationStore revocationStore) : JwtBearerEvents
{
    public override async Task TokenValidated(TokenValidatedContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (context.SecurityToken is not JsonWebToken jwt || !JwtAccessTokenService.TryRead(jwt, out var claims) || claims is null)
        {
            context.Fail("Token sin los claims obligatorios.");
            return;
        }

        try
        {
            if (await revocationStore.IsRevoked(claims.Jti, context.HttpContext.RequestAborted).ConfigureAwait(false))
            {
                context.Fail("Token revocado.");
            }
        }
        catch (ArgumentException)
        {
            // jti con formato inválido (solo posible con la clave de firma comprometida): no se concede acceso.
            context.Fail("Identificador de token no válido.");
        }
    }
}
