using PetMate.Domain.Common;
using PetMate.Domain.Users;

namespace PetMate.UnitTests.Domain;

public sealed class UserTests
{
    private static readonly DateTime Now = new(2026, 11, 3, 12, 0, 0, DateTimeKind.Utc);
    private const string ValidHash = "$argon2id$v=19$m=65536,t=3,p=1$c2FsdHNhbHRzYWx0$aGFzaGhhc2hoYXNoaGFzaGhhc2hoYXNoaGFzaGhhc2g";

    [Theory]
    [InlineData("  Ana.Perez@Example.COM ", "ana.perez@example.com")]
    [InlineData("a@b.co", "a@b.co")]
    public void Email_is_trimmed_and_lowercased(string raw, string expected)
        => Assert.Equal(expected, Email.Create(raw).Value);

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("sin-arroba")]
    [InlineData("@dominio.com")]
    [InlineData("a@@b.com")]
    [InlineData("a b@dominio.com")]
    [InlineData("a@dominio")]
    [InlineData("a@.dominio.com")]
    [InlineData("a@dominio..com")]
    public void Email_rejects_invalid_values(string? raw)
    {
        Assert.False(Email.TryCreate(raw, out var email));
        Assert.Null(email);
        Assert.Throws<DomainException>(() => Email.Create(raw));
    }

    [Fact]
    public void Email_rejects_values_longer_than_254()
        => Assert.Throws<DomainException>(() => Email.Create(new string('a', 250) + "@b.co"));

    [Fact]
    public void Equal_emails_with_different_case_are_the_same_identity()
        => Assert.Equal(Email.Create("A@B.com"), Email.Create("a@b.COM"));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("plain-password")]
    [InlineData("$argon2i$v=19$m=1,t=1,p=1$a$b")]
    [InlineData("$argon2id$con espacio")]
    public void PasswordHash_rejects_non_argon2id_values(string? value)
        => Assert.Throws<DomainException>(() => PasswordHash.Create(value));

    [Fact]
    public void PasswordHash_does_not_leak_value_in_ToString()
        => Assert.DoesNotContain("argon2id", PasswordHash.Create(ValidHash).ToString(), StringComparison.Ordinal);

    [Fact]
    public void RegisterOwner_creates_an_owner_and_never_business()
    {
        var user = User.RegisterOwner(Email.Create("ana@example.com"), PasswordHash.Create(ValidHash), Now);

        Assert.Equal(UserRole.Owner, user.Role);
        Assert.NotEqual(Guid.Empty, user.Id);
        Assert.Equal(Now, user.CreatedAtUtc);
        Assert.Equal(Now, user.UpdatedAtUtc);
    }

    [Fact]
    public void RegisterOwner_requires_utc_dates()
        => Assert.Throws<DomainException>(() => User.RegisterOwner(
            Email.Create("ana@example.com"), PasswordHash.Create(ValidHash), DateTime.SpecifyKind(Now, DateTimeKind.Unspecified)));

    [Fact]
    public void Roles_are_exactly_owner_and_business()
        => Assert.Equal(["Business", "Owner"], Enum.GetNames<UserRole>().Order(StringComparer.Ordinal));

    [Fact]
    public void User_exposes_no_public_role_setter()
    {
        var roleProperty = typeof(User).GetProperty(nameof(User.Role))!;

        Assert.False(roleProperty.SetMethod?.IsPublic ?? false);
    }

    [Fact]
    public void Restore_rejects_undefined_role_and_empty_id()
    {
        var email = Email.Create("ana@example.com");
        var hash = PasswordHash.Create(ValidHash);

        Assert.Throws<DomainException>(() => User.Restore(Guid.NewGuid(), email, hash, (UserRole)99, Now, Now));
        Assert.Throws<DomainException>(() => User.Restore(Guid.Empty, email, hash, UserRole.Owner, Now, Now));
    }

    [Fact]
    public void ChangePasswordHash_replaces_hash_and_updates_timestamp()
    {
        var user = User.RegisterOwner(Email.Create("ana@example.com"), PasswordHash.Create(ValidHash), Now);
        var newHash = PasswordHash.Create(ValidHash.Replace("t=3", "t=4", StringComparison.Ordinal));
        var later = Now.AddMinutes(5);

        user.ChangePasswordHash(newHash, later);

        Assert.Equal(newHash, user.PasswordHash);
        Assert.Equal(later, user.UpdatedAtUtc);
        Assert.Equal(Now, user.CreatedAtUtc);
    }

    // ---------- pm-038: nombre visible ----------

    [Fact]
    public void New_owner_has_no_display_name()
        => Assert.Null(User.RegisterOwner(Email.Create("ana@example.com"), PasswordHash.Create(ValidHash), Now).DisplayName);

    [Fact]
    public void ChangeDisplayName_trims_and_updates_timestamp()
    {
        var user = User.RegisterOwner(Email.Create("ana@example.com"), PasswordHash.Create(ValidHash), Now);
        var later = Now.AddMinutes(1);

        user.ChangeDisplayName("  Ana Pérez  ", later);

        Assert.Equal("Ana Pérez", user.DisplayName);
        Assert.Equal(later, user.UpdatedAtUtc);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("Ana\nPérez")]
    [InlineData("Ana\u0000")]
    public void ChangeDisplayName_rejects_blank_or_control_characters(string value)
    {
        var user = User.RegisterOwner(Email.Create("ana@example.com"), PasswordHash.Create(ValidHash), Now);

        Assert.Throws<DomainException>(() => user.ChangeDisplayName(value, Now));
        Assert.Null(user.DisplayName);
    }

    [Fact]
    public void DisplayName_accepts_80_characters_and_rejects_81()
    {
        Assert.True(User.IsValidDisplayName(new string('a', User.MaxDisplayNameLength)));
        Assert.False(User.IsValidDisplayName(new string('a', User.MaxDisplayNameLength + 1)));
    }

    [Fact]
    public void Restore_keeps_display_name_and_tolerates_its_absence()
    {
        var email = Email.Create("ana@example.com");
        var hash = PasswordHash.Create(ValidHash);

        Assert.Equal("Ana", User.Restore(Guid.NewGuid(), email, hash, UserRole.Owner, Now, Now, "Ana").DisplayName);
        Assert.Null(User.Restore(Guid.NewGuid(), email, hash, UserRole.Owner, Now, Now).DisplayName);
    }
}
