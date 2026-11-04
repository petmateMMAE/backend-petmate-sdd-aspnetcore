using PetMate.Domain.Sessions;

namespace PetMate.Application.Persistence;

/// <summary>Persistencia de sesiones refresh y sus tokens (solo hashes). Las operaciones críticas son atómicas por documento.</summary>
public interface ISessionRepository
{
    /// <summary>Guarda la sesión y su primer token.</summary>
    Task AddSession(RefreshSession session, RefreshToken firstToken, CancellationToken cancellationToken);

    /// <summary>Guarda el token que sustituye a uno consumido; hereda el vencimiento absoluto de la sesión.</summary>
    Task AddToken(RefreshToken token, DateTime sessionAbsoluteExpiresAtUtc, CancellationToken cancellationToken);

    Task<RefreshToken?> FindTokenByHash(RefreshTokenHash tokenHash, CancellationToken cancellationToken);

    /// <summary>
    /// Marca el token como consumido solo si seguía sin consumir. Devuelve <c>true</c> a un único llamador
    /// aunque haya concurrencia; <c>false</c> indica que ya estaba consumido (posible replay).
    /// </summary>
    Task<bool> TryConsumeToken(Guid tokenId, DateTime consumedAtUtc, CancellationToken cancellationToken);

    Task<RefreshSession?> FindSession(Guid sessionId, CancellationToken cancellationToken);

    /// <summary>Persiste la actividad (último uso y vencimiento por inactividad) de una sesión no revocada.</summary>
    /// <returns><c>true</c> si la sesión existía y no estaba revocada (la actividad se aplicó); <c>false</c> si ya estaba revocada.</returns>
    Task<bool> UpdateActivity(RefreshSession session, CancellationToken cancellationToken);

    /// <summary>Persiste la revocación si la sesión aún no estaba revocada (la primera revocación prevalece).</summary>
    Task Revoke(RefreshSession session, CancellationToken cancellationToken);
}
