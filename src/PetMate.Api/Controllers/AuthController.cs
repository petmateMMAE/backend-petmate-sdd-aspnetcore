using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using PetMate.Api.Contracts.Auth;
using PetMate.Api.Contracts.Responses;
using PetMate.Application.Authentication;
using PetMate.Application.Mediator;

namespace PetMate.Api.Controllers;

/// <summary>
/// Controlador delgado: traduce HTTP a Commands y resultados a HTTP. No contiene reglas de negocio ni accede a Atlas/Redis.
/// Los mensajes de error son genéricos y no distinguen causas sensibles.
/// </summary>
[ApiController]
[Route("api/v1/auth")]
public sealed partial class AuthController(ISender sender, ILogger<AuthController> logger) : ControllerBase
{
    public const string RateLimitPolicy = "auth";

    [AllowAnonymous]
    [EnableRateLimiting(RateLimitPolicy)]
    [ServiceFilter(typeof(PetMate.Api.Authentication.AuthIpThrottleFilter))]
    [HttpPost("register")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<ValidationErrorsData>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<ObjectResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new RegisterCommand(request.Email ?? string.Empty, request.Password ?? string.Empty), cancellationToken);
        return result.IsSuccess
            ? ApiResponseFactory.Create(StatusCodes.Status201Created, "Cuenta creada correctamente.", ToResponse(result.Value!))
            : Failure(result.Error, result.RetryAfter);
    }

    [AllowAnonymous]
    [EnableRateLimiting(RateLimitPolicy)]
    [ServiceFilter(typeof(PetMate.Api.Authentication.AuthIpThrottleFilter))]
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status429TooManyRequests)]
    public async Task<ObjectResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new LoginCommand(request.Email ?? string.Empty, request.Password ?? string.Empty, PetMate.Api.Configuration.RateLimitKey.For(HttpContext.Connection.RemoteIpAddress)), cancellationToken);
        if (!result.IsSuccess && result.Error is AuthError.InvalidCredentials or AuthError.TooManyRequests)
        {
            // Evento de seguridad (I2): solo un seudónimo de la cuenta, nunca el correo, la contraseña ni la IP.
            var account = SecurityEventPseudonym.From(request.Email);
            if (result.Error == AuthError.InvalidCredentials)
            {
                LogLoginFailed(logger, account);
            }
            else
            {
                LogAuthThrottled(logger, "login", account);
            }
        }

        return result.IsSuccess
            ? ApiResponseFactory.Ok("Inicio de sesión correcto.", ToResponse(result.Value!))
            : Failure(result.Error, result.RetryAfter);
    }

    [AllowAnonymous]
    [EnableRateLimiting(RateLimitPolicy)]
    [ServiceFilter(typeof(PetMate.Api.Authentication.AuthIpThrottleFilter))]
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status429TooManyRequests)]
    public async Task<ObjectResult> Refresh([FromBody] RefreshRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new RefreshCommand(request.RefreshToken ?? string.Empty), cancellationToken);
        if (!result.IsSuccess && result.Error == AuthError.InvalidRefreshToken)
        {
            LogRefreshRejected(logger);
        }
        else if (!result.IsSuccess && result.Error == AuthError.TooManyRequests)
        {
            LogAuthThrottled(logger, "refresh", "n/a");
        }

        return result.IsSuccess
            ? ApiResponseFactory.Ok("Sesión renovada correctamente.", ToResponse(result.Value!))
            : Failure(result.Error, result.RetryAfter);
    }

    [Authorize]
    [EnableRateLimiting(UsersController.RateLimitPolicy)]
    [HttpPost("logout")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status429TooManyRequests)]
    public async Task<ObjectResult> Logout([FromBody] LogoutRequest request, CancellationToken cancellationToken)
    {
        // Identidad tomada solo del Access Token ya validado (firma, vigencia y lista negra).
        if (!Guid.TryParse(User.FindFirst("sub")?.Value, out var userId)
            || User.FindFirst("jti")?.Value is not { Length: > 0 } jti
            || !long.TryParse(User.FindFirst("exp")?.Value, NumberStyles.None, CultureInfo.InvariantCulture, out var expiresAt))
        {
            return ApiResponseFactory.Create<object?>(StatusCodes.Status401Unauthorized, ApiResponseMessages.ForStatusCode(StatusCodes.Status401Unauthorized), null);
        }

        var result = await sender.Send(
            new LogoutCommand(userId, jti, DateTimeOffset.FromUnixTimeSeconds(expiresAt).UtcDateTime, request.RefreshToken ?? string.Empty),
            cancellationToken);
        return result.IsSuccess
            ? ApiResponseFactory.Ok<object?>("Sesión cerrada correctamente.", null)
            : Failure(result.Error);
    }

    [LoggerMessage(EventId = 2001, Level = LogLevel.Warning, Message = "Evento de seguridad: inicio de sesión fallido (cuenta {Account}).")]
    private static partial void LogLoginFailed(ILogger logger, string account);

    [LoggerMessage(EventId = 2002, Level = LogLevel.Warning, Message = "Evento de seguridad: {Operation} limitado por exceso de intentos (cuenta {Account}).")]
    private static partial void LogAuthThrottled(ILogger logger, string operation, string account);

    [LoggerMessage(EventId = 2003, Level = LogLevel.Warning, Message = "Evento de seguridad: refresh rechazado (token inválido, vencido o reutilizado).")]
    private static partial void LogRefreshRejected(ILogger logger);

    private static AuthResponse ToResponse(AuthTokensResult tokens) => new(
        tokens.AccessToken.Value,
        "Bearer",
        tokens.AccessToken.ExpiresAtUtc,
        tokens.RefreshToken,
        tokens.RefreshTokenExpiresAtUtc,
        new AuthUserResponse(tokens.UserId, tokens.Email, tokens.Role.ToString()));

    private ObjectResult Failure(AuthError error, TimeSpan? retryAfter = null)
    {
        if (error == AuthError.TooManyRequests)
        {
            // Retry-After en segundos enteros (mínimo 1). El mensaje es el mismo para cuota por correo, bloqueo y hashing saturado.
            var seconds = (int)Math.Ceiling(Math.Max(1, (retryAfter ?? TimeSpan.FromSeconds(1)).TotalSeconds));
            Response.Headers.RetryAfter = seconds.ToString(CultureInfo.InvariantCulture);
            return ApiResponseFactory.Create<object?>(StatusCodes.Status429TooManyRequests, ApiResponseMessages.ForStatusCode(StatusCodes.Status429TooManyRequests), null);
        }

        return Map(error);
    }

    private static ObjectResult Map(AuthError error) => error switch
    {
        AuthError.EmailAlreadyRegistered => ApiResponseFactory.Create<object?>(
            StatusCodes.Status409Conflict, "No se pudo completar el registro con los datos proporcionados.", null),
        AuthError.InvalidCredentials => ApiResponseFactory.Create<object?>(
            StatusCodes.Status401Unauthorized, "Credenciales inválidas.", null),
        AuthError.InvalidRefreshToken => ApiResponseFactory.Create<object?>(
            StatusCodes.Status401Unauthorized, "La sesión no es válida o ha expirado.", null),
        AuthError.Forbidden => ApiResponseFactory.Create<object?>(
            StatusCodes.Status403Forbidden, ApiResponseMessages.ForStatusCode(StatusCodes.Status403Forbidden), null),
        _ => ApiResponseFactory.InternalServerError(),
    };
}
