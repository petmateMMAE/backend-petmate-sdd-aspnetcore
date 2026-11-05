using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using PetMate.Api.Authorization;
using PetMate.Domain.Users;

namespace PetMate.SecurityTests.Authorization;

/// <summary>pm-033 / T022: definición y evaluación de las políticas con principales sintéticos (sin servidor HTTP).</summary>
public sealed class AuthorizationPoliciesTests
{
    private static readonly string[] NamedPolicies = [AuthorizationPolicies.RequireOwner, AuthorizationPolicies.RequireBusiness];
    private static readonly string[] RoleConstants = [AuthorizationPolicies.OwnerRole, AuthorizationPolicies.BusinessRole];

    [Fact]
    public async Task Default_and_fallback_policies_exist_and_require_an_authenticated_owner_or_business()
    {
        await using var provider = Build();
        var policies = provider.GetRequiredService<IAuthorizationPolicyProvider>();

        foreach (var policy in new[] { await policies.GetDefaultPolicyAsync(), await policies.GetFallbackPolicyAsync() })
        {
            Assert.NotNull(policy);
            Assert.Contains(policy.Requirements, r => r is DenyAnonymousAuthorizationRequirement);
            var claim = Assert.Single(policy.Requirements.OfType<ClaimsAuthorizationRequirement>());
            Assert.Equal(AuthorizationPolicies.RoleClaim, claim.ClaimType);
            Assert.Equal(["Owner", "Business"], claim.AllowedValues!.Order(StringComparer.Ordinal).Reverse());
        }
    }

    [Fact]
    public async Task Policy_names_are_exactly_RequireOwner_and_RequireBusiness_and_there_is_no_admin_policy()
    {
        await using var provider = Build();
        var policies = provider.GetRequiredService<IAuthorizationPolicyProvider>();
        var declared = typeof(AuthorizationPolicies)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(f => f.IsLiteral && f.Name.StartsWith("Require", StringComparison.Ordinal))
            .Select(f => (string)f.GetRawConstantValue()!)
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(["RequireBusiness", "RequireOwner"], declared);
        foreach (var name in declared)
        {
            Assert.NotNull(await policies.GetPolicyAsync(name));
        }

        Assert.Null(await policies.GetPolicyAsync("RequireAdmin"));
        Assert.Null(await policies.GetPolicyAsync("Admin"));
    }

    [Fact]
    public void Role_constants_match_the_domain_roles_and_there_is_no_admin_role()
    {
        Assert.Equal(
            Enum.GetNames<UserRole>().Order(StringComparer.Ordinal),
            RoleConstants.Order(StringComparer.Ordinal));
        Assert.Equal("role", AuthorizationPolicies.RoleClaim);
        Assert.DoesNotContain(Enum.GetNames<UserRole>(), n => n.Contains("Admin", StringComparison.OrdinalIgnoreCase));
    }

    [Theory]
    [InlineData(AuthorizationPolicies.RequireOwner, "Owner", true)]
    [InlineData(AuthorizationPolicies.RequireOwner, "Business", false)]
    [InlineData(AuthorizationPolicies.RequireOwner, "Admin", false)]
    [InlineData(AuthorizationPolicies.RequireOwner, "owner", false)]
    [InlineData(AuthorizationPolicies.RequireOwner, "Owner ", false)]
    [InlineData(AuthorizationPolicies.RequireOwner, null, false)]
    [InlineData(AuthorizationPolicies.RequireBusiness, "Business", true)]
    [InlineData(AuthorizationPolicies.RequireBusiness, "Owner", false)]
    [InlineData(AuthorizationPolicies.RequireBusiness, "business", false)]
    [InlineData(AuthorizationPolicies.RequireBusiness, null, false)]
    public async Task Named_policies_decide_only_by_the_exact_role_claim(string policy, string? role, bool expected)
    {
        await using var provider = Build();
        var authorization = provider.GetRequiredService<IAuthorizationService>();

        var result = await authorization.AuthorizeAsync(Authenticated(role), resource: null, policy);

        Assert.Equal(expected, result.Succeeded);
    }

    [Theory]
    [InlineData("Owner", true)]
    [InlineData("Business", true)]
    [InlineData("Admin", false)]
    [InlineData("owner", false)]
    [InlineData(null, false)]
    public async Task Fallback_and_default_policies_admit_only_authenticated_known_roles(string? role, bool expected)
    {
        await using var provider = Build();
        var authorization = provider.GetRequiredService<IAuthorizationService>();
        var policies = provider.GetRequiredService<IAuthorizationPolicyProvider>();

        foreach (var policy in new[] { (await policies.GetDefaultPolicyAsync()), (await policies.GetFallbackPolicyAsync())! })
        {
            var result = await authorization.AuthorizeAsync(Authenticated(role), resource: null, policy);
            Assert.Equal(expected, result.Succeeded);
        }
    }

    [Fact]
    public async Task Anonymous_principal_is_denied_by_every_policy()
    {
        await using var provider = Build();
        var authorization = provider.GetRequiredService<IAuthorizationService>();
        var policies = provider.GetRequiredService<IAuthorizationPolicyProvider>();
        var anonymous = new ClaimsPrincipal(new ClaimsIdentity());

        foreach (var name in NamedPolicies)
        {
            Assert.False((await authorization.AuthorizeAsync(anonymous, resource: null, name)).Succeeded);
        }

        Assert.False((await authorization.AuthorizeAsync(anonymous, resource: null, (await policies.GetFallbackPolicyAsync())!)).Succeeded);
    }

    [Fact]
    public async Task Role_in_a_different_claim_type_is_ignored()
    {
        await using var provider = Build();
        var authorization = provider.GetRequiredService<IAuthorizationService>();
        var identity = new ClaimsIdentity([new Claim(ClaimTypes.Role, "Owner")], authenticationType: "test");

        var result = await authorization.AuthorizeAsync(new ClaimsPrincipal(identity), resource: null, AuthorizationPolicies.RequireOwner);

        Assert.False(result.Succeeded);
    }

    private static ServiceProvider Build()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddPetMateAuthorization();
        return services.BuildServiceProvider();
    }

    private static ClaimsPrincipal Authenticated(string? role)
    {
        var claims = new List<Claim> { new("sub", Guid.NewGuid().ToString("D")) };
        if (role is not null)
        {
            claims.Add(new Claim(AuthorizationPolicies.RoleClaim, role));
        }

        return new ClaimsPrincipal(new ClaimsIdentity(claims, authenticationType: "test"));
    }
}
