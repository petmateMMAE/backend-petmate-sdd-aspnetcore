using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using PetMate.Api.Authorization;
using PetMate.Api.Contracts.Devices;
using PetMate.Api.Contracts.Responses;
using PetMate.Application.Mediator;
using PetMate.Application.Notifications;

namespace PetMate.Api.Controllers;

/// <summary>Preferencias de aviso propias (pm-157). La cuenta sale solo del token de acceso.</summary>
[ApiController]
[Authorize(Policy = AuthorizationPolicies.RequireUser)]
[Route("api/v1/notification-settings")]
[EnableRateLimiting(UsersController.RateLimitPolicy)]
public sealed class NotificationSettingsController(ISender sender) : ControllerBase
{
    [HttpGet("get")]
    [ProducesResponseType(typeof(ApiResponse<NotificationSettingsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<ObjectResult> Get(CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(User.FindFirst("sub")?.Value, out var userId))
        {
            return Unauthenticated();
        }

        var settings = await sender.Send(new GetNotificationSettingsQuery(userId), cancellationToken);
        return ApiResponseFactory.Ok("Preferencias obtenidas correctamente.", ToResponse(settings));
    }

    [HttpPut("update")]
    [ProducesResponseType(typeof(ApiResponse<NotificationSettingsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ValidationErrorsData>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<ObjectResult> Update([FromBody] UpdateNotificationSettingsRequest request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(User.FindFirst("sub")?.Value, out var userId))
        {
            return Unauthenticated();
        }

        var settings = await sender.Send(
            new UpdateNotificationSettingsCommand(userId, request.Vaccines, request.Reviews, request.Chat, request.Followups, request.Sound, request.ReminderDaysBefore),
            cancellationToken);
        return ApiResponseFactory.Ok("Preferencias actualizadas correctamente.", ToResponse(settings));
    }

    private static ObjectResult Unauthenticated() => ApiResponseFactory.Create<object?>(
        StatusCodes.Status401Unauthorized, ApiResponseMessages.ForStatusCode(StatusCodes.Status401Unauthorized), null);

    private static NotificationSettingsResponse ToResponse(NotificationSettings s)
        => new(s.Vaccines, s.Reviews, s.Chat, s.Followups, s.Sound, s.ReminderDaysBefore);
}
