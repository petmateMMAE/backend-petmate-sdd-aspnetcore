using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PetMate.Application.Security;
using PetMate.Domain.Users;
using PetMate.Infrastructure.DependencyInjection;
using PetMate.Infrastructure.Security;
using Xunit.Abstractions;

namespace PetMate.SecurityTests.Passwords;

public sealed partial class Argon2idPasswordHasherTests(ITestOutputHelper output)
{
    // Parámetros mínimos admitidos: mantienen las pruebas rápidas sin bajar del mínimo OWASP.
    private static readonly Argon2idOptions Minimum = new(Argon2idOptions.MinMemoryKiB, Argon2idOptions.MinIterations);
    private const string Password = "Contraseña-Segura-123!";

    [GeneratedRegex(@"^\$argon2id\$v=19\$m=(\d+),t=(\d+),p=(\d+)\$[A-Za-z0-9+/]+\$[A-Za-z0-9+/]+$")]
    private static partial Regex PhcFormat();

    [Fact]
    public void Hash_is_versioned_phc_with_configured_parameters_and_never_contains_the_password()
    {
        var hash = new Argon2idPasswordHasher(Minimum).Hash(Password);

        var match = PhcFormat().Match(hash);
        Assert.True(match.Success, "El hash no tiene formato PHC argon2id.");
        Assert.Equal(Argon2idOptions.MinMemoryKiB.ToString(System.Globalization.CultureInfo.InvariantCulture), match.Groups[1].Value);
        Assert.Equal(Argon2idOptions.MinIterations.ToString(System.Globalization.CultureInfo.InvariantCulture), match.Groups[2].Value);
        Assert.Equal("1", match.Groups[3].Value);
        Assert.DoesNotContain(Password, hash, StringComparison.Ordinal);
        Assert.Equal(hash, PasswordHash.Create(hash).Value);
    }

    [Fact]
    public void Same_password_produces_different_hashes_because_of_the_random_salt()
    {
        var hasher = new Argon2idPasswordHasher(Minimum);

        var first = hasher.Hash(Password);
        var second = hasher.Hash(Password);

        Assert.NotEqual(first, second);
        Assert.Equal(PasswordVerificationResult.Success, hasher.Verify(Password, first));
        Assert.Equal(PasswordVerificationResult.Success, hasher.Verify(Password, second));
    }

    [Fact]
    public void Verify_distinguishes_correct_and_incorrect_passwords()
    {
        var hasher = new Argon2idPasswordHasher(Minimum);
        var hash = hasher.Hash(Password);

        Assert.Equal(PasswordVerificationResult.Success, hasher.Verify(Password, hash));
        Assert.Equal(PasswordVerificationResult.Failed, hasher.Verify(Password + "x", hash));
        Assert.Equal(PasswordVerificationResult.Failed, hasher.Verify(" " + Password, hash));
        Assert.Equal(PasswordVerificationResult.Failed, hasher.Verify(Password.ToUpperInvariant(), hash));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("texto-plano")]
    [InlineData("$argon2id$")]
    [InlineData("$argon2id$v=19$m=19456,t=2,p=1$c2FsdA$")]
    [InlineData("$argon2id$v=19$m=abc,t=2,p=1$c2FsdA$aGFzaA")]
    [InlineData("$argon2i$v=19$m=19456,t=2,p=1$c2FsdHNhbHRzYWx0$aGFzaGhhc2hoYXNoaGFzaGhhc2hoYXNoaGFzaGhhc2g")]
    [InlineData("$2a$12$abcdefghijklmnopqrstuuMXHg5NpRBQmM5j3j6mQe1gYy6dJm5Wm")]
    public void Malformed_or_foreign_hashes_fail_without_throwing(string? malformed)
        => Assert.Equal(PasswordVerificationResult.Failed, new Argon2idPasswordHasher(Minimum).Verify(Password, malformed!));

    [Fact]
    public void Oversized_or_truncated_hashes_and_empty_passwords_fail_without_throwing()
    {
        var hasher = new Argon2idPasswordHasher(Minimum);
        var hash = hasher.Hash(Password);

        Assert.Equal(PasswordVerificationResult.Failed, hasher.Verify(Password, hash[..^5]));
        Assert.Equal(PasswordVerificationResult.Failed, hasher.Verify(Password, hash + new string('a', 500)));
        Assert.Equal(PasswordVerificationResult.Failed, hasher.Verify(string.Empty, hash));
        Assert.Equal(PasswordVerificationResult.Failed, hasher.Verify(null!, hash));
        Assert.Equal(PasswordVerificationResult.Failed, hasher.Verify(new string('a', 5000), hash));
    }

    [Fact]
    public void Hash_rejects_empty_null_and_oversized_passwords()
    {
        var hasher = new Argon2idPasswordHasher(Minimum);

        Assert.Throws<ArgumentException>(() => hasher.Hash(string.Empty));
        Assert.Throws<ArgumentNullException>(() => hasher.Hash(null!));
        Assert.Throws<ArgumentException>(() => hasher.Hash(new string('a', 2000)));
    }

    [Fact]
    public void Hash_supports_unicode_passwords()
    {
        var hasher = new Argon2idPasswordHasher(Minimum);
        var hash = hasher.Hash("pässwörd-日本語-🐶");

        Assert.Equal(PasswordVerificationResult.Success, hasher.Verify("pässwörd-日本語-🐶", hash));
        Assert.Equal(PasswordVerificationResult.Failed, hasher.Verify("passwoerd-日本語-🐶", hash));
    }

    [Fact]
    public void Verify_reports_rehash_needed_when_stored_parameters_are_weaker_than_current()
    {
        var oldHash = new Argon2idPasswordHasher(Minimum).Hash(Password);
        var stronger = new Argon2idPasswordHasher(new Argon2idOptions(Argon2idOptions.MinMemoryKiB * 2, Argon2idOptions.MinIterations + 1));

        Assert.Equal(PasswordVerificationResult.SuccessRehashNeeded, stronger.Verify(Password, oldHash));
        Assert.Equal(PasswordVerificationResult.Failed, stronger.Verify("incorrecta", oldHash));

        var refreshed = stronger.Hash(Password);
        Assert.Equal(PasswordVerificationResult.Success, stronger.Verify(Password, refreshed));
    }

    [Theory]
    [InlineData(Argon2idOptions.MinMemoryKiB - 1, 3)]
    [InlineData(65_536, Argon2idOptions.MinIterations - 1)]
    [InlineData(Argon2idOptions.MaxMemoryKiB + 1, 3)]
    [InlineData(65_536, Argon2idOptions.MaxIterations + 1)]
    public void Options_reject_parameters_below_owasp_minimum_or_above_the_cap(int memoryKiB, int iterations)
        => Assert.Throws<ArgumentOutOfRangeException>(() => new Argon2idOptions(memoryKiB, iterations));

    [Fact]
    public void Defaults_exceed_the_owasp_minimum_and_match_the_approved_plan()
    {
        Assert.Equal(65_536, Argon2idOptions.Default.MemoryKiB);
        Assert.Equal(3, Argon2idOptions.Default.Iterations);
        Assert.True(Argon2idOptions.Default.MemoryKiB >= Argon2idOptions.MinMemoryKiB);
        Assert.True(Argon2idOptions.Default.Iterations >= Argon2idOptions.MinIterations);
    }

    [Fact]
    public void Parse_uses_defaults_overrides_and_reports_only_key_names_on_error()
    {
        Assert.Equal(Argon2idOptions.Default, Argon2idOptions.Parse(Configuration()));
        var custom = Argon2idOptions.Parse(Configuration(("ARGON2_MEMORY_KIB", "32768"), ("ARGON2_ITERATIONS", "4")));
        Assert.Equal(32_768, custom.MemoryKiB);
        Assert.Equal(4, custom.Iterations);

        var error = Assert.Throws<InvalidOperationException>(
            () => Argon2idOptions.Parse(Configuration(("ARGON2_MEMORY_KIB", "1024"), ("ARGON2_ITERATIONS", "uno"))));
        Assert.Contains("ARGON2_MEMORY_KIB", error.Message, StringComparison.Ordinal);
        Assert.Contains("ARGON2_ITERATIONS", error.Message, StringComparison.Ordinal);
        Assert.DoesNotContain("1024", error.Message, StringComparison.Ordinal);
        Assert.DoesNotContain("uno", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Domain_user_has_no_plain_password_member_and_registers_only_a_hash()
    {
        var members = typeof(User).GetMembers().Select(member => member.Name.Replace("get_", string.Empty, StringComparison.Ordinal));

        Assert.DoesNotContain(members, name => name.Contains("Password", StringComparison.OrdinalIgnoreCase) && name != nameof(User.PasswordHash)
            && name != nameof(User.ChangePasswordHash));
        var hash = PasswordHash.Create(new Argon2idPasswordHasher(Minimum).Hash(Password));
        var user = User.RegisterOwner(Email.Create("a@b.co"), hash, DateTime.UtcNow);
        Assert.DoesNotContain(Password, user.PasswordHash.Value, StringComparison.Ordinal);
    }

    [Fact]
    public void Infrastructure_registers_the_hasher_behind_the_application_abstraction()
    {
        var configuration = Configuration(
            ("MONGODB_URI", "mongodb+srv://usuario_ficticio:password_ficticio@cluster-ficticio.mongodb.net/?retryWrites=true&w=majority"),
            ("MONGODB_DATABASE", "petmate_test_ficticio"),
            ("REDIS_AUTH_CONNECTION", "redis-auth-ficticio:6380,password=pw-a,ssl=True"),
            ("REDIS_CACHE_CONNECTION", "redis-cache-ficticio:6381,password=pw-b,ssl=True"),
            ("JWT_ISSUER", "https://issuer-prueba.local"),
            ("JWT_AUDIENCE", "petmate-api-prueba"),
            ("JWT_SIGNING_KEY", "clave-de-prueba-0123456789-abcdefghijkl"));
        using var provider = new ServiceCollection().AddLogging().AddInfrastructure(configuration).BuildServiceProvider();

        var hasher = provider.GetRequiredService<IPasswordHasher>();

        Assert.IsType<Argon2idPasswordHasher>(hasher);
    }

    [Fact]
    public void Benchmark_default_parameters_complete_within_a_usable_login_budget()
    {
        var hasher = new Argon2idPasswordHasher(Argon2idOptions.Default);
        _ = hasher.Hash(Password); // calentamiento
        var samples = new List<double>();
        for (var i = 0; i < 3; i++)
        {
            var watch = Stopwatch.StartNew();
            _ = hasher.Hash(Password);
            samples.Add(watch.Elapsed.TotalMilliseconds);
        }

        var median = samples.Order().ElementAt(1);
        output.WriteLine($"BENCHMARK Argon2id m={Argon2idOptions.Default.MemoryKiB}KiB t={Argon2idOptions.Default.Iterations} p=1 mediana={median:F0} ms");
        Assert.InRange(median, 5, 3000);
    }

    [Fact]
    public void Tampered_hash_fails_verification()
    {
        var hasher = new Argon2idPasswordHasher(Minimum);
        var hash = hasher.Hash(Password);
        var last = hash[^1] == 'A' ? 'B' : 'A';

        Assert.Equal(PasswordVerificationResult.Failed, hasher.Verify(Password, hash[..^1] + last));
    }

    [Theory]
    [InlineData("$argon2id$v=19$m=99999999,t=3,p=1$c2FsdHNhbHRzYWx0$aGFzaGhhc2hoYXNoaGFzaGhhc2hoYXNoaGFzaGhhc2g")]
    [InlineData("$argon2id$v=19$m=65536,t=999,p=1$c2FsdHNhbHRzYWx0$aGFzaGhhc2hoYXNoaGFzaGhhc2hoYXNoaGFzaGhhc2g")]
    [InlineData("$argon2id$v=19$m=65536,t=3,p=8$c2FsdHNhbHRzYWx0$aGFzaGhhc2hoYXNoaGFzaGhhc2hoYXNoaGFzaGhhc2g")]
    [InlineData("$argon2id$v=16$m=65536,t=3,p=1$c2FsdHNhbHRzYWx0$aGFzaGhhc2hoYXNoaGFzaGhhc2hoYXNoaGFzaGhhc2g")]
    [InlineData("$argon2id$v=19$m=1024,t=3,p=1$c2FsdHNhbHRzYWx0$aGFzaGhhc2hoYXNoaGFzaGhhc2hoYXNoaGFzaGhhc2g")]
    public void Hashes_with_out_of_range_parameters_are_rejected_before_any_expensive_work(string hostile)
    {
        var watch = Stopwatch.StartNew();

        var result = new Argon2idPasswordHasher(Minimum).Verify(Password, hostile);

        Assert.Equal(PasswordVerificationResult.Failed, result);
        Assert.True(watch.ElapsedMilliseconds < 50, "La verificación de un hash hostil no debe consumir recursos.");
    }

    [Fact]
    public void Password_limit_is_exactly_1024_bytes()
    {
        var hasher = new Argon2idPasswordHasher(Minimum);
        var atLimit = new string('a', 1024);

        var hash = hasher.Hash(atLimit);

        Assert.Equal(PasswordVerificationResult.Success, hasher.Verify(atLimit, hash));
        Assert.Throws<ArgumentException>(() => hasher.Hash(atLimit + "a"));
        Assert.Equal(PasswordVerificationResult.Failed, hasher.Verify(atLimit + "a", hash));
        Assert.Throws<ArgumentException>(() => hasher.Hash(new string('ñ', 600)));
    }

    [Fact]
    public void Stronger_stored_hash_is_flagged_for_rehash_when_configuration_lowers_parameters()
    {
        // Política documentada: los parámetros solo deben subirse; si se bajan, el siguiente login degrada el hash.
        var strong = new Argon2idPasswordHasher(new Argon2idOptions(Argon2idOptions.MinMemoryKiB * 2, Argon2idOptions.MinIterations + 1))
            .Hash(Password);

        Assert.Equal(PasswordVerificationResult.SuccessRehashNeeded, new Argon2idPasswordHasher(Minimum).Verify(Password, strong));
    }

    private static IConfiguration Configuration(params (string Key, string Value)[] values)
        => new ConfigurationBuilder()
            .AddInMemoryCollection(values.ToDictionary(item => item.Key, item => (string?)item.Value))
            .Build();
}
