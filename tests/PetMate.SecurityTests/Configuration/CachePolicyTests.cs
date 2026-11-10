namespace PetMate.SecurityTests.Configuration;

/// <summary>
/// Política de caché (pm-051): nada se cachea salvo decisión documentada. El código de producción no consume <c>ICacheStore</c>
/// fuera de su definición, su adaptador y su registro, y la política clasifica los datos y cada ruta del catálogo.
/// </summary>
public sealed class CachePolicyTests
{
    private static readonly string[] Allowed =
    [
        "src/PetMate.Application/Caching/ICacheStore.cs",
        "src/PetMate.Infrastructure/Redis/RedisCacheStore.cs",
        "src/PetMate.Infrastructure/DependencyInjection/InfrastructureServiceCollectionExtensions.cs",
    ];

    [Fact]
    public void No_production_code_consumes_the_cache_store_without_a_documented_decision()
    {
        var root = RepositoryRoot();
        var offenders = Directory.EnumerateFiles(Path.Combine(root, "src"), "*.cs", SearchOption.AllDirectories)
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            .Where(path => File.ReadAllText(path).Contains("ICacheStore", StringComparison.Ordinal))
            .Select(path => Path.GetRelativePath(root, path).Replace(Path.DirectorySeparatorChar, '/'))
            .Where(path => !Allowed.Contains(path))
            .ToList();

        Assert.Empty(offenders);
    }

    [Fact]
    public void The_cache_adapter_depends_only_on_the_cache_connection_and_never_on_redis_auth()
    {
        var source = File.ReadAllText(Path.Combine(RepositoryRoot(), "src/PetMate.Infrastructure/Redis/RedisCacheStore.cs"));

        Assert.Contains("RedisCacheConnection", source, StringComparison.Ordinal);
        Assert.DoesNotContain("RedisAuthConnection", source, StringComparison.Ordinal);
    }

    [Fact]
    public void The_policy_classifies_data_and_covers_every_route_of_the_catalog()
    {
        var root = RepositoryRoot();
        var policy = File.ReadAllText(Path.Combine(root, "docs/security/cache-policy.md"));
        var catalog = File.ReadAllText(Path.Combine(root, "docs/api/endpoints.md"));

        foreach (var term in new[] { "Público", "Privado", "Personal", "Sensible", "TTL", "ningún endpoint usa la caché" })
        {
            Assert.Contains(term, policy, StringComparison.OrdinalIgnoreCase);
        }

        foreach (var route in new[] { "/auth/register", "/auth/login", "/auth/refresh", "/auth/logout", "/users/me", "/pets/get-all", "/pets/get-by-id", "/health/", "mediator-probe" })
        {
            Assert.Contains(route, policy, StringComparison.Ordinal);
        }

        Assert.Contains("/api/v1/pets/get-all", catalog, StringComparison.Ordinal);
    }

    private static string RepositoryRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "PetMate.sln")))
            {
                return directory.FullName;
            }
        }

        throw new DirectoryNotFoundException("No se encontró la raíz del repositorio.");
    }
}
