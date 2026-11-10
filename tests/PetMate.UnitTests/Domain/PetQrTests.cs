using PetMate.Domain.Pets;

namespace PetMate.UnitTests.Domain;

public sealed class PetQrTests
{
    private static readonly DateTime Now = new(2026, 11, 9, 12, 0, 0, DateTimeKind.Utc);

    private static PetDetails Details() => new("Firulais", "Perro", null, null, PetSex.Unknown, null);

    [Fact]
    public void Generate_returns_a_43_char_base64url_token_and_an_8_char_unambiguous_code()
    {
        var qr = PetQr.Generate();

        Assert.Equal(PetQr.TokenLength, qr.Token.Length);
        Assert.Matches("^[A-Za-z0-9_-]{43}$", qr.Token);
        Assert.Equal(PetQr.CodeLength, qr.Code.Length);
        Assert.All(qr.Code, c => Assert.Contains(c, PetQr.CodeAlphabet));
        Assert.True(PetQr.IsWellFormed(qr.Token, qr.Code));
    }

    [Fact]
    public void Generated_values_do_not_repeat()
    {
        var all = Enumerable.Range(0, 500).Select(_ => PetQr.Generate()).ToArray();

        Assert.Equal(500, all.Select(q => q.Token).Distinct().Count());
        Assert.Equal(500, all.Select(q => q.Code).Distinct().Count());
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("corto", "ABCD2345")]
    [InlineData("0123456789012345678901234567890123456789012", "ABCDEFGI")]
    [InlineData("0123456789012345678901234567890123456789012", "ABCD0O1I")]
    [InlineData("0123456789012345678901234567890123456789+/=", "ABCD2345")]
    public void Malformed_values_are_not_accepted(string? token, string? code)
        => Assert.False(PetQr.IsWellFormed(token, code));

    [Fact]
    public void Each_new_pet_gets_its_own_qr()
    {
        var first = Pet.Create(Guid.NewGuid(), Details(), Now);
        var second = Pet.Create(Guid.NewGuid(), Details(), Now);

        Assert.NotEqual(first.Qr.Token, second.Qr.Token);
        Assert.NotEqual(first.Qr.Code, second.Qr.Code);
    }

    [Fact]
    public void Regenerate_replaces_the_qr_and_moves_the_update_date()
    {
        var pet = Pet.Create(Guid.NewGuid(), Details(), Now);
        var before = pet.Qr;

        pet.RegenerateQr(Now.AddMinutes(5));

        Assert.NotEqual(before.Token, pet.Qr.Token);
        Assert.NotEqual(before.Code, pet.Qr.Code);
        Assert.Equal(Now.AddMinutes(5), pet.UpdatedAtUtc);
        Assert.Equal(Now, pet.CreatedAtUtc);
    }
}
