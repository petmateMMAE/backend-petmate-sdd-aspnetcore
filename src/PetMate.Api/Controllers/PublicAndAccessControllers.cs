using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using PetMate.Api.Authentication;
using PetMate.Api.Contracts.Businesses;
using PetMate.Api.Contracts.Responses;
using PetMate.Application.Access;
using PetMate.Application.Businesses;
using PetMate.Application.Mediator;
using PetMate.Domain.Access;

namespace PetMate.Api.Controllers;

// ---------- Contratos ----------

public sealed record NearbyBusinessResponse(Guid Id, string Name, string Type, string? PhotoUrl, bool IsOpenNow, List<TimeRangeContract> TodayHours, double DistanceMeters);

public sealed record PublicServiceResponse(Guid Id, string Name, string? Description, decimal? Price);

/// <summary>Perfil público del negocio. No incluye la cédula profesional; un dato de contacto ausente es <c>null</c>.</summary>
public sealed record PublicBusinessResponse(
    Guid Id,
    string Name,
    string Type,
    string? Description,
    string? PhotoUrl,
    bool IsOpenNow,
    List<TimeRangeContract> TodayHours,
    Dictionary<string, List<TimeRangeContract>> Schedule,
    double Latitude,
    double Longitude,
    string Address,
    string? Phone,
    string? WhatsApp,
    List<PublicServiceResponse> Services);

/// <summary>
/// Autorización que da el dueño: <c>businessId</c> ausente = cualquier negocio que escanee; <c>data</c>: <c>Vaccines</c>,
/// <c>Dewormings</c>, <c>Weights</c>, <c>Consultations</c> (las alergias siempre se incluyen); <c>durationMinutes</c>: 30, 1440 o 10080.
/// </summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record CreateAccessGrantRequest(Guid? PetId, Guid? BusinessId, List<string>? Data, int? DurationMinutes);

public sealed record AccessGrantResponse(Guid Id, Guid PetId, Guid? BusinessId, List<string> Data, bool IncludesAllergies, DateTime CreatedAt, DateTime ExpiresAt);

/// <summary><c>code</c>: el token del QR (43 caracteres) o el código corto de 8 caracteres (sin distinguir mayúsculas).</summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record ResolveScanRequest(string? Code);

public sealed record ScanAllergyResponse(string Name, string Kind, string? Notes);

/// <summary>Ficha básica (pm-128): solo estos campos. <c>petId</c> aparece únicamente para una veterinaria.</summary>
public sealed record ScanFichaResponse(
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] Guid? PetId,
    string? PhotoUrl,
    string Name,
    string Species,
    string? Breed,
    int? AgeYears,
    int? AgeMonths,
    List<ScanAllergyResponse> Allergies,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] List<ScanVaccineResponse>? Vaccines = null,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] List<ScanDewormingResponse>? Dewormings = null,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] List<ScanWeightResponse>? Weights = null,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] List<ScanConsultationResponse>? Consultations = null);

// Ficha completa de la veterinaria (pm-127): solo los grupos autorizados; de solo lectura.
public sealed record ScanVaccineResponse(string Name, DateOnly AppliedOn, DateOnly? NextDoseOn, string? Batch, string? AppliedBy);

public sealed record ScanDewormingResponse(string Product, string? Type, DateOnly AppliedOn, DateOnly? NextOn, string? AppliedBy);

public sealed record ScanWeightResponse(DateOnly MeasuredOn, decimal Kilograms);

public sealed record ScanConsultationResponse(Guid Id, DateOnly ConsultedOn, string? BusinessName, string Reason, string Diagnosis, List<MedicationResponse> Medications);

// ---------- Lecturas públicas del mapa (pm-116 a pm-121) ----------

/// <summary>Lecturas públicas de negocios: accesibles sin cuenta (Visitante). Límite por IP compartido entre réplicas (pm-120).</summary>
[ApiController]
[AllowAnonymous]
[Route("api/v1/businesses")]
[EnableRateLimiting(RateLimitPolicy)]
[ServiceFilter(typeof(PublicBusinessIpThrottleFilter))]
public sealed class PublicBusinessesController(ISender sender) : ControllerBase
{
    /// <summary>Política de límite por IP (<c>PUBLIC_BUSINESS_RATE_LIMIT_PER_MINUTE</c>, por defecto 120 por minuto).</summary>
    public const string RateLimitPolicy = "public-business";

    [HttpGet("get-nearby")]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<NearbyBusinessResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ValidationErrorsData>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status429TooManyRequests)]
    public async Task<ObjectResult> GetNearby(
        [FromQuery] double? latitude,
        [FromQuery] double? longitude,
        [FromQuery] double? radiusKm,
        [FromQuery] string? types,
        [FromQuery] string? search,
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetNearbyBusinessesQuery(latitude, longitude, radiusKm, types, search, page, pageSize), cancellationToken);
        var items = result.Items.Select(i => new NearbyBusinessResponse(i.Id, i.Name, i.Type.ToString(), i.PhotoUrl, i.IsOpenNow, i.TodayHours.Select(r => new TimeRangeContract(r.Open, r.Close)).ToList(), i.DistanceMeters)).ToArray();
        return ApiResponseFactory.Ok("Negocios obtenidos correctamente.", new PagedResponse<NearbyBusinessResponse>(items, result.Page, result.PageSize, result.TotalCount, result.TotalPages));
    }

    [HttpGet("get-by-id/{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<PublicBusinessResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status429TooManyRequests)]
    public async Task<ObjectResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var profile = await sender.Send(new GetPublicBusinessQuery(id), cancellationToken);
        if (profile is null)
        {
            return ApiResponseFactory.Create<object?>(StatusCodes.Status404NotFound, ApiResponseMessages.ForStatusCode(StatusCodes.Status404NotFound), null);
        }

        return ApiResponseFactory.Ok("Negocio obtenido correctamente.", new PublicBusinessResponse(
            profile.Id,
            profile.Name,
            profile.Type.ToString(),
            profile.Description,
            profile.PhotoUrl,
            profile.IsOpenNow,
            profile.TodayHours.Select(r => new TimeRangeContract(r.Open, r.Close)).ToList(),
            profile.Schedule.ToDictionary(d => d.Key, d => d.Value.Select(r => new TimeRangeContract(r.Open, r.Close)).ToList()),
            profile.Latitude,
            profile.Longitude,
            profile.Address,
            profile.Phone,
            profile.WhatsApp,
            profile.Services.Select(s => new PublicServiceResponse(s.Id, s.Name, s.Description, s.Price)).ToList()));
    }
}

// ---------- Autorizaciones de acceso (pm-123) ----------

/// <summary>
/// El dueño da acceso a un negocio a datos de su mascota por un tiempo limitado. No existe ningún endpoint para revocar ni para
/// listar autorizaciones (pm-123#CA-04): la única forma de cortar el acceso es regenerar el QR.
/// </summary>
[Route("api/v1/access-grants")]
public sealed class AccessGrantsController(ISender sender) : BusinessControllerBase(sender)
{
    [HttpPost("create")]
    [ProducesResponseType(typeof(ApiResponse<AccessGrantResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<ValidationErrorsData>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ObjectResult> Create([FromBody] CreateAccessGrantRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetOwnerId(out var ownerId))
        {
            return Status(StatusCodes.Status401Unauthorized);
        }

        var data = (request.Data ?? []).Select(d => ParseEnum<PetDataCategory>(d) ?? default).ToArray();
        var result = await Sender.Send(new CreateAccessGrantCommand(ownerId, request.PetId ?? Guid.Empty, request.BusinessId, data, request.DurationMinutes), cancellationToken);
        return result.Status switch
        {
            AccessStatus.Ok => ApiResponseFactory.Create(StatusCodes.Status201Created, "Autorización creada correctamente.", new AccessGrantResponse(
                result.Value!.Id, result.Value.PetId, result.Value.BusinessId, result.Value.Data.Select(c => c.ToString()).ToList(), true, result.Value.CreatedAtUtc, result.Value.ExpiresAtUtc)),
            AccessStatus.BusinessNotFound => ApiResponseFactory.Create(
                StatusCodes.Status400BadRequest, ApiResponseMessages.ForStatusCode(StatusCodes.Status400BadRequest), new ValidationErrorsData([new ValidationErrorItem("businessId", "El negocio no existe.")])),
            _ => Status(StatusCodes.Status404NotFound),
        };
    }
}

// ---------- Escaneo (pm-125, pm-126, pm-128) ----------

/// <summary>Un negocio escanea el QR de una mascota. Solo con una autorización vigente recibe datos.</summary>
[Route("api/v1/scans")]
public sealed class ScansController(ISender sender) : BusinessControllerBase(sender)
{
    [HttpPost("resolve")]
    [ServiceFilter(typeof(PublicBusinessIpThrottleFilter))]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    [ProducesResponseType(typeof(ApiResponse<ScanFichaResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ValidationErrorsData>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status429TooManyRequests)]
    public async Task<ObjectResult> Resolve([FromBody] ResolveScanRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetOwnerId(out var accountId))
        {
            return Status(StatusCodes.Status401Unauthorized);
        }

        var result = await Sender.Send(new ResolveScanCommand(accountId, request.Code ?? string.Empty), cancellationToken);
        switch (result.Status)
        {
            case AccessStatus.Ok:
                var f = result.Value!;
                return ApiResponseFactory.Ok("Acceso concedido.", new ScanFichaResponse(
                    f.PetId, f.PhotoUrl, f.Name, f.Species, f.Breed, f.AgeYears, f.AgeMonths,
                    f.Allergies.Select(a => new ScanAllergyResponse(a.Name, a.Kind.ToString(), a.Notes)).ToList(),
                    f.Vaccines?.Select(v => new ScanVaccineResponse(v.Name, v.AppliedOn, v.NextDoseOn, v.Batch, v.AppliedBy)).ToList(),
                    f.Dewormings?.Select(d => new ScanDewormingResponse(d.Product, d.Type?.ToString(), d.AppliedOn, d.NextOn, d.AppliedBy)).ToList(),
                    f.Weights?.Select(w => new ScanWeightResponse(w.MeasuredOn, w.Kilograms)).ToList(),
                    f.Consultations?.Select(c => new ScanConsultationResponse(c.Id, c.ConsultedOn, c.BusinessName, c.Reason, c.Diagnosis, c.Medications.Select(m => new MedicationResponse(m.Name, m.Dose, m.Frequency, m.Duration, m.Instructions)).ToList())).ToList()));
            case AccessStatus.NoBusiness:
                return ApiResponseFactory.Create<object?>(StatusCodes.Status403Forbidden, "La cuenta no tiene un negocio asociado.", null);
            case AccessStatus.Forbidden:
                return ApiResponseFactory.Create<object?>(StatusCodes.Status403Forbidden, "No hay una autorización vigente para acceder a esta mascota.", null);
            case AccessStatus.TooManyRequests:
                var seconds = (int)Math.Ceiling(Math.Max(1, (result.RetryAfter ?? TimeSpan.FromSeconds(1)).TotalSeconds));
                Response.Headers.RetryAfter = seconds.ToString(System.Globalization.CultureInfo.InvariantCulture);
                return Status(StatusCodes.Status429TooManyRequests);
            default:
                return ApiResponseFactory.Create<object?>(StatusCodes.Status404NotFound, "No se encontró una mascota con ese código.", null);
        }
    }
}
