using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PetMate.Api.Contracts.Carnet;
using PetMate.Api.Contracts.Responses;
using PetMate.Application.Access;
using PetMate.Application.Carnet;
using PetMate.Application.Followups;
using PetMate.Application.Mediator;
using PetMate.Domain.Carnet;
using PetMate.Domain.Chat;
using PetMate.Domain.Followups;

namespace PetMate.Api.Controllers;

// ---------- Contratos (pm-130 a pm-149) ----------

public sealed record AttendedPetResponse(string PetName, string? PhotoUrl, DateTime ScannedAt);

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record MedicationRequest(string? Name, string? Dose, string? Frequency, string? Duration, string? Instructions);

public sealed record MedicationResponse(string Name, string Dose, string Frequency, string Duration, string? Instructions);

/// <summary>Consulta: <c>reason</c> y <c>diagnosis</c> obligatorios, <c>weightKg</c> opcional y receta con hasta 10 medicamentos.</summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record CreateConsultationRequest(Guid? PetId, string? Reason, string? Diagnosis, decimal? WeightKg, List<MedicationRequest>? Medications);

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record AddMedicationRequest(Guid? ConsultationId, string? Name, string? Dose, string? Frequency, string? Duration, string? Instructions);

public sealed record ConsultationResponse(Guid Id, Guid PetId, DateOnly ConsultedOn, RecordOriginResponse Origin, string Reason, string Diagnosis, decimal? WeightKg, List<MedicationResponse> Medications, DateTime CreatedAt);

public sealed record ConsultationSummaryResponse(Guid Id, DateOnly ConsultedOn, string? BusinessName, string Reason, string Diagnosis);

public sealed record HistoryItemResponse(string Kind, Guid Id, DateOnly Date, string Title, RecordOriginResponse Origin);

/// <summary>Vacuna aplicada por la veterinaria: «aplicada por» y el origen los fija el servidor.</summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record VaccineByBusinessRequest(Guid? PetId, DateOnly? AppliedOn, string? Name, string? Batch, DateOnly? NextDoseOn);

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record DewormingByBusinessRequest(Guid? PetId, DateOnly? AppliedOn, string? Product, string? Type, DateOnly? NextOn);

/// <summary><c>nextReviewInDays</c>: 3, 7 o 15 (opcional).</summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record CreateFollowupRequest(Guid? ConsultationId, int? NextReviewInDays);

/// <summary><c>status</c>: <c>Treating</c>, <c>Improving</c> o <c>Discharged</c>. Dar de alta elimina la próxima revisión.</summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record UpdateFollowupRequest(string? Status, int? NextReviewInDays);

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record AddFollowupNoteRequest(Guid? FollowupId, string? Text);

public sealed record FollowupNoteResponse(DateTime At, string Text);

public sealed record FollowupResponse(Guid Id, Guid ConsultationId, Guid PetId, string PetName, string Status, DateOnly? NextReviewOn, bool IsOverdue, List<FollowupNoteResponse> Notes, Guid? ConversationId, DateTime CreatedAt, DateTime UpdatedAt);

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record SendMessageRequest(Guid? ConversationId, string? Text);

public sealed record MessageResponse(Guid Id, Guid ConversationId, long Seq, string Kind, bool IsMine, string Text, DateTime CreatedAt);

public sealed record MessageListResponse(List<MessageResponse> Items, bool HasMore);

public sealed record ConversationSummaryResponse(Guid Id, Guid FollowupId, bool IsClosed, MessageResponse? LastMessage, long UnreadCount, DateTime UpdatedAt);

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record MarkReadRequest(Guid? ConversationId, Guid? UpToMessageId);

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record ReopenConversationRequest(Guid? ConversationId);

/// <summary>Base de los controladores de las operaciones de una veterinaria y del chat: traduce los resultados a HTTP.</summary>
public abstract class VeterinaryControllerBase(ISender sender) : BusinessControllerBase(sender)
{
    protected static ObjectResult FailAccess(AccessStatus status, string? forbidden = null) => status switch
    {
        AccessStatus.NoBusiness => ApiResponseFactory.Create<object?>(StatusCodes.Status403Forbidden, "La cuenta no tiene un negocio asociado.", null),
        AccessStatus.NotVeterinary => ApiResponseFactory.Create<object?>(StatusCodes.Status403Forbidden, "Solo una veterinaria puede realizar esta operación.", null),
        AccessStatus.Forbidden => ApiResponseFactory.Create<object?>(StatusCodes.Status403Forbidden, forbidden ?? "No hay una autorización vigente para esta mascota.", null),
        AccessStatus.Conflict => ApiResponseFactory.Create<object?>(StatusCodes.Status409Conflict, "La operación está en conflicto: ya existe o se alcanzó el máximo permitido.", null),
        AccessStatus.Closed => ApiResponseFactory.Create<object?>(StatusCodes.Status409Conflict, "La conversación está cerrada.", null),
        AccessStatus.TooManyRequests => Status(StatusCodes.Status429TooManyRequests),
        _ => Status(StatusCodes.Status404NotFound),
    };

    protected static RecordOriginResponse ToResponse(RecordOriginResult origin) => new(origin.Origin.ToString(), origin.BusinessId, origin.BusinessName);

    protected static ObjectResult FailCarnet(CarnetStatus status) => status == CarnetStatus.NotFound ? Status(StatusCodes.Status404NotFound) : Status(StatusCodes.Status500InternalServerError);

    protected static MessageResponse ToResponse(MessageResult m) => new(m.Id, m.ConversationId, m.Seq, m.Kind.ToString(), m.IsMine, m.Text, m.CreatedAtUtc);

    protected static FollowupResponse ToResponse(FollowupResult f)
        => new(f.Id, f.ConsultationId, f.PetId, f.PetName, f.Status.ToString(), f.NextReviewOn, f.IsOverdue, f.Notes.Select(n => new FollowupNoteResponse(n.AtUtc, n.Text)).ToList(), f.ConversationId, f.CreatedAtUtc, f.UpdatedAtUtc);

    protected static ConsultationResponse ToResponse(ConsultationResult c)
        => new(c.Id, c.PetId, c.ConsultedOn, ToResponse(c.Origin), c.Reason, c.Diagnosis, c.WeightKg, c.Medications.Select(m => new MedicationResponse(m.Name, m.Dose, m.Frequency, m.Duration, m.Instructions)).ToList(), c.CreatedAtUtc);

    protected static ConversationSummaryResponse ToResponse(ConversationSummary c)
        => new(c.Id, c.FollowupId, c.IsClosed, c.LastMessage is null ? null : ToResponse(c.LastMessage), c.UnreadCount, c.UpdatedAtUtc);
}

// ---------- Consultas (pm-131 a pm-134) ----------

[Route("api/v1/consultations")]
public sealed class ConsultationsController(ISender sender) : VeterinaryControllerBase(sender)
{
    [HttpPost("create")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<ConsultationResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<ValidationErrorsData>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ObjectResult> Create([FromBody] CreateConsultationRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetOwnerId(out var accountId))
        {
            return Status(StatusCodes.Status401Unauthorized);
        }

        var medications = (request.Medications ?? []).Select(m => new MedicationDetails(m?.Name, m?.Dose, m?.Frequency, m?.Duration, m?.Instructions)).ToArray();
        var details = new ConsultationDetails(request.Reason, request.Diagnosis, request.WeightKg, medications);
        var result = await Sender.Send(new CreateConsultationCommand(accountId, request.PetId ?? Guid.Empty, details), cancellationToken);
        return result.Status == AccessStatus.Ok ? ApiResponseFactory.Create(StatusCodes.Status201Created, "Consulta registrada correctamente.", ToResponse(result.Value!)) : FailAccess(result.Status);
    }

    [HttpPost("add-medication")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<ConsultationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ValidationErrorsData>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ObjectResult> AddMedication([FromBody] AddMedicationRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetOwnerId(out var accountId))
        {
            return Status(StatusCodes.Status401Unauthorized);
        }

        var medication = new MedicationDetails(request.Name, request.Dose, request.Frequency, request.Duration, request.Instructions);
        var result = await Sender.Send(new AddMedicationCommand(accountId, request.ConsultationId ?? Guid.Empty, medication), cancellationToken);
        return result.Status == AccessStatus.Ok ? ApiResponseFactory.Ok("Medicamento agregado correctamente.", ToResponse(result.Value!)) : FailAccess(result.Status);
    }

    [HttpGet("get-all")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<ConsultationSummaryResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ValidationErrorsData>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ObjectResult> GetAll([FromQuery] Guid? petId, [FromQuery] int? page, [FromQuery] int? pageSize, CancellationToken cancellationToken)
    {
        if (!TryGetOwnerId(out var ownerId))
        {
            return Status(StatusCodes.Status401Unauthorized);
        }

        var result = await Sender.Send(new ListConsultationsQuery(ownerId, petId ?? Guid.Empty, page, pageSize), cancellationToken);
        if (result.Status != CarnetStatus.Ok)
        {
            return FailCarnet(result.Status);
        }

        var data = result.Value!;
        return ApiResponseFactory.Ok("Consultas obtenidas correctamente.", new PagedResponse<ConsultationSummaryResponse>(
            data.Items.Select(i => new ConsultationSummaryResponse(i.Id, i.ConsultedOn, i.BusinessName, i.Reason, i.Diagnosis)).ToArray(), data.Page, data.PageSize, data.TotalCount, data.TotalPages));
    }

    [HttpGet("get-by-id/{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<ConsultationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ObjectResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        if (!TryGetOwnerId(out var ownerId))
        {
            return Status(StatusCodes.Status401Unauthorized);
        }

        var result = await Sender.Send(new GetConsultationQuery(ownerId, id), cancellationToken);
        return result.Status == CarnetStatus.Ok ? ApiResponseFactory.Ok("Consulta obtenida correctamente.", ToResponse(result.Value!)) : FailCarnet(result.Status);
    }
}

// ---------- Historial médico (pm-137) ----------

[Route("api/v1/history")]
public sealed class HistoryController(ISender sender) : VeterinaryControllerBase(sender)
{
    [HttpGet("get-all")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<HistoryItemResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ValidationErrorsData>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ObjectResult> GetAll([FromQuery] Guid? petId, [FromQuery] string? types, [FromQuery] int? page, [FromQuery] int? pageSize, CancellationToken cancellationToken)
    {
        if (!TryGetOwnerId(out var ownerId))
        {
            return Status(StatusCodes.Status401Unauthorized);
        }

        var result = await Sender.Send(new GetHistoryQuery(ownerId, petId ?? Guid.Empty, types, page, pageSize), cancellationToken);
        if (result.Status != CarnetStatus.Ok)
        {
            return FailCarnet(result.Status);
        }

        var data = result.Value!;
        return ApiResponseFactory.Ok("Historial obtenido correctamente.", new PagedResponse<HistoryItemResponse>(
            data.Items.Select(i => new HistoryItemResponse(i.Kind.ToString(), i.Id, i.Date, i.Title, ToResponse(i.Origin))).ToArray(), data.Page, data.PageSize, data.TotalCount, data.TotalPages));
    }
}

// ---------- Vacunas y desparasitaciones aplicadas por la veterinaria (pm-135, pm-136) ----------

[Route("api/v1/vaccines")]
public sealed class VaccinesByBusinessController(ISender sender) : VeterinaryControllerBase(sender)
{
    [HttpPost("create-by-business")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<VaccineResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<ValidationErrorsData>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ObjectResult> Create([FromBody] VaccineByBusinessRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetOwnerId(out var accountId))
        {
            return Status(StatusCodes.Status401Unauthorized);
        }

        var details = new VaccineDetails(request.AppliedOn, request.Name, request.Batch, null, request.NextDoseOn);
        var result = await Sender.Send(new CreateVaccineByBusinessCommand(accountId, request.PetId ?? Guid.Empty, details), cancellationToken);
        if (result.Status != AccessStatus.Ok)
        {
            return FailAccess(result.Status);
        }

        var v = result.Value!;
        return ApiResponseFactory.Create(StatusCodes.Status201Created, "Vacuna registrada correctamente.", new VaccineResponse(v.Id, v.PetId, v.AppliedOn, v.Name, v.Batch, v.AppliedBy, v.NextDoseOn, ToResponse(v.Origin), v.CreatedAtUtc));
    }
}

[Route("api/v1/dewormings")]
public sealed class DewormingsByBusinessController(ISender sender) : VeterinaryControllerBase(sender)
{
    [HttpPost("create-by-business")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<DewormingResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<ValidationErrorsData>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ObjectResult> Create([FromBody] DewormingByBusinessRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetOwnerId(out var accountId))
        {
            return Status(StatusCodes.Status401Unauthorized);
        }

        var details = new DewormingDetails(request.AppliedOn, request.Product, ParseEnum<DewormingType>(request.Type), null, request.NextOn);
        var result = await Sender.Send(new CreateDewormingByBusinessCommand(accountId, request.PetId ?? Guid.Empty, details), cancellationToken);
        if (result.Status != AccessStatus.Ok)
        {
            return FailAccess(result.Status);
        }

        var d = result.Value!;
        return ApiResponseFactory.Create(StatusCodes.Status201Created, "Desparasitación registrada correctamente.", new DewormingResponse(d.Id, d.PetId, d.AppliedOn, d.Product, d.Type?.ToString(), d.AppliedBy, d.NextOn, ToResponse(d.Origin), d.CreatedAtUtc));
    }
}

// ---------- Mascotas atendidas (pm-130) ----------

[Route("api/v1/scans")]
public sealed class AttendedScansController(ISender sender) : VeterinaryControllerBase(sender)
{
    [HttpGet("get-attended")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<AttendedPetResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ValidationErrorsData>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<ObjectResult> GetAttended([FromQuery] string? period, [FromQuery] int? page, [FromQuery] int? pageSize, CancellationToken cancellationToken)
    {
        if (!TryGetOwnerId(out var accountId))
        {
            return Status(StatusCodes.Status401Unauthorized);
        }

        var result = await Sender.Send(new GetAttendedPetsQuery(accountId, period, page, pageSize), cancellationToken);
        if (result.Status != AccessStatus.Ok)
        {
            return FailAccess(result.Status);
        }

        var data = result.Value!;
        return ApiResponseFactory.Ok("Mascotas atendidas obtenidas correctamente.", new PagedResponse<AttendedPetResponse>(
            data.Items.Select(i => new AttendedPetResponse(i.PetName, i.PhotoUrl, i.ScannedAtUtc)).ToArray(), data.Page, data.PageSize, data.TotalCount, data.TotalPages));
    }
}

// ---------- Seguimientos (pm-138 a pm-142) ----------

[Route("api/v1/followups")]
public sealed class FollowupsController(ISender sender) : VeterinaryControllerBase(sender)
{
    [HttpPost("create")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<FollowupResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<ValidationErrorsData>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<ObjectResult> Create([FromBody] CreateFollowupRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetOwnerId(out var accountId))
        {
            return Status(StatusCodes.Status401Unauthorized);
        }

        var result = await Sender.Send(new CreateFollowupCommand(accountId, request.ConsultationId ?? Guid.Empty, request.NextReviewInDays), cancellationToken);
        return result.Status == AccessStatus.Ok ? ApiResponseFactory.Create(StatusCodes.Status201Created, "Seguimiento abierto correctamente.", ToResponse(result.Value!)) : FailAccess(result.Status);
    }

    [HttpPut("update/{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<FollowupResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ValidationErrorsData>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ObjectResult> Update(Guid id, [FromBody] UpdateFollowupRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetOwnerId(out var accountId))
        {
            return Status(StatusCodes.Status401Unauthorized);
        }

        var result = await Sender.Send(new UpdateFollowupCommand(accountId, id, ParseEnum<FollowupStatus>(request.Status), request.NextReviewInDays), cancellationToken);
        return result.Status == AccessStatus.Ok ? ApiResponseFactory.Ok("Seguimiento actualizado correctamente.", ToResponse(result.Value!)) : FailAccess(result.Status);
    }

    [HttpPost("add-note")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<FollowupResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ValidationErrorsData>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ObjectResult> AddNote([FromBody] AddFollowupNoteRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetOwnerId(out var accountId))
        {
            return Status(StatusCodes.Status401Unauthorized);
        }

        var result = await Sender.Send(new AddFollowupNoteCommand(accountId, request.FollowupId ?? Guid.Empty, request.Text ?? string.Empty), cancellationToken);
        return result.Status == AccessStatus.Ok ? ApiResponseFactory.Ok("Nota agregada correctamente.", ToResponse(result.Value!)) : FailAccess(result.Status);
    }

    [HttpGet("get-by-consultation/{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<FollowupResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ObjectResult> GetByConsultation(Guid id, CancellationToken cancellationToken)
    {
        if (!TryGetOwnerId(out var ownerId))
        {
            return Status(StatusCodes.Status401Unauthorized);
        }

        var result = await Sender.Send(new GetFollowupByConsultationQuery(ownerId, id), cancellationToken);
        return result.Status == AccessStatus.Ok ? ApiResponseFactory.Ok("Seguimiento obtenido correctamente.", ToResponse(result.Value!)) : FailAccess(result.Status);
    }

    [HttpGet("get-all")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<FollowupResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ValidationErrorsData>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<ObjectResult> GetAll([FromQuery] DateOnly? reviewDate, [FromQuery] bool? onlyActive, [FromQuery] int? page, [FromQuery] int? pageSize, CancellationToken cancellationToken)
    {
        if (!TryGetOwnerId(out var accountId))
        {
            return Status(StatusCodes.Status401Unauthorized);
        }

        var result = await Sender.Send(new ListFollowupsQuery(accountId, reviewDate, onlyActive ?? false, page, pageSize), cancellationToken);
        if (result.Status != AccessStatus.Ok)
        {
            return FailAccess(result.Status);
        }

        var data = result.Value!;
        return ApiResponseFactory.Ok("Seguimientos obtenidos correctamente.", new PagedResponse<FollowupResponse>(data.Items.Select(ToResponse).ToArray(), data.Page, data.PageSize, data.TotalCount, data.TotalPages));
    }
}

// ---------- Chat (pm-143 a pm-149) ----------

[Route("api/v1/messages")]
public sealed class MessagesController(ISender sender) : VeterinaryControllerBase(sender)
{
    [HttpPost("send")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<MessageResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<ValidationErrorsData>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<ObjectResult> Send([FromBody] SendMessageRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetOwnerId(out var accountId))
        {
            return Status(StatusCodes.Status401Unauthorized);
        }

        var result = await Sender.Send(new SendMessageCommand(accountId, request.ConversationId ?? Guid.Empty, request.Text ?? string.Empty), cancellationToken);
        return result.Status == AccessStatus.Ok ? ApiResponseFactory.Create(StatusCodes.Status201Created, "Mensaje enviado correctamente.", ToResponse(result.Value!)) : FailAccess(result.Status);
    }

    [HttpGet("get-all")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<MessageListResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ValidationErrorsData>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ObjectResult> GetAll([FromQuery] Guid? conversationId, [FromQuery] Guid? after, [FromQuery] int? pageSize, CancellationToken cancellationToken)
    {
        if (!TryGetOwnerId(out var accountId))
        {
            return Status(StatusCodes.Status401Unauthorized);
        }

        var result = await Sender.Send(new ListMessagesQuery(accountId, conversationId ?? Guid.Empty, after, pageSize), cancellationToken);
        return result.Status == AccessStatus.Ok
            ? ApiResponseFactory.Ok("Mensajes obtenidos correctamente.", new MessageListResponse(result.Value!.Items.Select(ToResponse).ToList(), result.Value.HasMore))
            : FailAccess(result.Status);
    }
}

[Route("api/v1/conversations")]
public sealed class ConversationsController(ISender sender) : VeterinaryControllerBase(sender)
{
    [HttpGet("get-all")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<ConversationSummaryResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ValidationErrorsData>), StatusCodes.Status400BadRequest)]
    public async Task<ObjectResult> GetAll([FromQuery] int? page, [FromQuery] int? pageSize, CancellationToken cancellationToken)
    {
        if (!TryGetOwnerId(out var accountId))
        {
            return Status(StatusCodes.Status401Unauthorized);
        }

        var data = await Sender.Send(new ListConversationsQuery(accountId, page, pageSize), cancellationToken);
        return ApiResponseFactory.Ok("Conversaciones obtenidas correctamente.", new PagedResponse<ConversationSummaryResponse>(data.Items.Select(ToResponse).ToArray(), data.Page, data.PageSize, data.TotalCount, data.TotalPages));
    }

    [HttpPost("mark-read")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<ConversationSummaryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ValidationErrorsData>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ObjectResult> MarkRead([FromBody] MarkReadRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetOwnerId(out var accountId))
        {
            return Status(StatusCodes.Status401Unauthorized);
        }

        var result = await Sender.Send(new MarkReadCommand(accountId, request.ConversationId ?? Guid.Empty, request.UpToMessageId), cancellationToken);
        return result.Status == AccessStatus.Ok ? ApiResponseFactory.Ok("Conversación marcada como leída.", ToResponse(result.Value!)) : FailAccess(result.Status);
    }

    [HttpPost("reopen")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<FollowupResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ValidationErrorsData>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ObjectResult> Reopen([FromBody] ReopenConversationRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetOwnerId(out var accountId))
        {
            return Status(StatusCodes.Status401Unauthorized);
        }

        var result = await Sender.Send(new ReopenConversationCommand(accountId, request.ConversationId ?? Guid.Empty), cancellationToken);
        return result.Status == AccessStatus.Ok ? ApiResponseFactory.Ok("Conversación reabierta correctamente.", ToResponse(result.Value!)) : FailAccess(result.Status, "Solo la veterinaria puede reabrir la conversación.");
    }
}
