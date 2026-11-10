namespace PetMate.SecurityTests.Configuration;

/// <summary>
/// Registro de decisiones de servicios Azure (pm-063): cada componente tiene su ADR con alternativas dentro de Azure y cita la
/// clasificación de costo del estudio de pm-062; ningún servicio se elige por haber tenido un nivel gratuito.
/// </summary>
public sealed class DecisionRecordsTests
{
    private static readonly string[] ComponentAdrs =
    [
        "ADR-005-balanceo-y-https.md",
        "ADR-006-almacenamiento-de-imagenes.md",
        "ADR-007-cdn.md",
        "ADR-008-observabilidad.md",
        "ADR-009-registro-de-contenedores.md",
        "ADR-010-secretos.md",
    ];

    [Fact]
    public void Every_component_adr_lists_alternatives_inside_azure_and_cites_the_cost_study()
    {
        foreach (var file in ComponentAdrs)
        {
            var text = Read($"docs/decisions/{file}");

            Assert.Contains("## Alternativas dentro de Azure", text, StringComparison.Ordinal);
            Assert.Contains("azure-cost-study.md", text, StringComparison.Ordinal);
            Assert.Contains("nivel gratuito", text, StringComparison.Ordinal);
            Assert.Matches("Clasificación|clasificación", text);
        }
    }

    [Fact]
    public void The_compute_and_redis_decision_lists_discarded_alternatives_and_the_index_covers_every_component()
    {
        var adr003 = Read("docs/decisions/ADR-003-azure-container-apps.md");
        var index = Read("docs/decisions/README.md");

        Assert.Contains("## Alternativas descartadas", adr003, StringComparison.Ordinal);
        Assert.Contains("App Service", adr003, StringComparison.Ordinal);
        Assert.Contains("Azure Managed Redis", adr003, StringComparison.Ordinal);
        foreach (var component in new[] { "Cómputo", "Redis", "Balanceo", "Almacenamiento", "CDN", "Observabilidad", "Registro de contenedores", "Secretos" })
        {
            Assert.Contains(component, index, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void The_cost_study_covers_the_required_components_with_official_sources_and_a_consultation_date()
    {
        var study = Read("docs/infrastructure/azure-cost-study.md");

        Assert.Contains("Fecha de consulta: **2026-11-08**", study, StringComparison.Ordinal);
        foreach (var component in new[] { "Cómputo", "Load Balancer", "salida de datos", "DNS", "Registro de contenedores", "Almacenamiento", "CDN", "Redis Auth", "Redis Cache", "Observabilidad", "Secretos", "Estado de IaC", "CI/CD", "Atlas" })
        {
            Assert.Contains(component, study, StringComparison.OrdinalIgnoreCase);
        }

        Assert.True(System.Text.RegularExpressions.Regex.Count(study, @"https://") >= 20, "Cada componente debe llevar su URL oficial.");
        Assert.Contains("Conflicto académico", study, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("no se reduce ningún requisito", study, StringComparison.OrdinalIgnoreCase);
    }

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
}
