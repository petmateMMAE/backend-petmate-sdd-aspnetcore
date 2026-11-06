using PetMate.Domain.Sessions;
using PetMate.Domain.Users;

namespace PetMate.Infrastructure.Persistence;

/// <summary>Conversión explícita dominio ↔ documento. Rehidratar valida de nuevo las invariantes del dominio.</summary>
internal static class MongoMapping
{
    public static UserDocument ToDocument(User user) => new()
    {
        Id = user.Id,
        Email = user.Email.Value,
        PasswordHash = user.PasswordHash.Value,
        Role = (int)user.Role,
        DisplayName = user.DisplayName,
        CreatedAtUtc = user.CreatedAtUtc,
        UpdatedAtUtc = user.UpdatedAtUtc,
    };

    public static User ToDomain(UserDocument document) => User.Restore(
        document.Id,
        Email.Create(document.Email),
        PasswordHash.Create(document.PasswordHash),
        (UserRole)document.Role,
        AsUtc(document.CreatedAtUtc),
        AsUtc(document.UpdatedAtUtc),
        document.DisplayName);

    public static RefreshSessionDocument ToDocument(RefreshSession session) => new()
    {
        Id = session.Id,
        UserId = session.UserId,
        CreatedAtUtc = session.CreatedAtUtc,
        LastUsedAtUtc = session.LastUsedAtUtc,
        IdleExpiresAtUtc = session.IdleExpiresAtUtc,
        AbsoluteExpiresAtUtc = session.AbsoluteExpiresAtUtc,
        RevokedAtUtc = session.RevokedAtUtc,
        RevocationReason = session.RevocationReason is { } reason ? (int)reason : null,
    };

    public static RefreshSession ToDomain(RefreshSessionDocument document) => RefreshSession.Restore(
        document.Id,
        document.UserId,
        AsUtc(document.CreatedAtUtc),
        AsUtc(document.LastUsedAtUtc),
        AsUtc(document.IdleExpiresAtUtc),
        AsUtc(document.AbsoluteExpiresAtUtc),
        document.RevokedAtUtc is { } revoked ? AsUtc(revoked) : null,
        document.RevocationReason is { } reason ? (SessionRevocationReason)reason : null);

    public static RefreshTokenDocument ToDocument(RefreshToken token, DateTime absoluteExpiresAtUtc) => new()
    {
        Id = token.Id,
        SessionId = token.SessionId,
        TokenHash = token.TokenHash.Value,
        IssuedAtUtc = token.IssuedAtUtc,
        ConsumedAtUtc = token.ConsumedAtUtc,
        AbsoluteExpiresAtUtc = absoluteExpiresAtUtc,
    };

    public static RefreshToken ToDomain(RefreshTokenDocument document) => RefreshToken.Restore(
        document.Id,
        document.SessionId,
        RefreshTokenHash.Create(document.TokenHash),
        AsUtc(document.IssuedAtUtc),
        document.ConsumedAtUtc is { } consumed ? AsUtc(consumed) : null);

    // El driver devuelve Utc con [BsonDateTimeOptions]; esto protege además a los documentos construidos en memoria.
    private static DateTime AsUtc(DateTime value)
        => value.Kind == DateTimeKind.Utc ? value : DateTime.SpecifyKind(value, DateTimeKind.Utc);
}
