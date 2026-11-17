using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using PetMate.Api.Contracts.Auth;
using PetMate.Api.Contracts.Responses;
using PetMate.Application.Accounts;
using PetMate.Application.Authentication;
using PetMate.Application.Mediator;

namespace PetMate.Api.Controllers;

/// <summary>
/// Eliminación de cuenta sin entrar a la app (pm-164, pm-165): el usuario pide el enlace con su correo y lo confirma desde el
/// correo. Públicas, con el límite por IP de las rutas de autenticación y la cuota por correo del caso de uso.
/// </summary>
[ApiController]
[AllowAnonymous]
[Route("api/v1/account-deletion")]
[EnableRateLimiting(AuthController.RateLimitPolicy)]
[ServiceFilter(typeof(PetMate.Api.Authentication.AuthIpThrottleFilter))]
public sealed class AccountDeletionController(ISender sender) : ControllerBase
{
    [HttpPost("request")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ValidationErrorsData>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status429TooManyRequests)]
    public async Task<ObjectResult> RequestDeletion([FromBody] RequestAccountDeletionRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new RequestAccountDeletionCommand(request.Email ?? string.Empty), cancellationToken);
        return result.IsSuccess
            ? ApiResponseFactory.Ok<object?>("Si el correo está registrado, recibirás un enlace para confirmar la eliminación de tu cuenta.", null)
            : TooManyRequests(result.RetryAfter);
    }

    [HttpPost("confirm")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ValidationErrorsData>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status429TooManyRequests)]
    public async Task<ObjectResult> Confirm([FromBody] ConfirmAccountDeletionRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ConfirmAccountDeletionCommand(request.Token ?? string.Empty), cancellationToken);
        if (result.IsSuccess)
        {
            return ApiResponseFactory.Ok<object?>("Cuenta eliminada correctamente.", null);
        }

        return result.Error == AuthError.TooManyRequests
            ? TooManyRequests(result.RetryAfter)
            : ApiResponseFactory.Create<object?>(StatusCodes.Status400BadRequest, "El enlace no es válido o ya venció. Solicita uno nuevo.", null);
    }

    private ObjectResult TooManyRequests(TimeSpan? retryAfter)
    {
        var seconds = (int)Math.Ceiling(Math.Max(1, (retryAfter ?? TimeSpan.FromSeconds(1)).TotalSeconds));
        Response.Headers.RetryAfter = seconds.ToString(CultureInfo.InvariantCulture);
        return ApiResponseFactory.Create<object?>(StatusCodes.Status429TooManyRequests, ApiResponseMessages.ForStatusCode(StatusCodes.Status429TooManyRequests), null);
    }
}
