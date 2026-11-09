using System.Security.Cryptography;
using System.Text;

namespace PetMate.Api.Controllers;

/// <summary>
/// Seudónimo estable para correlacionar eventos de seguridad de una misma cuenta en los logs sin registrar el correo: los
/// primeros 12 caracteres hexadecimales del SHA-256 del correo normalizado (hallazgo I2). No es reversible de forma directa, pero
/// un atacante con una lista de correos podría confirmar uno; por eso solo se usa en logs internos y nunca se devuelve al cliente.
/// </summary>
internal static class SecurityEventPseudonym
{
    public static string From(string? email)
    {
        var normalized = (email ?? string.Empty).Trim().ToUpperInvariant();
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(normalized)))[..12].ToLowerInvariant();
    }
}
