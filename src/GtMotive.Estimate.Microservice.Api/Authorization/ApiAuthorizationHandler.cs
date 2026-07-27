using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using GtMotive.Estimate.Microservice.Domain.Interfaces;

namespace GtMotive.Estimate.Microservice.Api.Authorization;

public sealed class ApiAuthorizationHandler(
    Domain.Interfaces.IAuthorizationService authorizationService)
    : AuthorizationHandler<ApiAuthorizationRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ApiAuthorizationRequirement requirement)
    {
        if (!AuthorizationCatalog.IsKnownResource(requirement.ResourceName) ||
            requirement.PolicyNames.Count == 0 ||
            requirement.PolicyNames.Any(policyName =>
                !AuthorizationCatalog.IsPolicyForResource(
                    policyName,
                    requirement.ResourceName)))
        {
            context.Fail();
            return;
        }

        foreach (var policyName in requirement.PolicyNames)
        {
            if (!await authorizationService.Authorize(
                    context.User,
                    requirement.ResourceName,
                    policyName))
            {
                context.Fail();
                return;
            }
        }

        context.Succeed(requirement);
    }
}

