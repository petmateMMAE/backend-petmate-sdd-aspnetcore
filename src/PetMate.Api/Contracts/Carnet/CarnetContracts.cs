using System.Text.Json.Serialization;

namespace PetMate.Api.Contracts.Carnet;

// Contratos HTTP del carnet de salud (pm-097 a pm-103). Fechas en formato yyyy-MM-dd; las solicitudes rechazan miembros
// desconocidos (el dueño, el origen y las fechas de creación no se pueden enviar desde el cliente).

/// <summary>Quién creó el registro: <c>Owner</c> (el dueño) o <c>Business</c> (un negocio, con su identificador y nombre).</summary>
public sealed record RecordOriginResponse(string Type, Guid? BusinessId, string? BusinessName);

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record VaccineRequest(Guid? PetId, DateOnly? AppliedOn, string? Name, string? Batch, string? AppliedBy, DateOnly? NextDoseOn);

public sealed record VaccineResponse(
    Guid Id,
    Guid PetId,
    DateOnly AppliedOn,
    string Name,
    string? Batch,
    string? AppliedBy,
    DateOnly? NextDoseOn,
    RecordOriginResponse Origin,
    DateTime CreatedAt);

/// <summary><c>type</c>: <c>Internal</c> o <c>External</c> (opcional).</summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record DewormingRequest(Guid? PetId, DateOnly? AppliedOn, string? Product, string? Type, string? AppliedBy, DateOnly? NextOn);

public sealed record DewormingResponse(
    Guid Id,
    Guid PetId,
    DateOnly AppliedOn,
    string Product,
    string? Type,
    string? AppliedBy,
    DateOnly? NextOn,
    RecordOriginResponse Origin,
    DateTime CreatedAt);

/// <summary><c>kilograms</c>: mayor que 0, con un decimal.</summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record WeightRequest(Guid? PetId, DateOnly? MeasuredOn, decimal? Kilograms);

public sealed record WeightResponse(Guid Id, Guid PetId, DateOnly MeasuredOn, decimal Kilograms, RecordOriginResponse Origin, DateTime CreatedAt);

/// <summary><c>kind</c>: <c>Allergy</c> o <c>Condition</c>. Al crear, el estado siempre es <c>Active</c>.</summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record AllergyCreateRequest(Guid? PetId, string? Name, string? Kind, string? Notes);

/// <summary><c>status</c>: <c>Active</c> o <c>Resolved</c> (obligatorio al actualizar).</summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record AllergyUpdateRequest(string? Name, string? Kind, string? Notes, string? Status);

public sealed record AllergyResponse(
    Guid Id,
    Guid PetId,
    string Name,
    string Kind,
    string? Notes,
    string Status,
    RecordOriginResponse Origin,
    DateTime CreatedAt,
    DateTime UpdatedAt);
