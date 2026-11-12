using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PetMate.Api.Configuration;
using PetMate.Api.Contracts.Responses;
using PetMate.Application.Security;

namespace PetMate.Api.Authentication;

/// <summary>Límite por IP de las lecturas públicas de negocios por minuto (<c>PUBLIC_BUSINESS_RATE_LIMIT_PER_MINUTE</c>).</summary>
public sealed record PublicBusinessIpThrottleOptions(int PermitsPerMinute);

/// <summary>
/// Límite por IP de las lecturas públicas del mapa compartido entre réplicas (pm-120), con el mismo esquema que
/// <see cref="AuthIpThrottleFilter"/>: cuenta en Redis de autenticación, de modo que con N instancias el límite no se multiplica
/// por N. Falla cerrado. Al superarlo, 429 con <c>Retry-After</c>.
/// </summary>
public sealed class PublicBusinessIpThrottleFilter(IAuthAttemptThrottle throttle, PublicBusinessIpThrottleOptions options) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);

        var ct = context.HttpContext.RequestAborted;
        var key = RateLimitKey.For(context.HttpContext.Connection.RemoteIpAddress);
        var rule = new AuthThrottleRule(options.PermitsPerMinute, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
        var wait = await throttle.GetRemainingLock(AuthThrottleRules.PublicBusinessIpScope, key, ct).ConfigureAwait(false)
            ?? await throttle.RecordAttempt(AuthThrottleRules.PublicBusinessIpScope, key, rule, ct).ConfigureAwait(false);
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
