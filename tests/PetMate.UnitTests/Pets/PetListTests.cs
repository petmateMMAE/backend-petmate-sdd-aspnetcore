using PetMate.Application.Pagination;
using PetMate.Application.Pets;
using PetMate.Domain.Pets;
using PetMate.TestDoubles;

namespace PetMate.UnitTests.Pets;

/// <summary>Convención de paginación (pm-047) y listado de mascotas propias (pm-041).</summary>
public sealed class PetListTests
{
    private readonly ManualClock clock = new(new DateTimeOffset(2026, 11, 8, 15, 0, 0, TimeSpan.Zero));
    private readonly InMemoryPetRepository pets = new();

    private async Task Create(Guid owner, string name)
    {
        var details = new PetDetails(name, "Perro", null, null, PetSex.Male, null);
        await new CreatePetHandler(pets, clock).Handle(new CreatePetCommand(owner, details), CancellationToken.None);
        clock.Advance(TimeSpan.FromMinutes(1));
    }

    [Fact]
    public void The_convention_uses_the_approved_values()
    {
        Assert.Equal(1, PaginationConvention.DefaultPage);
        Assert.Equal(20, PaginationConvention.DefaultPageSize);
        Assert.Equal(50, PaginationConvention.MaxPageSize);
        Assert.Equal(1, PaginationConvention.ResolvePage(null));
        Assert.Equal(20, PaginationConvention.ResolvePageSize(null));
        Assert.Empty(PaginationConvention.Check(null, null));
        Assert.Empty(PaginationConvention.Check(1, 1));
        Assert.Empty(PaginationConvention.Check(int.MaxValue, 50));
    }

    [Theory]
    [InlineData(0, null, "page")]
    [InlineData(-1, null, "page")]
    [InlineData(int.MinValue, null, "page")]
    [InlineData(null, 0, "pageSize")]
    [InlineData(null, 51, "pageSize")]
    [InlineData(null, -3, "pageSize")]
    [InlineData(null, int.MaxValue, "pageSize")]
    public void Out_of_range_values_are_rejected_naming_the_parameter(int? page, int? pageSize, string field)
        => Assert.Equal(field, Assert.Single(PaginationConvention.Check(page, pageSize)).Field);

    [Fact]
    public void Both_invalid_parameters_are_reported_together()
        => Assert.Equal(["page", "pageSize"], PaginationConvention.Check(0, 99).Select(f => f.Field));

    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 1)]
    [InlineData(20, 1)]
    [InlineData(21, 2)]
    [InlineData(100, 5)]
    public void Total_pages_rounds_up(long total, int expectedPages)
        => Assert.Equal(expectedPages, new PagedResult<int>([], 1, 20, total).TotalPages);

    [Fact]
    public async Task List_returns_only_the_owner_pets_newest_first_with_the_default_page()
    {
        var ana = Guid.NewGuid();
        await Create(ana, "Primera");
        await Create(Guid.NewGuid(), "Ajena");
        await Create(ana, "Segunda");

        var page = await new ListMyPetsHandler(pets).Handle(new ListMyPetsQuery(ana, null, null), CancellationToken.None);

        Assert.Equal(["Segunda", "Primera"], page.Items.Select(p => p.Name));
        Assert.Equal((1, 20, 2L, 1), (page.Page, page.PageSize, page.TotalCount, page.TotalPages));
    }

    [Fact]
    public async Task List_pages_without_repeating_or_losing_pets()
    {
        var ana = Guid.NewGuid();
        for (var i = 0; i < 5; i++)
        {
            await Create(ana, $"M{i}");
        }

        var handler = new ListMyPetsHandler(pets);
        var one = await handler.Handle(new ListMyPetsQuery(ana, 1, 2), CancellationToken.None);
        var two = await handler.Handle(new ListMyPetsQuery(ana, 2, 2), CancellationToken.None);
        var three = await handler.Handle(new ListMyPetsQuery(ana, 3, 2), CancellationToken.None);

        Assert.Equal(["M4", "M3", "M2", "M1", "M0"], one.Items.Concat(two.Items).Concat(three.Items).Select(p => p.Name));
        Assert.Equal(3, one.TotalPages);
        Assert.Single(three.Items);
    }

    [Theory]
    [InlineData(3, 20)]
    [InlineData(int.MaxValue, 50)]
    public async Task A_page_beyond_the_last_is_empty_and_never_overflows(int page, int pageSize)
    {
        var ana = Guid.NewGuid();
        await Create(ana, "Unica");

        var result = await new ListMyPetsHandler(pets).Handle(new ListMyPetsQuery(ana, page, pageSize), CancellationToken.None);

        Assert.Empty(result.Items);
        Assert.Equal(1, result.TotalCount);
    }

    [Fact]
    public async Task A_user_without_pets_gets_an_empty_page_and_never_sees_other_pets()
    {
        await Create(Guid.NewGuid(), "Ajena");

        var result = await new ListMyPetsHandler(pets).Handle(new ListMyPetsQuery(Guid.NewGuid(), null, null), CancellationToken.None);

        Assert.Empty(result.Items);
        Assert.Equal((0L, 0), (result.TotalCount, result.TotalPages));
    }

    [Fact]
    public async Task The_query_validator_applies_the_convention()
    {
        var validator = new ListMyPetsQueryValidator();

        Assert.Empty(await validator.Validate(new ListMyPetsQuery(Guid.NewGuid(), 2, 50), CancellationToken.None));
        Assert.Equal("pageSize", Assert.Single(await validator.Validate(new ListMyPetsQuery(Guid.NewGuid(), 1, 51), CancellationToken.None)).Field);
    }
}
