namespace PetMate.Api.Configuration;

/// <summary>
/// Lector mínimo de archivos <c>.env</c> sin dependencias externas (T004, CA-005).
/// Solo interpreta asignaciones simples <c>CLAVE=valor</c>; nunca registra ni imprime valores.
/// </summary>
public static class DotEnvFile
{
    private const string FileName = ".env";
    private const string ExportPrefix = "export";

    /// <summary>
    /// Convierte líneas de un archivo <c>.env</c> en un diccionario de variables.
    /// Admite comentarios con <c>#</c>, líneas vacías, prefijo <c>export</c>,
    /// comillas simples o dobles y el separador <c>=</c> dentro del valor.
    /// Si una clave se repite, prevalece la última asignación.
    /// </summary>
    /// <exception cref="FormatException">
    /// La asignación es inválida. El mensaje indica solo el número de línea y, cuando es
    /// seguro mostrarla, la clave; nunca el valor.
    /// </exception>
    public static IReadOnlyDictionary<string, string> Parse(IEnumerable<string> lines)
    {
        ArgumentNullException.ThrowIfNull(lines);

        var values = new Dictionary<string, string>(StringComparer.Ordinal);
        var lineNumber = 0;

        foreach (var line in lines)
        {
            lineNumber++;

            var content = line.Trim();

            // Línea vacía o comentario completo.
            if (content.Length == 0 || content[0] == '#')
            {
                continue;
            }

            // Prefijo opcional `export` seguido de un separador en blanco.
            if (content.StartsWith(ExportPrefix, StringComparison.Ordinal)
                && content.Length > ExportPrefix.Length
                && char.IsWhiteSpace(content[ExportPrefix.Length]))
            {
                content = content[(ExportPrefix.Length + 1)..].TrimStart();
            }

            var separatorIndex = content.IndexOf('=', StringComparison.Ordinal);

            if (separatorIndex < 0)
            {
                throw new FormatException(
                    $"Línea {lineNumber}: falta el separador '=' en la asignación. " +
                    "El contenido no se muestra por seguridad.");
            }

            var name = content[..separatorIndex].Trim();

            if (!IsValidName(name))
            {
                throw new FormatException(
                    $"Línea {lineNumber}: nombre de variable inválido{FormatNameHint(name)}. " +
                    "Use letras, dígitos y guion bajo, comenzando por letra o guion bajo. " +
                    "El valor no se muestra por seguridad.");
            }

            values[name] = ParseValue(content[(separatorIndex + 1)..], lineNumber);
        }

        return values;
    }

    /// <summary>
    /// Busca un archivo <c>.env</c> en <paramref name="startDirectory"/> (por defecto el
    /// directorio actual) y asciende por los directorios padre hasta encontrarlo.
    /// Devuelve un diccionario vacío si el archivo no existe; no lanza en ese caso.
    /// </summary>
    public static IReadOnlyDictionary<string, string> Load(string? startDirectory = null)
    {
        var directory = ResolveStartDirectory(startDirectory);

        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, FileName);

            if (File.Exists(candidate))
            {
                return Parse(File.ReadAllLines(candidate));
            }

            directory = directory.Parent;
        }

        return new Dictionary<string, string>(StringComparer.Ordinal);
    }

    /// <summary>
    /// Lee únicamente el archivo <c>.env</c> ubicado directamente en
    /// <paramref name="directory"/>. No recorre directorios padre.
    /// </summary>
    public static IReadOnlyDictionary<string, string> LoadFromDirectory(string directory)
    {
        var resolvedDirectory = ResolveStartDirectory(directory);
        var candidate = Path.Combine(resolvedDirectory.FullName, FileName);

        return File.Exists(candidate)
            ? Parse(File.ReadAllLines(candidate))
            : new Dictionary<string, string>(StringComparer.Ordinal);
    }

    /// <summary>
    /// Aplica las variables del archivo <c>.env</c> encontrado al entorno del proceso.
    /// Las variables ya definidas en el entorno tienen prioridad y no se sobrescriben.
    /// No registra ni imprime nombres ni valores.
    /// </summary>
    public static void LoadIntoEnvironment(string? startDirectory = null)
    {
        var values = Load(startDirectory);

        foreach (var (name, value) in values)
        {
            var current = Environment.GetEnvironmentVariable(name);

            // Toda variable presente, incluso vacía, conserva prioridad sobre `.env`.
            if (current is null)
            {
                Environment.SetEnvironmentVariable(name, value, EnvironmentVariableTarget.Process);
            }
        }
    }

    /// <summary>
    /// Aplica solo el <c>.env</c> ubicado directamente en <paramref name="directory"/>
    /// al entorno del proceso. Una variable de entorno presente, inclusive vacía,
    /// nunca se sobrescribe.
    /// </summary>
    public static void LoadIntoEnvironmentFromDirectory(string directory)
    {
        var values = LoadFromDirectory(directory);

        foreach (var (name, value) in values)
        {
            if (Environment.GetEnvironmentVariable(name) is null)
            {
                Environment.SetEnvironmentVariable(name, value, EnvironmentVariableTarget.Process);
            }
        }
    }

    private static string ParseValue(string rawValue, int lineNumber)
    {
        var value = rawValue.Trim();

        if (value.Length == 0)
        {
            return string.Empty;
        }

        if (value[0] is '"' or '\'')
        {
            var quote = value[0];

            if (value.Length < 2 || value[^1] != quote)
            {
                throw new FormatException(
                    $"Línea {lineNumber}: valor entre comillas sin cerrar. " +
                    "El contenido no se muestra por seguridad.");
            }

            // Valor entre comillas: se conserva íntegro, sin comentarios inline.
            return value[1..^1];
        }

        // Valor sin comillas: un `#` precedido de espacio inicia un comentario inline.
        var commentIndex = FindInlineCommentIndex(value);

        return commentIndex < 0 ? value : value[..commentIndex].TrimEnd();
    }

    private static int FindInlineCommentIndex(string value)
    {
        for (var index = 1; index < value.Length; index++)
        {
            if (value[index] == '#' && char.IsWhiteSpace(value[index - 1]))
            {
                return index;
            }
        }

        return -1;
    }

    private static bool IsValidName(string name)
    {
        if (name.Length == 0)
        {
            return false;
        }

        if (!char.IsAsciiLetter(name[0]) && name[0] != '_')
        {
            return false;
        }

        for (var index = 1; index < name.Length; index++)
        {
            var character = name[index];

            if (!char.IsAsciiLetterOrDigit(character) && character != '_')
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Devuelve la clave para el mensaje de error solo cuando no puede contener
    /// credenciales (sin <c>:</c>, <c>@</c>, <c>/</c>, comillas ni espacios).
    /// </summary>
    private static string FormatNameHint(string name)
    {
        if (name.Length == 0 || name.Length > 64 || !IsSafeToDisplay(name))
        {
            return string.Empty;
        }

        return $" '{name}'";
    }

    private static bool IsSafeToDisplay(string text)
    {
        foreach (var character in text)
        {
            var allowed = char.IsAsciiLetterOrDigit(character)
                || character is '_' or '-' or '.';

            if (!allowed)
            {
                return false;
            }
        }

        return true;
    }

    private static DirectoryInfo ResolveStartDirectory(string? startDirectory)
    {
        var path = string.IsNullOrWhiteSpace(startDirectory)
            ? Directory.GetCurrentDirectory()
            : startDirectory;

        return new DirectoryInfo(Path.GetFullPath(path));
    }
}
