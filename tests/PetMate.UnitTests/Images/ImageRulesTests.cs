using System.Text;
using PetMate.Application.Images;

namespace PetMate.UnitTests.Images;

/// <summary>
/// Tipos y tamaños permitidos (pm-080#CA-01) y detección por bytes mágicos (pm-080#CA-02). Lo que decide es el
/// contenido: el nombre del archivo y el <c>Content-Type</c> del cliente no participan.
/// </summary>
public sealed class ImageRulesTests
{
    private static readonly byte[] Jpeg = [0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, 0x49, 0x46, 0x00, 0x01];
    private static readonly byte[] Png = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00, 0x00, 0x0D];
    private static readonly byte[] WebP = [0x52, 0x49, 0x46, 0x46, 0x24, 0x00, 0x00, 0x00, 0x57, 0x45, 0x42, 0x50];

    [Fact]
    public void Detects_the_three_allowed_types()
    {
        Assert.Equal(ImageRules.Jpeg, ImageRules.Detect(Jpeg));
        Assert.Equal(ImageRules.Png, ImageRules.Detect(Png));
        Assert.Equal(ImageRules.WebP, ImageRules.Detect(WebP));
    }

    [Fact]
    public void Rejects_anything_that_is_not_one_of_them()
    {
        Assert.Null(ImageRules.Detect(Encoding.UTF8.GetBytes("MZ ejecutable")));
        Assert.Null(ImageRules.Detect(Encoding.UTF8.GetBytes("GIF89a")));
        Assert.Null(ImageRules.Detect(Encoding.UTF8.GetBytes("%PDF-1.7")));
        Assert.Null(ImageRules.Detect([]));

        // Un RIFF que no es WebP (por ejemplo un WAV) no pasa.
        Assert.Null(ImageRules.Detect([0x52, 0x49, 0x46, 0x46, 0x24, 0x00, 0x00, 0x00, 0x57, 0x41, 0x56, 0x45]));
    }

    [Fact]
    public void A_truncated_header_is_not_a_valid_image()
    {
        Assert.Null(ImageRules.Detect(Jpeg.AsSpan(0, 2)));
        Assert.Null(ImageRules.Detect(Png.AsSpan(0, 7)));
        Assert.Null(ImageRules.Detect(WebP.AsSpan(0, 11)));
    }

    [Fact]
    public void The_extension_comes_from_the_detected_type_not_from_the_file_name()
    {
        Assert.Equal(".jpg", ImageRules.ExtensionFor(ImageRules.Jpeg));
        Assert.Equal(".png", ImageRules.ExtensionFor(ImageRules.Png));
        Assert.Equal(".webp", ImageRules.ExtensionFor(ImageRules.WebP));
    }

    [Fact]
    public void A_business_photo_allows_one_megabyte_and_a_pet_photo_two()
    {
        Assert.Equal(1024 * 1024, ImageRules.MaxBytes(ImageContainer.BusinessPhotos));
        Assert.Equal(2 * 1024 * 1024, ImageRules.MaxBytes(ImageContainer.PetPhotos));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void An_empty_file_is_a_missing_file(long length)
    {
        Assert.Equal(ImageRejection.Missing, ImageRules.Check(ImageContainer.PetPhotos, length, Jpeg, out var contentType));
        Assert.Null(contentType);
    }

    [Fact]
    public void Size_is_checked_before_content()
    {
        var rejection = ImageRules.Check(ImageContainer.BusinessPhotos, ImageRules.MaxBusinessPhotoBytes + 1, Jpeg, out var contentType);

        Assert.Equal(ImageRejection.TooLarge, rejection);
        Assert.Null(contentType);
    }

    [Fact]
    public void A_valid_image_passes_and_reports_its_type()
    {
        var rejection = ImageRules.Check(ImageContainer.PetPhotos, Png.Length, Png, out var contentType);

        Assert.Equal(ImageRejection.None, rejection);
        Assert.Equal(ImageRules.Png, contentType);
    }

    [Fact]
    public void A_file_at_the_exact_limit_is_accepted()
    {
        var rejection = ImageRules.Check(ImageContainer.BusinessPhotos, ImageRules.MaxBusinessPhotoBytes, WebP, out var contentType);

        Assert.Equal(ImageRejection.None, rejection);
        Assert.Equal(ImageRules.WebP, contentType);
    }

    [Fact]
    public void The_object_key_is_built_from_identifiers_never_from_the_file_name()
    {
        var owner = Guid.Parse("11111111-2222-3333-4444-555555555555");
        var pet = Guid.Parse("66666666-7777-8888-9999-000000000000");

        Assert.Equal("11111111222233334444555555555555/66666666777788889999000000000000", PhotoKeys.ForPet(owner, pet));
        Assert.Equal("66666666777788889999000000000000", PhotoKeys.ForBusiness(pet));
    }

    [Fact]
    public void The_temporary_url_lasts_fifteen_minutes()
        => Assert.Equal(TimeSpan.FromMinutes(15), PhotoKeys.TemporaryUrlLifetime);
}
