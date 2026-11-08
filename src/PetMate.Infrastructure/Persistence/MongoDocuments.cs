using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PetMate.Infrastructure.Persistence;

/// <summary>Nombres de colección. El prefijo existe solo para que las pruebas de integración usen colecciones propias.</summary>
public sealed record MongoCollectionNames(string Prefix = "")
{
    public string Users => Prefix + "users";

    public string Sessions => Prefix + "refresh_sessions";

    public string Tokens => Prefix + "refresh_tokens";

    public string Pets => Prefix + "pets";
}

// Documentos de persistencia: viven en Infrastructure y mantienen Domain libre de atributos Bson.
// Guid en representación estándar (UUID) y fechas siempre en UTC.
internal sealed class UserDocument
{
    [BsonId]
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid Id { get; set; }

    [BsonElement("email")]
    public string Email { get; set; } = string.Empty;

    [BsonElement("passwordHash")]
    public string PasswordHash { get; set; } = string.Empty;

    [BsonElement("role")]
    public int Role { get; set; }

    /// <summary>Nombre visible (pm-038). Ausente en las cuentas que nunca lo han definido.</summary>
    [BsonElement("displayName")]
    [BsonIgnoreIfNull]
    public string? DisplayName { get; set; }

    [BsonElement("createdAtUtc")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime CreatedAtUtc { get; set; }

    [BsonElement("updatedAtUtc")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime UpdatedAtUtc { get; set; }
}

internal sealed class RefreshSessionDocument
{
    [BsonId]
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid Id { get; set; }

    [BsonElement("userId")]
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid UserId { get; set; }

    [BsonElement("createdAtUtc")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime CreatedAtUtc { get; set; }

    [BsonElement("lastUsedAtUtc")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime LastUsedAtUtc { get; set; }

    [BsonElement("idleExpiresAtUtc")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime IdleExpiresAtUtc { get; set; }

    [BsonElement("absoluteExpiresAtUtc")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime AbsoluteExpiresAtUtc { get; set; }

    [BsonElement("revokedAtUtc")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime? RevokedAtUtc { get; set; }

    [BsonElement("revocationReason")]
    public int? RevocationReason { get; set; }
}

internal sealed class RefreshTokenDocument
{
    [BsonId]
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid Id { get; set; }

    [BsonElement("sessionId")]
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid SessionId { get; set; }

    [BsonElement("tokenHash")]
    public string TokenHash { get; set; } = string.Empty;

    [BsonElement("issuedAtUtc")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime IssuedAtUtc { get; set; }

    [BsonElement("consumedAtUtc")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime? ConsumedAtUtc { get; set; }

    /// <summary>Copia del límite absoluto de la sesión; la usa el índice TTL.</summary>
    [BsonElement("absoluteExpiresAtUtc")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime AbsoluteExpiresAtUtc { get; set; }
}

/// <summary>Mascota (pm-039). El borrado es físico: no hay campo de estado.</summary>
internal sealed class PetDocument
{
    [BsonId]
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid Id { get; set; }

    [BsonElement("ownerId")]
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid OwnerId { get; set; }

    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    [BsonElement("species")]
    public string Species { get; set; } = string.Empty;

    [BsonElement("breed")]
    [BsonIgnoreIfNull]
    public string? Breed { get; set; }

    /// <summary>Fecha de nacimiento como fecha de calendario (sin hora ni zona), formato ISO yyyy-MM-dd.</summary>
    [BsonElement("birthDate")]
    [BsonIgnoreIfNull]
    public string? BirthDate { get; set; }

    [BsonElement("sex")]
    public int Sex { get; set; }

    [BsonElement("photoUrl")]
    [BsonIgnoreIfNull]
    public string? PhotoUrl { get; set; }

    [BsonElement("createdAtUtc")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime CreatedAtUtc { get; set; }

    [BsonElement("updatedAtUtc")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime UpdatedAtUtc { get; set; }
}
