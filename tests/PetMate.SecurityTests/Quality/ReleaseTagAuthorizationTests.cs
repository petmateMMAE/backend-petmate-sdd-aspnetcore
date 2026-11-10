using System.Text.RegularExpressions;

namespace PetMate.SecurityTests.Quality;

/// <summary>
/// Autorización expresa de la etiqueta de versión (pm-059 CA-02): el pipeline no etiqueta con el solo merge a <c>main</c>; el paso
/// de etiqueta exige un lanzamiento manual con la casilla <c>create_tag</c>. Lee el flujo real del repositorio.
/// </summary>
public sealed class ReleaseTagAuthorizationTests
{
    [Fact]
    public void The_tag_step_only_runs_for_a_manual_dispatch_that_ticks_create_tag()
    {
        var workflow = File.ReadAllText(Path.Combine(RepositoryRoot(), ".github/workflows/pipeline.yml"));

        var step = Regex.Match(workflow, @"- name: Etiqueta de versión[^\n]*\n\s+if: ([^\n]+)\n", RegexOptions.None);
        Assert.True(step.Success, "No se encontró el paso de etiqueta con su condición.");
        var condition = step.Groups[1].Value;

        Assert.Contains("github.event_name == 'workflow_dispatch'", condition, StringComparison.Ordinal);
        Assert.Contains("inputs.create_tag == true", condition, StringComparison.Ordinal);
        Assert.DoesNotContain("push", condition, StringComparison.Ordinal);
    }

    [Fact]
    public void The_create_tag_input_exists_is_boolean_and_defaults_to_false()
    {
        var workflow = File.ReadAllText(Path.Combine(RepositoryRoot(), ".github/workflows/pipeline.yml"));

        var input = Regex.Match(workflow, @"create_tag:\s*\n(?<body>(?:\s+[^\n]+\n){1,4})", RegexOptions.None);
        Assert.True(input.Success, "Falta la entrada create_tag en workflow_dispatch.");
        Assert.Contains("type: boolean", input.Groups["body"].Value, StringComparison.Ordinal);
        Assert.Contains("default: false", input.Groups["body"].Value, StringComparison.Ordinal);
    }

    [Fact]
    public void Only_one_step_creates_or_pushes_tags_and_no_other_release_mechanism_exists()
    {
        var workflow = File.ReadAllText(Path.Combine(RepositoryRoot(), ".github/workflows/pipeline.yml"));

        Assert.Equal(1, Regex.Count(workflow, @"git tag ", RegexOptions.None));
        Assert.Equal(1, Regex.Count(workflow, @"git push origin ""refs/tags/", RegexOptions.None));
        foreach (var forbidden in new[] { "push --tags", "--follow-tags", "push origin --tags", "gh release", "create-release", "action-gh-release" })
        {
            Assert.DoesNotContain(forbidden, workflow, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void The_deploy_job_never_runs_for_pull_requests_and_only_on_main()
    {
        var workflow = File.ReadAllText(Path.Combine(RepositoryRoot(), ".github/workflows/pipeline.yml"));

        Assert.Contains("github.event_name != 'pull_request' && github.ref == 'refs/heads/main'", workflow, StringComparison.Ordinal);
    }

    [Fact]
    public void The_version_input_is_passed_through_the_environment_and_never_interpolated_into_a_script()
    {
        var workflow = File.ReadAllText(Path.Combine(RepositoryRoot(), ".github/workflows/pipeline.yml"));

        Assert.Contains("INPUT_VERSION: ${{ inputs.version }}", workflow, StringComparison.Ordinal);
        Assert.DoesNotContain("V=\"${{ inputs.version }}\"", workflow, StringComparison.Ordinal);
        Assert.Contains("la versión debe coincidir con el archivo VERSION", workflow, StringComparison.Ordinal);
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
