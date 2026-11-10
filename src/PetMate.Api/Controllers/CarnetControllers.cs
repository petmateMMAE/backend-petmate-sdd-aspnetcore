using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using PetMate.Api.Authorization;
using PetMate.Api.Contracts.Carnet;
using PetMate.Api.Contracts.Responses;
using PetMate.Application.Carnet;
using PetMate.Application.Mediator;
using PetMate.Application.Pagination;
using PetMate.Domain.Carnet;

namespace PetMate.Api.Controllers;

/// <summary>
/// Base de los controladores del carnet de salud (pm-096 a pm-103). Solo el rol Owner; el dueño sale únicamente del claim
/// <c>sub</c>. Una mascota o un registro ajenos o inexistentes responden 404 por igual.
/// </summary>
[ApiController]
[Authorize(Policy = AuthorizationPolicies.RequireUser)]
[EnableRateLimiting(RateLimitPolicy)]
public abstract class CarnetControllerBase(ISender sender) : ControllerBase
{
    /// <summary>Política de límite por usuario (<c>CARNET_RATE_LIMIT_PER_MINUTE</c>, por defecto 60 por minuto).</summary>
    public const string RateLimitPolicy = "carnet";

    protected ISender Sender => sender;

    protected bool TryGetOwnerId(out Guid ownerId) => Guid.TryParse(User.FindFirst("sub")?.Value, out ownerId);

    protected static ObjectResult Status(int statusCode)
        => ApiResponseFactory.Create<object?>(statusCode, ApiResponseMessages.ForStatusCode(statusCode), null);

    protected static ObjectResult Fail(CarnetStatus status) => status switch
    {
        CarnetStatus.NotFound => Status(StatusCodes.Status404NotFound),
        CarnetStatus.LimitReached => ApiResponseFactory.Create<object?>(
            StatusCodes.Status409Conflict, $"Se alcanzó el máximo de {CarnetLimits.MaxRecordsPerPetAndType} registros de este tipo para la mascota.", null),
        CarnetStatus.BusinessRecord => ApiResponseFactory.Create<object?>(StatusCodes.Status403Forbidden, CarnetLimits.BusinessRecordMessage, null),
        _ => Status(StatusCodes.Status500InternalServerError),
    };

    protected static RecordOriginResponse ToResponse(RecordOriginResult origin)
        => new(origin.Origin.ToString(), origin.BusinessId, origin.BusinessName);

    protected static PagedResponse<TOut> ToPage<TIn, TOut>(PagedResult<TIn> page, Func<TIn, TOut> map)
        => new(page.Items.Select(map).ToArray(), page.Page, page.PageSize, page.TotalCount, page.TotalPages);

    /// <summary>Un texto que no es un valor del enum se convierte en un valor no definido para que el validador responda 400 con el campo.</summary>
    protected static T? ParseEnum<T>(string? value)
        where T : struct, Enum
        => value is null
            ? null
            : Enum.TryParse<T>(value, ignoreCase: false, out var parsed) && Enum.IsDefined(parsed) && Enum.GetName(parsed) == value ? parsed : default(T);
}

/// <summary>Vacunas de las mascotas del dueño (pm-097, pm-098).</summary>
[Route("api/v1/vaccines")]
public sealed class VaccinesController(ISender sender) : CarnetControllerBase(sender)
{
    [HttpPost("create")]
    [ProducesResponseType(typeof(ApiResponse<VaccineResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<ValidationErrorsData>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<ObjectResult> Create([FromBody] VaccineRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetOwnerId(out var ownerId))
        {
            return Status(StatusCodes.Status401Unauthorized);
        }

        var details = new VaccineDetails(request.AppliedOn, request.Name, request.Batch, request.AppliedBy, request.NextDoseOn);
        var result = await Sender.Send(new CreateVaccineCommand(ownerId, request.PetId ?? Guid.Empty, details), cancellationToken);
        return result.Status == CarnetStatus.Ok
            ? ApiResponseFactory.Create(StatusCodes.Status201Created, "Vacuna registrada correctamente.", ToResponse(result.Value!))
            : Fail(result.Status);
    }

    [HttpGet("get-all")]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<VaccineResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ValidationErrorsData>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ObjectResult> GetAll([FromQuery] Guid? petId, [FromQuery] int? page, [FromQuery] int? pageSize, CancellationToken cancellationToken)
    {
        if (!TryGetOwnerId(out var ownerId))
        {
            return Status(StatusCodes.Status401Unauthorized);
        }

        var result = await Sender.Send(new ListVaccinesQuery(ownerId, petId ?? Guid.Empty, page, pageSize), cancellationToken);
        return result.Status == CarnetStatus.Ok
            ? ApiResponseFactory.Ok("Vacunas obtenidas correctamente.", ToPage(result.Value!, ToResponse))
            : Fail(result.Status);
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

        var status = await Sender.Send(new DeleteVaccineCommand(ownerId, id), cancellationToken);
        return status == CarnetStatus.Ok ? ApiResponseFactory.Create<object?>(StatusCodes.Status200OK, "Vacuna eliminada correctamente.", null) : Fail(status);
    }

    private static VaccineResponse ToResponse(VaccineResult v)
        => new(v.Id, v.PetId, v.AppliedOn, v.Name, v.Batch, v.AppliedBy, v.NextDoseOn, ToResponse(v.Origin), v.CreatedAtUtc);
}

/// <summary>Desparasitaciones de las mascotas del dueño (pm-099, pm-100).</summary>
[Route("api/v1/dewormings")]
public sealed class DewormingsController(ISender sender) : CarnetControllerBase(sender)
{
    [HttpPost("create")]
    [ProducesResponseType(typeof(ApiResponse<DewormingResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<ValidationErrorsData>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<ObjectResult> Create([FromBody] DewormingRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetOwnerId(out var ownerId))
        {
            return Status(StatusCodes.Status401Unauthorized);
        }

        var details = new DewormingDetails(request.AppliedOn, request.Product, ParseEnum<DewormingType>(request.Type), request.AppliedBy, request.NextOn);
        var result = await Sender.Send(new CreateDewormingCommand(ownerId, request.PetId ?? Guid.Empty, details), cancellationToken);
        return result.Status == CarnetStatus.Ok
            ? ApiResponseFactory.Create(StatusCodes.Status201Created, "Desparasitación registrada correctamente.", ToResponse(result.Value!))
            : Fail(result.Status);
    }

    [HttpGet("get-all")]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<DewormingResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ValidationErrorsData>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ObjectResult> GetAll([FromQuery] Guid? petId, [FromQuery] int? page, [FromQuery] int? pageSize, CancellationToken cancellationToken)
    {
        if (!TryGetOwnerId(out var ownerId))
        {
            return Status(StatusCodes.Status401Unauthorized);
        }

        var result = await Sender.Send(new ListDewormingsQuery(ownerId, petId ?? Guid.Empty, page, pageSize), cancellationToken);
        return result.Status == CarnetStatus.Ok
            ? ApiResponseFactory.Ok("Desparasitaciones obtenidas correctamente.", ToPage(result.Value!, ToResponse))
            : Fail(result.Status);
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

        var status = await Sender.Send(new DeleteDewormingCommand(ownerId, id), cancellationToken);
        return status == CarnetStatus.Ok ? ApiResponseFactory.Create<object?>(StatusCodes.Status200OK, "Desparasitación eliminada correctamente.", null) : Fail(status);
    }

    private static DewormingResponse ToResponse(DewormingResult d)
        => new(d.Id, d.PetId, d.AppliedOn, d.Product, d.Type?.ToString(), d.AppliedBy, d.NextOn, ToResponse(d.Origin), d.CreatedAtUtc);
}

/// <summary>Pesos de las mascotas del dueño (pm-101).</summary>
[Route("api/v1/weights")]
public sealed class WeightsController(ISender sender) : CarnetControllerBase(sender)
{
    [HttpPost("create")]
    [ProducesResponseType(typeof(ApiResponse<WeightResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<ValidationErrorsData>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<ObjectResult> Create([FromBody] WeightRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetOwnerId(out var ownerId))
        {
            return Status(StatusCodes.Status401Unauthorized);
        }

        var result = await Sender.Send(new CreateWeightCommand(ownerId, request.PetId ?? Guid.Empty, new WeightDetails(request.MeasuredOn, request.Kilograms)), cancellationToken);
        return result.Status == CarnetStatus.Ok
            ? ApiResponseFactory.Create(StatusCodes.Status201Created, "Peso registrado correctamente.", ToResponse(result.Value!))
            : Fail(result.Status);
    }

    [HttpGet("get-all")]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<WeightResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ValidationErrorsData>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ObjectResult> GetAll([FromQuery] Guid? petId, [FromQuery] int? page, [FromQuery] int? pageSize, CancellationToken cancellationToken)
    {
        if (!TryGetOwnerId(out var ownerId))
        {
            return Status(StatusCodes.Status401Unauthorized);
        }

        var result = await Sender.Send(new ListWeightsQuery(ownerId, petId ?? Guid.Empty, page, pageSize), cancellationToken);
        return result.Status == CarnetStatus.Ok
            ? ApiResponseFactory.Ok("Pesos obtenidos correctamente.", ToPage(result.Value!, ToResponse))
            : Fail(result.Status);
    }

    private static WeightResponse ToResponse(WeightResult w) => new(w.Id, w.PetId, w.MeasuredOn, w.Kilograms, ToResponse(w.Origin), w.CreatedAtUtc);
}

/// <summary>Alergias y condiciones de las mascotas del dueño (pm-102, pm-103).</summary>
[Route("api/v1/allergies")]
public sealed class AllergiesController(ISender sender) : CarnetControllerBase(sender)
{
    [HttpPost("create")]
    [ProducesResponseType(typeof(ApiResponse<AllergyResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<ValidationErrorsData>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<ObjectResult> Create([FromBody] AllergyCreateRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetOwnerId(out var ownerId))
        {
            return Status(StatusCodes.Status401Unauthorized);
        }

        var details = new AllergyDetails(request.Name, ParseEnum<AllergyKind>(request.Kind), request.Notes, null);
        var result = await Sender.Send(new CreateAllergyCommand(ownerId, request.PetId ?? Guid.Empty, details), cancellationToken);
        return result.Status == CarnetStatus.Ok
            ? ApiResponseFactory.Create(StatusCodes.Status201Created, "Registro creado correctamente.", ToResponse(result.Value!))
            : Fail(result.Status);
    }

    [HttpGet("get-all")]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<AllergyResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ValidationErrorsData>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ObjectResult> GetAll([FromQuery] Guid? petId, [FromQuery] int? page, [FromQuery] int? pageSize, CancellationToken cancellationToken)
    {
        if (!TryGetOwnerId(out var ownerId))
        {
            return Status(StatusCodes.Status401Unauthorized);
        }

        var result = await Sender.Send(new ListAllergiesQuery(ownerId, petId ?? Guid.Empty, page, pageSize), cancellationToken);
        return result.Status == CarnetStatus.Ok
            ? ApiResponseFactory.Ok("Alergias y condiciones obtenidas correctamente.", ToPage(result.Value!, ToResponse))
            : Fail(result.Status);
    }

    [HttpPut("update/{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<AllergyResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ValidationErrorsData>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ObjectResult> Update(Guid id, [FromBody] AllergyUpdateRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetOwnerId(out var ownerId))
        {
            return Status(StatusCodes.Status401Unauthorized);
        }

        var details = new AllergyDetails(request.Name, ParseEnum<AllergyKind>(request.Kind), request.Notes, ParseEnum<AllergyStatus>(request.Status));
        var result = await Sender.Send(new UpdateAllergyCommand(ownerId, id, details), cancellationToken);
        return result.Status == CarnetStatus.Ok
            ? ApiResponseFactory.Ok("Registro actualizado correctamente.", ToResponse(result.Value!))
            : Fail(result.Status);
    }

    private static AllergyResponse ToResponse(AllergyResult a)
        => new(a.Id, a.PetId, a.Name, a.Kind.ToString(), a.Notes, a.Status.ToString(), ToResponse(a.Origin), a.CreatedAtUtc, a.UpdatedAtUtc);
}
