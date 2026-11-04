using System.Text.RegularExpressions;

namespace PetMate.SecurityTests.Quality;

/// <summary>
/// Detección de secretos $0 (T012) sobre los archivos versionables del repositorio. El archivo <c>.env</c>
/// real queda fuera por diseño (está ignorado y nunca se lee). Un hallazgo falla la prueba.
/// </summary>
public sealed class SecretScanTests
{
    private static readonly string[] ExcludedDirectories = ["bin", "obj", ".git", ".vs", "node_modules"];
    private static readonly string[] TextExtensions =
        [".cs", ".csproj", ".props", ".json", ".md", ".yml", ".yaml", ".ps1", ".sh", ".example", ".editorconfig", ".sln", ".txt", ".config"];

    // Marcadores que indican un valor ficticio, de ejemplo o de prueba.
    private static readonly string[] SafeMarkers =
        ["ficticio", "placeholder", "ejemplo", "example", "no_usar", "test", "dummy", "fake", "sample", "xxxx", "<", "${", "$(", "***", "invalid", "127.0.0.1", "never-expose", "usuario:"];

    private static readonly (string Name, Regex Pattern)[] Rules =
    [
        ("Clave privada", new Regex(@"-----BEGIN (RSA |EC |DSA |OPENSSH |PGP )?PRIVATE KEY-----", RegexOptions.Compiled)),
        ("Credencial en URI MongoDB", new Regex(@"mongodb(\+srv)?://[^:\s/@""']+:[^@\s""']+@", RegexOptions.Compiled)),
        ("Token JWT", new Regex(@"eyJ[A-Za-z0-9_-]{10,}\.eyJ[A-Za-z0-9_-]{10,}\.[A-Za-z0-9_-]{10,}", RegexOptions.Compiled)),
        ("Clave de acceso AWS", new Regex(@"AKIA[0-9A-Z]{16}", RegexOptions.Compiled)),
        ("Clave de cuenta Azure", new Regex(@"AccountKey=[A-Za-z0-9+/=]{40,}", RegexOptions.Compiled)),
        ("Asignación de secreto largo", new Regex(
            @"(password|passwd|secret|api[_-]?key|signing[_-]?key|token)\s*[=:]\s*[""']?[A-Za-z0-9+/=_\-]{24,}", RegexOptions.Compiled | RegexOptions.IgnoreCase)),
    ];

    [Fact]
    public void Repository_files_contain_no_secrets()
    {
        var findings = Scan(EnumerateRepositoryFiles(FindRepositoryRoot()));

        Assert.True(findings.Count == 0, "Posibles secretos (archivo:línea:regla): " + string.Join("; ", findings));
    }

    [Theory]
    [InlineData("-----BEGIN RSA " + "PRIVATE KEY-----")]
    [InlineData("conn = mongodb+srv://admin:Zx9kQ2mV8pLw@cluster0.abcde.mongodb.net/")]
    [InlineData("AKIA" + "ABCDEFGHIJKLMNOP")]
    [InlineData("JWT_SIGNING_KEY=9f86d081884c7d659a2feaa0c55ad015a3bf4f1b2b0b822cd15d6c15b0f00a08")]
    [InlineData("token: eyJhbGciOiJIUzI1NiJ9.eyJzdWIiOiIxMjM0NTY3ODkwIn0.dBjftJeZ4CVPmB92K27uhbUJU1p1r")]
    public void Scanner_detects_planted_secrets(string line)
        => Assert.NotEmpty(ScanLine(line));

    [Theory]
    [InlineData("MONGODB_URI=mongodb+srv://usuario_ficticio:password_ficticio@cluster-ficticio.mongodb.net/")]
    [InlineData("JWT_SIGNING_KEY=clave_de_firma_placeholder_no_usar")]
    [InlineData("password=test-auth-pw,ssl=True")]
    public void Scanner_ignores_documented_placeholders(string line)
        => Assert.Empty(ScanLine(line));

    private static List<string> Scan(IEnumerable<string> files)
    {
        var findings = new List<string>();
        foreach (var file in files)
        {
            var lines = File.ReadAllLines(file);
            for (var index = 0; index < lines.Length; index++)
            {
                findings.AddRange(ScanLine(lines[index]).Select(rule => $"{Path.GetFileName(file)}:{index + 1}:{rule}"));
            }
        }

        return findings;
    }

    private static List<string> ScanLine(string line)
    {
        if (SafeMarkers.Any(marker => line.Contains(marker, StringComparison.OrdinalIgnoreCase)))
        {
            return [];
        }

        return [.. Rules.Where(rule => rule.Pattern.IsMatch(line)).Select(rule => rule.Name)];
    }

    private static IEnumerable<string> EnumerateRepositoryFiles(string root)
        => Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories)
            .Where(path => !path.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                .Any(segment => ExcludedDirectories.Contains(segment, StringComparer.OrdinalIgnoreCase)))
            .Where(path => !IsRealEnvFile(path))
            // Este archivo contiene a propósito los controles positivos del escáner.
            .Where(path => !Path.GetFileName(path).Equals("SecretScanTests.cs", StringComparison.Ordinal))
            .Where(path => TextExtensions.Contains(Path.GetExtension(path), StringComparer.OrdinalIgnoreCase)
                || Path.GetFileName(path) is ".env.example" or "Dockerfile" or ".dockerignore" or ".gitignore");

    // `.env` y `.env.*` reales no se leen jamás; `.env.example` sí se escanea.
    private static bool IsRealEnvFile(string path)
    {
        var name = Path.GetFileName(path);
        return name.StartsWith(".env", StringComparison.OrdinalIgnoreCase)
            && !name.Equals(".env.example", StringComparison.OrdinalIgnoreCase);
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "PetMate.sln")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new InvalidOperationException("No se encontró la raíz del repositorio.");
    }
}
