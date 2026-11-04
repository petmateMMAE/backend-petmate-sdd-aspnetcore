using System.Xml.Linq;

namespace PetMate.SecurityTests.OpenApi;

public sealed class OpenApiSecurityTests
{
    [Fact]
    public void Api_project_does_not_reference_alternative_documentation_packages()
    {
        var projectPath = FindProjectPath();
        var packageReferences = XDocument.Load(projectPath)
            .Descendants("PackageReference")
            .Select(reference => reference.Attribute("Include")?.Value)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .ToArray();

        Assert.DoesNotContain(packageReferences, name => name!.Contains("Swashbuckle", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(packageReferences, name => name!.Contains("NSwag", StringComparison.OrdinalIgnoreCase));
    }

    private static string FindProjectPath()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, "src", "PetMate.Api", "PetMate.Api.csproj");
            if (File.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("No se encontró PetMate.Api.csproj.");
    }
}
