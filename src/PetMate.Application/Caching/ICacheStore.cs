namespace PetMate.Application.Caching;

/// <summary>
/// Caché distribuida (pm-050) respaldada exclusivamente por Redis Cache. Es una optimización, nunca una fuente de verdad:
/// toda falla se trata como un fallo de caché (<c>Get</c> devuelve <c>null</c>; <c>Set</c> y <c>Remove</c> devuelven <c>false</c>)
/// y nunca propaga excepciones, de modo que la corrección funcional no depende de ella. Qué datos pueden guardarse lo define la
/// política de <c>docs/security/cache-policy.md</c> (pm-051): por defecto, ninguno.
/// </summary>
public interface ICacheStore
{
    Task<string?> Read(CacheKey key, CancellationToken cancellationToken);

    /// <summary>Guarda el valor con TTL obligatorio (entre <see cref="CacheTtl.Minimum"/> y <see cref="CacheTtl.Maximum"/>).</summary>
    Task<bool> Write(CacheKey key, string value, TimeSpan ttl, CancellationToken cancellationToken);

    Task<bool> Remove(CacheKey key, CancellationToken cancellationToken);
}

/// <summary>Límites del TTL: toda entrada expira; no existen entradas permanentes.</summary>
public static class CacheTtl
{
    public static readonly TimeSpan Minimum = TimeSpan.FromSeconds(1);
    public static readonly TimeSpan Maximum = TimeSpan.FromHours(1);

    public static void EnsureValid(TimeSpan ttl)
    {
        if (ttl < Minimum || ttl > Maximum)
        {
            throw new ArgumentOutOfRangeException(nameof(ttl), "El TTL de caché debe estar entre 1 segundo y 1 hora.");
        }
    }
}
