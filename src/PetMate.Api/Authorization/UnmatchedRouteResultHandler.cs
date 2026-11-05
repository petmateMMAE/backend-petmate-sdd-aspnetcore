using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Routing;

namespace PetMate.Api.Authorization;

/// <summary>
/// La política de respaldo (denegación por defecto) también se evalúa cuando ninguna ruta coincide y para los endpoints
/// sintéticos del enrutador (405 por método no permitido, 415 por tipo de contenido). Para no convertir esos 404, 405 y 415
/// en 401, la solicitud sin endpoint de aplicación (<see cref="RouteEndpoint"/>) continúa la canalización y termina en su
/// respuesta habitual. Con un endpoint de aplicación, el resultado lo resuelve el manejador estándar de ASP.NET Core
/// (401 con wrapper para no autenticados, 403 con wrapper para autenticados sin permiso).
/// </summary>
public sealed class UnmatchedRouteResultHandler : IAuthorizationMiddlewareResultHandler
{
    private readonly AuthorizationMiddlewareResultHandler defaultHandler = new();

    public Task HandleAsync(RequestDelegate next, HttpContext context, AuthorizationPolicy policy, PolicyAuthorizationResult authorizeResult)
    {
        ArgumentNullException.ThrowIfNull(next);
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(policy);
        ArgumentNullException.ThrowIfNull(authorizeResult);

        return !authorizeResult.Succeeded && context.GetEndpoint() is not RouteEndpoint
            ? next(context)
            : defaultHandler.HandleAsync(next, context, policy, authorizeResult);
    }
}
