using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using PetMate.Application.Authentication;
using PetMate.Domain.Sessions;

namespace PetMate.Infrastructure.Security;

/// <summary>
/// Refresh token opaco: 256 bits del generador criptográfico del sistema, en Base64Url (43 caracteres).
/// Solo se persiste su SHA-256 (hex). Un token de alta entropía no necesita un hash lento ni salt.
/// </summary>
public sealed class RefreshTokenFactory : IRefreshTokenFactory
{
    private const int TokenBytes = 32;
    private const int EncodedLength = 43;

    public RefreshTokenMaterial Create()
    {
        var raw = Base64UrlEncoder.Encode(RandomNumberGenerator.GetBytes(TokenBytes));
        return new RefreshTokenMaterial(raw, Hash(raw));
    }

    public bool TryHash(string rawToken, out RefreshTokenHash? hash)
    {
        hash = null;
        if (rawToken is not { Length: EncodedLength } || !rawToken.All(IsBase64UrlChar))
        {
            return false;
        }

        hash = Hash(rawToken);
        return true;
    }

    private static RefreshTokenHash Hash(string raw)
        => RefreshTokenHash.Create(Convert.ToHexString(SHA256.HashData(Encoding.ASCII.GetBytes(raw))));

    private static bool IsBase64UrlChar(char c) => char.IsAsciiLetterOrDigit(c) || c is '-' or '_';
}
