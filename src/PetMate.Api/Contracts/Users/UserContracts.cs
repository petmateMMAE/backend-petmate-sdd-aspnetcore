using System.Text.Json.Serialization;

namespace PetMate.Api.Contracts.Users;

// Contratos HTTP del perfil propio (pm-037, pm-038). Se rechazan propiedades desconocidas: el correo y el rol
// no se pueden cambiar desde aquí (evita mass assignment).

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record UpdateProfileRequest(string? DisplayName);

/// <summary>Perfil del usuario autenticado. Nunca incluye el hash de la contraseña ni tokens.</summary>
public sealed record UserProfileResponse(Guid Id, string Email, string Role, string? DisplayName, DateTime CreatedAt)
{
    public override string ToString() => nameof(UserProfileResponse);
}
