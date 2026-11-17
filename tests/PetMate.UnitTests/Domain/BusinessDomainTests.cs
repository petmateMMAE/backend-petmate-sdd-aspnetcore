using PetMate.Domain.Businesses;
using PetMate.Domain.Common;

namespace PetMate.UnitTests.Domain;

/// <summary>pm-105 a pm-107, pm-112: modelo del negocio, horario estructurado y servicios.</summary>
public sealed class BusinessDomainTests
{
    private static readonly DateTime Now = new(2026, 11, 10, 12, 0, 0, DateTimeKind.Utc);
    private static readonly Guid Owner = Guid.NewGuid();

    private static ScheduleInput Week(params (string Day, string Open, string Close)[] ranges)
        => new(ranges.GroupBy(r => r.Day).ToDictionary(g => g.Key, g => (IReadOnlyList<(string Open, string Close)>)g.Select(r => (r.Open, r.Close)).ToArray()));

    private static BusinessDetails Valid(BusinessType type = BusinessType.Store, string? license = null, ScheduleInput? schedule = null)
        => new(type, "  Pet Shop Sol ", "55 1234 5678", null, "Av. Reforma 10", "  Todo para tu mascota ", 19.4326, -99.1332, license, schedule);

    [Fact]
    public void Create_normalizes_and_stores_every_public_field_and_the_private_license()
    {
        var business = Business.Create(Owner, Valid(BusinessType.Veterinary, " 123456 ", Week(("monday", "09:00", "18:00"))), Now);

        Assert.Equal("Pet Shop Sol", business.Name);
        Assert.Equal("5512345678", business.Phone);
        Assert.Null(business.WhatsApp);
        Assert.Equal("Todo para tu mascota", business.Description);
        Assert.Equal(BusinessType.Veterinary, business.Type);
        Assert.Equal(Owner, business.OwnerId);
        Assert.Equal(19.4326, business.Latitude);
        Assert.Equal(" 123456 ".Trim(), business.LicenseNumber);
        Assert.Single(business.Schedule.RangesFor(DayOfWeek.Monday));
    }

    [Fact]
    public void A_veterinary_needs_the_license_and_other_types_do_not()
    {
        Assert.Contains("licenseNumber", Valid(BusinessType.Veterinary, null).InvalidFields());
        Assert.Contains("licenseNumber", Valid(BusinessType.Veterinary, "  ").InvalidFields());
        Assert.Empty(Valid(BusinessType.Store, null).InvalidFields());
        Assert.Empty(Valid(BusinessType.Grooming, null).InvalidFields());
        Assert.Empty(Valid(BusinessType.Veterinary, "ABC-1").InvalidFields());
    }

    [Theory]
    [InlineData(-90.1, 0, "latitude")]
    [InlineData(90.1, 0, "latitude")]
    [InlineData(0, -180.1, "longitude")]
    [InlineData(0, 180.1, "longitude")]
    [InlineData(double.NaN, 0, "latitude")]
    public void Coordinates_out_of_range_are_rejected(double lat, double lng, string field)
        => Assert.Contains(field, Business.InvalidLocationFields(lat, lng));

    [Fact]
    public void Missing_or_invalid_fields_are_named()
    {
        var details = new BusinessDetails(null, " ", "123", "abc", " ", new string('d', 501), null, null, null, Week(("funday", "09:00", "18:00")));

        Assert.Equal(
            ["address", "description", "latitude", "longitude", "name", "phone", "schedule", "type", "whatsApp"],
            details.InvalidFields().Order(StringComparer.Ordinal));
        Assert.Throws<DomainException>(() => Business.Create(Owner, details, Now));
    }

    [Fact]
    public void Update_changes_the_profile_but_never_the_type_or_the_location()
    {
        var business = Business.Create(Owner, Valid(BusinessType.Grooming), Now);

        business.UpdateProfile(new BusinessProfile("Nuevo nombre", null, "5598765432", "5511112222", Week(("tuesday", "10:00", "14:00"))), Now.AddMinutes(5));

        Assert.Equal("Nuevo nombre", business.Name);
        Assert.Null(business.Description);
        Assert.Equal("5511112222", business.WhatsApp);
        Assert.Equal(BusinessType.Grooming, business.Type);
        Assert.Equal(19.4326, business.Latitude);
        Assert.Equal(Now.AddMinutes(5), business.UpdatedAtUtc);
        Assert.Empty(business.Schedule.RangesFor(DayOfWeek.Monday));
    }

    [Fact]
    public void MoveTo_validates_and_moves_only_the_coordinates()
    {
        var business = Business.Create(Owner, Valid(), Now);

        business.MoveTo(20.5, -100.4, Now.AddMinutes(1));

        Assert.Equal((20.5, -100.4), (business.Latitude, business.Longitude));
        Assert.Throws<DomainException>(() => business.MoveTo(95, 0, Now));
        Assert.Equal(20.5, business.Latitude);
    }

    // ---------- Horario (pm-106) ----------

    [Fact]
    public void A_valid_schedule_is_ordered_per_day_and_a_day_without_ranges_is_closed()
    {
        Assert.True(WeeklySchedule.TryParse(Week(("monday", "14:00", "18:00"), ("monday", "09:00", "13:00")).Days, out var schedule));

        Assert.Equal([new TimeOnly(9, 0), new TimeOnly(14, 0)], schedule!.RangesFor(DayOfWeek.Monday).Select(r => r.Open));
        Assert.Empty(schedule.RangesFor(DayOfWeek.Sunday));
        Assert.False(WeeklySchedule.Closed.IsOpenAt(DateTimeOffset.UtcNow, WeeklySchedule.MexicoCity));
    }

    [Theory]
    [InlineData("18:00", "09:00")]
    [InlineData("09:00", "09:00")]
    [InlineData("9:00", "18:00")]
    [InlineData("09:00", "25:00")]
    [InlineData("nueve", "18:00")]
    public void An_invalid_range_is_rejected(string open, string close)
        => Assert.False(WeeklySchedule.TryParse(Week(("monday", open, close)).Days, out _));

    [Fact]
    public void Overlapping_ranges_of_the_same_day_are_rejected_but_touching_ones_are_fine()
    {
        Assert.False(WeeklySchedule.TryParse(Week(("monday", "09:00", "14:00"), ("monday", "13:00", "18:00")).Days, out _));
        Assert.True(WeeklySchedule.TryParse(Week(("monday", "09:00", "14:00"), ("monday", "14:00", "18:00")).Days, out _));
        Assert.True(WeeklySchedule.TryParse(Week(("monday", "09:00", "14:00"), ("tuesday", "09:00", "14:00")).Days, out _));
    }

    [Fact]
    public void More_than_six_ranges_in_a_day_are_rejected()
    {
        var many = Enumerable.Range(0, 7).Select(i => ("monday", $"{8 + i:00}:00", $"{8 + i:00}:30")).ToArray();

        Assert.False(WeeklySchedule.TryParse(Week(many).Days, out _));
    }

    [Theory]
    [InlineData("2026-11-09T16:00:00Z", true)]   // lunes 10:00 en Ciudad de México
    [InlineData("2026-11-09T15:00:00Z", true)]   // lunes 09:00: la apertura cuenta
    [InlineData("2026-11-09T14:59:00Z", false)]  // lunes 08:59
    [InlineData("2026-11-10T00:00:00Z", false)]  // lunes 18:00: el cierre no cuenta
    [InlineData("2026-11-10T02:00:00Z", false)]  // lunes 20:00 local, aunque en UTC ya sea martes
    [InlineData("2026-11-10T16:00:00Z", false)]  // martes: sin franjas, cerrado
    public void Open_now_is_computed_in_the_Mexico_City_zone(string instant, bool expected)
    {
        Assert.True(WeeklySchedule.TryParse(Week(("monday", "09:00", "18:00")).Days, out var schedule));

        Assert.Equal(expected, schedule!.IsOpenAt(DateTimeOffset.Parse(instant, System.Globalization.CultureInfo.InvariantCulture), WeeklySchedule.MexicoCity));
    }

    [Fact]
    public void The_early_utc_morning_of_tuesday_still_belongs_to_monday_locally()
    {
        Assert.True(WeeklySchedule.TryParse(Week(("monday", "09:00", "23:30")).Days, out var schedule));

        // 2026-11-10 03:00 UTC = lunes 21:00 en Ciudad de México.
        Assert.True(schedule!.IsOpenAt(DateTimeOffset.Parse("2026-11-10T03:00:00Z", System.Globalization.CultureInfo.InvariantCulture), WeeklySchedule.MexicoCity));
    }

    // ---------- Servicios (pm-112) ----------

    [Fact]
    public void A_service_needs_only_a_name_and_without_a_price_it_is_null_not_zero()
    {
        var service = BusinessService.Create(Guid.NewGuid(), new ServiceDetails("  Baño  ", null, null), Now);

        Assert.Equal("Baño", service.Name);
        Assert.Null(service.Price);
        Assert.Null(service.Description);
    }

    [Theory]
    [InlineData(0, true)]
    [InlineData(250.5, true)]
    [InlineData(-1, false)]
    [InlineData(10.123, false)]
    [InlineData(1_000_001, false)]
    public void Price_must_be_zero_or_more_with_two_decimals(double price, bool valid)
        => Assert.Equal(valid, new ServiceDetails("X", null, (decimal)price).InvalidFields().Count == 0);

    [Fact]
    public void Service_update_replaces_the_data_and_invalid_data_is_rejected()
    {
        var service = BusinessService.Create(Guid.NewGuid(), new ServiceDetails("Baño", "Con secado", 200m), Now);

        service.Update(new ServiceDetails("Baño y corte", null, null), Now.AddMinutes(1));

        Assert.Equal(("Baño y corte", null, null), (service.Name, service.Description, service.Price));
        Assert.Throws<DomainException>(() => service.Update(new ServiceDetails(" ", null, null), Now));
        Assert.Throws<DomainException>(() => BusinessService.Create(Guid.Empty, new ServiceDetails("X", null, null), Now));
    }
}
