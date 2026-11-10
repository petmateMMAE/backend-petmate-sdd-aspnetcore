using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using PetMate.Api.Configuration;

namespace PetMate.SecurityTests.Authentication;

public sealed class RateLimitAndProxyTests
{
    [Fact]
    public void IPv4_clients_are_partitioned_by_full_address_and_missing_ip_shares_one_partition()
    {
        Assert.Equal("203.0.113.7", RateLimitKey.For(IPAddress.Parse("203.0.113.7")));
        Assert.NotEqual(RateLimitKey.For(IPAddress.Parse("203.0.113.7")), RateLimitKey.For(IPAddress.Parse("203.0.113.8")));
        Assert.Equal(RateLimitKey.Unknown, RateLimitKey.For(null));
    }

    [Fact]
    public void IPv6_addresses_in_the_same_slash_64_share_a_partition_and_other_prefixes_do_not()
    {
        var a = RateLimitKey.For(IPAddress.Parse("2001:db8:1:2::1"));
        var b = RateLimitKey.For(IPAddress.Parse("2001:db8:1:2:ffff:ffff:ffff:ffff"));
        var other = RateLimitKey.For(IPAddress.Parse("2001:db8:1:3::1"));

        Assert.Equal(a, b);
        Assert.NotEqual(a, other);
    }

    [Fact]
    public void IPv4_mapped_IPv6_addresses_use_the_IPv4_key()
        => Assert.Equal("198.51.100.4", RateLimitKey.For(IPAddress.Parse("::ffff:198.51.100.4")));

    [Theory]
    [InlineData("TRUSTED_PROXIES", "no-es-una-ip")]
    [InlineData("AUTH_RATE_LIMIT_PER_MINUTE", "0")]
    [InlineData("AUTH_RATE_LIMIT_PER_MINUTE", "abc")]
    [InlineData("AUTH_RATE_LIMIT_PER_MINUTE", "100000")]
    [InlineData("PETS_RATE_LIMIT_PER_MINUTE", "0")]
    [InlineData("PETS_RATE_LIMIT_PER_MINUTE", "abc")]
    [InlineData("PETS_RATE_LIMIT_PER_MINUTE", "100000")]
    [InlineData("CARNET_RATE_LIMIT_PER_MINUTE", "0")]
    [InlineData("CARNET_RATE_LIMIT_PER_MINUTE", "abc")]
    [InlineData("CARNET_RATE_LIMIT_PER_MINUTE", "100000")]
    [InlineData("USERS_RATE_LIMIT_PER_MINUTE", "0")]
    [InlineData("USERS_RATE_LIMIT_PER_MINUTE", "abc")]
    [InlineData("USERS_RATE_LIMIT_PER_MINUTE", "100000")]
    public void Invalid_proxy_or_rate_limit_settings_fail_fast_without_echoing_the_value(string key, string value)
    {
        var builder = ApplicationBootstrap.CreateBuilder([], new WebApplicationOptions { EnvironmentName = Environments.Development });
        builder.Configuration.AddInMemoryCollection(ValidConfiguration().Append(new KeyValuePair<string, string?>(key, value)));

        var error = Assert.Throws<InvalidOperationException>(() => ApplicationBootstrap.ConfigureCompositionRoot(builder));

        Assert.Contains(key, error.Message, StringComparison.Ordinal);
        if (value.Any(char.IsLetter))
        {
            Assert.DoesNotContain(value, error.Message, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void Valid_trusted_proxies_are_accepted()
    {
        var builder = ApplicationBootstrap.CreateBuilder([], new WebApplicationOptions { EnvironmentName = Environments.Development });
        builder.Configuration.AddInMemoryCollection(ValidConfiguration().Append(new KeyValuePair<string, string?>("TRUSTED_PROXIES", "10.0.0.5, 2001:db8::1")));

        ApplicationBootstrap.ConfigureCompositionRoot(builder);
    }

    private static Dictionary<string, string?> ValidConfiguration() => new()
    {
        ["MONGODB_URI"] = "mongodb://usuario:clave-test@test.invalid:27017/",
        ["MONGODB_DATABASE"] = "petmate-tests",
        ["REDIS_AUTH_CONNECTION"] = "redis-auth-test:6380,password=test-auth-pw,ssl=True",
        ["REDIS_CACHE_CONNECTION"] = "redis-cache-test:6381,password=test-cache-pw,ssl=True",
        ["JWT_ISSUER"] = "https://issuer-prueba.local",
        ["JWT_AUDIENCE"] = "petmate-api-prueba",
        ["JWT_SIGNING_KEY"] = "clave-de-prueba-0123456789-abcdefghijkl",
    };
}
