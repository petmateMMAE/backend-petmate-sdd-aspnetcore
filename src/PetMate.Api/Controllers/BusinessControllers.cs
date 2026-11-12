using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using PetMate.Api.Authorization;
using PetMate.Api.Contracts.Businesses;
using PetMate.Api.Contracts.Images;
using PetMate.Api.Contracts.Responses;
using PetMate.Application.Businesses;
using PetMate.Application.Images;
using PetMate.Application.Mediator;
using PetMate.Domain.Businesses;

namespace PetMate.Api.Controllers;

/// <summary>
/// Base de los controladores del negocio afiliado (pm-107 a pm-115). Cualquier User autenticado; el negocio es siempre el de la
/// cuenta del token (nunca un identificador del cliente), así que solo el dueño puede ver o cambiar su negocio y su catálogo.
/// </summary>
[ApiController]
[Authorize(Policy = AuthorizationPolicies.RequireUser)]
[EnableRateLimiting(RateLimitPolicy)]
public abstract class BusinessControllerBase(ISender sender) : ControllerBase
{
    /// <summary>Política de límite por usuario (<c>BUSINESS_RATE_LIMIT_PER_MINUTE</c>, por defecto 60 por minuto).</summary>
    public const string RateLimitPolicy = "business";

    protected ISender Sender => sender;

    protected bool TryGetOwnerId(out Guid ownerId) => Guid.TryParse(User.FindFirst("sub")?.Value, out ownerId);

    protected static ObjectResult Status(int statusCode)
        => ApiResponseFactory.Create<object?>(statusCode, ApiResponseMessages.ForStatusCode(statusCode), null);

    protected static ObjectResult Fail(BusinessStatus status) => status switch
    {
        BusinessStatus.NoBusiness => ApiResponseFactory.Create<object?>(StatusCodes.Status403Forbidden, "La cuenta no tiene un negocio asociado.", null),
        BusinessStatus.AlreadyHasBusiness => ApiResponseFactory.Create<object?>(StatusCodes.Status409Conflict, "La cuenta ya tiene un negocio asociado.", null),
        BusinessStatus.NotFound => Status(StatusCodes.Status404NotFound),
        _ => Status(StatusCodes.Status500InternalServerError),
    };

    /// <summary>Un texto que no es un valor del enum se convierte en un valor no definido para que el validador responda 400 con el campo.</summary>
    protected static T? ParseEnum<T>(string? value)
        where T : struct, Enum
        => value is null
            ? null
            : Enum.TryParse<T>(value, ignoreCase: false, out var parsed) && Enum.IsDefined(parsed) && Enum.GetName(parsed) == value ? parsed : default(T);

    protected static ScheduleInput? ToSchedule(Dictionary<string, List<TimeRangeContract>>? schedule)
        => schedule is null
            ? null
            : new ScheduleInput(schedule.ToDictionary(
                d => d.Key,
                d => (IReadOnlyList<(string Open, string Close)>)(d.Value ?? []).Select(r => (r.Open ?? string.Empty, r.Close ?? string.Empty)).ToArray()));

    protected static BusinessResponse ToResponse(BusinessResult b) => new(
        b.Id,
        b.Type.ToString(),
        b.Name,
        b.Phone,
        b.WhatsApp,
        b.Address,
        b.Description,
        b.Latitude,
        b.Longitude,
        b.LicenseNumber,
        b.Schedule.ToDictionary(d => d.Key, d => d.Value.Select(r => new TimeRangeContract(r.Open, r.Close)).ToList()),
        b.CreatedAtUtc,
        b.UpdatedAtUtc);
}

/// <summary>Negocio afiliado de la cuenta (pm-107 a pm-110).</summary>
[Route("api/v1/businesses")]
public sealed class BusinessesController(ISender sender, IImageStorage storage) : BusinessControllerBase(sender)
{
    [HttpPost("create")]
    [ProducesResponseType(typeof(ApiResponse<BusinessResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<ValidationErrorsData>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<ObjectResult> Create([FromBody] CreateBusinessRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetOwnerId(out var ownerId))
        {
            return Status(StatusCodes.Status401Unauthorized);
        }

        var details = new BusinessDetails(
            ParseEnum<BusinessType>(request.Type),
            request.Name,
            request.Phone,
            request.WhatsApp,
            request.Address,
            request.Description,
            request.Latitude,
            request.Longitude,
            request.LicenseNumber,
            ToSchedule(request.Schedule));
        var result = await Sender.Send(new CreateBusinessCommand(ownerId, details), cancellationToken);
        return result.Status == BusinessStatus.Ok
            ? ApiResponseFactory.Create(StatusCodes.Status201Created, "Negocio registrado correctamente.", ToResponse(result.Value!))
            : Fail(result.Status);
    }

    [HttpPut("update-location")]
    [ProducesResponseType(typeof(ApiResponse<BusinessResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ValidationErrorsData>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<ObjectResult> UpdateLocation([FromBody] UpdateBusinessLocationRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetOwnerId(out var ownerId))
        {
            return Status(StatusCodes.Status401Unauthorized);
        }

        var result = await Sender.Send(new UpdateBusinessLocationCommand(ownerId, request.Latitude, request.Longitude), cancellationToken);
        return result.Status == BusinessStatus.Ok ? ApiResponseFactory.Ok("Ubicación actualizada correctamente.", ToResponse(result.Value!)) : Fail(result.Status);
    }

    [HttpPut("update")]
    [ProducesResponseType(typeof(ApiResponse<BusinessResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ValidationErrorsData>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<ObjectResult> Update([FromBody] UpdateBusinessRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetOwnerId(out var ownerId))
        {
            return Status(StatusCodes.Status401Unauthorized);
        }

        var profile = new BusinessProfile(request.Name, request.Description, request.Phone, request.WhatsApp, ToSchedule(request.Schedule));
        var result = await Sender.Send(new UpdateBusinessCommand(ownerId, profile), cancellationToken);
        return result.Status == BusinessStatus.Ok ? ApiResponseFactory.Ok("Negocio actualizado correctamente.", ToResponse(result.Value!)) : Fail(result.Status);
    }

    [HttpDelete("delete")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<ObjectResult> Delete(CancellationToken cancellationToken)
    {
        if (!TryGetOwnerId(out var ownerId))
        {
            return Status(StatusCodes.Status401Unauthorized);
        }

        var status = await Sender.Send(new DeleteBusinessCommand(ownerId), cancellationToken);
        return status == BusinessStatus.Ok ? ApiResponseFactory.Create<object?>(StatusCodes.Status200OK, "Negocio dado de baja correctamente.", null) : Fail(status);
    }

    /// <summary>
    /// Sube o reemplaza la foto del local (pm-111). Cuerpo <c>multipart/form-data</c> con el campo <c>file</c>:
    /// JPEG, PNG o WebP de hasta 1 MB. La foto es pública: sale en el perfil del negocio y en el mapa.
    /// </summary>
    [HttpPost("upload-photo")]
    [DisableFormValueModelBinding]
    [ProducesResponseType(typeof(ApiResponse<BusinessPhotoResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status413PayloadTooLarge)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status503ServiceUnavailable)]
    public async Task<ObjectResult> UploadPhoto(CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(storage);
        if (!TryGetOwnerId(out var ownerId))
        {
            return Status(StatusCodes.Status401Unauthorized);
        }

        if (!storage.IsEnabled)
        {
            return PhotoResponses.Unavailable();
        }

        var (rejection, photo) = await PhotoUploadReader.Read(Request, ImageContainer.BusinessPhotos, cancellationToken);
        if (PhotoResponses.Rejected(rejection, ImageContainer.BusinessPhotos) is { } error)
        {
            return error;
        }

        using var accepted = photo!;
        var outcome = await Sender.Send(new UploadBusinessPhotoCommand(ownerId, accepted.ContentType, accepted.Content), cancellationToken);
        return outcome.Status == BusinessStatus.Ok && outcome.Value is { } url
            ? ApiResponseFactory.Create(StatusCodes.Status200OK, "Fotografía guardada correctamente.", new BusinessPhotoResponse(url.AbsoluteUri))
            : Fail(outcome.Status);
    }

    /// <summary>Quita la foto del local (pm-111).</summary>
    [HttpDelete("delete-photo")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<ObjectResult> DeletePhoto(CancellationToken cancellationToken)
    {
        if (!TryGetOwnerId(out var ownerId))
        {
            return Status(StatusCodes.Status401Unauthorized);
        }

        var status = await Sender.Send(new DeleteBusinessPhotoCommand(ownerId), cancellationToken);
        return status == BusinessStatus.Ok
            ? ApiResponseFactory.Create<object?>(StatusCodes.Status200OK, "Fotografía eliminada correctamente.", null)
            : Fail(status);
    }
}

/// <summary>Catálogo de servicios del negocio de la cuenta (pm-112 a pm-115).</summary>
[Route("api/v1/services")]
public sealed class ServicesController(ISender sender) : BusinessControllerBase(sender)
{
    [HttpPost("create")]
    [ProducesResponseType(typeof(ApiResponse<ServiceResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<ValidationErrorsData>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<ObjectResult> Create([FromBody] ServiceRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetOwnerId(out var ownerId))
        {
            return Status(StatusCodes.Status401Unauthorized);
        }

        var result = await Sender.Send(new CreateServiceCommand(ownerId, new ServiceDetails(request.Name, request.Description, request.Price)), cancellationToken);
        return result.Status == BusinessStatus.Ok
            ? ApiResponseFactory.Create(StatusCodes.Status201Created, "Servicio creado correctamente.", ToResponse(result.Value!))
            : Fail(result.Status);
    }

    [HttpGet("get-all")]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<ServiceResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ValidationErrorsData>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<ObjectResult> GetAll([FromQuery] int? page, [FromQuery] int? pageSize, CancellationToken cancellationToken)
    {
        if (!TryGetOwnerId(out var ownerId))
        {
            return Status(StatusCodes.Status401Unauthorized);
        }

        var result = await Sender.Send(new ListServicesQuery(ownerId, page, pageSize), cancellationToken);
        if (result.Status != BusinessStatus.Ok)
        {
            return Fail(result.Status);
        }

        var data = result.Value!;
        return ApiResponseFactory.Ok(
            "Servicios obtenidos correctamente.",
            new PagedResponse<ServiceResponse>(data.Items.Select(ToResponse).ToArray(), data.Page, data.PageSize, data.TotalCount, data.TotalPages));
    }

    [HttpPut("update/{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ServiceResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ValidationErrorsData>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ObjectResult> Update(Guid id, [FromBody] ServiceRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetOwnerId(out var ownerId))
        {
            return Status(StatusCodes.Status401Unauthorized);
        }

        var result = await Sender.Send(new UpdateServiceCommand(ownerId, id, new ServiceDetails(request.Name, request.Description, request.Price)), cancellationToken);
        return result.Status == BusinessStatus.Ok ? ApiResponseFactory.Ok("Servicio actualizado correctamente.", ToResponse(result.Value!)) : Fail(result.Status);
    }

    [HttpDelete("delete/{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ObjectResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        if (!TryGetOwnerId(out var ownerId))
        {
            return Status(StatusCodes.Status401Unauthorized);
        }

        var status = await Sender.Send(new DeleteServiceCommand(ownerId, id), cancellationToken);
        return status == BusinessStatus.Ok ? ApiResponseFactory.Create<object?>(StatusCodes.Status200OK, "Servicio eliminado correctamente.", null) : Fail(status);
    }

    private static ServiceResponse ToResponse(ServiceResult s) => new(s.Id, s.Name, s.Description, s.Price, s.CreatedAtUtc, s.UpdatedAtUtc);
}
