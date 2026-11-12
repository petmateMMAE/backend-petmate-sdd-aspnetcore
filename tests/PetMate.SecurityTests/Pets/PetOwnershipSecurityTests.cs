using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetMate.Api.Authorization;
using PetMate.Api.Contracts.Pets;
using PetMate.Api.Controllers;
using PetMate.Application.Pagination;
using PetMate.Application.Persistence;
using PetMate.Application.Pets;

namespace PetMate.SecurityTests.Pets;

/// <summary>
/// Propiedad de las mascotas (pm-045): la verificación del propietario vive en cada consulta y cada caso de uso, no solo en
/// el controlador, y el cliente nunca puede elegir ni ver al propietario. Pruebas estructurales: el comportamiento de extremo
/// a extremo está en ContractTests (Owner A frente a Owner B) y en las pruebas de integración con Atlas.
/// </summary>
public sealed class PetOwnershipSecurityTests
{
    private static readonly string[] OwnerNames = ["ownerId"];

    private static readonly string[] AllowedActionParameters = ["request", "id", "page", "pageSize"];

    [Fact]
    public void Every_repository_operation_except_add_requires_the_owner()
    {
        // Add, UpdateOwned y ReplaceQrOwned reciben la entidad, que lleva su propietario; los dos últimos filtran por Id y OwnerId de la entidad.
        // FindByQr es la única lectura sin dueño (pm-125): solo la usa el escaneo de un negocio, que exige una autorización vigente.
        string[] entityBased = [nameof(IPetRepository.Add), nameof(IPetRepository.UpdateOwned), nameof(IPetRepository.ReplaceQrOwned)];
        var methods = typeof(IPetRepository).GetMethods().Where(m => !entityBased.Contains(m.Name) && m.Name != nameof(IPetRepository.FindByQr)).ToArray();

        // La única lectura sin dueño recibe solo el token o código del QR.
        Assert.Equal([typeof(string), typeof(CancellationToken)], typeof(IPetRepository).GetMethod(nameof(IPetRepository.FindByQr))!.GetParameters().Select(p => p.ParameterType));

        Assert.NotEmpty(methods);
        Assert.All(methods, method => Assert.Contains(method.GetParameters(), p => OwnerNames.Contains(p.Name, StringComparer.Ordinal) && p.ParameterType == typeof(Guid)));
        Assert.All(entityBased,
            name => Assert.Contains(typeof(IPetRepository).GetMethod(name)!.GetParameters(), p => p.ParameterType == typeof(PetMate.Domain.Pets.Pet)));
    }

    [Fact]
    public void The_repository_has_no_operation_that_reads_or_writes_a_pet_by_id_alone()
    {
        var byIdOnly = typeof(IPetRepository).GetMethods()
            .Where(m => m.GetParameters().Any(p => p.Name == "petId") && !m.GetParameters().Any(p => p.Name == "ownerId"));

        Assert.Empty(byIdOnly);
    }

    [Fact]
    public void Every_pet_request_carries_the_owner_taken_from_the_token_as_its_first_member()
    {
        var requests = typeof(CreatePetCommand).Assembly.GetTypes()
            .Where(type => type.Namespace == typeof(CreatePetCommand).Namespace && (type.Name.EndsWith("Command", StringComparison.Ordinal) || type.Name.EndsWith("Query", StringComparison.Ordinal)))
            .ToArray();

        Assert.Equal(7, requests.Length);
        Assert.All(requests, request =>
        {
            var first = request.GetConstructors().Single().GetParameters()[0];
            Assert.Equal("OwnerId", first.Name);
            Assert.Equal(typeof(Guid), first.ParameterType);
        });
    }

    [Fact]
    public void The_api_never_exposes_or_accepts_the_owner_of_a_pet()
    {
        Assert.DoesNotContain(typeof(PetResponse).GetProperties(), p => p.Name.Contains("Owner", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(typeof(PetResult).GetProperties(), p => p.Name.Contains("Owner", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(typeof(PetRequest).GetProperties(), p => p.Name.Contains("Owner", StringComparison.OrdinalIgnoreCase) || p.Name is "Id" or "UserId");
    }

    [Fact]
    public void The_pets_controller_requires_the_owner_policy_and_allows_nothing_anonymous()
    {
        var controller = typeof(PetsController);
        var authorize = Assert.Single(controller.GetCustomAttributes<AuthorizeAttribute>());

        Assert.Equal(AuthorizationPolicies.RequireUser, authorize.Policy);
        Assert.Empty(controller.GetCustomAttributes<AllowAnonymousAttribute>());
        Assert.All(controller.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
            action => Assert.Empty(action.GetCustomAttributes<AllowAnonymousAttribute>()));
    }

    [Fact]
    public void No_pets_action_takes_the_owner_from_the_route_query_or_body()
    {
        var parameters = typeof(PetsController).GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .SelectMany(m => m.GetParameters())
            .Where(p => p.ParameterType != typeof(CancellationToken))
            .Select(p => p.Name!)
            .ToArray();

        Assert.DoesNotContain(parameters, name => name.Contains("owner", StringComparison.OrdinalIgnoreCase) || name.Contains("user", StringComparison.OrdinalIgnoreCase));
        Assert.All(parameters, name => Assert.Contains(name, AllowedActionParameters));
    }

    [Fact]
    public void The_pets_controller_is_rate_limited_per_user_and_creation_is_capped_per_owner()
    {
        var limiter = Assert.Single(typeof(PetsController).GetCustomAttributes<Microsoft.AspNetCore.RateLimiting.EnableRateLimitingAttribute>());

        Assert.Equal(PetsController.RateLimitPolicy, limiter.PolicyName);
        Assert.Equal(100, PetLimits.MaxPetsPerOwner);
        Assert.Equal(16L * 1024, PetMate.Api.Middleware.RequestBodyLimitMiddleware.MaxRequestBodyBytes);
    }

    [Fact]
    public void Every_authenticated_and_anonymous_auth_action_that_costs_work_is_rate_limited()
    {
        var actions = typeof(PetMate.Api.Controllers.AuthController).GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

        Assert.Equal(6, actions.Length);
        Assert.All(actions, action => Assert.NotEmpty(action.GetCustomAttributes<Microsoft.AspNetCore.RateLimiting.EnableRateLimitingAttribute>()));
    }

    [Fact]
    public void Listing_is_bounded_so_a_client_cannot_ask_for_every_pet_at_once()
    {
        Assert.Equal(50, PaginationConvention.MaxPageSize);
        Assert.NotEmpty(PaginationConvention.Check(1, int.MaxValue));
    }
}
