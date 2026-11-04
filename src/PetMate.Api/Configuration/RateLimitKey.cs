using System.Net;

namespace PetMate.Api.Configuration;

/// <summary>
/// Clave de partición del rate limiting: IPv4 completa; IPv6 agrupada por prefijo /64 (rotar direcciones dentro del
/// mismo /64 no evade el límite); sin IP, una única partición compartida.
/// </summary>
public static class RateLimitKey
{
    public const string Unknown = "desconocida";

    public static string For(IPAddress? address)
    {
        if (address is null)
        {
            return Unknown;
        }

        if (address.IsIPv4MappedToIPv6)
        {
            address = address.MapToIPv4();
        }

        if (address.AddressFamily != System.Net.Sockets.AddressFamily.InterNetworkV6)
        {
            return address.ToString();
        }

        Span<byte> bytes = stackalloc byte[16];
        return address.TryWriteBytes(bytes, out _) ? "v6:" + Convert.ToHexString(bytes[..8]) : Unknown;
    }
}
