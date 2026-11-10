using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;

namespace PetMate.ContractTests.Architecture;

/// <summary>
/// Convenciones de ruta (pm-007 CA-04 y CA-05): cada acción de controlador de producción coincide con una fila del catálogo
/// vivo <c>docs/api/endpoints.md</c> y viceversa, y todas las rutas cumplen las reglas de nombrado documentadas en el mismo archivo
/// (sección «Reglas de nombrado de rutas»). Se ejecutan sobre los tipos reales, así que un recurso futuro que rompa la regla
/// o falte en el catálogo hace fallar la compilación de pruebas en CI.
/// </summary>
public sealed partial class RouteCatalogTests
{
    private const string Base = "/api/v1/";

    // Subrecursos permitidos como segmento intermedio (users/me/get): se amplía solo con una decisión documentada.
    private static readonly string[] AllowedSubResources = ["me"];

    [Trait("Endpoint", "transversal")]
    [Fact]
    public void Catalog_rows_and_production_routes_are_the_same_set()
    {
        var catalog = CatalogRoutes();
        var code = ProductionRoutes();

        Assert.NotEmpty(catalog);
        Assert.Equal(catalog.Order(StringComparer.Ordinal), code.Order(StringComparer.Ordinal));
    }

    [Trait("Endpoint", "transversal")]
    [Fact]
    public void Every_business_route_uses_the_versioned_base_and_a_literal_action()
    {
        var routes = ProductionRoutes();

        Assert.NotEmpty(routes);
        Assert.All(routes, route =>
        {
            Assert.StartsWith(Base, route.Split(' ')[1], StringComparison.Ordinal);
            var segments = Segments(route);
            Assert.InRange(segments.Length, 2, 3);
        });
    }

    [Trait("Endpoint", "transversal")]
    [Fact]
    public void Route_segments_follow_the_documented_naming_rules()
    {
        var problems = new List<string>();
        foreach (var route in ProductionRoutes())
        {
            var path = route.Split(' ')[1];
            var segments = Segments(route);

            // Recurso: sustantivo en minúsculas y con guiones (pets, users, auth, health, technical).
            if (!Kebab().IsMatch(segments[0]))
            {
                problems.Add($"{route}: el recurso '{segments[0]}' debe estar en minúsculas con guiones");
            }

            for (var i = 1; i < segments.Length; i++)
            {
                var segment = segments[i];
                var isLast = i == segments.Length - 1;
                if (segment.StartsWith('{'))
                {
                    // Parámetro de ruta: solo al final y con el nombre «id».
                    if (!isLast || segment != "{id}")
                    {
                        problems.Add($"{route}: el parámetro '{segment}' solo puede ser '{{id}}' y debe ir al final");
                    }
                }
                else if (!Kebab().IsMatch(segment))
                {
                    problems.Add($"{route}: el segmento '{segment}' debe estar en minúsculas con guiones");
                }
            }

            // La acción es literal (no un parámetro) y, con subrecurso, este debe estar permitido.
            var literals = segments.Where(s => !s.StartsWith('{')).ToArray();
            if (literals.Length < 2)
            {
                problems.Add($"{route}: falta la acción literal ({Base}{{recurso}}/{{acción}})");
            }
            else if (literals.Length == 3 && !AllowedSubResources.Contains(literals[1], StringComparer.Ordinal))
            {
                problems.Add($"{route}: el subrecurso '{literals[1]}' no está en la lista permitida");
            }

            if (path.EndsWith('/') || path.Contains("//", StringComparison.Ordinal) || path.Any(char.IsAsciiLetterUpper))
            {
                problems.Add($"{route}: no debe terminar en '/' ni usar mayúsculas");
            }
        }

        Assert.True(problems.Count == 0, string.Join(Environment.NewLine, problems));
    }

    [Trait("Endpoint", "transversal")]
    [Fact]
    public void The_naming_rules_are_documented_in_the_catalog()
    {
        var text = File.ReadAllText(CatalogPath());

        Assert.Contains("Reglas de nombrado de rutas", text, StringComparison.Ordinal);
        Assert.Contains("RouteCatalogTests", text, StringComparison.Ordinal);
        Assert.All(AllowedSubResources, sub => Assert.Contains($"`{sub}`", text, StringComparison.Ordinal));
    }

    [Trait("Endpoint", "transversal")]
    [Fact]
    public void The_technical_probe_is_identified_as_non_business_in_the_catalog()
    {
        var rows = File.ReadAllLines(CatalogPath()).Where(line => line.Contains("/api/v1/technical/mediator-probe`", StringComparison.Ordinal) && line.StartsWith("| GET |", StringComparison.Ordinal)).ToArray();

        Assert.Contains(rows, row => row.Contains("no funcional de negocio", StringComparison.Ordinal));
    }

    // ---------- Utilidades ----------

    private static string[] Segments(string route)
        => route.Split(' ')[1][Base.Length..].Split('/', StringSplitOptions.RemoveEmptyEntries);

    private static string CatalogPath()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            var candidate = Path.Combine(directory.FullName, "docs", "api", "endpoints.md");
            if (File.Exists(candidate))
            {
                return candidate;
            }
        }

        throw new FileNotFoundException("No se encontró docs/api/endpoints.md subiendo desde el directorio de pruebas.");
    }

    private static string[] CatalogRoutes()
        => File.ReadAllLines(CatalogPath())
            .Select(line => CatalogRow().Match(line))
            .Where(match => match.Success && !match.Groups["rest"].Value.Contains("Planeado", StringComparison.OrdinalIgnoreCase))
            .Select(match => $"{match.Groups["method"].Value} {match.Groups["path"].Value}")
            .Distinct(StringComparer.Ordinal)
            .ToArray();

    private static string[] ProductionRoutes()
    {
        var routes = new List<string>();
        var controllers = typeof(Program).Assembly.GetTypes().Where(type => typeof(ControllerBase).IsAssignableFrom(type) && !type.IsAbstract);
        foreach (var controller in controllers)
        {
            var prefix = controller.GetCustomAttribute<RouteAttribute>()?.Template ?? string.Empty;
            foreach (var action in controller.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                foreach (var http in action.GetCustomAttributes<HttpMethodAttribute>())
                {
                    var template = string.IsNullOrEmpty(http.Template) ? prefix : $"{prefix.TrimEnd('/')}/{http.Template}";
                    var path = "/" + Constraints().Replace(template, "{$1}");
                    foreach (var verb in http.HttpMethods)
                    {
                        routes.Add($"{verb} {path}");
                    }
                }
            }
        }

        return [.. routes.Distinct(StringComparer.Ordinal)];
    }

    [GeneratedRegex("^[a-z]+(-[a-z]+)*$")]
    private static partial Regex Kebab();

    [GeneratedRegex(@"\{(\w+):\w+\}")]
    private static partial Regex Constraints();

    [GeneratedRegex(@"^\| (?<method>GET|POST|PUT|DELETE) \| `(?<path>/api/v1/[^`]+)` \|(?<rest>.*)$")]
    private static partial Regex CatalogRow();
}
