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
        "auth-forgot-password",
        "auth-reset-password",
        "users-me-get",
        "users-me-update",
        "health-live",
        "health-ready",
        "technical-mediator-probe",
        "pets-create",
        "pets-get-all",
        "pets-get-by-id",
        "pets-update",
        "pets-delete",
        "pets-get-qr",
        "pets-regenerate-qr",
        "vaccines-create",
        "vaccines-get-all",
        "vaccines-delete",
        "dewormings-create",
        "dewormings-get-all",
        "dewormings-delete",
        "weights-create",
        "weights-get-all",
        "allergies-create",
        "allergies-get-all",
        "allergies-update",
        "businesses-create",
        "businesses-update-location",
        "businesses-update",
        "businesses-delete",
        "services-create",
        "services-get-all",
        "services-update",
        "services-delete",
        "businesses-get-nearby",
        "businesses-get-by-id",
        "access-grants-create",
        "scans-resolve",
        "consultations-create",
        "consultations-add-medication",
        "consultations-get-all",
        "consultations-get-by-id",
        "history-get-all",
        "vaccines-create-by-business",
        "dewormings-create-by-business",
        "scans-get-attended",
        "followups-create",
        "followups-update",
        "followups-add-note",
        "followups-get-by-consultation",
        "followups-get-all",
        "messages-send",
        "messages-get-all",
        "conversations-get-all",
        "conversations-mark-read",
        "conversations-reopen",
        "pets-upload-photo",
        "pets-get-photo-url",
        "pets-delete-photo",
        "businesses-upload-photo",
        "businesses-delete-photo",
    ];

    // Ruta HTTP de cada acción de la API de producción -> endpoint del pipeline.
    private static readonly Dictionary<string, string> RouteToEndpoint = new(StringComparer.Ordinal)
    {
        ["POST api/v1/auth/register"] = "auth-register",
        ["POST api/v1/auth/login"] = "auth-login",
        ["POST api/v1/auth/refresh"] = "auth-refresh",
        ["POST api/v1/auth/logout"] = "auth-logout",
        ["POST api/v1/auth/forgot-password"] = "auth-forgot-password",
        ["POST api/v1/auth/reset-password"] = "auth-reset-password",
        ["GET api/v1/users/me/get"] = "users-me-get",
        ["PUT api/v1/users/me/update"] = "users-me-update",
        ["GET api/v1/health/live"] = "health-live",
        ["GET api/v1/health/ready"] = "health-ready",
        ["GET api/v1/technical/mediator-probe"] = "technical-mediator-probe",
        ["POST api/v1/pets/create"] = "pets-create",
        ["GET api/v1/pets/get-all"] = "pets-get-all",
        ["GET api/v1/pets/get-by-id/{id:guid}"] = "pets-get-by-id",
        ["PUT api/v1/pets/update/{id:guid}"] = "pets-update",
        ["DELETE api/v1/pets/delete/{id:guid}"] = "pets-delete",
        ["GET api/v1/pets/get-qr/{id:guid}"] = "pets-get-qr",
        ["POST api/v1/pets/regenerate-qr/{id:guid}"] = "pets-regenerate-qr",
        ["POST api/v1/vaccines/create"] = "vaccines-create",
        ["GET api/v1/vaccines/get-all"] = "vaccines-get-all",
        ["DELETE api/v1/vaccines/delete/{id:guid}"] = "vaccines-delete",
        ["POST api/v1/dewormings/create"] = "dewormings-create",
        ["GET api/v1/dewormings/get-all"] = "dewormings-get-all",
        ["DELETE api/v1/dewormings/delete/{id:guid}"] = "dewormings-delete",
        ["POST api/v1/weights/create"] = "weights-create",
        ["GET api/v1/weights/get-all"] = "weights-get-all",
        ["POST api/v1/allergies/create"] = "allergies-create",
        ["GET api/v1/allergies/get-all"] = "allergies-get-all",
        ["PUT api/v1/allergies/update/{id:guid}"] = "allergies-update",
        ["POST api/v1/businesses/create"] = "businesses-create",
        ["PUT api/v1/businesses/update-location"] = "businesses-update-location",
        ["PUT api/v1/businesses/update"] = "businesses-update",
        ["DELETE api/v1/businesses/delete"] = "businesses-delete",
        ["POST api/v1/services/create"] = "services-create",
        ["GET api/v1/services/get-all"] = "services-get-all",
        ["PUT api/v1/services/update/{id:guid}"] = "services-update",
        ["DELETE api/v1/services/delete/{id:guid}"] = "services-delete",
        ["GET api/v1/businesses/get-nearby"] = "businesses-get-nearby",
        ["GET api/v1/businesses/get-by-id/{id:guid}"] = "businesses-get-by-id",
        ["POST api/v1/access-grants/create"] = "access-grants-create",
        ["POST api/v1/scans/resolve"] = "scans-resolve",
        ["POST api/v1/consultations/create"] = "consultations-create",
        ["POST api/v1/consultations/add-medication"] = "consultations-add-medication",
        ["GET api/v1/consultations/get-all"] = "consultations-get-all",
        ["GET api/v1/consultations/get-by-id/{id:guid}"] = "consultations-get-by-id",
        ["GET api/v1/history/get-all"] = "history-get-all",
        ["POST api/v1/vaccines/create-by-business"] = "vaccines-create-by-business",
        ["POST api/v1/dewormings/create-by-business"] = "dewormings-create-by-business",
        ["GET api/v1/scans/get-attended"] = "scans-get-attended",
        ["POST api/v1/followups/create"] = "followups-create",
        ["PUT api/v1/followups/update/{id:guid}"] = "followups-update",
        ["POST api/v1/followups/add-note"] = "followups-add-note",
        ["GET api/v1/followups/get-by-consultation/{id:guid}"] = "followups-get-by-consultation",
        ["GET api/v1/followups/get-all"] = "followups-get-all",
        ["POST api/v1/messages/send"] = "messages-send",
        ["GET api/v1/messages/get-all"] = "messages-get-all",
        ["GET api/v1/conversations/get-all"] = "conversations-get-all",
        ["POST api/v1/conversations/mark-read"] = "conversations-mark-read",
        ["POST api/v1/conversations/reopen"] = "conversations-reopen",
        ["POST api/v1/pets/upload-photo/{id:guid}"] = "pets-upload-photo",
        ["GET api/v1/pets/get-photo-url/{id:guid}"] = "pets-get-photo-url",
        ["DELETE api/v1/pets/delete-photo/{id:guid}"] = "pets-delete-photo",
        ["POST api/v1/businesses/upload-photo"] = "businesses-upload-photo",
        ["DELETE api/v1/businesses/delete-photo"] = "businesses-delete-photo",
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
