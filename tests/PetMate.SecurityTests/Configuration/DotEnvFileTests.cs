using PetMate.Api.Configuration;

namespace PetMate.SecurityTests.Configuration;

/// <summary>
/// Verifica el lector de archivos <c>.env</c> sin dependencias externas (T004, CA-005).
/// Ninguna prueba imprime valores de configuración ni secretos.
/// </summary>
public sealed class DotEnvFileTests
{
    // Prefijo único para no interferir con variables reales del entorno.
    private const string Prefix = "PETMATE_T004_SECURITY_";

    [Fact]
    public void Parse_IgnoresCommentsAndEmptyLines()
    {
        var values = DotEnvFile.Parse(
        [
            "# Comentario completo",
            string.Empty,
            "   ",
            $"{Prefix}ALPHA=uno",
        ]);

        Assert.Single(values);
        Assert.Equal("uno", values[$"{Prefix}ALPHA"]);
    }

    [Fact]
    public void Parse_SupportsExportPrefixAndQuotedValues()
    {
        var values = DotEnvFile.Parse(
        [
            $"export {Prefix}BETA=\"valor con espacios\"",
            $"{Prefix}GAMMA='otro valor'",
            $"{Prefix}DELTA=\"\"",
        ]);

        Assert.Equal("valor con espacios", values[$"{Prefix}BETA"]);
        Assert.Equal("otro valor", values[$"{Prefix}GAMMA"]);
        Assert.Equal(string.Empty, values[$"{Prefix}DELTA"]);
    }

    [Fact]
    public void Parse_KeepsEqualsSignAndHashInsideUnquotedValues()
    {
        var values = DotEnvFile.Parse(
        [
            $"{Prefix}CONNECTION=clave=valor",
            $"{Prefix}TOKEN=abc#def",
            $"{Prefix}WITH_COMMENT=valor real # comentario inline",
        ]);

        Assert.Equal("clave=valor", values[$"{Prefix}CONNECTION"]);
        Assert.Equal("abc#def", values[$"{Prefix}TOKEN"]);
        Assert.Equal("valor real", values[$"{Prefix}WITH_COMMENT"]);
    }

    [Fact]
    public void Parse_KeepsLastAssignmentForDuplicatedKeys()
    {
        var values = DotEnvFile.Parse(
        [
            $"{Prefix}DUPLICADA=primero",
            $"{Prefix}DUPLICADA=segundo",
        ]);

        Assert.Single(values);
        Assert.Equal("segundo", values[$"{Prefix}DUPLICADA"]);
    }

    [Fact]
    public void Parse_InvalidVariableName_ThrowsWithoutLeakingValue()
    {
        const string secretValue = "valor-sensible-que-no-debe-filtrarse";

        var exception = Assert.Throws<FormatException>(
            () => DotEnvFile.Parse([$"1NOMBRE_INVALIDO={secretValue}"]));

        Assert.DoesNotContain(secretValue, exception.Message, StringComparison.Ordinal);
        Assert.Contains("Línea 1", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Parse_LineWithoutSeparator_ThrowsWithoutLeakingContent()
    {
        const string sensitiveContent = "mongodb+srv://usuario:clave-sensible@host-ficticio";

        var exception = Assert.Throws<FormatException>(
            () => DotEnvFile.Parse([sensitiveContent]));

        Assert.DoesNotContain("clave-sensible", exception.Message, StringComparison.Ordinal);
        Assert.Contains("Línea 1", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Load_ReadsDotEnvFromStartDirectory()
    {
        var directory = CreateTemporaryDirectory();

        try
        {
            File.WriteAllText(
                Path.Combine(directory, ".env"),
                $"{Prefix}DESDE_ARCHIVO=\"valor local\"\n");

            var values = DotEnvFile.Load(directory);

            Assert.Equal("valor local", values[$"{Prefix}DESDE_ARCHIVO"]);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void Load_ReturnsEmptyDictionary_WhenDotEnvDoesNotExist()
    {
        var directory = CreateTemporaryDirectory();

        try
        {
            var values = DotEnvFile.Load(directory);

            Assert.Empty(values);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void Load_SearchesParentDirectories()
    {
        var root = CreateTemporaryDirectory();
        var child = Path.Combine(root, "nivel1", "nivel2");
        Directory.CreateDirectory(child);

        try
        {
            File.WriteAllText(Path.Combine(root, ".env"), $"{Prefix}PADRE=desde-padre\n");

            var values = DotEnvFile.Load(child);

            Assert.Equal("desde-padre", values[$"{Prefix}PADRE"]);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void LoadFromDirectory_DoesNotSearchParentDirectories()
    {
        var root = CreateTemporaryDirectory();
        var child = Path.Combine(root, "child");
        Directory.CreateDirectory(child);

        try
        {
            File.WriteAllText(Path.Combine(root, ".env"), $"{Prefix}PADRE=desde-padre\n");

            var values = DotEnvFile.LoadFromDirectory(child);

            Assert.Empty(values);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void LoadIntoEnvironment_KeepsExistingValuesIncludingEmptyAndAddsNewOne()
    {
        var directory = CreateTemporaryDirectory();
        var existingName = $"{Prefix}EXISTENTE";
        var emptyName = $"{Prefix}VACIA";
        var newName = $"{Prefix}NUEVA";

        try
        {
            Environment.SetEnvironmentVariable(existingName, "valor-original", EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable(emptyName, string.Empty, EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable(newName, null, EnvironmentVariableTarget.Process);

            File.WriteAllText(
                Path.Combine(directory, ".env"),
                $"{existingName}=valor-de-archivo\n{emptyName}=valor-de-archivo\n{newName}=valor-nuevo\n");

            DotEnvFile.LoadIntoEnvironment(directory);

            Assert.Equal("valor-original", Environment.GetEnvironmentVariable(existingName));
            Assert.Equal(string.Empty, Environment.GetEnvironmentVariable(emptyName));
            Assert.Equal("valor-nuevo", Environment.GetEnvironmentVariable(newName));
        }
        finally
        {
            Environment.SetEnvironmentVariable(existingName, null, EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable(emptyName, null, EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable(newName, null, EnvironmentVariableTarget.Process);
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void LoadIntoEnvironment_DoesNotThrow_WhenDotEnvDoesNotExist()
    {
        var directory = CreateTemporaryDirectory();

        try
        {
            var exception = Record.Exception(() => DotEnvFile.LoadIntoEnvironment(directory));

            Assert.Null(exception);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    private static string CreateTemporaryDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Prefix}tmp-{Guid.NewGuid():N}");
        Directory.CreateDirectory(path);

        return path;
    }
}
