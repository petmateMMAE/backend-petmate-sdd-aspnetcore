using PetMate.Domain.Common;

namespace PetMate.Domain.Sessions;

/// <summary>Un refresh token emitido dentro de una sesión. Se consume una sola vez; presentarlo de nuevo es señal de replay.</summary>
public sealed class RefreshToken
{
    private RefreshToken(Guid id, Guid sessionId, RefreshTokenHash tokenHash, DateTime issuedAtUtc, DateTime? consumedAtUtc)
    {
        Id = id;
        SessionId = sessionId;
        TokenHash = tokenHash;
        IssuedAtUtc = issuedAtUtc;
        ConsumedAtUtc = consumedAtUtc;
    }

    public Guid Id { get; }

    public Guid SessionId { get; }

    public RefreshTokenHash TokenHash { get; }

    public DateTime IssuedAtUtc { get; }

    public DateTime? ConsumedAtUtc { get; private set; }

    public bool IsConsumed => ConsumedAtUtc is not null;

    public static RefreshToken Issue(Guid sessionId, RefreshTokenHash tokenHash, DateTime nowUtc)
    {
        ArgumentNullException.ThrowIfNull(tokenHash);
        return new RefreshToken(
            Guid.CreateVersion7(),
            Guard.NotEmpty(sessionId, nameof(sessionId)),
            tokenHash,
            Guard.Utc(nowUtc, nameof(nowUtc)),
            null);
    }

    /// <summary>Rehidrata un token persistido. Interno: visible solo para Infrastructure y las pruebas (InternalsVisibleTo).</summary>
    internal static RefreshToken Restore(Guid id, Guid sessionId, RefreshTokenHash tokenHash, DateTime issuedAtUtc, DateTime? consumedAtUtc)
    {
        ArgumentNullException.ThrowIfNull(tokenHash);
        var issued = Guard.Utc(issuedAtUtc, nameof(issuedAtUtc));
        DateTime? consumedUtc = consumedAtUtc is { } consumed ? Guard.Utc(consumed, nameof(consumedAtUtc)) : null;
        if (consumedUtc < issued)
        {
            throw new DomainException("Las fechas del refresh token no son coherentes.");
        }

        return new RefreshToken(Guard.NotEmpty(id, nameof(id)), Guard.NotEmpty(sessionId, nameof(sessionId)), tokenHash, issued, consumedUtc);
    }

    public void Consume(DateTime nowUtc)
    {
        if (IsConsumed)
        {
            throw new DomainException("El refresh token ya fue consumido.");
        }

        ConsumedAtUtc = Guard.Utc(nowUtc, nameof(nowUtc));
    }
}
