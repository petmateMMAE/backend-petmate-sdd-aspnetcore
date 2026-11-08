using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;

namespace PetMate.ContractTests.Architecture;

/// <summary>
/// Prueba de arquitectura de las pruebas de contrato: el pipeline (CI/CD) ejecuta un job por endpoint con
/// <c>dotnet test --filter "Endpoint=&lt;valor&gt;"</c>. Si un método no tiene el Trait, o un endpoint nuevo no tiene su job,
/// alguna prueba dejaría de ejecutarse en el pipeline sin que nadie lo note. Este archivo se mantiene junto con
/// docs/testing/endpoints.md y con la matriz de .github/workflows/pipeline.yml.
/// </summary>
public sealed class EndpointTraitTests
{
    private const string TraitName = "Endpoint";
    private const string Transversal = "transversal";

    // Endpoints con job propio en el pipeline (matriz de endpoint-tests).
    private static readonly string[] EndpointNames =
    [
        "auth-register",
        "auth-login",
        "auth-refresh",
        "auth-logout",
        "users-me-get",
        "users-me-update",
        "health-live",
        "health-ready",
        "technical-mediator-probe",
        "pets-create",
        "pets-get-by-id",
        "pets-update",
        "pets-delete",
    ];

    // Ruta HTTP de cada acción de la API de producción -> endpoint del pipeline.
    private static readonly Dictionary<string, string> RouteToEndpoint = new(StringComparer.Ordinal)
    {
        ["POST api/v1/auth/register"] = "auth-register",
        ["POST api/v1/auth/login"] = "auth-login",
        ["POST api/v1/auth/refresh"] = "auth-refresh",
        ["POST api/v1/auth/logout"] = "auth-logout",
        ["GET api/v1/users/me/get"] = "users-me-get",
        ["PUT api/v1/users/me/update"] = "users-me-update",
        ["GET api/v1/health/live"] = "health-live",
        ["GET api/v1/health/ready"] = "health-ready",
        ["GET api/v1/technical/mediator-probe"] = "technical-mediator-probe",
        ["POST api/v1/pets/create"] = "pets-create",
        ["GET api/v1/pets/get-by-id/{id:guid}"] = "pets-get-by-id",
        ["PUT api/v1/pets/update/{id:guid}"] = "pets-update",
        ["DELETE api/v1/pets/delete/{id:guid}"] = "pets-delete",
    };

    [Trait(TraitName, Transversal)]
    [Fact]
    public void Every_contract_test_method_declares_a_known_endpoint_trait()
    {
        var allowed = EndpointNames.Append(Transversal).ToHashSet(StringComparer.Ordinal);
        var problems = new List<string>();

        foreach (var method in TestMethods())
        {
            var values = EndpointTraits(method).ToArray();
            if (values.Length == 0)
            {
                problems.Add($"{method.DeclaringType!.Name}.{method.Name}: sin Trait(\"{TraitName}\", ...)");
            }
            else if (values.Any(value => !allowed.Contains(value)))
            {
                problems.Add($"{method.DeclaringType!.Name}.{method.Name}: valor desconocido ({string.Join(", ", values)})");
            }
        }

        Assert.True(problems.Count == 0, "Pruebas de contrato sin job en el pipeline:" + Environment.NewLine + string.Join(Environment.NewLine, problems));
    }

    [Trait(TraitName, Transversal)]
    [Fact]
    public void Every_endpoint_of_the_pipeline_matrix_has_at_least_one_test()
    {
        var used = TestMethods().SelectMany(EndpointTraits).ToHashSet(StringComparer.Ordinal);

        Assert.All(EndpointNames, name => Assert.Contains(name, used));
        Assert.Contains(Transversal, used);
    }

    [Trait(TraitName, Transversal)]
    [Fact]
    public void Every_production_controller_action_maps_to_an_endpoint_of_the_pipeline_matrix()
    {
        var actual = ProductionRoutes().ToArray();

        Assert.Equal(RouteToEndpoint.Keys.Order(StringComparer.Ordinal), actual.Order(StringComparer.Ordinal));
        Assert.All(RouteToEndpoint.Values, name => Assert.Contains(name, EndpointNames));
    }

    private static IEnumerable<MethodInfo> TestMethods()
        => typeof(EndpointTraitTests).Assembly.GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false })
            .SelectMany(type => type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            .Where(method => method.GetCustomAttributes(typeof(FactAttribute), inherit: true).Length > 0);

    private static IEnumerable<string> EndpointTraits(MethodInfo method)
        => method.GetCustomAttributesData()
            .Concat(method.DeclaringType!.GetCustomAttributesData())
            .Where(data => data.AttributeType == typeof(TraitAttribute)
                && data.ConstructorArguments.Count == 2
                && data.ConstructorArguments[0].Value as string == TraitName)
            .Select(data => (string)data.ConstructorArguments[1].Value!);

    private static IEnumerable<string> ProductionRoutes()
    {
        var controllers = typeof(Program).Assembly.GetTypes().Where(type => typeof(ControllerBase).IsAssignableFrom(type) && !type.IsAbstract);
        foreach (var controller in controllers)
        {
            var prefix = controller.GetCustomAttribute<RouteAttribute>()?.Template ?? string.Empty;
            foreach (var action in controller.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                foreach (var http in action.GetCustomAttributes<HttpMethodAttribute>())
                {
                    var route = string.IsNullOrEmpty(http.Template) ? prefix : $"{prefix.TrimEnd('/')}/{http.Template}";
                    foreach (var verb in http.HttpMethods)
                    {
                        yield return $"{verb} {route}";
                    }
                }
            }
        }
    }
}
