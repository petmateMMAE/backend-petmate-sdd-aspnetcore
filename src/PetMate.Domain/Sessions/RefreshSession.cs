using PetMate.Domain.Common;

namespace PetMate.Domain.Sessions;

/// <summary>Sesión/familia de refresh tokens de un dispositivo. Vence por inactividad (renovable) y por vida absoluta (no renovable).</summary>
public sealed class RefreshSession
{
    private RefreshSession(
        Guid id,
        Guid userId,
        DateTime createdAtUtc,
        DateTime lastUsedAtUtc,
        DateTime idleExpiresAtUtc,
        DateTime absoluteExpiresAtUtc,
        DateTime? revokedAtUtc,
        SessionRevocationReason? revocationReason)
    {
        Id = id;
        UserId = userId;
        CreatedAtUtc = createdAtUtc;
        LastUsedAtUtc = lastUsedAtUtc;
        IdleExpiresAtUtc = idleExpiresAtUtc;
        AbsoluteExpiresAtUtc = absoluteExpiresAtUtc;
        RevokedAtUtc = revokedAtUtc;
        RevocationReason = revocationReason;
    }

    public Guid Id { get; }

    public Guid UserId { get; }

    public DateTime CreatedAtUtc { get; }

    public DateTime LastUsedAtUtc { get; private set; }

    public DateTime IdleExpiresAtUtc { get; private set; }

    public DateTime AbsoluteExpiresAtUtc { get; }

    public DateTime? RevokedAtUtc { get; private set; }

    public SessionRevocationReason? RevocationReason { get; private set; }

    public bool IsRevoked => RevokedAtUtc is not null;

    public static RefreshSession Start(Guid userId, DateTime nowUtc, SessionPolicy policy)
    {
        ArgumentNullException.ThrowIfNull(policy);
        var now = Guard.Utc(nowUtc, nameof(nowUtc));
        var absolute = now + policy.AbsoluteLifetime;
        return new RefreshSession(
            Guid.CreateVersion7(),
            Guard.NotEmpty(userId, nameof(userId)),
            now,
            now,
            Earliest(now + policy.IdleTimeout, absolute),
            absolute,
            null,
            null);
    }

    /// <summary>Rehidrata una sesión persistida. Interno: visible solo para Infrastructure y las pruebas (InternalsVisibleTo).</summary>
    internal static RefreshSession Restore(
        Guid id,
        Guid userId,
        DateTime createdAtUtc,
        DateTime lastUsedAtUtc,
        DateTime idleExpiresAtUtc,
        DateTime absoluteExpiresAtUtc,
        DateTime? revokedAtUtc,
        SessionRevocationReason? revocationReason)
    {
        if ((revokedAtUtc is null) != (revocationReason is null) || (revocationReason is { } reason && !Enum.IsDefined(reason)))
        {
            throw new DomainException("El estado de revocación de la sesión no es consistente.");
        }

        var created = Guard.Utc(createdAtUtc, nameof(createdAtUtc));
        var lastUsed = Guard.Utc(lastUsedAtUtc, nameof(lastUsedAtUtc));
        var idle = Guard.Utc(idleExpiresAtUtc, nameof(idleExpiresAtUtc));
        var absolute = Guard.Utc(absoluteExpiresAtUtc, nameof(absoluteExpiresAtUtc));
        if (lastUsed < created || absolute <= created || idle > absolute)
        {
            throw new DomainException("Las fechas de la sesión no son coherentes.");
        }

        return new RefreshSession(
            Guard.NotEmpty(id, nameof(id)),
            Guard.NotEmpty(userId, nameof(userId)),
            created,
            lastUsed,
            idle,
            absolute,
            revokedAtUtc is { } revoked ? Guard.Utc(revoked, nameof(revokedAtUtc)) : null,
            revocationReason);
    }

    public bool IsActive(DateTime nowUtc)
    {
        var now = Guard.Utc(nowUtc, nameof(nowUtc));
        return !IsRevoked && now < IdleExpiresAtUtc && now < AbsoluteExpiresAtUtc;
    }

    /// <summary>Registra un uso válido: renueva la ventana de inactividad sin superar el límite absoluto.</summary>
    public void RegisterUse(DateTime nowUtc, SessionPolicy policy)
    {
        ArgumentNullException.ThrowIfNull(policy);
        if (!IsActive(nowUtc) || nowUtc < LastUsedAtUtc)
        {
            throw new DomainException("La sesión no está activa.");
        }

        LastUsedAtUtc = nowUtc;
        IdleExpiresAtUtc = Earliest(nowUtc + policy.IdleTimeout, AbsoluteExpiresAtUtc);
    }

    /// <summary>Revoca la sesión. Es idempotente: la primera revocación conserva su fecha y motivo.</summary>
    public void Revoke(DateTime nowUtc, SessionRevocationReason reason)
    {
        if (!Enum.IsDefined(reason))
        {
            throw new DomainException("El motivo de revocación no es válido.");
        }

        var now = Guard.Utc(nowUtc, nameof(nowUtc));
        if (IsRevoked)
        {
            return;
        }

        RevokedAtUtc = now;
        RevocationReason = reason;
    }

    private static DateTime Earliest(DateTime a, DateTime b) => a <= b ? a : b;
}
