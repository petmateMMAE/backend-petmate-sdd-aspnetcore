using System.Text.Json.Serialization;

namespace PetMate.Api.Contracts.Pets;

// Contratos HTTP de mascotas (pm-040, pm-042, pm-043, pm-044). Se rechazan propiedades desconocidas: el propietario,
// el identificador y las fechas no se pueden enviar desde el cliente (evita mass assignment).

/// <summary>Datos de una mascota para crear o actualizar. <c>birthDate</c> en formato yyyy-MM-dd; <c>sex</c>: Male, Female o Unknown.</summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record PetRequest(string? Name, string? Species, string? Breed, DateOnly? BirthDate, string? Sex, string? PhotoUrl);

/// <summary>Mascota devuelta al propietario. Nunca incluye el identificador del propietario.</summary>
public sealed record PetResponse(
    Guid Id,
    string Name,
    string Species,
    string? Breed,
    DateOnly? BirthDate,
    string Sex,
    string? PhotoUrl,
    DateTime CreatedAt,
    DateTime UpdatedAt);

/// <summary>QR permanente de una mascota (pm-094): token que va dentro del código y código corto para escribirlo a mano.</summary>
public sealed record PetQrResponse(Guid PetId, string QrToken, string QrCode);
