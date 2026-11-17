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

    public Task<Pet?> FindByQr(string tokenOrCode, CancellationToken cancellationToken)
        => Task.FromResult(pets.Values.FirstOrDefault(p => p.Qr.Token == tokenOrCode || string.Equals(p.Qr.Code, tokenOrCode, StringComparison.OrdinalIgnoreCase)));

    public Task<bool> ReplaceQrOwned(Pet pet, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(pet);
        return Task.FromResult(pets.TryGetValue(pet.Id, out var stored) && stored.OwnerId == pet.OwnerId);
    }

    public Task<bool> DeleteOwned(Guid petId, Guid ownerId, CancellationToken cancellationToken)
        => Task.FromResult(pets.TryGetValue(petId, out var pet) && pet.OwnerId == ownerId && pets.TryRemove(petId, out _));
}

/// <summary>Repositorio en memoria de registros del carnet (equivale al filtro por dueño de Atlas).</summary>
public class InMemoryHealthRecordRepository<TRecord>(Func<IEnumerable<TRecord>, IEnumerable<TRecord>> order)
    where TRecord : PetMate.Domain.Carnet.HealthRecord
{
    private readonly ConcurrentDictionary<Guid, TRecord> records = new();

    public IReadOnlyCollection<TRecord> All => records.Values.ToArray();

    public Task Add(TRecord record, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(record);
        records[record.Id] = record;
        return Task.CompletedTask;
    }

    public Task<TRecord?> FindOwned(Guid recordId, Guid ownerId, CancellationToken cancellationToken)
        => Task.FromResult(records.TryGetValue(recordId, out var record) && record.OwnerId == ownerId ? record : null);

    public Task<IReadOnlyList<TRecord>> ListByPet(Guid petId, Guid ownerId, int skip, int take, CancellationToken cancellationToken)
        => Task.FromResult<IReadOnlyList<TRecord>>(order(records.Values.Where(r => r.PetId == petId && r.OwnerId == ownerId)).Skip(skip).Take(take).ToArray());

    public Task<long> CountByPet(Guid petId, Guid ownerId, CancellationToken cancellationToken)
        => Task.FromResult((long)records.Values.Count(r => r.PetId == petId && r.OwnerId == ownerId));

    public Task<bool> DeleteOwned(Guid recordId, Guid ownerId, CancellationToken cancellationToken)
        => Task.FromResult(records.TryGetValue(recordId, out var record) && record.OwnerId == ownerId && records.TryRemove(recordId, out _));

    public Task<long> DeleteByPet(Guid petId, Guid ownerId, CancellationToken cancellationToken)
    {
        var ids = records.Values.Where(r => r.PetId == petId && r.OwnerId == ownerId).Select(r => r.Id).ToArray();
        return Task.FromResult((long)ids.Count(id => records.TryRemove(id, out _)));
    }

    public Task<bool> ReplaceOwned(TRecord record, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(record);
        return Task.FromResult(record.CanOwnerModify && records.TryGetValue(record.Id, out var stored) && stored.OwnerId == record.OwnerId);
    }
}

public sealed class InMemoryVaccineRepository()
    : InMemoryHealthRecordRepository<PetMate.Domain.Carnet.Vaccine>(items => items.OrderByDescending(v => v.AppliedOn).ThenByDescending(v => v.Id)), PetMate.Application.Carnet.IVaccineRepository;

public sealed class InMemoryDewormingRepository()
    : InMemoryHealthRecordRepository<PetMate.Domain.Carnet.Deworming>(items => items.OrderByDescending(d => d.AppliedOn).ThenByDescending(d => d.Id)), PetMate.Application.Carnet.IDewormingRepository;

public sealed class InMemoryWeightRepository()
    : InMemoryHealthRecordRepository<PetMate.Domain.Carnet.WeightEntry>(items => items.OrderByDescending(w => w.MeasuredOn).ThenByDescending(w => w.Id)), PetMate.Application.Carnet.IWeightRepository;

public sealed class InMemoryAllergyRepository()
    : InMemoryHealthRecordRepository<PetMate.Domain.Carnet.Allergy>(items => items.OrderBy(a => a.Status).ThenByDescending(a => a.CreatedAtUtc).ThenByDescending(a => a.Id)), PetMate.Application.Carnet.IAllergyRepository
{
    public Task<bool> UpdateOwned(PetMate.Domain.Carnet.Allergy allergy, CancellationToken cancellationToken) => ReplaceOwned(allergy, cancellationToken);
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
/// <summary>Consultas en memoria.</summary>
public sealed class InMemoryConsultationRepository()
    : InMemoryHealthRecordRepository<PetMate.Domain.Carnet.Consultation>(items => items.OrderByDescending(c => c.ConsultedOn).ThenByDescending(c => c.Id)), PetMate.Application.Access.IConsultationRepository
{
    public Task<PetMate.Domain.Carnet.Consultation?> FindByIdForBusiness(Guid consultationId, Guid businessId, CancellationToken cancellationToken)
        => Task.FromResult(All.FirstOrDefault(c => c.Id == consultationId && c.Provenance.BusinessId == businessId));

    public Task<bool> ReplaceForBusiness(PetMate.Domain.Carnet.Consultation consultation, Guid businessId, CancellationToken cancellationToken)
        => Task.FromResult(consultation.Provenance.BusinessId == businessId && All.Any(c => c.Id == consultation.Id));
}

/// <summary>Seguimientos en memoria con unicidad por consulta.</summary>
public sealed class InMemoryFollowupRepository : PetMate.Application.Followups.IFollowupRepository
{
    private readonly List<PetMate.Domain.Followups.Followup> items = [];

    public IReadOnlyCollection<PetMate.Domain.Followups.Followup> All
    {
        get
        {
            lock (items)
            {
                return [.. items];
            }
        }
    }

    public Task<bool> TryAdd(PetMate.Domain.Followups.Followup followup, CancellationToken cancellationToken)
    {
        lock (items)
        {
            if (items.Any(f => f.ConsultationId == followup.ConsultationId))
            {
                return Task.FromResult(false);
            }

            items.Add(followup);
            return Task.FromResult(true);
        }
    }

    public Task<PetMate.Domain.Followups.Followup?> FindForBusiness(Guid followupId, Guid businessId, CancellationToken cancellationToken)
        => Task.FromResult(All.FirstOrDefault(f => f.Id == followupId && f.BusinessId == businessId));

    public Task<PetMate.Domain.Followups.Followup?> FindByConsultation(Guid consultationId, Guid ownerId, CancellationToken cancellationToken)
        => Task.FromResult(All.FirstOrDefault(f => f.ConsultationId == consultationId && f.OwnerId == ownerId));

    public Task<PetMate.Domain.Followups.Followup?> FindById(Guid followupId, CancellationToken cancellationToken)
        => Task.FromResult(All.FirstOrDefault(f => f.Id == followupId));

    public Task<bool> Update(PetMate.Domain.Followups.Followup followup, CancellationToken cancellationToken)
        => Task.FromResult(All.Any(f => f.Id == followup.Id && f.BusinessId == followup.BusinessId));

    private IEnumerable<PetMate.Domain.Followups.Followup> Filter(Guid businessId, DateOnly? reviewOn, bool onlyActive)
        => All.Where(f => f.BusinessId == businessId && (reviewOn is null || f.NextReviewOn == reviewOn) && (!onlyActive || f.Status != PetMate.Domain.Followups.FollowupStatus.Discharged));

    public Task<IReadOnlyList<PetMate.Domain.Followups.Followup>> ListByBusiness(Guid businessId, DateOnly? reviewOn, bool onlyActive, int skip, int take, CancellationToken cancellationToken)
        => Task.FromResult<IReadOnlyList<PetMate.Domain.Followups.Followup>>(Filter(businessId, reviewOn, onlyActive).OrderByDescending(f => f.UpdatedAtUtc).ThenByDescending(f => f.Id).Skip(skip).Take(take).ToArray());

    public Task<long> CountByBusiness(Guid businessId, DateOnly? reviewOn, bool onlyActive, CancellationToken cancellationToken)
        => Task.FromResult((long)Filter(businessId, reviewOn, onlyActive).Count());

    public Task<long> DeleteByPet(Guid petId, CancellationToken cancellationToken)
    {
        lock (items)
        {
            return Task.FromResult((long)items.RemoveAll(f => f.PetId == petId));
        }
    }
}

/// <summary>Conversaciones en memoria con secuencia atómica.</summary>
public sealed class InMemoryConversationRepository : PetMate.Application.Followups.IConversationRepository
{
    private readonly object gate = new();
    private readonly List<ConversationState> items = [];

    private sealed class ConversationState(PetMate.Domain.Chat.Conversation initial)
    {
        public PetMate.Domain.Chat.Conversation Value { get; set; } = initial;

        public long LastSeq { get; set; }

        public long OwnerRead { get; set; }

        public long BusinessRead { get; set; }

        public bool Closed { get; set; }

        public DateTime UpdatedAt { get; set; } = initial.UpdatedAtUtc;
    }

    private static PetMate.Domain.Chat.Conversation Snapshot(ConversationState s)
        => PetMate.Domain.Chat.Conversation.Restore(s.Value.Id, s.Value.FollowupId, s.Value.PetId, s.Value.OwnerAccountId, s.Value.BusinessId, s.Value.BusinessAccountId, s.Closed, s.LastSeq, s.OwnerRead, s.BusinessRead, s.Value.CreatedAtUtc, s.UpdatedAt);

    public Task<bool> TryAdd(PetMate.Domain.Chat.Conversation conversation, CancellationToken cancellationToken)
    {
        lock (gate)
        {
            if (items.Any(s => s.Value.FollowupId == conversation.FollowupId))
            {
                return Task.FromResult(false);
            }

            items.Add(new ConversationState(conversation));
            return Task.FromResult(true);
        }
    }

    public Task<PetMate.Domain.Chat.Conversation?> FindById(Guid conversationId, CancellationToken cancellationToken)
    {
        lock (gate)
        {
            var state = items.FirstOrDefault(s => s.Value.Id == conversationId);
            return Task.FromResult(state is null ? null : Snapshot(state));
        }
    }

    public Task<PetMate.Domain.Chat.Conversation?> FindByFollowup(Guid followupId, CancellationToken cancellationToken)
    {
        lock (gate)
        {
            var state = items.FirstOrDefault(s => s.Value.FollowupId == followupId);
            return Task.FromResult(state is null ? null : Snapshot(state));
        }
    }

    public Task<IReadOnlyList<PetMate.Domain.Chat.Conversation>> ListForAccount(Guid accountId, int skip, int take, CancellationToken cancellationToken)
    {
        lock (gate)
        {
            return Task.FromResult<IReadOnlyList<PetMate.Domain.Chat.Conversation>>(items.Where(s => s.Value.HasParticipant(accountId)).OrderByDescending(s => s.UpdatedAt).ThenByDescending(s => s.Value.Id).Skip(skip).Take(take).Select(Snapshot).ToArray());
        }
    }

    public Task<long> CountForAccount(Guid accountId, CancellationToken cancellationToken)
    {
        lock (gate)
        {
            return Task.FromResult((long)items.Count(s => s.Value.HasParticipant(accountId)));
        }
    }

    public Task<long> NextSeq(Guid conversationId, DateTime nowUtc, CancellationToken cancellationToken)
    {
        lock (gate)
        {
            var state = items.First(s => s.Value.Id == conversationId);
            state.LastSeq++;
            state.UpdatedAt = nowUtc;
            return Task.FromResult(state.LastSeq);
        }
    }

    public Task MarkRead(Guid conversationId, bool businessSide, long upToSeq, CancellationToken cancellationToken)
    {
        lock (gate)
        {
            var state = items.First(s => s.Value.Id == conversationId);
            if (businessSide)
            {
                state.BusinessRead = Math.Max(state.BusinessRead, upToSeq);
            }
            else
            {
                state.OwnerRead = Math.Max(state.OwnerRead, upToSeq);
            }
        }

        return Task.CompletedTask;
    }

    public Task SetClosed(Guid conversationId, bool closed, DateTime nowUtc, CancellationToken cancellationToken)
    {
        lock (gate)
        {
            var state = items.First(s => s.Value.Id == conversationId);
            state.Closed = closed;
            state.UpdatedAt = nowUtc;
        }

        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<Guid>> IdsByPet(Guid petId, CancellationToken cancellationToken)
    {
        lock (gate)
        {
            return Task.FromResult<IReadOnlyList<Guid>>(items.Where(s => s.Value.PetId == petId).Select(s => s.Value.Id).ToArray());
        }
    }

    public Task<long> DeleteByPet(Guid petId, CancellationToken cancellationToken)
    {
        lock (gate)
        {
            return Task.FromResult((long)items.RemoveAll(s => s.Value.PetId == petId));
        }
    }

    public int Count
    {
        get
        {
            lock (gate)
            {
                return items.Count;
            }
        }
    }
}

/// <summary>Mensajes en memoria.</summary>
public sealed class InMemoryMessageRepository : PetMate.Application.Followups.IMessageRepository
{
    private readonly List<PetMate.Domain.Chat.Message> items = [];

    public IReadOnlyCollection<PetMate.Domain.Chat.Message> All
    {
        get
        {
            lock (items)
            {
                return [.. items];
            }
        }
    }

    public Task Add(PetMate.Domain.Chat.Message message, CancellationToken cancellationToken)
    {
        lock (items)
        {
            items.Add(message);
        }

        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<PetMate.Domain.Chat.Message>> List(Guid conversationId, long afterSeq, int take, CancellationToken cancellationToken)
        => Task.FromResult<IReadOnlyList<PetMate.Domain.Chat.Message>>(All.Where(m => m.ConversationId == conversationId && m.Seq > afterSeq).OrderBy(m => m.Seq).Take(take).ToArray());

    public Task<PetMate.Domain.Chat.Message?> Find(Guid conversationId, Guid messageId, CancellationToken cancellationToken)
        => Task.FromResult(All.FirstOrDefault(m => m.ConversationId == conversationId && m.Id == messageId));

    public Task<PetMate.Domain.Chat.Message?> Last(Guid conversationId, CancellationToken cancellationToken)
        => Task.FromResult(All.Where(m => m.ConversationId == conversationId).OrderByDescending(m => m.Seq).FirstOrDefault());

    public Task<long> CountUnread(Guid conversationId, long afterSeq, Guid exceptSender, CancellationToken cancellationToken)
        => Task.FromResult((long)All.Count(m => m.ConversationId == conversationId && m.Seq > afterSeq && m.SenderAccountId != exceptSender));

    public Task<long> DeleteByConversations(IReadOnlyCollection<Guid> conversationIds, CancellationToken cancellationToken)
    {
        lock (items)
        {
            return Task.FromResult((long)items.RemoveAll(m => conversationIds.Contains(m.ConversationId)));
        }
    }
}

/// <summary>Autorizaciones de acceso en memoria.</summary>
public sealed class InMemoryAccessGrantRepository : PetMate.Application.Access.IAccessGrantRepository
{
    private readonly List<PetMate.Domain.Access.AccessGrant> grants = [];

    public IReadOnlyCollection<PetMate.Domain.Access.AccessGrant> All
    {
        get
        {
            lock (grants)
            {
                return [.. grants];
            }
        }
    }

    public Task Add(PetMate.Domain.Access.AccessGrant grant, CancellationToken cancellationToken)
    {
        lock (grants)
        {
            grants.Add(grant);
        }

        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<PetMate.Domain.Access.AccessGrant>> FindActive(Guid petId, Guid businessId, DateTime nowUtc, CancellationToken cancellationToken)
    {
        lock (grants)
        {
            return Task.FromResult<IReadOnlyList<PetMate.Domain.Access.AccessGrant>>(grants.Where(g => g.PetId == petId && g.IsActiveAt(nowUtc) && g.Covers(businessId)).ToArray());
        }
    }

    public Task<long> CancelActiveForPet(Guid petId, Guid ownerId, DateTime nowUtc, CancellationToken cancellationToken)
    {
        lock (grants)
        {
            var active = grants.Where(g => g.PetId == petId && g.OwnerId == ownerId && g.IsActiveAt(nowUtc)).ToArray();
            foreach (var grant in active)
            {
                grant.Cancel(nowUtc);
            }

            return Task.FromResult((long)active.Length);
        }
    }

    public Task<long> DeleteByPet(Guid petId, Guid ownerId, CancellationToken cancellationToken)
    {
        lock (grants)
        {
            return Task.FromResult((long)grants.RemoveAll(g => g.PetId == petId && g.OwnerId == ownerId));
        }
    }
}

/// <summary>Registro de escaneos en memoria.</summary>
public sealed class InMemoryScanLogRepository : PetMate.Application.Access.IScanLogRepository
{
    private readonly List<PetMate.Domain.Access.ScanLog> logs = [];

    public IReadOnlyCollection<PetMate.Domain.Access.ScanLog> All
    {
        get
        {
            lock (logs)
            {
                return [.. logs];
            }
        }
    }

    public Task Add(PetMate.Domain.Access.ScanLog log, CancellationToken cancellationToken)
    {
        lock (logs)
        {
            logs.Add(log);
        }

        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<PetMate.Domain.Access.ScanLog>> ListByBusiness(Guid businessId, DateTime? sinceUtc, int skip, int take, CancellationToken cancellationToken)
        => Task.FromResult<IReadOnlyList<PetMate.Domain.Access.ScanLog>>(All.Where(l => l.BusinessId == businessId && (sinceUtc is null || l.AtUtc >= sinceUtc)).OrderByDescending(l => l.AtUtc).ThenByDescending(l => l.Id).Skip(skip).Take(take).ToArray());

    public Task<long> CountByBusiness(Guid businessId, DateTime? sinceUtc, CancellationToken cancellationToken)
        => Task.FromResult((long)All.Count(l => l.BusinessId == businessId && (sinceUtc is null || l.AtUtc >= sinceUtc)));

    public Task<long> DeleteByPet(Guid petId, CancellationToken cancellationToken)
    {
        lock (logs)
        {
            return Task.FromResult((long)logs.RemoveAll(l => l.PetId == petId));
        }
    }
}

/// <summary>Caché en memoria para las pruebas.</summary>
public sealed class InMemoryCacheStore : PetMate.Application.Caching.ICacheStore
{
    private readonly System.Collections.Concurrent.ConcurrentDictionary<string, string> items = new();

    public bool Failing { get; set; }

    public int Count => items.Count;

    public Task<string?> Read(PetMate.Application.Caching.CacheKey key, CancellationToken cancellationToken)
        => Task.FromResult(!Failing && items.TryGetValue(key.Value, out var value) ? value : null);

    public Task<bool> Write(PetMate.Application.Caching.CacheKey key, string value, TimeSpan ttl, CancellationToken cancellationToken)
    {
        if (Failing)
        {
            return Task.FromResult(false);
        }

        items[key.Value] = value;
        return Task.FromResult(true);
    }

    public Task<bool> Remove(PetMate.Application.Caching.CacheKey key, CancellationToken cancellationToken) => Task.FromResult(items.TryRemove(key.Value, out _));
}

/// <summary>Negocios en memoria con unicidad por cuenta (equivale al índice único de Atlas).</summary>
public sealed class InMemoryBusinessRepository : PetMate.Application.Businesses.IBusinessRepository
{
    private readonly ConcurrentDictionary<Guid, PetMate.Domain.Businesses.Business> byOwner = new();

    public IReadOnlyCollection<PetMate.Domain.Businesses.Business> All => byOwner.Values.ToArray();

    public Task<bool> TryAdd(PetMate.Domain.Businesses.Business business, CancellationToken cancellationToken)
        => Task.FromResult(byOwner.TryAdd(business.OwnerId, business));

    public Task<PetMate.Domain.Businesses.Business?> FindByOwner(Guid ownerId, CancellationToken cancellationToken)
        => Task.FromResult(byOwner.TryGetValue(ownerId, out var business) ? business : null);

    public Task<PetMate.Domain.Businesses.Business?> FindById(Guid businessId, CancellationToken cancellationToken)
        => Task.FromResult(byOwner.Values.FirstOrDefault(b => b.Id == businessId));

    public Task<PetMate.Application.Businesses.NearbyPage> FindNearby(PetMate.Application.Businesses.NearbyQuery query, CancellationToken cancellationToken)
    {
        var key = string.IsNullOrEmpty(query.SearchKey) ? null : query.SearchKey;
        var hits = byOwner.Values
            .Where(b => query.Types is null or { Count: 0 } || query.Types.Contains(b.Type))
            .Where(b => key is null || PetMate.Domain.Businesses.Business.SearchKey(b.Name).Contains(key, StringComparison.Ordinal))
            .Select(b => new PetMate.Application.Businesses.NearbyHit(b, Haversine(query.Latitude, query.Longitude, b.Latitude, b.Longitude)))
            .Where(h => h.DistanceMeters <= query.RadiusMeters)
            .OrderBy(h => h.DistanceMeters).ThenBy(h => h.Business.Id)
            .ToArray();
        return Task.FromResult(new PetMate.Application.Businesses.NearbyPage(hits.Skip(query.Skip).Take(query.Take).ToArray(), hits.Length));
    }

    private static double Haversine(double lat1, double lng1, double lat2, double lng2)
    {
        const double R = 6371008.8;
        var dLat = (lat2 - lat1) * Math.PI / 180;
        var dLng = (lng2 - lng1) * Math.PI / 180;
        var a = (Math.Sin(dLat / 2) * Math.Sin(dLat / 2)) + (Math.Cos(lat1 * Math.PI / 180) * Math.Cos(lat2 * Math.PI / 180) * Math.Sin(dLng / 2) * Math.Sin(dLng / 2));
        return R * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    }

    public Task<bool> UpdateOwned(PetMate.Domain.Businesses.Business business, CancellationToken cancellationToken)
        => Task.FromResult(byOwner.TryGetValue(business.OwnerId, out var stored) && stored.Id == business.Id);

    public Task<bool> DeleteByOwner(Guid ownerId, CancellationToken cancellationToken)
        => Task.FromResult(byOwner.TryRemove(ownerId, out _));
}

/// <summary>Catálogo de servicios en memoria.</summary>
public sealed class InMemoryBusinessServiceRepository : PetMate.Application.Businesses.IBusinessServiceRepository
{
    private readonly ConcurrentDictionary<Guid, PetMate.Domain.Businesses.BusinessService> items = new();

    public IReadOnlyCollection<PetMate.Domain.Businesses.BusinessService> All => items.Values.ToArray();

    public Task Add(PetMate.Domain.Businesses.BusinessService service, CancellationToken cancellationToken)
    {
        items[service.Id] = service;
        return Task.CompletedTask;
    }

    public Task<long> CountByBusiness(Guid businessId, CancellationToken cancellationToken)
        => Task.FromResult((long)items.Values.Count(s => s.BusinessId == businessId));

    public Task<IReadOnlyList<PetMate.Domain.Businesses.BusinessService>> ListByBusiness(Guid businessId, int skip, int take, CancellationToken cancellationToken)
        => Task.FromResult<IReadOnlyList<PetMate.Domain.Businesses.BusinessService>>(items.Values.Where(s => s.BusinessId == businessId)
            .OrderBy(s => s.CreatedAtUtc).ThenBy(s => s.Id).Skip(skip).Take(take).ToArray());

    public Task<PetMate.Domain.Businesses.BusinessService?> FindOwned(Guid serviceId, Guid businessId, CancellationToken cancellationToken)
        => Task.FromResult(items.TryGetValue(serviceId, out var s) && s.BusinessId == businessId ? s : null);

    public Task<bool> UpdateOwned(PetMate.Domain.Businesses.BusinessService service, CancellationToken cancellationToken)
        => Task.FromResult(items.TryGetValue(service.Id, out var s) && s.BusinessId == service.BusinessId);

    public Task<bool> DeleteOwned(Guid serviceId, Guid businessId, CancellationToken cancellationToken)
        => Task.FromResult(items.TryGetValue(serviceId, out var s) && s.BusinessId == businessId && items.TryRemove(serviceId, out _));

    public Task<long> DeleteByBusiness(Guid businessId, CancellationToken cancellationToken)
    {
        var ids = items.Values.Where(s => s.BusinessId == businessId).Select(s => s.Id).ToArray();
        return Task.FromResult((long)ids.Count(id => items.TryRemove(id, out _)));
    }
}

/// <summary>Correo falso: guarda los envíos para comprobarlos y puede simular un fallo del proveedor.</summary>
public sealed class FakeEmailSender : PetMate.Application.Authentication.IEmailSender
{
    private readonly List<(string To, string Token)> sent = [];

    public bool Succeeds { get; set; } = true;

    public bool Throws { get; set; }

    public IReadOnlyList<(string To, string Token)> Sent
    {
        get
        {
            lock (sent)
            {
                return [.. sent];
            }
        }
    }

    public Task<bool> SendPasswordReset(string toEmail, string resetToken, CancellationToken cancellationToken)
    {
        if (Throws)
        {
            throw new InvalidOperationException("proveedor caído");
        }

        lock (sent)
        {
            sent.Add((toEmail, resetToken));
        }

        return Task.FromResult(Succeeds);
    }
}

/// <summary>Tokens de restablecimiento en memoria con consumo atómico.</summary>
public sealed class InMemoryPasswordResetRepository : PetMate.Application.Authentication.IPasswordResetRepository
{
    private readonly object gate = new();
    private readonly Dictionary<string, (PetMate.Domain.Users.PasswordResetToken Token, bool Used)> byHash = [];

    public int Count
    {
        get
        {
            lock (gate)
            {
                return byHash.Count;
            }
        }
    }

    public string? StoredHashOf(Guid userId)
    {
        lock (gate)
        {
            return byHash.Values.Where(v => v.Token.UserId == userId).Select(v => v.Token.TokenHash.Value).FirstOrDefault();
        }
    }

    public Task Add(PetMate.Domain.Users.PasswordResetToken token, CancellationToken cancellationToken)
    {
        lock (gate)
        {
            byHash[token.TokenHash.Value] = (token, false);
        }

        return Task.CompletedTask;
    }

    public bool Throws { get; set; }

    public Task InvalidateAllForUser(Guid userId, DateTime nowUtc, CancellationToken cancellationToken)
    {
        if (Throws)
        {
            throw new PetMate.Application.Persistence.PersistenceUnavailableException(new InvalidOperationException("atlas caído"));
        }

        lock (gate)
        {
            foreach (var key in byHash.Where(e => e.Value.Token.UserId == userId && !e.Value.Used).Select(e => e.Key).ToArray())
            {
                byHash[key] = (byHash[key].Token, true);
            }
        }

        return Task.CompletedTask;
    }

    public Task<Guid?> TryConsume(RefreshTokenHash tokenHash, DateTime nowUtc, CancellationToken cancellationToken)
    {
        lock (gate)
        {
            if (!byHash.TryGetValue(tokenHash.Value, out var entry) || entry.Used || entry.Token.ExpiresAtUtc <= nowUtc)
            {
                return Task.FromResult<Guid?>(null);
            }

            byHash[tokenHash.Value] = (entry.Token, true);
            return Task.FromResult<Guid?>(entry.Token.UserId);
        }
    }
}

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

    public Task<IReadOnlyList<Guid>> RevokeAllForUser(Guid userId, DateTime nowUtc, SessionRevocationReason reason, CancellationToken cancellationToken)
    {
        lock (gate)
        {
            var ids = new List<Guid>();
            foreach (var session in sessions.Values.Where(s => s.UserId == userId && !s.IsRevoked))
            {
                session.Revoke(nowUtc, reason);
                ids.Add(session.Id);
            }

            return Task.FromResult<IReadOnlyList<Guid>>(ids);
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
