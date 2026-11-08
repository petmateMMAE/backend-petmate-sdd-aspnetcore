using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using PetMate.Api.Contracts.Responses;
using PetMate.Api.Contracts.Users;
using PetMate.Application.Mediator;
using PetMate.Application.Users;

namespace PetMate.Api.Controllers;

/// <summary>
/// Perfil propio (pm-037, pm-038). La identidad sale solo del claim <c>sub</c> del Access Token ya validado:
/// nunca de la ruta, el cuerpo ni la query, así que un usuario no puede leer ni cambiar el perfil de otro.
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1/users")]
[EnableRateLimiting(RateLimitPolicy)]
public sealed class UsersController(ISender sender) : ControllerBase
{
    /// <summary>Política de límite por usuario (<c>USERS_RATE_LIMIT_PER_MINUTE</c>, por defecto 60 por minuto).</summary>
    public const string RateLimitPolicy = "users";

    [HttpGet("me/get")]
    [ProducesResponseType(typeof(ApiResponse<UserProfileResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ObjectResult> GetMine(CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthenticated();
        }

        var profile = await sender.Send(new GetMyProfileQuery(userId), cancellationToken);
        return profile is null ? NotFoundAccount() : ApiResponseFactory.Ok("Perfil obtenido correctamente.", ToResponse(profile));
    }

    [HttpPut("me/update")]
    [ProducesResponseType(typeof(ApiResponse<UserProfileResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ValidationErrorsData>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ObjectResult> UpdateMine([FromBody] UpdateProfileRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthenticated();
        }

        var profile = await sender.Send(new UpdateMyProfileCommand(userId, request.DisplayName ?? string.Empty), cancellationToken);
        return profile is null ? NotFoundAccount() : ApiResponseFactory.Ok("Perfil actualizado correctamente.", ToResponse(profile));
    }

    private bool TryGetUserId(out Guid userId) => Guid.TryParse(User.FindFirst("sub")?.Value, out userId);

    private static ObjectResult Unauthenticated() => ApiResponseFactory.Create<object?>(
        StatusCodes.Status401Unauthorized, ApiResponseMessages.ForStatusCode(StatusCodes.Status401Unauthorized), null);

    private static ObjectResult NotFoundAccount() => ApiResponseFactory.Create<object?>(
        StatusCodes.Status404NotFound, ApiResponseMessages.ForStatusCode(StatusCodes.Status404NotFound), null);

    private static UserProfileResponse ToResponse(UserProfileResult profile)
        => new(profile.Id, profile.Email, profile.Role.ToString(), profile.DisplayName, profile.CreatedAtUtc);
}
