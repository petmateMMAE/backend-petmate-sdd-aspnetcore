using System.Xml.Linq;

namespace PetMate.UnitTests.Architecture;

/// <summary>
/// Verifica la dirección de dependencias de Clean Architecture declarada en la constitución (§3.2)
/// y en specs/00-foundation (RF-003, RF-004, CA-002).
/// </summary>
public sealed class CleanArchitectureTests
{
    private const string SolutionFileName = "PetMate.sln";

    // Proyectos fuente con su ruta relativa a la raíz del repositorio.
    private static readonly IReadOnlyDictionary<string, string> SourceProjectPaths = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["PetMate.Domain"] = Path.Combine("src", "PetMate.Domain", "PetMate.Domain.csproj"),
        ["PetMate.Application"] = Path.Combine("src", "PetMate.Application", "PetMate.Application.csproj"),
        ["PetMate.Infrastructure"] = Path.Combine("src", "PetMate.Infrastructure", "PetMate.Infrastructure.csproj"),
        ["PetMate.Api"] = Path.Combine("src", "PetMate.Api", "PetMate.Api.csproj"),
    };

    // Proyectos de prueba con su ruta relativa a la raíz del repositorio.
    private static readonly IReadOnlyDictionary<string, string> TestProjectPaths = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["PetMate.UnitTests"] = Path.Combine("tests", "PetMate.UnitTests", "PetMate.UnitTests.csproj"),
        ["PetMate.IntegrationTests"] = Path.Combine("tests", "PetMate.IntegrationTests", "PetMate.IntegrationTests.csproj"),
        ["PetMate.SecurityTests"] = Path.Combine("tests", "PetMate.SecurityTests", "PetMate.SecurityTests.csproj"),
        ["PetMate.ContractTests"] = Path.Combine("tests", "PetMate.ContractTests", "PetMate.ContractTests.csproj"),
    };

    // Referencias mínimas autorizadas para cada proyecto de prueba (T002).
    private static readonly IReadOnlyDictionary<string, string[]> RequiredTestReferences = new Dictionary<string, string[]>(StringComparer.Ordinal)
    {
        ["PetMate.UnitTests"] = ["PetMate.Domain", "PetMate.Application"],
        ["PetMate.IntegrationTests"] = ["PetMate.Application", "PetMate.Infrastructure"],
        ["PetMate.SecurityTests"] = ["PetMate.Application", "PetMate.Api"],
        ["PetMate.ContractTests"] = ["PetMate.Application", "PetMate.Api"],
    };

    private static readonly Lazy<string> RepositoryRoot = new(LocateRepositoryRoot);

    [Fact]
    public void Domain_ShouldNotReferenceAnyOtherProject()
    {
        var references = ReadProjectReferences("PetMate.Domain");

        Assert.True(
            references.Length == 0,
            $"Clean Architecture violation: project 'PetMate.Domain' must not reference any other project, " +
            $"but found: {Format(references)}.");
    }

    [Fact]
    public void Application_ShouldReferenceOnlyDomain()
        => AssertExactReferences("PetMate.Application", "PetMate.Domain");

    [Fact]
    public void Infrastructure_ShouldReferenceOnlyApplicationAndDomain()
        => AssertExactReferences("PetMate.Infrastructure", "PetMate.Application", "PetMate.Domain");

    [Fact]
    public void Api_ShouldReferenceOnlyApplicationAndInfrastructure()
        => AssertExactReferences("PetMate.Api", "PetMate.Application", "PetMate.Infrastructure");

    [Fact]
    public void TestProjects_ShouldNotReferenceEachOther()
    {
        var testProjects = TestProjectPaths.Keys.ToArray();

        foreach (var project in testProjects)
        {
            var forbidden = ReadProjectReferences(project)
                .Where(reference => testProjects.Contains(reference, StringComparer.Ordinal))
                .ToArray();

            Assert.True(
                forbidden.Length == 0,
                $"Clean Architecture violation: test project '{project}' must not reference another test project, " +
                $"but found: {Format(forbidden)}.");
        }
    }

    [Fact]
    public void TestProjects_ShouldReferenceOnlySourceProjects()
    {
        var sourceProjects = SourceProjectPaths.Keys.ToArray();

        foreach (var project in TestProjectPaths.Keys)
        {
            var unexpected = ReadProjectReferences(project)
                .Where(reference => !sourceProjects.Contains(reference, StringComparer.Ordinal))
                .ToArray();

            Assert.True(
                unexpected.Length == 0,
                $"Clean Architecture violation: test project '{project}' must reference only source projects " +
                $"({Format(sourceProjects)}), but found: {Format(unexpected)}.");
        }
    }

    [Fact]
    public void TestProjects_ShouldDeclareTheProjectsTheyVerify()
    {
        foreach (var (project, expected) in RequiredTestReferences)
        {
            var references = ReadProjectReferences(project);
            var missing = expected
                .Where(reference => !references.Contains(reference, StringComparer.Ordinal))
                .ToArray();

            Assert.True(
                missing.Length == 0,
                $"Project '{project}' is missing required project references: {Format(missing)}. " +
                $"Current references: {Format(references)}.");
        }
    }

    private static void AssertExactReferences(string projectName, params string[] expected)
    {
        var references = ReadProjectReferences(projectName);

        var unexpected = references
            .Where(reference => !expected.Contains(reference, StringComparer.Ordinal))
            .ToArray();
        var missing = expected
            .Where(reference => !references.Contains(reference, StringComparer.Ordinal))
            .ToArray();

        Assert.True(
            unexpected.Length == 0,
            $"Clean Architecture violation: project '{projectName}' has unexpected project references: " +
            $"{Format(unexpected)}. Allowed references: {Format(expected)}.");
        Assert.True(
            missing.Length == 0,
            $"Project '{projectName}' is missing required project references: {Format(missing)}.");
    }

    /// <summary>
    /// Lee los <c>ProjectReference</c> declarados en el archivo del proyecto y devuelve
    /// únicamente el nombre de cada proyecto referenciado.
    /// </summary>
    private static string[] ReadProjectReferences(string projectName)
    {
        var relativePath = SourceProjectPaths.TryGetValue(projectName, out var sourcePath)
            ? sourcePath
            : TestProjectPaths.TryGetValue(projectName, out var testPath)
                ? testPath
                : throw new ArgumentOutOfRangeException(nameof(projectName), projectName, "Unknown project name.");

        var projectFile = Path.Combine(RepositoryRoot.Value, relativePath);

        Assert.True(File.Exists(projectFile), $"Project file not found for '{projectName}': {projectFile}");

        var document = XDocument.Load(projectFile);

        return document
            .Descendants()
            .Where(element => element.Name.LocalName == "ProjectReference")
            .Select(element => element.Attribute("Include")?.Value)
            .OfType<string>()
            .Select(GetProjectName)
            .ToArray();
    }

    /// <summary>
    /// Extrae el nombre del proyecto desde la ruta declarada en <c>Include</c>,
    /// admitiendo separadores de Windows y de Unix.
    /// </summary>
    private static string GetProjectName(string includePath)
    {
        var normalized = includePath.Replace('\\', '/');
        var fileName = normalized[(normalized.LastIndexOf('/') + 1)..];

        return Path.GetFileNameWithoutExtension(fileName);
    }

    /// <summary>
    /// Sube desde el directorio del binario de pruebas hasta encontrar la solución,
    /// de modo que la prueba funcione en local y en CI sin rutas absolutas.
    /// </summary>
    private static string LocateRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, SolutionFileName)))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException(
            $"Could not locate '{SolutionFileName}' while walking up from '{AppContext.BaseDirectory}'.");
    }

    private static string Format(IEnumerable<string> projectNames)
    {
        var names = projectNames.ToArray();

        return names.Length == 0 ? "(none)" : string.Join(", ", names);
    }
}
