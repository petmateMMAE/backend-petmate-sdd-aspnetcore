using PetMate.Application.Mediator;
using PetMate.Application.Security;
using PetMate.Domain.Users;

namespace PetMate.Application.Authentication;

/// <summary>Tokens emitidos a una sesión. <c>ToString</c> no imprime tokens ni correo.</summary>
public sealed record AuthTokensResult(
    AccessToken AccessToken,
    string RefreshToken,
    DateTime RefreshTokenExpiresAtUtc,
    Guid UserId,
    string Email,
    UserRole Role)
{
    public override string ToString() => "AuthTokensResult";
}

// Los comandos transportan secretos (contraseña, refresh token): su ToString nunca imprime valores.
// Register no recibe rol: el alta pública siempre crea un Owner.
public sealed record RegisterCommand(string Email, string Password) : ICommand<AuthResult<AuthTokensResult>>
{
    public override string ToString() => nameof(RegisterCommand);
}

/// <param name="ClientKey">Clave opaca del cliente (IP normalizada) para el bloqueo por par (correo, IP); <c>null</c> cuenta solo por correo.</param>
public sealed record LoginCommand(string Email, string Password, string? ClientKey = null) : ICommand<AuthResult<AuthTokensResult>>
{
    public override string ToString() => nameof(LoginCommand);
}

public sealed record RefreshCommand(string RefreshToken) : ICommand<AuthResult<AuthTokensResult>>
{
    public override string ToString() => nameof(RefreshCommand);
}

/// <summary>Identidad (<c>sub</c>, <c>jti</c>, <c>exp</c>) tomada del Access Token ya validado, más el refresh token de la sesión a cerrar.</summary>
public sealed record LogoutCommand(Guid UserId, string Jti, DateTime AccessTokenExpiresAtUtc, string RefreshToken)
    : ICommand<AuthResult<Unit>>
{
    public override string ToString() => nameof(LogoutCommand);
}
