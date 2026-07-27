using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.AspNetCore.Authorization;

namespace GtMotive.Estimate.Microservice.Api.Authorization
{
    [ExcludeFromCodeCoverage]
    public static class AuthorizationOptionsExtensions
    {
        public static void Configure(AuthorizationOptions options)
        {
            ArgumentNullException.ThrowIfNull(options);

            foreach (var policyName in AuthorizationCatalog.Policies.All.OrderBy(
                         name => name,
                         StringComparer.Ordinal))
            {
                options.AddPolicy(
                    policyName,
                    policy => policy.RequireClaim(
                        AuthorizationCatalog.PermissionClaimType,
                        policyName));
            }
        }
    }
}
