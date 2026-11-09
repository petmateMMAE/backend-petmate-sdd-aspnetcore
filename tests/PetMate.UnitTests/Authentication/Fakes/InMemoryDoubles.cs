using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using PetMate.Application.Authentication;
using PetMate.Application.Persistence;
using PetMate.Application.Security;
using PetMate.Domain.Pets;
using PetMate.Domain.Sessions;
using PetMate.Domain.Users;

namespace PetMate.TestDoubles;

/// <summary>Reloj controlable para pruebas deterministas.</summary>
public sealed class ManualClock(DateTimeOffset start) : TimeProvider
{
    private readonly object gate = new();
    private DateTimeOffset now = start;

    public override DateTimeOffset GetUtcNow()
    {
        lock (gate)
        {
            return now;
        }
    }

    public void Advance(TimeSpan delta)
    {
        lock (gate)
        {
            now += delta;
        }
    }
}

/// <summary>Repositorio de mascotas en memoria; como Atlas, cada operación filtra por propietario y el borrado es físico.</summary>
public sealed class InMemoryPetRepository : IPetRepository
{
    private readonly ConcurrentDictionary<Guid, Pet> pets = new();

    public IReadOnlyCollection<Pet> All => pets.Values.ToArray();

    public Task Add(Pet pet, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(pet);
        pets[pet.Id] = pet;
        return Task.CompletedTask;
    }

    public Task<Pet?> FindOwned(Guid petId, Guid ownerId, CancellationToken cancellationToken)
        => Task.FromResult(pets.TryGetValue(petId, out var pet) && pet.OwnerId == ownerId ? pet : null);

    public Task<IReadOnlyList<Pet>> ListByOwner(Guid ownerId, int skip, int take, CancellationToken cancellationToken)
        => Task.FromResult<IReadOnlyList<Pet>>(pets.Values.Where(p => p.OwnerId == ownerId)
            .OrderByDescending(p => p.CreatedAtUtc).ThenByDescending(p => p.Id).Skip(skip).Take(take).ToArray());

    public Task<long> CountByOwner(Guid ownerId, CancellationToken cancellationToken)
        => Task.FromResult((long)pets.Values.Count(p => p.OwnerId == ownerId));

    public Task<bool> UpdateOwned(Pet pet, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(pet);
        return Task.FromResult(pets.TryGetValue(pet.Id, out var stored) && stored.OwnerId == pet.OwnerId);
    }

    public Task<bool> DeleteOwned(Guid petId, Guid ownerId, CancellationToken cancellationToken)
        => Task.FromResult(pets.TryGetValue(petId, out var pet) && pet.OwnerId == ownerId && pets.TryRemove(petId, out _));
}

/// <summary>Repositorio de usuarios en memoria con unicidad atómica de correo (equivale al índice único de Atlas).</summary>
public sealed class InMemoryUserRepository : IUserRepository
{
    private readonly ConcurrentDictionary<string, User> byEmail = new();
    private readonly ConcurrentDictionary<Guid, User> byId = new();

    public IReadOnlyCollection<User> All => [.. byId.Values];

    public Task<bool> TryAdd(User user, CancellationToken cancellationToken)
    {
        if (!byEmail.TryAdd(user.Email.Value, user))
        {
            return Task.FromResult(false);
        }

        byId[user.Id] = user;
        return Task.FromResult(true);
    }

    public Task<User?> FindByEmail(Email email, CancellationToken cancellationToken)
        => Task.FromResult(byEmail.TryGetValue(email.Value, out var user) ? user : null);

    public Task<User?> FindById(Guid id, CancellationToken cancellationToken)
        => Task.FromResult(byId.TryGetValue(id, out var user) ? user : null);

    public Task UpdatePasswordHash(User user, CancellationToken cancellationToken) => Task.CompletedTask;

    /// <summary>El usuario en memoria es la misma instancia: el cambio ya quedó aplicado.</summary>
    public Task UpdateProfile(User user, CancellationToken cancellationToken) => Task.CompletedTask;
}

/// <summary>Repositorio de sesiones en memoria; <c>TryConsumeToken</c> es atómico (lock), como el update condicional de Atlas.</summary>
public sealed class InMemorySessionRepository : ISessionRepository
{
    private readonly object gate = new();
    private readonly Dictionary<Guid, RefreshSession> sessions = [];
    private readonly Dictionary<string, RefreshToken> tokensByHash = [];
    private readonly Dictionary<Guid, RefreshToken> tokensById = [];

    public int TokenCount
    {
        get
        {
            lock (gate)
            {
                return tokensById.Count;
            }
        }
    }

    public Task AddSession(RefreshSession session, RefreshToken firstToken, CancellationToken cancellationToken)
    {
        lock (gate)
        {
            sessions[session.Id] = Copy(session);
            Store(firstToken);
        }

        return Task.CompletedTask;
    }

    public Task AddToken(RefreshToken token, DateTime sessionAbsoluteExpiresAtUtc, CancellationToken cancellationToken)
    {
        lock (gate)
        {
            Store(token);
        }

        return Task.CompletedTask;
    }

    public Task<RefreshToken?> FindTokenByHash(RefreshTokenHash tokenHash, CancellationToken cancellationToken)
    {
        lock (gate)
        {
            return Task.FromResult(tokensByHash.TryGetValue(tokenHash.Value, out var token) ? token : null);
        }
    }

    public Task<bool> TryConsumeToken(Guid tokenId, DateTime consumedAtUtc, CancellationToken cancellationToken)
    {
        lock (gate)
        {
            if (!tokensById.TryGetValue(tokenId, out var token) || token.IsConsumed)
            {
                return Task.FromResult(false);
            }

            token.Consume(consumedAtUtc);
            return Task.FromResult(true);
        }
    }

    public Task<RefreshSession?> FindSession(Guid sessionId, CancellationToken cancellationToken)
    {
        lock (gate)
        {
            return Task.FromResult(sessions.TryGetValue(sessionId, out var session) ? Copy(session) : null);
        }
    }

    public Task<bool> UpdateActivity(RefreshSession session, CancellationToken cancellationToken)
    {
        lock (gate)
        {
            if (sessions.TryGetValue(session.Id, out var stored) && !stored.IsRevoked)
            {
                sessions[session.Id] = Copy(session);
                return Task.FromResult(true);
            }
        }

        return Task.FromResult(false);
    }

    public Task Revoke(RefreshSession session, CancellationToken cancellationToken)
    {
        lock (gate)
        {
            if (sessions.TryGetValue(session.Id, out var stored) && !stored.IsRevoked)
            {
                sessions[session.Id] = Copy(session);
            }
        }

        return Task.CompletedTask;
    }

    // Como Atlas, cada lectura devuelve una copia independiente: dos solicitudes concurrentes nunca comparten la misma instancia
    // (compartirla hacía que un perdedor revocara el objeto del ganador y la prueba de concurrencia fallara de forma intermitente).
    private static RefreshSession Copy(RefreshSession session)
        => RefreshSession.Restore(
            session.Id,
            session.UserId,
            session.CreatedAtUtc,
            session.LastUsedAtUtc,
            session.IdleExpiresAtUtc,
            session.AbsoluteExpiresAtUtc,
            session.RevokedAtUtc,
            session.RevocationReason);

    public bool IsSessionRevoked(Guid sessionId)
    {
        lock (gate)
        {
            return sessions[sessionId].IsRevoked;
        }
    }

    public IReadOnlyList<RefreshSession> Sessions()
    {
        lock (gate)
        {
            return [.. sessions.Values];
        }
    }

    private void Store(RefreshToken token)
    {
        tokensByHash[token.TokenHash.Value] = token;
        tokensById[token.Id] = token;
    }
}


/// <summary>Lista negra en memoria con vencimiento por reloj.</summary>
public sealed class InMemoryRevocationStore(TimeProvider clock) : ITokenRevocationStore
{
    private readonly ConcurrentDictionary<string, DateTime> revoked = new();

    public bool Fail { get; set; }

    public Task Revoke(string jti, DateTime expiresAtUtc, CancellationToken cancellationToken)
    {
        ThrowIfFailing();
        if (expiresAtUtc > clock.GetUtcNow().UtcDateTime)
        {
            revoked[jti] = expiresAtUtc;
        }

        return Task.CompletedTask;
    }

    public Task<bool> IsRevoked(string jti, CancellationToken cancellationToken)
    {
        ThrowIfFailing();
        return Task.FromResult(revoked.TryGetValue(jti, out var expires) && expires > clock.GetUtcNow().UtcDateTime);
    }

    private readonly ConcurrentDictionary<Guid, DateTime> revokedSessions = new();

    public Task RevokeSession(Guid sessionId, DateTime untilUtc, CancellationToken cancellationToken)
    {
        ThrowIfFailing();
        if (untilUtc > clock.GetUtcNow().UtcDateTime)
        {
            revokedSessions[sessionId] = untilUtc;
        }

        return Task.CompletedTask;
    }

    public Task<bool> IsSessionRevoked(Guid sessionId, CancellationToken cancellationToken)
    {
        ThrowIfFailing();
        return Task.FromResult(revokedSessions.TryGetValue(sessionId, out var until) && until > clock.GetUtcNow().UtcDateTime);
    }

    public bool ContainsSession(Guid sessionId) => revokedSessions.ContainsKey(sessionId);

    public bool Contains(string jti) => revoked.ContainsKey(jti);

    private void ThrowIfFailing()
    {
        if (Fail)
        {
            throw new AuthDependencyUnavailableException(new InvalidOperationException("detalle-interno"));
        }
    }
}

/// <summary>Hasher determinista y rápido para pruebas de handlers (no es seguro; solo para dobles).</summary>
public sealed class FakeHasher : IPasswordHasher
{
    private int hashCalls;
    private int verifyCalls;

    public int HashCalls => Volatile.Read(ref hashCalls);

    public int VerifyCalls => Volatile.Read(ref verifyCalls);

    /// <summary>Si es verdadero, <c>Verify</c> reporta que el hash necesita regenerarse.</summary>
    public bool RehashNeeded { get; set; }

    public string Hash(string password)
    {
        Interlocked.Increment(ref hashCalls);
        return "$argon2id$v=19$m=19456,t=2,p=1$fake$" + Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(password)));
    }

    public PasswordVerificationResult Verify(string password, string passwordHash)
    {
        Interlocked.Increment(ref verifyCalls);
        if (!passwordHash.EndsWith(Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(password))), StringComparison.Ordinal))
        {
            return PasswordVerificationResult.Failed;
        }

        return RehashNeeded ? PasswordVerificationResult.SuccessRehashNeeded : PasswordVerificationResult.Success;
    }
}

/// <summary>Emisor de access tokens falso: el "token" es el jti; suficiente para probar handlers.</summary>
public sealed class FakeAccessTokenService(TimeProvider clock) : IAccessTokenService
{
    public TimeSpan Lifetime => TimeSpan.FromMinutes(15);

    public AccessToken Issue(Guid userId, UserRole role, Guid? sessionId = null)
    {
        var jti = Guid.NewGuid().ToString("N");
        return new AccessToken($"fake.{userId:N}.{role}.{jti}", jti, clock.GetUtcNow().UtcDateTime.AddMinutes(15));
    }

    public Task<AccessTokenClaims?> Validate(string token, CancellationToken cancellationToken)
        => Task.FromResult<AccessTokenClaims?>(null);
}

/// <summary>Fábrica de refresh tokens determinista (contador) con hash SHA-256 real.</summary>
public sealed class FakeRefreshTokenFactory : IRefreshTokenFactory
{
    private int counter;

    public RefreshTokenMaterial Create()
    {
        var raw = Interlocked.Increment(ref counter).ToString("D43", System.Globalization.CultureInfo.InvariantCulture);
        return new RefreshTokenMaterial(raw, HashOf(raw));
    }

    public bool TryHash(string rawToken, out RefreshTokenHash? hash)
    {
        hash = null;
        if (rawToken is not { Length: 43 } || !rawToken.All(char.IsAsciiLetterOrDigit))
        {
            return false;
        }

        hash = HashOf(rawToken);
        return true;
    }

    private static RefreshTokenHash HashOf(string raw)
        => RefreshTokenHash.Create(Convert.ToHexString(SHA256.HashData(Encoding.ASCII.GetBytes(raw))));
}

/// <summary>Cuota y bloqueo por sujeto en memoria, con el reloj controlado (equivalente a la implementación Redis).</summary>
public sealed class InMemoryAuthAttemptThrottle(TimeProvider clock) : IAuthAttemptThrottle
{
    private readonly object gate = new();
    private readonly Dictionary<string, (int Count, DateTime ExpiresAt)> counters = [];
    private readonly Dictionary<string, DateTime> locks = [];

    public bool Fail { get; set; }

    public Task<TimeSpan?> GetRemainingLock(string scope, string subject, CancellationToken cancellationToken)
    {
        ThrowIfFailing();
        lock (gate)
        {
            var now = clock.GetUtcNow().UtcDateTime;
            return Task.FromResult<TimeSpan?>(locks.TryGetValue(scope + ":" + subject, out var until) && until > now ? until - now : null);
        }
    }

    public Task<TimeSpan?> RecordAttempt(string scope, string subject, AuthThrottleRule rule, CancellationToken cancellationToken)
    {
        ThrowIfFailing();
        lock (gate)
        {
            var now = clock.GetUtcNow().UtcDateTime;
            var key = scope + ":" + subject;
            if (locks.TryGetValue(key, out var lockedUntil) && lockedUntil > now)
            {
                // Con un bloqueo activo no se cuenta: igual que el script de Redis.
                return Task.FromResult<TimeSpan?>(lockedUntil - now);
            }

            var current = counters.TryGetValue(key, out var entry) && entry.ExpiresAt > now ? entry : (0, now + rule.Window);
            var count = current.Item1 + 1;
            if (count > rule.MaxAttempts)
            {
                counters.Remove(key);
                locks[key] = now + rule.LockDuration;
                return Task.FromResult<TimeSpan?>(rule.LockDuration);
            }

            counters[key] = (count, current.Item2);
            return Task.FromResult<TimeSpan?>(null);
        }
    }

    public Task Clear(string scope, string subject, CancellationToken cancellationToken)
    {
        ThrowIfFailing();
        lock (gate)
        {
            counters.Remove(scope + ":" + subject);
        }

        return Task.CompletedTask;
    }

    private void ThrowIfFailing()
    {
        if (Fail)
        {
            throw new AuthDependencyUnavailableException(new InvalidOperationException("Redis simulado no disponible."));
        }
    }
}

/// <summary>Apilado de handlers sobre dobles en memoria.</summary>
public sealed class AuthTestHarness
{
    public AuthTestHarness()
    {
        Clock = new ManualClock(new DateTimeOffset(2026, 11, 3, 12, 0, 0, TimeSpan.Zero));
        Users = new InMemoryUserRepository();
        Sessions = new InMemorySessionRepository();
        Revocations = new InMemoryRevocationStore(Clock);
        Throttle = new InMemoryAuthAttemptThrottle(Clock);
        Hasher = new FakeHasher();
        Tokens = new FakeRefreshTokenFactory();
        Access = new FakeAccessTokenService(Clock);
        var issuer = new AuthTokenIssuer(Sessions, Tokens, Access, SessionPolicy.Default);
        Register = new RegisterHandler(Users, Hasher, issuer, Clock);
        Login = new LoginHandler(Users, Hasher, issuer, Clock, Throttle);
        Refresh = new RefreshHandler(Users, Sessions, Tokens, Access, SessionPolicy.Default, Clock, Throttle, Revocations);
        Logout = new LogoutHandler(Sessions, Revocations, Tokens, Access, Clock);
    }

    public ManualClock Clock { get; }

    public InMemoryAuthAttemptThrottle Throttle { get; }

    public InMemoryUserRepository Users { get; }

    public InMemorySessionRepository Sessions { get; }

    public InMemoryRevocationStore Revocations { get; }

    public FakeHasher Hasher { get; }

    public FakeRefreshTokenFactory Tokens { get; }

    public FakeAccessTokenService Access { get; }

    public RegisterHandler Register { get; }

    public LoginHandler Login { get; }

    public RefreshHandler Refresh { get; }

    public LogoutHandler Logout { get; }
}
