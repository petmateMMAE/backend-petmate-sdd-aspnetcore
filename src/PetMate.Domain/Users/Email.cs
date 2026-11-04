using PetMate.Domain.Common;

namespace PetMate.Domain.Users;

/// <summary>Correo electrónico normalizado (recortado y en minúsculas invariantes); base de la unicidad de cuentas.</summary>
public sealed record Email
{
    public const int MaxLength = 254;
    private const int MaxLocalPartLength = 64;

    private Email(string value) => Value = value;

    public string Value { get; }

    public static Email Create(string? raw)
        => TryCreate(raw, out var email) ? email! : throw new DomainException("El correo electrónico no es válido.");

    public static bool TryCreate(string? raw, out Email? email)
    {
        email = null;
        var normalized = raw?.Trim().ToLowerInvariant();
        if (string.IsNullOrEmpty(normalized) || normalized.Length > MaxLength || normalized.Any(IsForbidden))
        {
            return false;
        }

        var at = normalized.IndexOf('@', StringComparison.Ordinal);
        if (at <= 0 || at != normalized.LastIndexOf('@') || at > MaxLocalPartLength)
        {
            return false;
        }

        var domain = normalized[(at + 1)..];
        if (domain.Length < 3 || !domain.Contains('.', StringComparison.Ordinal) || domain.StartsWith('.') || domain.EndsWith('.')
            || domain.Contains("..", StringComparison.Ordinal))
        {
            return false;
        }

        email = new Email(normalized);
        return true;
    }

    public override string ToString() => Value;

    private static bool IsForbidden(char c) => char.IsWhiteSpace(c) || char.IsControl(c);
}
