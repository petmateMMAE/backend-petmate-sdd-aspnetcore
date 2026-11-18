using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using PetMate.Api.Authorization;
using PetMate.Api.Contracts.Devices;
using PetMate.Api.Contracts.Responses;
using PetMate.Application.Mediator;
using PetMate.Application.Reminders;

namespace PetMate.Api.Controllers;

/// <summary>Recordatorios del dueño (pm-156). La cuenta sale solo del token de acceso.</summary>
[ApiController]
[Authorize(Policy = AuthorizationPolicies.RequireUser)]
[Route("api/v1/reminders")]
[EnableRateLimiting(UsersController.RateLimitPolicy)]
public sealed class RemindersController(ISender sender) : ControllerBase
{
    [HttpGet("get-all")]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<ReminderResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ValidationErrorsData>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<ObjectResult> GetAll([FromQuery] int? page, [FromQuery] int? pageSize, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(User.FindFirst("sub")?.Value, out var userId))
        {
            return ApiResponseFactory.Create<object?>(StatusCodes.Status401Unauthorized, ApiResponseMessages.ForStatusCode(StatusCodes.Status401Unauthorized), null);
        }

        var data = await sender.Send(new ListRemindersQuery(userId, page, pageSize), cancellationToken);
        return ApiResponseFactory.Ok(
            "Recordatorios obtenidos correctamente.",
            new PagedResponse<ReminderResponse>(
                data.Items.Select(r => new ReminderResponse(r.Id, r.Kind, r.PetId, r.PetName, r.Title, r.DueOn, r.RemindOn, r.IsRead)).ToArray(),
                data.Page, data.PageSize, data.TotalCount, data.TotalPages));
    }

    /// <summary>pm-156#CA-02: marca como leído un recordatorio propio. Idempotente; uno ajeno o inexistente responde 404.</summary>
    [HttpPost("mark-read")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ValidationErrorsData>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ObjectResult> MarkRead([FromBody] MarkReminderReadRequest request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(User.FindFirst("sub")?.Value, out var userId))
        {
            return ApiResponseFactory.Create<object?>(StatusCodes.Status401Unauthorized, ApiResponseMessages.ForStatusCode(StatusCodes.Status401Unauthorized), null);
        }

        return await sender.Send(new MarkReminderReadCommand(userId, request.Id ?? Guid.Empty), cancellationToken)
            ? ApiResponseFactory.Ok<object?>("Recordatorio marcado como leído.", null)
            : ApiResponseFactory.Create<object?>(StatusCodes.Status404NotFound, ApiResponseMessages.ForStatusCode(StatusCodes.Status404NotFound), null);
    }
}
