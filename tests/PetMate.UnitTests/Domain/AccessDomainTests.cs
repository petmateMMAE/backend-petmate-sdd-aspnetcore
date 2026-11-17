using PetMate.Domain.Access;
using PetMate.Domain.Businesses;
using PetMate.Domain.Common;

namespace PetMate.UnitTests.Domain;

/// <summary>pm-118, pm-122, pm-124, pm-129: búsqueda por nombre, autorización de acceso y registro de escaneos.</summary>
public sealed class AccessDomainTests
{
    private static readonly DateTime Now = new(2026, 11, 10, 12, 0, 0, DateTimeKind.Utc);

    [Theory]
    [InlineData("  Veterinaria   PEÑALOSA ", "veterinaria penalosa")]
    [InlineData("Estética Luna", "estetica luna")]
    [InlineData("ÁÉÍÓÚ Ü Ñ", "aeiou u n")]
    [InlineData("ya-sin acentos", "ya-sin acentos")]
    public void Search_key_ignores_case_accents_and_extra_spaces(string input, string expected)
        => Assert.Equal(expected, Business.SearchKey(input));

    [Fact]
    public void A_grant_lasts_the_chosen_duration_and_dedupes_the_data()
    {
        var grant = AccessGrant.Create(Guid.NewGuid(), Guid.NewGuid(), null, [PetDataCategory.Weights, PetDataCategory.Vaccines, PetDataCategory.Vaccines], 1440, Now);

        Assert.Equal(Now.AddDays(1), grant.ExpiresAtUtc);
        Assert.Equal([PetDataCategory.Vaccines, PetDataCategory.Weights], grant.Data);
        Assert.True(AccessGrant.IncludesAllergies);
        Assert.Null(grant.BusinessId);
    }

    [Theory]
    [InlineData(30, true)]
    [InlineData(1440, true)]
    [InlineData(10080, true)]
    [InlineData(0, false)]
    [InlineData(60, false)]
    [InlineData(-30, false)]
    public void Only_the_three_durations_are_valid(int minutes, bool valid)
    {
        Assert.Equal(valid, AccessGrant.IsValidDuration(minutes));
        if (!valid)
        {
            Assert.Throws<DomainException>(() => AccessGrant.Create(Guid.NewGuid(), Guid.NewGuid(), null, [], minutes, Now));
        }
    }

    [Fact]
    public void An_expired_grant_stops_granting_even_though_it_still_exists_and_the_boundary_is_exclusive()
    {
        var grant = AccessGrant.Create(Guid.NewGuid(), Guid.NewGuid(), null, [], 30, Now);

        Assert.True(grant.IsActiveAt(Now.AddMinutes(29)));
        Assert.False(grant.IsActiveAt(Now.AddMinutes(30)));
        Assert.False(grant.IsActiveAt(Now.AddHours(2)));
    }

    [Fact]
    public void A_grant_covers_its_business_or_every_business_when_it_has_none()
    {
        var business = Guid.NewGuid();
        var specific = AccessGrant.Create(Guid.NewGuid(), Guid.NewGuid(), business, [], 30, Now);
        var any = AccessGrant.Create(Guid.NewGuid(), Guid.NewGuid(), null, [], 30, Now);

        Assert.True(specific.Covers(business));
        Assert.False(specific.Covers(Guid.NewGuid()));
        Assert.True(any.Covers(Guid.NewGuid()));
    }

    [Fact]
    public void Cancelling_ends_the_grant_immediately_and_never_extends_an_expired_one()
    {
        var grant = AccessGrant.Create(Guid.NewGuid(), Guid.NewGuid(), null, [], 10080, Now);

        grant.Cancel(Now.AddMinutes(5));
        Assert.False(grant.IsActiveAt(Now.AddMinutes(5)));
        Assert.True(grant.IsActiveAt(Now.AddMinutes(4)));

        var expired = AccessGrant.Create(Guid.NewGuid(), Guid.NewGuid(), null, [], 30, Now);
        expired.Cancel(Now.AddDays(5));
        Assert.Equal(Now.AddMinutes(30), expired.ExpiresAtUtc);
    }

    [Fact]
    public void A_grant_rejects_undefined_categories_and_non_utc_dates()
    {
        Assert.Throws<DomainException>(() => AccessGrant.Create(Guid.NewGuid(), Guid.NewGuid(), null, [(PetDataCategory)0], 30, Now));
        Assert.Throws<DomainException>(() => AccessGrant.Create(Guid.NewGuid(), Guid.NewGuid(), null, [], 30, DateTime.SpecifyKind(Now, DateTimeKind.Unspecified)));
        Assert.Throws<DomainException>(() => AccessGrant.Create(Guid.Empty, Guid.NewGuid(), null, [], 30, Now));
    }

    [Fact]
    public void A_scan_log_keeps_business_pet_and_date()
    {
        var log = ScanLog.Record(Guid.NewGuid(), "Firulais", null, Guid.NewGuid(), Now);

        Assert.Equal(Now, log.AtUtc);
        Assert.Throws<DomainException>(() => ScanLog.Record(Guid.Empty, "X", null, Guid.NewGuid(), Now));
    }
}
