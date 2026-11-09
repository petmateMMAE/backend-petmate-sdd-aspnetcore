using System.Text;
using MongoDB.Bson;
using MongoDB.Driver;
using PetMate.Application.Authentication;
using PetMate.Application.Security;
using PetMate.Domain.Sessions;
using PetMate.Domain.Users;
using PetMate.Infrastructure.Persistence;
using PetMate.Infrastructure.Redis;
using PetMate.Infrastructure.Security;
using PetMate.IntegrationTests.Mongo;
using PetMate.TestDoubles;

namespace PetMate.IntegrationTests.Authentication;

/// <summary>
/// Flujo completo de autenticación contra Atlas (base de pruebas) y Redis Auth reales: Argon2id, JWT, refresh tokens,
/// índices únicos, consumo atómico y lista negra. Skipped si faltan credenciales o Redis; no cuentan como aprobadas.
/// </summary>
public sealed class AuthFlowIntegrationTests
{
    private const string Uri = "MONGODB_TEST_URI";
    private const string Database = "MONGODB_TEST_DATABASE";
    private const string RedisAuth = "PETMATE_IT_REDIS_AUTH";
    private const string Password = "contraseña-muy-segura-123";

    [RequiresEnvironmentFact(Uri, Database, RedisAuth)]
    public async Task Register_login_refresh_and_logout_work_end_to_end_with_real_atlas_and_redis()
    {
        await using var stack = await Stack.Create();

        var registered = Ok(await stack.Register.Handle(new RegisterCommand("Ana@Example.com", Password), default));

        // Atlas guarda solo el hash de la contraseña y solo el hash del refresh token.
        var userDocument = await stack.Raw<BsonDocument>(stack.Mongo.Names.Users).Find(FilterDefinition<BsonDocument>.Empty).SingleAsync();
        Assert.StartsWith("$argon2id$", userDocument["passwordHash"].AsString, StringComparison.Ordinal);
        Assert.DoesNotContain(Password, userDocument.ToJson(), StringComparison.Ordinal);
        Assert.Equal((int)UserRole.Owner, userDocument["role"].AsInt32);
        var tokenDocument = await stack.Raw<BsonDocument>(stack.Mongo.Names.Tokens).Find(FilterDefinition<BsonDocument>.Empty).SingleAsync();
        Assert.DoesNotContain(registered.RefreshToken, tokenDocument.ToJson(), StringComparison.Ordinal);
        Assert.Equal(64, tokenDocument["tokenHash"].AsString.Length);

        // El access token emitido es válido criptográficamente y lleva rol Owner.
        var claims = await stack.Jwt.Validate(registered.AccessToken.Value, default);
        Assert.Equal(UserRole.Owner, claims!.Role);

        var login = Ok(await stack.Login.Handle(new LoginCommand("ana@example.com", Password), default));
        var refreshed = Ok(await stack.Refresh.Handle(new RefreshCommand(login.RefreshToken), default));
        Assert.NotEqual(login.RefreshToken, refreshed.RefreshToken);

        stack.Track(refreshed.AccessToken.Jti);
        var logout = await stack.Logout.Handle(
            new LogoutCommand(refreshed.UserId, refreshed.AccessToken.Jti, refreshed.AccessToken.ExpiresAtUtc, refreshed.RefreshToken), default);
        Assert.True(logout.IsSuccess);

        // Redis Auth: la entrada existe con TTL ≈ vida restante; el refresh ya no sirve; Atlas marca la sesión revocada.
        var database = await stack.Redis.GetDatabase();
        var ttl = await database.KeyTimeToLiveAsync(RedisTokenRevocationStore.KeyPrefix + refreshed.AccessToken.Jti);
        Assert.NotNull(ttl);
        Assert.InRange(ttl.Value.TotalMinutes, 13, 15.1);
        Assert.True(await stack.Revocations.IsRevoked(refreshed.AccessToken.Jti, default));
        Assert.Equal(AuthError.InvalidRefreshToken, (await stack.Refresh.Handle(new RefreshCommand(refreshed.RefreshToken), default)).Error);
        var revoked = await stack.Raw<BsonDocument>(stack.Mongo.Names.Sessions).Find(Builders<BsonDocument>.Filter.Ne("revokedAtUtc", BsonNull.Value)).ToListAsync();
        Assert.Single(revoked);
    }

    [RequiresEnvironmentFact(Uri, Database, RedisAuth)]
    public async Task Duplicate_email_is_rejected_by_the_real_unique_index_even_when_registrations_race()
    {
        await using var stack = await Stack.Create();

        var results = await Task.WhenAll(Enumerable.Range(0, 10)
            .Select(_ => Task.Run(() => stack.Register.Handle(new RegisterCommand("race@example.com", Password), default))));

        Assert.Equal(1, results.Count(result => result.IsSuccess));
        Assert.All(results.Where(result => !result.IsSuccess), result => Assert.Equal(AuthError.EmailAlreadyRegistered, result.Error));
        Assert.Equal(1, await stack.Raw<BsonDocument>(stack.Mongo.Names.Users).CountDocumentsAsync(FilterDefinition<BsonDocument>.Empty));
    }

    [RequiresEnvironmentFact(Uri, Database, RedisAuth)]
    public async Task Concurrent_refresh_with_the_same_token_lets_exactly_one_request_win_and_revokes_the_session()
    {
        await using var stack = await Stack.Create();
        var first = Ok(await stack.Register.Handle(new RegisterCommand("ana@example.com", Password), default));

        var results = await Task.WhenAll(Enumerable.Range(0, 15)
            .Select(_ => Task.Run(() => stack.Refresh.Handle(new RefreshCommand(first.RefreshToken), default))));

        Assert.InRange(results.Count(result => result.IsSuccess), 0, 1); // nunca dos; un perdedor puede revocar antes de que el ganador actualice la sesión
        Assert.All(results.Where(result => !result.IsSuccess), result => Assert.Equal(AuthError.InvalidRefreshToken, result.Error));
        var session = await stack.Raw<BsonDocument>(stack.Mongo.Names.Sessions).Find(FilterDefinition<BsonDocument>.Empty).SingleAsync();
        Assert.Equal((int)SessionRevocationReason.ReplayDetected, session["revocationReason"].AsInt32);
    }

    [RequiresEnvironmentFact(Uri, Database, RedisAuth)]
    public async Task Replay_of_a_rotated_token_revokes_the_whole_family_in_atlas()
    {
        await using var stack = await Stack.Create();
        var first = Ok(await stack.Register.Handle(new RegisterCommand("ana@example.com", Password), default));
        var second = Ok(await stack.Refresh.Handle(new RefreshCommand(first.RefreshToken), default));

        var replay = await stack.Refresh.Handle(new RefreshCommand(first.RefreshToken), default);
        var newest = await stack.Refresh.Handle(new RefreshCommand(second.RefreshToken), default);

        Assert.Equal(AuthError.InvalidRefreshToken, replay.Error);
        Assert.Equal(AuthError.InvalidRefreshToken, newest.Error);
    }

    [RequiresEnvironmentFact(Uri, Database, RedisAuth)]
    public async Task Sessions_expire_after_the_idle_window_and_users_cannot_log_out_each_other()
    {
        await using var stack = await Stack.Create();
        var owner = Ok(await stack.Register.Handle(new RegisterCommand("dueno@example.com", Password), default));
        var attacker = Ok(await stack.Register.Handle(new RegisterCommand("intruso@example.com", Password), default));

        var forbidden = await stack.Logout.Handle(
            new LogoutCommand(attacker.UserId, attacker.AccessToken.Jti, attacker.AccessToken.ExpiresAtUtc, owner.RefreshToken), default);
        Assert.Equal(AuthError.Forbidden, forbidden.Error);
        Assert.False(await stack.Revocations.IsRevoked(attacker.AccessToken.Jti, default));

        stack.Clock.Advance(TimeSpan.FromDays(31));
        Assert.Equal(AuthError.InvalidRefreshToken, (await stack.Refresh.Handle(new RefreshCommand(owner.RefreshToken), default)).Error);
    }

    [RequiresEnvironmentFact(Uri, Database, RedisAuth)]
    public async Task Login_upgrades_an_outdated_password_hash_in_atlas()
    {
        await using var stack = await Stack.Create();
        Ok(await stack.Register.Handle(new RegisterCommand("ana@example.com", Password), default));
        var before = (await stack.Users.FindByEmail(Email.Create("ana@example.com"), default))!.PasswordHash.Value;

        await using var stronger = await Stack.CreateOn(stack.Mongo, new Argon2idOptions(Argon2idOptions.MinMemoryKiB * 2, Argon2idOptions.MinIterations + 1));
        Ok(await stronger.Login.Handle(new LoginCommand("ana@example.com", Password), default));

        var after = (await stack.Users.FindByEmail(Email.Create("ana@example.com"), default))!.PasswordHash.Value;
        Assert.NotEqual(before, after);
        Assert.Contains($"m={Argon2idOptions.MinMemoryKiB * 2},t={Argon2idOptions.MinIterations + 1}", after, StringComparison.Ordinal);
    }

    [RequiresEnvironmentFact(Uri, Database, RedisAuth)]
    public async Task Logout_with_redis_auth_down_fails_closed_after_revoking_the_session_in_atlas()
    {
        await using var stack = await Stack.Create();
        var tokens = Ok(await stack.Register.Handle(new RegisterCommand("ana@example.com", Password), default));
        await using var unreachable = new RedisAuthConnection("127.0.0.1:1,password=x,connectTimeout=300,abortConnect=false");
        var logout = new LogoutHandler(
            stack.Sessions, new RedisTokenRevocationStore(unreachable, stack.Clock), stack.RefreshTokens, stack.Jwt, stack.Clock);

        await Assert.ThrowsAsync<AuthDependencyUnavailableException>(() => logout.Handle(
            new LogoutCommand(tokens.UserId, tokens.AccessToken.Jti, tokens.AccessToken.ExpiresAtUtc, tokens.RefreshToken), default));

        Assert.Equal(AuthError.InvalidRefreshToken, (await stack.Refresh.Handle(new RefreshCommand(tokens.RefreshToken), default)).Error);
    }

    [RequiresEnvironmentFact(Uri, Database, RedisAuth)]
    public async Task Login_lockout_works_end_to_end_with_real_atlas_redis_and_argon2id()
    {
        await using var stack = await Stack.Create();
        Ok(await stack.Register.Handle(new RegisterCommand("lock.ana@example.com", Password), default));
        var database = await stack.Redis.GetDatabase();
        try
        {
            for (var i = 0; i < 5; i++)
            {
                Assert.Equal(AuthError.InvalidCredentials, (await stack.ThrottledLogin.Handle(new LoginCommand("lock.ana@example.com", "otra-contraseña-123"), default)).Error);
            }

            var sixth = await stack.ThrottledLogin.Handle(new LoginCommand("lock.ana@example.com", "otra-contraseña-123"), default);
            var rightPassword = await stack.ThrottledLogin.Handle(new LoginCommand("lock.ana@example.com", Password), default);

            Assert.Equal(AuthError.TooManyRequests, sixth.Error);
            Assert.Equal(AuthError.TooManyRequests, rightPassword.Error);
            Assert.InRange(rightPassword.RetryAfter!.Value.TotalMinutes, 14, 15);
            // Otra cuenta no se ve afectada y un correo inexistente se comporta igual que uno existente.
            Ok(await stack.Register.Handle(new RegisterCommand("lock.luis@example.com", Password), default));
            Ok(await stack.ThrottledLogin.Handle(new LoginCommand("lock.luis@example.com", Password), default));
            var results = new List<AuthError>();
            for (var i = 0; i < 7; i++)
            {
                results.Add((await stack.ThrottledLogin.Handle(new LoginCommand("lock.nadie@example.com", "otra-contraseña-123"), default)).Error);
            }

            Assert.Equal([.. Enumerable.Repeat(AuthError.InvalidCredentials, 5), AuthError.TooManyRequests, AuthError.TooManyRequests], results);
        }
        finally
        {
            foreach (var email in new[] { "lock.ana@example.com", "lock.luis@example.com", "lock.nadie@example.com" })
            {
                var suffix = "login:" + Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(Encoding.UTF8.GetBytes(email))).ToLowerInvariant();
                await database.KeyDeleteAsync(["auth:throttle:" + suffix, "auth:lock:" + suffix]);
            }
        }
    }

    [RequiresEnvironmentFact(Uri, Database, RedisAuth)]
    public async Task A_concurrent_burst_of_guesses_verifies_at_most_the_allowed_passwords_with_real_redis()
    {
        await using var stack = await Stack.Create();
        Ok(await stack.Register.Handle(new RegisterCommand("burst.ana@example.com", Password), default));
        var counting = new CountingHasher(stack.Hasher);
        var issuer = new AuthTokenIssuer(stack.Sessions, stack.RefreshTokens, stack.Jwt, SessionPolicy.Default);
        var login = new LoginHandler(stack.Users, counting, issuer, stack.Clock, stack.Throttle, stack.Limiter);
        var database = await stack.Redis.GetDatabase();
        try
        {
            var results = await Task.WhenAll(Enumerable.Range(0, 20)
                .Select(_ => Task.Run(() => login.Handle(new LoginCommand("burst.ana@example.com", "otra-contraseña-123"), default))));

            Assert.True(counting.Verifications <= AuthThrottleRules.Login.MaxAttempts, $"Se verificaron {counting.Verifications} contraseñas.");
            Assert.Equal(AuthThrottleRules.Login.MaxAttempts, results.Count(r => r.Error == AuthError.InvalidCredentials));
            Assert.Equal(20 - AuthThrottleRules.Login.MaxAttempts, results.Count(r => r.Error == AuthError.TooManyRequests));
        }
        finally
        {
            var suffix = "login:" + Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(Encoding.UTF8.GetBytes("burst.ana@example.com"))).ToLowerInvariant();
            await database.KeyDeleteAsync(["auth:throttle:" + suffix, "auth:lock:" + suffix]);
        }
    }

    private sealed class CountingHasher(IPasswordHasher inner) : IPasswordHasher
    {
        private int verifications;

        public int Verifications => Volatile.Read(ref verifications);

        public string Hash(string password) => inner.Hash(password);

        public PasswordVerificationResult Verify(string password, string passwordHash)
        {
            Interlocked.Increment(ref verifications);
            return inner.Verify(password, passwordHash);
        }
    }

    private static AuthTokensResult Ok(AuthResult<AuthTokensResult> result)
    {
        Assert.True(result.IsSuccess, $"Se esperaba éxito y se obtuvo {result.Error}.");
        return result.Value!;
    }

    /// <summary>Apilado real: Atlas de pruebas, Redis Auth de Compose, Argon2id con parámetros mínimos y JWT real.</summary>
    private sealed class Stack : IAsyncDisposable
    {
        private readonly MongoTestContext? ownedMongo;
        private readonly RedisAuthConnection? ownedRedis;

        private Stack(MongoTestContext mongo, bool ownsMongo, Argon2idOptions argon)
        {
            Mongo = mongo;
            ownedMongo = ownsMongo ? mongo : null;
            Clock = new ManualClock(DateTimeOffset.UtcNow);
            Users = mongo.Users;
            Sessions = mongo.Sessions;
            Redis = new RedisAuthConnection(Environment.GetEnvironmentVariable(RedisAuth)!);
            ownedRedis = Redis;
            Revocations = new RedisTokenRevocationStore(Redis, Clock);
            RefreshTokens = new RefreshTokenFactory();
            Hasher = new Argon2idPasswordHasher(argon);
            Jwt = new JwtAccessTokenService(
                new JwtOptions("https://issuer-integracion.local", "petmate-integracion", Encoding.UTF8.GetBytes("clave-de-integracion-0123456789-abcdefghi"), TimeSpan.FromMinutes(15)),
                Clock);
            var issuer = new AuthTokenIssuer(Sessions, RefreshTokens, Jwt, SessionPolicy.Default);
            Register = new RegisterHandler(Users, Hasher, issuer, Clock);
            Login = new LoginHandler(Users, Hasher, issuer, Clock);
            Throttle = new RedisAuthAttemptThrottle(Redis);
            Limiter = new BoundedPasswordHashingLimiter(2, 4);
            ThrottledLogin = new LoginHandler(Users, Hasher, issuer, Clock, Throttle, Limiter);
            Refresh = new RefreshHandler(Users, Sessions, RefreshTokens, Jwt, SessionPolicy.Default, Clock, null, Revocations);
            Logout = new LogoutHandler(Sessions, Revocations, RefreshTokens, Jwt, Clock);
        }

        public MongoTestContext Mongo { get; }

        public ManualClock Clock { get; }

        public MongoUserRepository Users { get; }

        public MongoSessionRepository Sessions { get; }

        public RedisAuthConnection Redis { get; }

        public RedisTokenRevocationStore Revocations { get; }

        public RefreshTokenFactory RefreshTokens { get; }

        public Argon2idPasswordHasher Hasher { get; }

        public JwtAccessTokenService Jwt { get; }

        public RegisterHandler Register { get; }

        public LoginHandler Login { get; }

        public RedisAuthAttemptThrottle Throttle { get; }

        public BoundedPasswordHashingLimiter Limiter { get; }

        public LoginHandler ThrottledLogin { get; }

        public RefreshHandler Refresh { get; }

        public LogoutHandler Logout { get; }

        public static async Task<Stack> Create()
            => new(await MongoTestContext.Create(), ownsMongo: true, new Argon2idOptions(Argon2idOptions.MinMemoryKiB, Argon2idOptions.MinIterations));

        public static Task<Stack> CreateOn(MongoTestContext mongo, Argon2idOptions argon)
            => Task.FromResult(new Stack(mongo, ownsMongo: false, argon));

        public IMongoCollection<T> Raw<T>(string collection) => Mongo.Database.GetCollection<T>(collection);

        public async ValueTask DisposeAsync()
        {
            // Solo se eliminan las claves de lista negra creadas por esta prueba y las colecciones propias.
            var database = await Redis.GetDatabase();
            foreach (var jti in issuedJtis)
            {
                await database.KeyDeleteAsync(RedisTokenRevocationStore.KeyPrefix + jti);
            }

            Hasher.Dispose();
            if (ownedRedis is not null)
            {
                await ownedRedis.DisposeAsync();
            }

            if (ownedMongo is not null)
            {
                await ownedMongo.DisposeAsync();
            }
        }

        /// <summary>Registra un jti para eliminarlo de Redis al terminar la prueba (solo claves propias).</summary>
        public void Track(string jti) => issuedJtis.Add(jti);

        private readonly List<string> issuedJtis = [];
    }
}
