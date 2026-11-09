using PetMate.Application.Authentication;
using PetMate.TestDoubles;

namespace PetMate.UnitTests.Authentication;

/// <summary>Endurecimiento del login y el registro (T101 H3, T102 H4): bloqueo por par (correo, IP), techo por correo e igualdad de tiempos.</summary>
public sealed class HardeningTests
{
    private const string Password = "contraseña-muy-segura-123";
    private const string Wrong = "otra-contraseña-123";

    private static async Task<AuthTestHarness> WithAccount()
    {
        var stack = new AuthTestHarness();
        var registered = await stack.Register.Handle(new RegisterCommand("ana@example.com", Password), CancellationToken.None);
        Assert.True(registered.IsSuccess);
        return stack;
    }

    private static Task<AuthResult<AuthTokensResult>> Login(AuthTestHarness stack, string password, string? ip)
        => stack.Login.Handle(new LoginCommand("ana@example.com", password, ip), CancellationToken.None);

    [Fact]
    public async Task Failures_from_one_ip_lock_only_that_pair_and_the_owner_can_still_log_in_from_another_ip()
    {
        var stack = await WithAccount();
        for (var i = 0; i < 5; i++)
        {
            Assert.Equal(AuthError.InvalidCredentials, (await Login(stack, Wrong, "ip-atacante")).Error);
        }

        var lockedAttacker = await Login(stack, Wrong, "ip-atacante");
        var lockedEvenWithRightPassword = await Login(stack, Password, "ip-atacante");
        var owner = await Login(stack, Password, "ip-propietario");

        Assert.Equal(AuthError.TooManyRequests, lockedAttacker.Error);
        Assert.Equal(AuthError.TooManyRequests, lockedEvenWithRightPassword.Error);
        Assert.True(owner.IsSuccess); // H3: el atacante no deja fuera a la víctima
    }

    [Fact]
    public async Task The_pair_lock_expires_after_fifteen_minutes()
    {
        var stack = await WithAccount();
        for (var i = 0; i < 6; i++)
        {
            await Login(stack, Wrong, "ip-a");
        }

        stack.Clock.Advance(TimeSpan.FromMinutes(15) + TimeSpan.FromSeconds(1));

        Assert.True((await Login(stack, Password, "ip-a")).IsSuccess);
    }

    [Fact]
    public async Task A_distributed_attack_hits_the_email_ceiling_and_waits_a_short_time_for_every_ip()
    {
        var stack = await WithAccount();
        AuthResult<AuthTokensResult>? last = null;
        for (var i = 0; i < 51; i++)
        {
            last = await Login(stack, Wrong, "ip-" + i); // cada IP falla una sola vez: nunca llega al bloqueo por par
        }

        Assert.Equal(AuthError.TooManyRequests, last!.Error);
        Assert.InRange(last.RetryAfter!.Value.TotalSeconds, 1, 60);
        var fromAnotherIp = await Login(stack, Password, "ip-nueva");
        Assert.Equal(AuthError.TooManyRequests, fromAnotherIp.Error);

        stack.Clock.Advance(TimeSpan.FromSeconds(61)); // la espera es de un minuto, no el bloqueo duro de 15
        Assert.True((await Login(stack, Password, "ip-nueva")).IsSuccess);
    }

    [Fact]
    public async Task A_successful_login_clears_only_the_pair_counter()
    {
        var stack = await WithAccount();
        for (var i = 0; i < 4; i++)
        {
            await Login(stack, Wrong, "ip-a");
        }

        Assert.True((await Login(stack, Password, "ip-a")).IsSuccess);
        for (var i = 0; i < 5; i++)
        {
            Assert.Equal(AuthError.InvalidCredentials, (await Login(stack, Wrong, "ip-a")).Error);
        }

        Assert.Equal(AuthError.TooManyRequests, (await Login(stack, Wrong, "ip-a")).Error);
    }

    [Fact]
    public async Task Without_a_client_key_the_lock_counts_by_email_alone()
    {
        var stack = await WithAccount();
        for (var i = 0; i < 5; i++)
        {
            await Login(stack, Wrong, null);
        }

        Assert.Equal(AuthError.TooManyRequests, (await Login(stack, Password, null)).Error);
    }

    [Fact]
    public async Task Registering_a_duplicate_email_still_computes_the_password_hash_so_timing_does_not_reveal_it()
    {
        var stack = await WithAccount();
        var before = stack.Hasher.HashCalls;

        var duplicate = await stack.Register.Handle(new RegisterCommand("ana@example.com", Password), CancellationToken.None);
        var fresh = await stack.Register.Handle(new RegisterCommand("nueva@example.com", Password), CancellationToken.None);

        Assert.Equal(AuthError.EmailAlreadyRegistered, duplicate.Error);
        Assert.True(fresh.IsSuccess);
        Assert.Equal(2, stack.Hasher.HashCalls - before); // un hash por intento, exista o no el correo (H4)
    }
}
