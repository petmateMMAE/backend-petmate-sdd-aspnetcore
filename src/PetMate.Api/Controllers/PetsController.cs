using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using PetMate.Api.Authorization;
using PetMate.Api.Contracts.Images;
using PetMate.Api.Contracts.Pets;
using PetMate.Api.Contracts.Responses;
using PetMate.Application.Images;
using PetMate.Application.Mediator;
using PetMate.Application.Pets;
using PetMate.Domain.Pets;

namespace PetMate.Api.Controllers;

/// <summary>
/// Mascotas del Owner autenticado (pm-040, pm-041, pm-042, pm-043, pm-044). Cualquier User autenticado (política RequireUser). El propietario
/// sale únicamente del claim <c>sub</c> del Access Token validado; una mascota ajena o inexistente responde 404 por igual.
/// </summary>
[ApiController]
[Authorize(Policy = AuthorizationPolicies.RequireUser)]
[Route("api/v1/pets")]
[EnableRateLimiting(RateLimitPolicy)]
public sealed class PetsController(ISender sender, IImageStorage storage) : ControllerBase
{
    /// <summary>Política de límite por usuario (<c>PETS_RATE_LIMIT_PER_MINUTE</c>, por defecto 60 por minuto).</summary>
    public const string RateLimitPolicy = "pets";

    [HttpPost("create")]
    [ProducesResponseType(typeof(ApiResponse<PetResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<ValidationErrorsData>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status429TooManyRequests)]
    public async Task<ObjectResult> Create([FromBody] PetRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetOwnerId(out var ownerId))
        {
            return Status(StatusCodes.Status401Unauthorized);
        }

        var outcome = await sender.Send(new CreatePetCommand(ownerId, ToDetails(request)), cancellationToken);
        return outcome.Pet is null
            ? ApiResponseFactory.Create<object?>(StatusCodes.Status409Conflict, $"Se alcanzó el máximo de {PetLimits.MaxPetsPerOwner} mascotas por propietario.", null)
            : ApiResponseFactory.Create(StatusCodes.Status201Created, "Mascota creada correctamente.", ToResponse(outcome.Pet));
    }

    /// <summary>Mascotas del Owner autenticado, paginadas (pm-041, convención de pm-047): <c>page</c> desde 1, <c>pageSize</c> 20 por defecto y 50 como máximo.</summary>
    [HttpGet("get-all")]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<PetResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ValidationErrorsData>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<ObjectResult> GetAll([FromQuery] int? page, [FromQuery] int? pageSize, CancellationToken cancellationToken)
    {
        if (!TryGetOwnerId(out var ownerId))
        {
            return Status(StatusCodes.Status401Unauthorized);
        }

        var result = await sender.Send(new ListMyPetsQuery(ownerId, page, pageSize), cancellationToken);
        var data = new PagedResponse<PetResponse>(result.Items.Select(ToResponse).ToArray(), result.Page, result.PageSize, result.TotalCount, result.TotalPages);
        return ApiResponseFactory.Ok("Mascotas obtenidas correctamente.", data);
    }

    [HttpGet("get-by-id/{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<PetResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ObjectResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        if (!TryGetOwnerId(out var ownerId))
        {
            return Status(StatusCodes.Status401Unauthorized);
        }

        var pet = await sender.Send(new GetMyPetQuery(ownerId, id), cancellationToken);
        return pet is null ? Status(StatusCodes.Status404NotFound) : ApiResponseFactory.Ok("Mascota obtenida correctamente.", ToResponse(pet));
    }

    [HttpPut("update/{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<PetResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ValidationErrorsData>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ObjectResult> Update(Guid id, [FromBody] PetRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetOwnerId(out var ownerId))
        {
            return Status(StatusCodes.Status401Unauthorized);
        }

        var pet = await sender.Send(new UpdateMyPetCommand(ownerId, id, ToDetails(request)), cancellationToken);
        return pet is null ? Status(StatusCodes.Status404NotFound) : ApiResponseFactory.Ok("Mascota actualizada correctamente.", ToResponse(pet));
    }

    [HttpGet("get-qr/{id:guid}")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    [ProducesResponseType(typeof(ApiResponse<PetQrResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ObjectResult> GetQr(Guid id, CancellationToken cancellationToken)
    {
        if (!TryGetOwnerId(out var ownerId))
        {
            return Status(StatusCodes.Status401Unauthorized);
        }

        var qr = await sender.Send(new GetPetQrQuery(ownerId, id), cancellationToken);
        return qr is null ? Status(StatusCodes.Status404NotFound) : ApiResponseFactory.Ok("QR obtenido correctamente.", ToQrResponse(qr));
    }

    [HttpPost("regenerate-qr/{id:guid}")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    [ProducesResponseType(typeof(ApiResponse<PetQrResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ObjectResult> RegenerateQr(Guid id, CancellationToken cancellationToken)
    {
        if (!TryGetOwnerId(out var ownerId))
        {
            return Status(StatusCodes.Status401Unauthorized);
        }

        var qr = await sender.Send(new RegeneratePetQrCommand(ownerId, id), cancellationToken);
        return qr is null ? Status(StatusCodes.Status404NotFound) : ApiResponseFactory.Ok("QR regenerado correctamente.", ToQrResponse(qr));
    }

    [HttpDelete("delete/{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ObjectResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        if (!TryGetOwnerId(out var ownerId))
        {
            return Status(StatusCodes.Status401Unauthorized);
        }

        var deleted = await sender.Send(new DeleteMyPetCommand(ownerId, id), cancellationToken);
        return deleted
            ? ApiResponseFactory.Create<object?>(StatusCodes.Status200OK, "Mascota eliminada correctamente.", null)
            : Status(StatusCodes.Status404NotFound);
    }

    /// <summary>
    /// Sube o reemplaza la foto de una mascota propia (pm-080). Cuerpo <c>multipart/form-data</c> con el campo
    /// <c>file</c>: JPEG, PNG o WebP de hasta 2 MB. El tipo se deduce de los bytes del archivo, no de su nombre.
    /// La foto es privada: la respuesta trae una URL temporal (pm-081).
    /// </summary>
    [HttpPost("upload-photo/{id:guid}")]
    [DisableFormValueModelBinding]
    [ProducesResponseType(typeof(ApiResponse<PetPhotoResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status413PayloadTooLarge)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status503ServiceUnavailable)]
    public async Task<ObjectResult> UploadPhoto(Guid id, CancellationToken cancellationToken)
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

        var (rejection, photo) = await PhotoUploadReader.Read(Request, ImageContainer.PetPhotos, cancellationToken);
        if (PhotoResponses.Rejected(rejection, ImageContainer.PetPhotos) is { } error)
        {
            return error;
        }

        using var accepted = photo!;
        var result = await sender.Send(new UploadPetPhotoCommand(ownerId, id, accepted.ContentType, accepted.Content), cancellationToken);
        return result is null
            ? Status(StatusCodes.Status404NotFound)
            : ApiResponseFactory.Create(StatusCodes.Status200OK, "Fotografía guardada correctamente.", ToPhotoResponse(result));
    }

    /// <summary>URL temporal de la foto de una mascota propia (pm-081); dura 15 minutos.</summary>
    [HttpGet("get-photo-url/{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<PetPhotoResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status429TooManyRequests)]
    public async Task<ObjectResult> GetPhotoUrl(Guid id, CancellationToken cancellationToken)
    {
        if (!TryGetOwnerId(out var ownerId))
        {
            return Status(StatusCodes.Status401Unauthorized);
        }

        var result = await sender.Send(new GetPetPhotoUrlQuery(ownerId, id), cancellationToken);
        return result is null
            ? Status(StatusCodes.Status404NotFound)
            : ApiResponseFactory.Create(StatusCodes.Status200OK, "Fotografía obtenida correctamente.", ToPhotoResponse(result));
    }

    /// <summary>Quita la foto de una mascota propia (pm-080).</summary>
    [HttpDelete("delete-photo/{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status429TooManyRequests)]
    public async Task<ObjectResult> DeletePhoto(Guid id, CancellationToken cancellationToken)
    {
        if (!TryGetOwnerId(out var ownerId))
        {
            return Status(StatusCodes.Status401Unauthorized);
        }

        return await sender.Send(new DeletePetPhotoCommand(ownerId, id), cancellationToken)
            ? ApiResponseFactory.Create<object?>(StatusCodes.Status200OK, "Fotografía eliminada correctamente.", null)
            : Status(StatusCodes.Status404NotFound);
    }

    private static PetPhotoResponse ToPhotoResponse(PetPhotoResult result)
        => new(result.PetId, result.Url.AbsoluteUri, result.ExpiresAtUtc);

    private bool TryGetOwnerId(out Guid ownerId) => Guid.TryParse(User.FindFirst("sub")?.Value, out ownerId);

    private static ObjectResult Status(int statusCode)
        => ApiResponseFactory.Create<object?>(statusCode, ApiResponseMessages.ForStatusCode(statusCode), null);

    // Un sexo ausente o desconocido se convierte en un valor no definido para que el validador responda 400 con el campo "sex".
    private static PetDetails ToDetails(PetRequest request)
        => new(
            request.Name ?? string.Empty,
            request.Species ?? string.Empty,
            request.Breed,
            request.BirthDate,
            request.Sex is null ? PetSex.Unknown : Enum.TryParse<PetSex>(request.Sex, ignoreCase: false, out var sex) && Enum.IsDefined(sex) && Enum.GetName(sex) == request.Sex ? sex : 0,
            request.PhotoUrl);

    private static PetQrResponse ToQrResponse(PetQrResult qr) => new(qr.PetId, qr.Token, qr.Code);

    private static PetResponse ToResponse(PetResult pet)
        => new(pet.Id, pet.Name, pet.Species, pet.Breed, pet.BirthDate, pet.Sex.ToString(), pet.PhotoUrl, pet.CreatedAtUtc, pet.UpdatedAtUtc);
}
