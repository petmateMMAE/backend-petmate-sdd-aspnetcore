namespace PetMate.IntegrationTests;

/// <summary>
/// Marca una prueba que necesita servicios externos. Si falta alguna variable, queda
/// <c>Skipped</c> con el motivo: nunca se reporta como aprobada sin ejecutarse.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class RequiresEnvironmentFactAttribute : FactAttribute
{
    public RequiresEnvironmentFactAttribute(params string[] variables)
    {
        ArgumentNullException.ThrowIfNull(variables);

        var missing = variables.Where(name =>
        Environment.GetEnvironmentVariable(name) is not { Length: > 0 } value
        || value.Contains("ficticio", StringComparison.OrdinalIgnoreCase)).ToArray();
        if (missing.Length > 0)
        {
            Skip = "Requiere servicios externos; variables no definidas o con placeholders: " + string.Join(", ", missing);
        }
    }
}
