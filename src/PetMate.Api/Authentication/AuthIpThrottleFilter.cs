using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PetMate.Api.Configuration;
using PetMate.Api.Contracts.Responses;
using PetMate.Application.Security;

namespace PetMate.Api.Authentication;

/// <summary>Límite por IP de las rutas de autenticación por minuto (<c>AUTH_RATE_LIMIT_PER_MINUTE</c>).</summary>
public sealed record AuthIpThrottleOptions(int PermitsPerMinute);

/// <summary>
/// Límite por IP de <c>/api/v1/auth/*</c> compartido entre réplicas (pm-036 CA-05, hallazgo H6). El limitador en proceso sigue
/// como primera barrera barata; este filtro cuenta en Redis de autenticación, de modo que con N instancias el límite no se
/// multiplica por N. Falla cerrado: si Redis no responde la excepción llega al middleware global (503). Al superar el límite la
/// IP queda en espera durante un minuto (<c>Retry-After</c>).
/// </summary>
public sealed class AuthIpThrottleFilter(IAuthAttemptThrottle throttle, AuthIpThrottleOptions options) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);

        var ct = context.HttpContext.RequestAborted;
        var key = RateLimitKey.For(context.HttpContext.Connection.RemoteIpAddress);
        var rule = new AuthThrottleRule(options.PermitsPerMinute, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
        var wait = await throttle.GetRemainingLock(AuthThrottleRules.IpScope, key, ct).ConfigureAwait(false)
            ?? await throttle.RecordAttempt(AuthThrottleRules.IpScope, key, rule, ct).ConfigureAwait(false);
        if (wait is { } remaining)
        {
            var seconds = (int)Math.Ceiling(Math.Max(1, remaining.TotalSeconds));
            context.HttpContext.Response.Headers.RetryAfter = seconds.ToString(CultureInfo.InvariantCulture);
            context.Result = ApiResponseFactory.Create<object?>(
                StatusCodes.Status429TooManyRequests,
                ApiResponseMessages.ForStatusCode(StatusCodes.Status429TooManyRequests),
                null);
            return;
        }

        await next().ConfigureAwait(false);
    }
}
