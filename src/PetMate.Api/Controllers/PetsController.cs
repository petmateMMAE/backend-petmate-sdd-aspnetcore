using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PetMate.Api.Authorization;
using PetMate.Api.Contracts.Pets;
using PetMate.Api.Contracts.Responses;
using PetMate.Application.Mediator;
using PetMate.Application.Pets;
using PetMate.Domain.Pets;

namespace PetMate.Api.Controllers;

/// <summary>
/// Mascotas del Owner autenticado (pm-040, pm-042, pm-043, pm-044). Solo el rol Owner (política RequireOwner). El propietario
/// sale únicamente del claim <c>sub</c> del Access Token validado; una mascota ajena o inexistente responde 404 por igual.
/// </summary>
[ApiController]
[Authorize(Policy = AuthorizationPolicies.RequireOwner)]
[Route("api/v1/pets")]
public sealed class PetsController(ISender sender) : ControllerBase
{
    [HttpPost("create")]
    [ProducesResponseType(typeof(ApiResponse<PetResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<ValidationErrorsData>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<ObjectResult> Create([FromBody] PetRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetOwnerId(out var ownerId))
        {
            return Status(StatusCodes.Status401Unauthorized);
        }

        var pet = await sender.Send(new CreatePetCommand(ownerId, ToDetails(request)), cancellationToken);
        return ApiResponseFactory.Create(StatusCodes.Status201Created, "Mascota creada correctamente.", ToResponse(pet));
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
            Enum.TryParse<PetSex>(request.Sex, ignoreCase: false, out var sex) && Enum.IsDefined(sex) && Enum.GetName(sex) == request.Sex ? sex : 0,
            request.PhotoUrl);

    private static PetResponse ToResponse(PetResult pet)
        => new(pet.Id, pet.Name, pet.Species, pet.Breed, pet.BirthDate, pet.Sex.ToString(), pet.PhotoUrl, pet.CreatedAtUtc, pet.UpdatedAtUtc);
}
