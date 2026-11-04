using PetMate.Domain.Common;

namespace PetMate.Domain.Users;

/// <summary>Hash Argon2id en formato PHC (<c>$argon2id$…</c>). Valor opaco: nunca contiene la contraseña.</summary>
public sealed record PasswordHash
{
    private const string Prefix = "$argon2id$";
    private const int MaxLength = 255;

    private PasswordHash(string value) => Value = value;

    public string Value { get; }

    public static PasswordHash Create(string? value)
    {
        if (string.IsNullOrEmpty(value) || value.Length > MaxLength
            || !value.StartsWith(Prefix, StringComparison.Ordinal) || value.Any(char.IsWhiteSpace))
        {
            throw new DomainException("El hash de contraseña no tiene el formato Argon2id esperado.");
        }

        return new PasswordHash(value);
    }

    // Evita que el hash aparezca en logs o trazas por accidente.
    public override string ToString() => "[PasswordHash]";
}
