using System.Text.Json.Serialization;

namespace PetMate.Api.Contracts.Auth;

// Contratos HTTP de autenticación. Se rechazan propiedades desconocidas (p. ej. "role"): evita mass assignment
// y garantiza que el cliente nunca elige su rol. Los valores secretos no se imprimen en ToString.

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record RegisterRequest(string? Email, string? Password)
{
    public override string ToString() => nameof(RegisterRequest);
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record LoginRequest(string? Email, string? Password)
{
    public override string ToString() => nameof(LoginRequest);
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record RefreshRequest(string? RefreshToken)
{
    public override string ToString() => nameof(RefreshRequest);
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record LogoutRequest(string? RefreshToken)
{
    public override string ToString() => nameof(LogoutRequest);
}

/// <summary>Usuario autenticado devuelto junto a los tokens.</summary>
public sealed record AuthUserResponse(Guid Id, string Email, string Role);

/// <summary>Respuesta de Register, Login y Refresh. El refresh token solo se entrega aquí, nunca se vuelve a mostrar.</summary>
public sealed record AuthResponse(
    string AccessToken,
    string TokenType,
    DateTime AccessTokenExpiresAt,
    string RefreshToken,
    DateTime RefreshTokenExpiresAt,
    AuthUserResponse User)
{
    public override string ToString() => nameof(AuthResponse);
}
