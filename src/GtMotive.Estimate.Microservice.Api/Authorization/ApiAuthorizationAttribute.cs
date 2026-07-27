using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;

namespace GtMotive.Estimate.Microservice.Api.Authorization;

[AttributeUsage(
    AttributeTargets.Class | AttributeTargets.Method,
    AllowMultiple = false,
    Inherited = true)]
public sealed class ApiAuthorizationAttribute :
    AuthorizeAttribute,
    IAuthorizationRequirementData
{
    public ApiAuthorizationAttribute(string resourceName, params string[] policyNames)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(resourceName);
        ArgumentNullException.ThrowIfNull(policyNames);

        var normalizedPolicies = policyNames
            .Select(policyName =>
            {
                ArgumentException.ThrowIfNullOrWhiteSpace(policyName);
                return policyName.Trim();
            })
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        if (normalizedPolicies.Length == 0)
        {
            throw new ArgumentException(
                "At least one authorization policy is required.",
                nameof(policyNames));
        }

        ResourceName = resourceName.Trim();
        PolicyNames = Array.AsReadOnly(normalizedPolicies);
    }

    public string ResourceName { get; }

    public IReadOnlyList<string> PolicyNames { get; }

    public IEnumerable<IAuthorizationRequirement> GetRequirements()
    {
        yield return new ApiAuthorizationRequirement(ResourceName, PolicyNames);
    }
}

