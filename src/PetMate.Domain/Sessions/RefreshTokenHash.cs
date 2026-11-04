using PetMate.Domain.Common;

namespace PetMate.Domain.Sessions;

/// <summary>SHA-256 (64 caracteres hexadecimales en minúsculas) de un refresh token opaco. Nunca el token en claro.</summary>
public sealed record RefreshTokenHash
{
    private const int HexLength = 64;

    private RefreshTokenHash(string value) => Value = value;

    public string Value { get; }

    public static RefreshTokenHash Create(string? value)
    {
        var normalized = value?.ToLowerInvariant();
        if (normalized is null || normalized.Length != HexLength || !normalized.All(Uri.IsHexDigit))
        {
            throw new DomainException("El hash del refresh token no tiene el formato esperado.");
        }

        return new RefreshTokenHash(normalized);
    }

    public override string ToString() => "[RefreshTokenHash]";
}
