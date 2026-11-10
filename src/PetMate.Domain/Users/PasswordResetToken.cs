using PetMate.Domain.Common;
using PetMate.Domain.Sessions;

namespace PetMate.Domain.Users;

/// <summary>
/// Token de un solo uso para restablecer la contraseña (pm-089, pm-090). Solo se guarda el hash; vence a la hora de emitirse y
/// se invalida al usarse.
/// </summary>
public sealed class PasswordResetToken
{
    /// <summary>Vigencia del enlace (pm-089#CA-02).</summary>
    public static readonly TimeSpan Lifetime = TimeSpan.FromHours(1);

    private PasswordResetToken(Guid id, Guid userId, RefreshTokenHash tokenHash, DateTime createdAtUtc, DateTime expiresAtUtc, DateTime? usedAtUtc)
    {
        Id = id;
        UserId = userId;
        TokenHash = tokenHash;
        CreatedAtUtc = createdAtUtc;
        ExpiresAtUtc = expiresAtUtc;
        UsedAtUtc = usedAtUtc;
    }

    public Guid Id { get; }

    public Guid UserId { get; }

    public RefreshTokenHash TokenHash { get; }

    public DateTime CreatedAtUtc { get; }

    public DateTime ExpiresAtUtc { get; }

    public DateTime? UsedAtUtc { get; }

    public static PasswordResetToken Issue(Guid userId, RefreshTokenHash tokenHash, DateTime nowUtc)
    {
        ArgumentNullException.ThrowIfNull(tokenHash);
        var now = Guard.Utc(nowUtc, nameof(nowUtc));
        return new PasswordResetToken(Guid.CreateVersion7(), Guard.NotEmpty(userId, nameof(userId)), tokenHash, now, now + Lifetime, null);
    }

    /// <summary>Rehidrata un token persistido. Interno: Infrastructure y pruebas.</summary>
    internal static PasswordResetToken Restore(Guid id, Guid userId, RefreshTokenHash tokenHash, DateTime createdAtUtc, DateTime expiresAtUtc, DateTime? usedAtUtc)
    {
        ArgumentNullException.ThrowIfNull(tokenHash);
        var created = Guard.Utc(createdAtUtc, nameof(createdAtUtc));
        var expires = Guard.Utc(expiresAtUtc, nameof(expiresAtUtc));
        return expires > created
            ? new PasswordResetToken(Guard.NotEmpty(id, nameof(id)), Guard.NotEmpty(userId, nameof(userId)), tokenHash, created, expires, usedAtUtc)
            : throw new DomainException("Las fechas del token de restablecimiento no son coherentes.");
    }
}
