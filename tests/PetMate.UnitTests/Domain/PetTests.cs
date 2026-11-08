using PetMate.Application.Pets;
using PetMate.Domain.Common;
using PetMate.Domain.Pets;

namespace PetMate.UnitTests.Domain;

/// <summary>pm-039: modelo Pet (nombre, especie, raza, fecha de nacimiento, sexo y foto) y sus validaciones.</summary>
public sealed class PetTests
{
    private static readonly DateTime Now = new(2026, 11, 8, 15, 0, 0, DateTimeKind.Utc);
    private static readonly DateOnly Today = DateOnly.FromDateTime(Now);
    private static readonly Guid Owner = Guid.NewGuid();

    private static PetDetails Valid(
        string name = "Firulais",
        string species = "Perro",
        string? breed = "Mestizo",
        DateOnly? birthDate = null,
        PetSex sex = PetSex.Male,
        string? photoUrl = "https://cdn.example.com/firulais.jpg")
        => new(name, species, breed, birthDate ?? new DateOnly(2020, 5, 1), sex, photoUrl);

    [Fact]
    public void Create_assigns_owner_id_and_utc_dates_and_trims_texts()
    {
        var pet = Pet.Create(Owner, Valid(name: "  Firulais  ", species: " Perro ", breed: "  "), Now);

        Assert.NotEqual(Guid.Empty, pet.Id);
        Assert.Equal(Owner, pet.OwnerId);
        Assert.Equal("Firulais", pet.Name);
        Assert.Equal("Perro", pet.Species);
        Assert.Null(pet.Breed);
        Assert.Equal(Now, pet.CreatedAtUtc);
        Assert.Equal(Now, pet.UpdatedAtUtc);
    }

    [Fact]
    public void Optional_fields_can_be_omitted()
    {
        var pet = Pet.Create(Owner, new PetDetails("Michi", "Gato", null, null, PetSex.Unknown, null), Now);

        Assert.Null(pet.Breed);
        Assert.Null(pet.BirthDate);
        Assert.Null(pet.PhotoUrl);
        Assert.Equal(PetSex.Unknown, pet.Sex);
    }

    [Fact]
    public void Create_requires_an_owner_and_utc_time()
    {
        Assert.Throws<DomainException>(() => Pet.Create(Guid.Empty, Valid(), Now));
        Assert.Throws<DomainException>(() => Pet.Create(Owner, Valid(), DateTime.SpecifyKind(Now, DateTimeKind.Local)));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("nombre\u0007con-control")]
    public void Invalid_name_is_rejected(string name)
        => Assert.Throws<DomainException>(() => Pet.Create(Owner, Valid(name: name), Now));

    [Fact]
    public void Text_lengths_are_enforced_at_the_boundaries()
    {
        Pet.Create(Owner, Valid(name: new string('a', Pet.MaxNameLength), species: new string('b', Pet.MaxSpeciesLength), breed: new string('c', Pet.MaxBreedLength)), Now);

        Assert.Throws<DomainException>(() => Pet.Create(Owner, Valid(name: new string('a', Pet.MaxNameLength + 1)), Now));
        Assert.Throws<DomainException>(() => Pet.Create(Owner, Valid(species: new string('b', Pet.MaxSpeciesLength + 1)), Now));
        Assert.Throws<DomainException>(() => Pet.Create(Owner, Valid(breed: new string('c', Pet.MaxBreedLength + 1)), Now));
        Assert.Throws<DomainException>(() => Pet.Create(Owner, Valid(species: " "), Now));
    }

    [Fact]
    public void Birth_date_cannot_be_in_the_future_or_older_than_50_years()
    {
        Pet.Create(Owner, Valid(birthDate: Today), Now);
        Pet.Create(Owner, Valid(birthDate: Today.AddYears(-Pet.MaxAgeInYears)), Now);

        Assert.Throws<DomainException>(() => Pet.Create(Owner, Valid(birthDate: Today.AddDays(1)), Now));
        Assert.Throws<DomainException>(() => Pet.Create(Owner, Valid(birthDate: Today.AddYears(-Pet.MaxAgeInYears).AddDays(-1)), Now));
    }

    [Fact]
    public void Sex_must_be_a_defined_value()
        => Assert.Throws<DomainException>(() => Pet.Create(Owner, Valid(sex: (PetSex)99), Now));

    [Theory]
    [InlineData("http://cdn.example.com/foto.jpg")]
    [InlineData("ftp://cdn.example.com/foto.jpg")]
    [InlineData("/fotos/local.jpg")]
    [InlineData("no es una url")]
    [InlineData("https://usuario:clave@cdn.example.com/foto.jpg")]
    public void Photo_url_must_be_absolute_https_without_credentials(string url)
        => Assert.Throws<DomainException>(() => Pet.Create(Owner, Valid(photoUrl: url), Now));

    [Fact]
    public void Update_replaces_editable_fields_but_keeps_identity_owner_and_creation_date()
    {
        var pet = Pet.Create(Owner, Valid(), Now);
        var later = Now.AddHours(2);

        pet.Update(new PetDetails("Max", "Perro", "Labrador", new DateOnly(2021, 1, 10), PetSex.Male, null), later);

        Assert.Equal("Max", pet.Name);
        Assert.Equal("Labrador", pet.Breed);
        Assert.Null(pet.PhotoUrl);
        Assert.Equal(Owner, pet.OwnerId);
        Assert.Equal(Now, pet.CreatedAtUtc);
        Assert.Equal(later, pet.UpdatedAtUtc);
    }

    [Fact]
    public void Invalid_update_leaves_the_pet_unchanged()
    {
        var pet = Pet.Create(Owner, Valid(), Now);

        Assert.Throws<DomainException>(() => pet.Update(Valid(name: ""), Now.AddHours(1)));

        Assert.Equal("Firulais", pet.Name);
        Assert.Equal(Now, pet.UpdatedAtUtc);
    }

    [Fact]
    public void Restore_revalidates_invariants_and_date_order()
    {
        var restored = Pet.Restore(Guid.NewGuid(), Owner, Valid(), Now, Now.AddMinutes(5));
        Assert.Equal("Firulais", restored.Name);

        Assert.Throws<DomainException>(() => Pet.Restore(Guid.NewGuid(), Owner, Valid(), Now, Now.AddMinutes(-5)));
        Assert.Throws<DomainException>(() => Pet.Restore(Guid.Empty, Owner, Valid(), Now, Now));
        Assert.Throws<DomainException>(() => Pet.Restore(Guid.NewGuid(), Guid.Empty, Valid(), Now, Now));
        Assert.Throws<DomainException>(() => Pet.Restore(Guid.NewGuid(), Owner, Valid(name: ""), Now, Now));
    }

    [Fact]
    public void Validation_reports_one_spanish_error_per_invalid_field_without_echoing_values()
    {
        var details = new PetDetails("", " ", new string('r', Pet.MaxBreedLength + 1), Today.AddDays(3), (PetSex)0, "http://inseguro.example.com/x.jpg");

        var failures = PetValidation.Check(details, Today);

        Assert.Equal(["birthDate", "breed", "name", "photoUrl", "sex", "species"], failures.Select(f => f.Field).Order(StringComparer.Ordinal));
        Assert.All(failures, failure => Assert.DoesNotContain("inseguro", failure.Message, StringComparison.Ordinal));
    }

    [Fact]
    public void Validation_accepts_valid_details_and_reports_missing_body()
    {
        Assert.Empty(PetValidation.Check(Valid(), Today));
        Assert.Single(PetValidation.Check(null, Today));
    }
}
