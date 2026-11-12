using System.Text.Json.Serialization;

namespace PetMate.Api.Contracts.Businesses;

// Contratos HTTP del negocio afiliado y su catálogo (pm-107 a pm-115). Las solicitudes rechazan miembros desconocidos:
// el tipo no se cambia (pm-109#CA-02), y el dueño, la cuenta y las fechas no se envían desde el cliente.

/// <summary>Franja de apertura: horas <c>HH:mm</c>, cierre posterior a la apertura.</summary>
public sealed record TimeRangeContract(string? Open, string? Close);

/// <summary>
/// Alta del negocio. <c>type</c>: <c>Veterinary</c>, <c>Store</c> o <c>Grooming</c>. <c>schedule</c>: días <c>monday</c> a
/// <c>sunday</c> con sus franjas; un día ausente o sin franjas es un día cerrado. <c>licenseNumber</c> solo se pide a las veterinarias.
/// </summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record CreateBusinessRequest(
    string? Type,
    string? Name,
    string? Phone,
    string? WhatsApp,
    string? Address,
    string? Description,
    double? Latitude,
    double? Longitude,
    string? LicenseNumber,
    Dictionary<string, List<TimeRangeContract>>? Schedule);

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record UpdateBusinessLocationRequest(double? Latitude, double? Longitude);

/// <summary>Perfil editable: nombre, descripción, teléfonos y horario. Enviar <c>type</c> u otro miembro devuelve 400.</summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record UpdateBusinessRequest(string? Name, string? Description, string? Phone, string? WhatsApp, Dictionary<string, List<TimeRangeContract>>? Schedule);

/// <summary>Negocio visto por su dueño (incluye la cédula profesional, que nunca se publica).</summary>
public sealed record BusinessResponse(
    Guid Id,
    string Type,
    string Name,
    string Phone,
    string? WhatsApp,
    string Address,
    string? Description,
    double Latitude,
    double Longitude,
    string? LicenseNumber,
    Dictionary<string, List<TimeRangeContract>> Schedule,
    DateTime CreatedAt,
    DateTime UpdatedAt);

/// <summary><c>price</c> es opcional; sin precio se devuelve <c>null</c>, nunca 0.</summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record ServiceRequest(string? Name, string? Description, decimal? Price);

public sealed record ServiceResponse(Guid Id, string Name, string? Description, decimal? Price, DateTime CreatedAt, DateTime UpdatedAt);
