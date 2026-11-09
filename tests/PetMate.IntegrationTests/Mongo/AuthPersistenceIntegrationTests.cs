using MongoDB.Bson;
using MongoDB.Driver;
using PetMate.Application.Persistence;
using PetMate.Domain.Sessions;
using PetMate.Domain.Users;
using PetMate.Infrastructure.Persistence;

namespace PetMate.IntegrationTests.Mongo;

/// <summary>Pruebas contra la base Atlas de pruebas. Skipped si no hay credenciales de prueba reales; no cuentan como aprobadas.</summary>
public sealed class AuthPersistenceIntegrationTests
{
    private const string Uri = "MONGODB_TEST_URI";
    private const string Database = "MONGODB_TEST_DATABASE";
    private const string ValidHash = "$argon2id$v=19$m=65536,t=3,p=1$c2FsdHNhbHRzYWx0$aGFzaGhhc2hoYXNoaGFzaGhhc2hoYXNoaGFzaGhhc2g";

    private static DateTime Now => DateTime.UtcNow;

    private static User NewUser(string email) => User.Register(Email.Create(email), PasswordHash.Create(ValidHash), "Ana", Now);

    private static string Sha256Hex() => Convert.ToHexString(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32)).ToLowerInvariant();

    [RequiresEnvironmentFact(Uri, Database)]
    public async Task Users_round_trip_by_email_and_id_preserving_role_and_utc_dates()
    {
        await using var context = await MongoTestContext.Create();
        var user = NewUser("Ana@Example.com");

        Assert.True(await context.Users.TryAdd(user, CancellationToken.None));

        var byEmail = await context.Users.FindByEmail(Email.Create("ana@example.COM"), CancellationToken.None);
        var byId = await context.Users.FindById(user.Id, CancellationToken.None);
        Assert.NotNull(byEmail);
        Assert.NotNull(byId);
        Assert.Equal(user.Id, byEmail.Id);
        Assert.Equal(UserRole.User, byEmail.Role);
        Assert.Equal(user.PasswordHash, byEmail.PasswordHash);
        Assert.Equal(DateTimeKind.Utc, byEmail.CreatedAtUtc.Kind);
        Assert.Null(await context.Users.FindByEmail(Email.Create("otra@example.com"), CancellationToken.None));
        Assert.Null(await context.Users.FindById(Guid.NewGuid(), CancellationToken.None));
    }

    [RequiresEnvironmentFact(Uri, Database)]
    public async Task Profile_update_persists_only_the_name_and_the_update_date_of_that_user()
    {
        await using var context = await MongoTestContext.Create();
        var ana = NewUser("ana.perfil@example.com");
        var luis = NewUser("luis.perfil@example.com");
        Assert.True(await context.Users.TryAdd(ana, CancellationToken.None));
        Assert.True(await context.Users.TryAdd(luis, CancellationToken.None));

        ana.ChangeDisplayName("  Ana Pérez ", Now.AddMinutes(1));
        await context.Users.UpdateProfile(ana, CancellationToken.None);

        var storedAna = await context.Users.FindById(ana.Id, CancellationToken.None);
        var storedLuis = await context.Users.FindById(luis.Id, CancellationToken.None);
        Assert.NotNull(storedAna);
        Assert.NotNull(storedLuis);
        Assert.Equal("Ana Pérez", storedAna.DisplayName);
        Assert.Equal(ana.UpdatedAtUtc, storedAna.UpdatedAtUtc, TimeSpan.FromMilliseconds(1));
        Assert.Equal(ana.Email, storedAna.Email);
        Assert.Equal(UserRole.User, storedAna.Role);
        Assert.Equal(ana.PasswordHash, storedAna.PasswordHash);
        Assert.Equal(ana.CreatedAtUtc, storedAna.CreatedAtUtc, TimeSpan.FromMilliseconds(1));
        Assert.Equal("Ana", storedLuis.DisplayName);
        Assert.Null(storedLuis.Phone);
    }

    [RequiresEnvironmentFact(Uri, Database)]
    public async Task Profile_update_persists_and_removes_the_optional_phone()
    {
        await using var context = await MongoTestContext.Create();
        var ana = NewUser("ana.celular@example.com");
        Assert.True(await context.Users.TryAdd(ana, CancellationToken.None));

        ana.ChangePhone("+52 55 1234 5678", Now.AddMinutes(1));
        await context.Users.UpdateProfile(ana, CancellationToken.None);
        Assert.Equal("5512345678", (await context.Users.FindById(ana.Id, CancellationToken.None))!.Phone);

        ana.ChangePhone(null, Now.AddMinutes(2));
        await context.Users.UpdateProfile(ana, CancellationToken.None);
        Assert.Null((await context.Users.FindById(ana.Id, CancellationToken.None))!.Phone);
    }

    [RequiresEnvironmentFact(Uri, Database)]
    public async Task Password_reset_token_is_consumed_once_only_while_valid_and_by_a_single_winner()
    {
        await using var context = await MongoTestContext.Create();
        var userId = Guid.NewGuid();
        var hash = RefreshTokenHash.Create(Sha256Hex());
        var token = PasswordResetToken.Issue(userId, hash, Now);
        await context.PasswordResets.Add(token, CancellationToken.None);

        Assert.Null(await context.PasswordResets.TryConsume(RefreshTokenHash.Create(Sha256Hex()), Now, CancellationToken.None));
        Assert.Null(await context.PasswordResets.TryConsume(hash, token.ExpiresAtUtc.AddSeconds(1), CancellationToken.None));
        var winners = await Task.WhenAll(Enumerable.Range(0, 8).Select(_ => context.PasswordResets.TryConsume(hash, Now, CancellationToken.None)));

        Assert.Equal(1, winners.Count(w => w == userId));
        Assert.Null(await context.PasswordResets.TryConsume(hash, Now, CancellationToken.None));
    }

    [RequiresEnvironmentFact(Uri, Database)]
    public async Task Revoking_all_sessions_of_a_user_returns_only_the_active_ones_of_that_user()
    {
        await using var context = await MongoTestContext.Create();
        var userId = Guid.NewGuid();
        var other = Guid.NewGuid();
        var active = RefreshSession.Start(userId, Now, SessionPolicy.Default);
        var already = RefreshSession.Start(userId, Now, SessionPolicy.Default);
        var foreign = RefreshSession.Start(other, Now, SessionPolicy.Default);
        foreach (var session in new[] { active, already, foreign })
        {
            await context.Sessions.AddSession(session, RefreshToken.Issue(session.Id, RefreshTokenHash.Create(Sha256Hex()), Now), CancellationToken.None);
        }

        already.Revoke(Now, SessionRevocationReason.Logout);
        await context.Sessions.Revoke(already, CancellationToken.None);

        var revoked = await context.Sessions.RevokeAllForUser(userId, Now.AddMinutes(1), SessionRevocationReason.PasswordReset, CancellationToken.None);

        Assert.Equal([active.Id], revoked);
        Assert.Equal(SessionRevocationReason.PasswordReset, (await context.Sessions.FindSession(active.Id, CancellationToken.None))!.RevocationReason);
        Assert.False((await context.Sessions.FindSession(foreign.Id, CancellationToken.None))!.IsRevoked);
        Assert.Empty(await context.Sessions.RevokeAllForUser(userId, Now.AddMinutes(2), SessionRevocationReason.PasswordReset, CancellationToken.None));
    }

    [RequiresEnvironmentFact(Uri, Database)]
    public async Task Duplicate_normalized_email_is_rejected_by_the_unique_index()
    {
        await using var context = await MongoTestContext.Create();

        Assert.True(await context.Users.TryAdd(NewUser("dup@example.com"), CancellationToken.None));
        Assert.False(await context.Users.TryAdd(NewUser("  DUP@example.COM "), CancellationToken.None));
    }

    [RequiresEnvironmentFact(Uri, Database)]
    public async Task Concurrent_registrations_of_the_same_email_create_exactly_one_user()
    {
        await using var context = await MongoTestContext.Create();

        var results = await Task.WhenAll(Enumerable.Range(0, 20)
            .Select(_ => Task.Run(() => context.Users.TryAdd(NewUser("race@example.com"), CancellationToken.None))));

        Assert.Equal(1, results.Count(created => created));
        Assert.Equal(1, await context.Database.GetCollection<BsonDocument>(context.Names.Users)
            .CountDocumentsAsync(FilterDefinition<BsonDocument>.Empty));
    }

    [RequiresEnvironmentFact(Uri, Database)]
    public async Task Password_hash_update_is_persisted()
    {
        await using var context = await MongoTestContext.Create();
        var user = NewUser("rehash@example.com");
        await context.Users.TryAdd(user, CancellationToken.None);
        var newHash = PasswordHash.Create(ValidHash.Replace("t=3", "t=4", StringComparison.Ordinal));
        user.ChangePasswordHash(newHash, Now.AddMinutes(1));

        await context.Users.UpdatePasswordHash(user, CancellationToken.None);

        var stored = await context.Users.FindById(user.Id, CancellationToken.None);
        Assert.Equal(newHash, stored!.PasswordHash);
    }

    [RequiresEnvironmentFact(Uri, Database)]
    public async Task Session_and_token_round_trip_storing_only_the_hash()
    {
        await using var context = await MongoTestContext.Create();
        var session = RefreshSession.Start(Guid.NewGuid(), Now, SessionPolicy.Default);
        var hash = RefreshTokenHash.Create(Sha256Hex());
        var token = RefreshToken.Issue(session.Id, hash, Now);

        await context.Sessions.AddSession(session, token, CancellationToken.None);

        var foundToken = await context.Sessions.FindTokenByHash(hash, CancellationToken.None);
        var foundSession = await context.Sessions.FindSession(session.Id, CancellationToken.None);
        Assert.NotNull(foundToken);
        Assert.NotNull(foundSession);
        Assert.Equal(token.Id, foundToken.Id);
        Assert.False(foundToken.IsConsumed);
        Assert.Equal(session.UserId, foundSession.UserId);
        Assert.Equal(DateTimeKind.Utc, foundSession.AbsoluteExpiresAtUtc.Kind);
        Assert.Null(await context.Sessions.FindTokenByHash(RefreshTokenHash.Create(Sha256Hex()), CancellationToken.None));
    }

    [RequiresEnvironmentFact(Uri, Database)]
    public async Task Concurrent_token_consumption_succeeds_for_exactly_one_caller()
    {
        await using var context = await MongoTestContext.Create();
        var session = RefreshSession.Start(Guid.NewGuid(), Now, SessionPolicy.Default);
        var token = RefreshToken.Issue(session.Id, RefreshTokenHash.Create(Sha256Hex()), Now);
        await context.Sessions.AddSession(session, token, CancellationToken.None);

        var results = await Task.WhenAll(Enumerable.Range(0, 20)
            .Select(_ => Task.Run(() => context.Sessions.TryConsumeToken(token.Id, Now, CancellationToken.None))));

        Assert.Equal(1, results.Count(consumed => consumed));
        var stored = await context.Sessions.FindTokenByHash(token.TokenHash, CancellationToken.None);
        Assert.True(stored!.IsConsumed);
        Assert.False(await context.Sessions.TryConsumeToken(token.Id, Now, CancellationToken.None));
    }

    [RequiresEnvironmentFact(Uri, Database)]
    public async Task Rotation_adds_a_new_token_and_revocation_keeps_the_first_reason()
    {
        await using var context = await MongoTestContext.Create();
        var session = RefreshSession.Start(Guid.NewGuid(), Now, SessionPolicy.Default);
        await context.Sessions.AddSession(session, RefreshToken.Issue(session.Id, RefreshTokenHash.Create(Sha256Hex()), Now), CancellationToken.None);
        var next = RefreshToken.Issue(session.Id, RefreshTokenHash.Create(Sha256Hex()), Now);
        await context.Sessions.AddToken(next, session.AbsoluteExpiresAtUtc, CancellationToken.None);
        Assert.NotNull(await context.Sessions.FindTokenByHash(next.TokenHash, CancellationToken.None));

        session.RegisterUse(Now.AddMinutes(1), SessionPolicy.Default);
        await context.Sessions.UpdateActivity(session, CancellationToken.None);
        session.Revoke(Now.AddMinutes(2), SessionRevocationReason.ReplayDetected);
        await context.Sessions.Revoke(session, CancellationToken.None);
        var secondDocument = MongoMapping.ToDocument(session);
        secondDocument.RevokedAtUtc = Now.AddMinutes(3);
        secondDocument.RevocationReason = (int)SessionRevocationReason.Logout;
        var second = MongoMapping.ToDomain(secondDocument);
        await context.Sessions.Revoke(second, CancellationToken.None);

        var stored = await context.Sessions.FindSession(session.Id, CancellationToken.None);
        Assert.True(stored!.IsRevoked);
        Assert.Equal(SessionRevocationReason.ReplayDetected, stored.RevocationReason);
        Assert.True(Math.Abs((stored.LastUsedAtUtc - session.LastUsedAtUtc).TotalMilliseconds) < 1);
    }

    [RequiresEnvironmentFact(Uri, Database)]
    public async Task Duplicate_token_hash_is_rejected_by_the_unique_index()
    {
        await using var context = await MongoTestContext.Create();
        var session = RefreshSession.Start(Guid.NewGuid(), Now, SessionPolicy.Default);
        var hash = RefreshTokenHash.Create(Sha256Hex());
        await context.Sessions.AddSession(session, RefreshToken.Issue(session.Id, hash, Now), CancellationToken.None);

        await Assert.ThrowsAsync<PersistenceUnavailableException>(
            () => context.Sessions.AddToken(RefreshToken.Issue(session.Id, hash, Now), session.AbsoluteExpiresAtUtc, CancellationToken.None));
    }

    [RequiresEnvironmentFact(Uri, Database)]
    public async Task Indexes_exist_with_expected_uniqueness_and_ttl_and_creation_is_idempotent()
    {
        await using var context = await MongoTestContext.Create();
        await context.Indexes.EnsureIndexes(CancellationToken.None);

        var users = await IndexesOf(context.Database, context.Names.Users);
        var tokens = await IndexesOf(context.Database, context.Names.Tokens);
        var sessions = await IndexesOf(context.Database, context.Names.Sessions);

        Assert.True(users[MongoIndexInitializer.UserEmailIndex]["unique"].AsBoolean);
        Assert.True(tokens[MongoIndexInitializer.TokenHashIndex]["unique"].AsBoolean);
        Assert.True(tokens.ContainsKey(MongoIndexInitializer.TokenSessionIndex));
        Assert.Equal(0, tokens[MongoIndexInitializer.TokenTtlIndex]["expireAfterSeconds"].ToInt32());
        Assert.True(sessions.ContainsKey(MongoIndexInitializer.SessionUserIndex));
        Assert.Equal(0, sessions[MongoIndexInitializer.SessionTtlIndex]["expireAfterSeconds"].ToInt32());
    }

    [Fact]
    public void Guard_rejects_unsafe_test_database_names()
    {
        Assert.Throws<InvalidOperationException>(() => MongoTestGuard.EnsureSafe("petmate_dev", null));
        Assert.Throws<InvalidOperationException>(() => MongoTestGuard.EnsureSafe("petmate_dev", "petmate_dev"));
        Assert.Throws<InvalidOperationException>(() => MongoTestGuard.EnsureSafe("petmate_dev", "petmate_prod"));
        Assert.Throws<InvalidOperationException>(() => MongoTestGuard.EnsureSafe("petmate_test", "PETMATE_TEST"));
        MongoTestGuard.EnsureSafe("petmate_dev", "petmate_test");
        MongoTestGuard.EnsureSafe(null, "petmate_test");
    }

    [Fact]
    public async Task Repositories_translate_provider_failures_without_leaking_connection_details()
    {
        var settings = MongoClientSettings.FromConnectionString("mongodb://usuario:clave-secreta@127.0.0.1:1/");
        settings.ServerSelectionTimeout = TimeSpan.FromMilliseconds(300);
        var database = new MongoClient(settings).GetDatabase("petmate_test_ficticio");
        var users = new MongoUserRepository(database, new MongoCollectionNames("x_"));
        var sessions = new MongoSessionRepository(database, new MongoCollectionNames("x_"));

        var userError = await Assert.ThrowsAsync<PersistenceUnavailableException>(
            () => users.FindByEmail(Email.Create("a@b.co"), CancellationToken.None));
        var addError = await Assert.ThrowsAsync<PersistenceUnavailableException>(
            () => users.TryAdd(NewUser("a@b.co"), CancellationToken.None));
        var sessionError = await Assert.ThrowsAsync<PersistenceUnavailableException>(
            () => sessions.FindSession(Guid.NewGuid(), CancellationToken.None));

        foreach (var error in new[] { userError, addError, sessionError })
        {
            Assert.DoesNotContain("clave-secreta", error.Message, StringComparison.Ordinal);
            Assert.DoesNotContain("127.0.0.1", error.Message, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void Mapping_round_trips_without_a_database_and_keeps_the_user_role_and_utc()
    {
        var ownerDocument = MongoMapping.ToDocument(NewUser("neg@example.com"));
        ownerDocument.Role = 1; // valor que antes se llamaba Owner (pm-086)
        var user = MongoMapping.ToDomain(ownerDocument);
        var session = RefreshSession.Start(user.Id, Now, SessionPolicy.Default);
        session.Revoke(Now, SessionRevocationReason.Logout);
        var token = RefreshToken.Issue(session.Id, RefreshTokenHash.Create(Sha256Hex()), Now);

        var restoredUser = MongoMapping.ToDomain(MongoMapping.ToDocument(user));
        var restoredSession = MongoMapping.ToDomain(MongoMapping.ToDocument(session));
        var restoredToken = MongoMapping.ToDomain(MongoMapping.ToDocument(token, session.AbsoluteExpiresAtUtc));

        Assert.Equal(UserRole.User, restoredUser.Role);
        Assert.Equal(user.Email, restoredUser.Email);
        Assert.Equal(SessionRevocationReason.Logout, restoredSession.RevocationReason);
        Assert.Equal(token.TokenHash, restoredToken.TokenHash);
        Assert.Equal(DateTimeKind.Utc, restoredSession.CreatedAtUtc.Kind);
    }

    [RequiresEnvironmentFact(Uri, Database)]
    public async Task Corrupt_stored_documents_surface_as_persistence_errors_not_domain_errors()
    {
        await using var context = await MongoTestContext.Create();
        var user = NewUser("corrupta@example.com");
        var document = MongoMapping.ToDocument(user);
        document.Role = 99;
        await context.Database.GetCollection<UserDocument>(context.Names.Users).InsertOneAsync(document);

        var error = await Assert.ThrowsAsync<PersistenceUnavailableException>(
            () => context.Users.FindById(user.Id, CancellationToken.None));

        Assert.DoesNotContain("99", error.Message, StringComparison.Ordinal);
    }

    [RequiresEnvironmentFact(Uri, Database)]
    public async Task Activity_updates_never_modify_an_already_revoked_session()
    {
        await using var context = await MongoTestContext.Create();
        var session = RefreshSession.Start(Guid.NewGuid(), Now, SessionPolicy.Default);
        await context.Sessions.AddSession(session, RefreshToken.Issue(session.Id, RefreshTokenHash.Create(Sha256Hex()), Now), CancellationToken.None);
        session.Revoke(Now.AddMinutes(1), SessionRevocationReason.Logout);
        await context.Sessions.Revoke(session, CancellationToken.None);
        var before = await context.Sessions.FindSession(session.Id, CancellationToken.None);

        var active = RefreshSession.Start(session.UserId, Now, SessionPolicy.Default);
        var changed = MongoMapping.ToDocument(active);
        changed.Id = session.Id;
        changed.LastUsedAtUtc = Now.AddDays(5);
        changed.IdleExpiresAtUtc = Now.AddDays(35);
        await context.Sessions.UpdateActivity(MongoMapping.ToDomain(changed), CancellationToken.None);

        var after = await context.Sessions.FindSession(session.Id, CancellationToken.None);
        Assert.Equal(before!.LastUsedAtUtc, after!.LastUsedAtUtc);
        Assert.Equal(before.IdleExpiresAtUtc, after.IdleExpiresAtUtc);
        Assert.True(after.IsRevoked);
    }

    [RequiresEnvironmentFact(Uri, Database)]
    public async Task Operator_like_email_text_cannot_alter_the_filter()
    {
        await using var context = await MongoTestContext.Create();
        await context.Users.TryAdd(NewUser("real@example.com"), CancellationToken.None);

        Assert.False(Email.TryCreate("{\"$ne\":null}", out _));
        Assert.Null(await context.Users.FindByEmail(Email.Create("a$ne@example.com"), CancellationToken.None));
    }

    private static async Task<Dictionary<string, BsonDocument>> IndexesOf(IMongoDatabase database, string collection)
    {
        using var cursor = await database.GetCollection<BsonDocument>(collection).Indexes.ListAsync();
        var list = await cursor.ToListAsync();
        return list.ToDictionary(index => index["name"].AsString);
    }
}
