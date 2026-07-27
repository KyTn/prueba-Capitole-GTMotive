using System.Linq;
using GtMotive.Estimate.Microservice.Api.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Xunit;

namespace GtMotive.Estimate.Microservice.UnitTests.Authorization;

public sealed class AuthorizationOptionsExtensionsTests
{
    [Fact]
    public void ConfigureRegistersEveryCatalogPolicyWithMatchingPermission()
    {
        var options = new AuthorizationOptions();

        AuthorizationOptionsExtensions.Configure(options);

        foreach (var policyName in AuthorizationCatalog.Policies.All)
        {
            var policy = options.GetPolicy(policyName);
            var requirement = Assert.Single(policy.Requirements.OfType<ClaimsAuthorizationRequirement>());
            Assert.Equal(AuthorizationCatalog.PermissionClaimType, requirement.ClaimType);
            Assert.Equal([policyName], requirement.AllowedValues);
        }
    }
}

