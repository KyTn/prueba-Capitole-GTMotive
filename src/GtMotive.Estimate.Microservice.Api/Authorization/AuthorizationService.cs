using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using GtMotive.Estimate.Microservice.Domain.Interfaces;

namespace GtMotive.Estimate.Microservice.Api.Authorization;

public sealed class AuthorizationService : IAuthorizationService
{
    public Task<bool> Authorize(
        ClaimsPrincipal user,
        object resource,
        string policyName)
    {
        ArgumentNullException.ThrowIfNull(user);
        ArgumentException.ThrowIfNullOrWhiteSpace(policyName);

        var resourceName = resource as string;
        var authorized =
            user.Identity?.IsAuthenticated == true &&
            AuthorizationCatalog.IsPolicyForResource(policyName, resourceName) &&
            user.Claims.Any(claim =>
                string.Equals(
                    claim.Type,
                    AuthorizationCatalog.PermissionClaimType,
                    StringComparison.Ordinal) &&
                string.Equals(claim.Value, policyName, StringComparison.Ordinal));

        return Task.FromResult(authorized);
    }
}
