using System.Text.RegularExpressions;

namespace PetMate.SecurityTests.Configuration;

/// <summary>
/// Estrategia de entornos (pm-064): cada entorno usa una base de Atlas distinta y los recursos Azure siguen las convenciones de
/// nombres y etiquetas documentadas en <c>docs/infrastructure/environments.md</c>. Lee los archivos reales del repositorio.
/// </summary>
public sealed partial class EnvironmentStrategyTests
{
    [Fact]
    public void Development_test_and_demo_use_three_different_atlas_databases()
    {
        var env = Read(".env.example");
        var development = Value(env, "MONGODB_DATABASE");
        var test = Value(env, "MONGODB_TEST_DATABASE");
        var demo = Regex.Match(Read("infra/azure/variables.tf"), "variable \"mongodb_database\"[^}]*default\\s*=\\s*\"(?<v>[^\"]+)\"", RegexOptions.Singleline).Groups["v"].Value;

        Assert.NotEmpty(development);
        Assert.NotEmpty(test);
        Assert.NotEmpty(demo);
        Assert.Equal(3, new[] { development, test, demo }.Distinct(StringComparer.OrdinalIgnoreCase).Count());
        Assert.Contains("test", test, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("test", demo, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Azure_resources_follow_the_documented_naming_patterns()
    {
        var main = Read("infra/azure/main.tf");

        Assert.Contains("\"vnet-petmate-${var.name_suffix}\"", main, StringComparison.Ordinal);
        Assert.Contains("\"log-petmate-${var.name_suffix}\"", main, StringComparison.Ordinal);
        Assert.Contains("\"cae-petmate-${var.name_suffix}\"", main, StringComparison.Ordinal);
        Assert.Contains("\"id-petmate-api-${var.name_suffix}\"", main, StringComparison.Ordinal);
        Assert.Contains("\"acrpetmate${var.name_suffix}${random_string.global.result}\"", main, StringComparison.Ordinal);
        Assert.Contains("\"stpetmate${var.name_suffix}${random_string.global.result}\"", main, StringComparison.Ordinal);
        Assert.Matches(NameSuffixRule(), Read("infra/azure/variables.tf"));
    }

    [Fact]
    public void Cost_tags_are_defined_and_applied_to_the_resources_of_the_app()
    {
        var variables = Read("infra/azure/variables.tf");
        var main = Read("infra/azure/main.tf");

        foreach (var tag in new[] { "proyecto", "componente", "entorno", "costo" })
        {
            Assert.Matches(new Regex($@"^\s*{tag}\s*=\s*""[^""]+""", RegexOptions.Multiline), variables);
        }

        Assert.Contains("version = var.api_version", main, StringComparison.Ordinal);
        Assert.True(Regex.Count(main, @"tags\s*=\s*local\.tags") >= 5, "Los recursos principales deben usar local.tags.");
    }

    [Fact]
    public void The_environment_document_describes_the_three_environments_and_the_conventions()
    {
        var text = Read("docs/infrastructure/environments.md");

        foreach (var expected in new[] { "Desarrollo", "Pruebas", "Demo", "petmate_dev", "petmate_test", "petmate_prod", "vnet-petmate-", "`proyecto`", "`costo`" })
        {
            Assert.Contains(expected, text, StringComparison.Ordinal);
        }
    }

    private static string Value(string env, string key)
        => Regex.Match(env, $"^{key}=(?<v>.*)$", RegexOptions.Multiline).Groups["v"].Value.Trim();

    private static string Read(string relative)
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            var candidate = Path.Combine(directory.FullName, relative.Replace('/', Path.DirectorySeparatorChar));
            if (File.Exists(candidate))
            {
                return File.ReadAllText(candidate);
            }
        }

        throw new FileNotFoundException($"No se encontró {relative} subiendo desde el directorio de pruebas.");
    }

    [GeneratedRegex(@"\^\[a-z0-9\]\{2,8\}\$")]
    private static partial Regex NameSuffixRule();
}
