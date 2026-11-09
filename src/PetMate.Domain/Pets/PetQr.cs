using System.Security.Cryptography;

namespace PetMate.Domain.Pets;

/// <summary>
/// Credenciales del QR permanente de una mascota (pm-093): un token aleatorio de 256 bits (base64url, 43 caracteres) que va
/// dentro del código QR y un código corto de 8 caracteres para escribirlo a mano. Ambos salen de un generador criptográfico
/// y no se derivan de ningún dato de la mascota, de modo que no se pueden adivinar.
/// </summary>
public sealed record PetQr(string Token, string Code)
{
    public const int TokenBytes = 32;
    public const int TokenLength = 43;
    public const int CodeLength = 8;

    // 32 símbolos sin caracteres ambiguos (sin I, O, 0 ni 1).
    public const string CodeAlphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

    public static PetQr Generate()
    {
        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(TokenBytes))
            .TrimEnd('=').Replace('+', '-').Replace('/', '_');
        var code = string.Create(CodeLength, 0, static (span, _) =>
        {
            for (var index = 0; index < span.Length; index++)
            {
                span[index] = CodeAlphabet[RandomNumberGenerator.GetInt32(CodeAlphabet.Length)];
            }
        });
        return new PetQr(token, code);
    }

    /// <summary>Indica si ambos valores tienen el formato esperado (se usa al rehidratar desde la base).</summary>
    public static bool IsWellFormed(string? token, string? code)
        => token is { Length: TokenLength } && code is { Length: CodeLength }
            && token.All(static c => char.IsAsciiLetterOrDigit(c) || c is '-' or '_')
            && code.All(static c => CodeAlphabet.Contains(c, StringComparison.Ordinal));
}
