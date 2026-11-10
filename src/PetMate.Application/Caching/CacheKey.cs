using System.Text.RegularExpressions;

namespace PetMate.Application.Caching;

/// <summary>
/// Clave de caché con la convención <c>petmate:cache:v{versión}:{área}:{id}</c> (pm-050). El área identifica el tipo de dato
/// (por ejemplo <c>catalog</c>) y el id un identificador técnico opaco; nunca puede contener correos, nombres ni otros datos
/// personales (el formato lo impide). Cambiar la forma de lo almacenado exige subir la versión.
/// </summary>
public sealed partial record CacheKey
{
    public const string Prefix = "petmate:cache:";
    public const int CurrentVersion = 1;

    private CacheKey(string value) => Value = value;

    public string Value { get; }

    public static CacheKey Create(string area, string id, int version = CurrentVersion)
    {
        if (string.IsNullOrEmpty(area) || !AreaPattern().IsMatch(area))
        {
            throw new ArgumentException("El área de caché debe ser minúsculas, números o guiones (1 a 32 caracteres).", nameof(area));
        }

        if (string.IsNullOrEmpty(id) || !IdPattern().IsMatch(id))
        {
            throw new ArgumentException("El id de caché debe ser un identificador opaco de letras, números o guiones (1 a 64 caracteres).", nameof(id));
        }

        ArgumentOutOfRangeException.ThrowIfLessThan(version, 1);
        return new CacheKey($"{Prefix}v{version}:{area}:{id}");
    }

    public override string ToString() => Value;

    [GeneratedRegex("^[a-z0-9-]{1,32}$")]
    private static partial Regex AreaPattern();

    [GeneratedRegex("^[A-Za-z0-9-]{1,64}$")]
    private static partial Regex IdPattern();
}
