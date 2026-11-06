using Microsoft.AspNetCore.Authorization;

namespace PetMate.Api.Authorization;

/// <summary>
/// Nombres de políticas y de roles de autorización (pm-033, T022). Convención: <c>Require&lt;Capacidad&gt;</c>.
/// Los nombres de rol deben coincidir con <c>UserRole</c> del dominio; una prueba lo verifica porque Api no referencia Domain.
/// No existe rol de administración (constitución §6.1).
/// </summary>
public static class AuthorizationPolicies
{
    /// <summary>Exige un usuario autenticado con el rol Owner.</summary>
    public const string RequireOwner = "RequireOwner";

    /// <summary>Exige un usuario autenticado con el rol Business.</summary>
    public const string RequireBusiness = "RequireBusiness";

    /// <summary>Nombre del claim de rol dentro del JWT validado.</summary>
    public const string RoleClaim = "role";

    /// <summary>Valor del claim de rol para propietarios de mascotas.</summary>
    public const string OwnerRole = "Owner";

    /// <summary>Valor del claim de rol para negocios.</summary>
    public const string BusinessRole = "Business";

    /// <summary>
    /// Registra la política por defecto y la política de respaldo (denegación por defecto) y las políticas por rol.
    /// Las decisiones usan únicamente claims de un JWT ya validado (firma, vigencia y lista negra); nunca cabeceras,
    /// cuerpo ni query.
    /// </summary>
    public static IServiceCollection AddPetMateAuthorization(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        // Un endpoint sin atributos de autorización exige esta política: se debe marcar [AllowAnonymous] de forma explícita.
        var baseline = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .RequireClaim(RoleClaim, OwnerRole, BusinessRole)
            .Build();

        services.AddAuthorizationBuilder()
            .SetDefaultPolicy(baseline)
            .SetFallbackPolicy(baseline)
            .AddPolicy(RequireOwner, policy => policy.RequireAuthenticatedUser().RequireClaim(RoleClaim, OwnerRole))
            .AddPolicy(RequireBusiness, policy => policy.RequireAuthenticatedUser().RequireClaim(RoleClaim, BusinessRole));

        services.AddSingleton<IAuthorizationMiddlewareResultHandler, UnmatchedRouteResultHandler>();

        return services;
    }
}
