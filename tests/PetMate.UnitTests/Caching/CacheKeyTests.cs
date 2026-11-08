using PetMate.Application.Caching;

namespace PetMate.UnitTests.Caching;

/// <summary>Convención de claves y TTL de la caché distribuida (pm-050 CA-02).</summary>
public sealed class CacheKeyTests
{
    [Fact]
    public void A_key_follows_the_documented_convention_and_is_versioned()
    {
        var id = Guid.NewGuid().ToString();

        Assert.Equal($"petmate:cache:v1:catalog:{id}", CacheKey.Create("catalog", id).Value);
        Assert.Equal("petmate:cache:v2:catalog:abc-1", CacheKey.Create("catalog", "abc-1", 2).ToString());
        Assert.StartsWith(CacheKey.Prefix, CacheKey.Create("breeds", "1").Value, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("", "id")]
    [InlineData("Catalog", "id")]
    [InlineData("catalog:x", "id")]
    [InlineData("a b", "id")]
    [InlineData("catalog", "")]
    [InlineData("catalog", "ana@example.com")]
    [InlineData("catalog", "con espacio")]
    [InlineData("catalog", "a:b")]
    [InlineData("catalog", "ñandú")]
    public void Keys_with_personal_data_separators_or_unsafe_characters_are_rejected(string area, string id)
        => Assert.Throws<ArgumentException>(() => CacheKey.Create(area, id));

    [Fact]
    public void Keys_reject_overlong_parts_and_invalid_versions()
    {
        Assert.Throws<ArgumentException>(() => CacheKey.Create(new string('a', 33), "id"));
        Assert.Throws<ArgumentException>(() => CacheKey.Create("catalog", new string('a', 65)));
        Assert.Throws<ArgumentOutOfRangeException>(() => CacheKey.Create("catalog", "id", 0));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(999)]
    [InlineData(3_600_001)]
    [InlineData(-5)]
    public void The_ttl_is_mandatory_and_bounded(int milliseconds)
        => Assert.Throws<ArgumentOutOfRangeException>(() => CacheTtl.EnsureValid(TimeSpan.FromMilliseconds(milliseconds)));

    [Theory]
    [InlineData(1_000)]
    [InlineData(60_000)]
    [InlineData(3_600_000)]
    public void A_ttl_within_the_bounds_is_accepted(int milliseconds)
        => CacheTtl.EnsureValid(TimeSpan.FromMilliseconds(milliseconds));
}
