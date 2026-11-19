using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using PetMate.Api.Authorization;
using PetMate.Api.Contracts.Devices;
using PetMate.Api.Contracts.Responses;
using PetMate.Application.Devices;
using PetMate.Application.Mediator;

namespace PetMate.Api.Controllers;

/// <summary>Dispositivos para avisos push (pm-151). El usuario es siempre el del token de acceso.</summary>
[ApiController]
[Authorize(Policy = AuthorizationPolicies.RequireUser)]
[Route("api/v1/devices")]
[EnableRateLimiting(UsersController.RateLimitPolicy)]
public sealed class DevicesController(ISender sender) : ControllerBase
{
    [HttpPost("register")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ValidationErrorsData>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<ObjectResult> Register([FromBody] RegisterDeviceRequest request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(User.FindFirst("sub")?.Value, out var userId))
        {
            return ApiResponseFactory.Create<object?>(StatusCodes.Status401Unauthorized, ApiResponseMessages.ForStatusCode(StatusCodes.Status401Unauthorized), null);
        }

        var status = await sender.Send(new RegisterDeviceCommand(userId, request.Token ?? string.Empty), cancellationToken);
        return status == RegisterDeviceStatus.Registered
            ? ApiResponseFactory.Ok<object?>("Dispositivo registrado correctamente.", null)
            : ApiResponseFactory.Create<object?>(StatusCodes.Status409Conflict, "Se alcanzó el máximo de dispositivos registrados.", null);
    }

    /// <summary>pm-152: quita el token del dispositivo del usuario. Idempotente: un token desconocido o ajeno también responde 200.</summary>
    [HttpPost("unregister")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ValidationErrorsData>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<ObjectResult> Unregister([FromBody] UnregisterDeviceRequest request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(User.FindFirst("sub")?.Value, out var userId))
        {
            return ApiResponseFactory.Create<object?>(StatusCodes.Status401Unauthorized, ApiResponseMessages.ForStatusCode(StatusCodes.Status401Unauthorized), null);
        }

        await sender.Send(new UnregisterDeviceCommand(userId, request.Token ?? string.Empty), cancellationToken);
        return ApiResponseFactory.Ok<object?>("Dispositivo eliminado correctamente.", null);
    }
}
