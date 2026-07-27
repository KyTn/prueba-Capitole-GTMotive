using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using GtMotive.Estimate.Microservice.Api.Authorization;
using Microsoft.AspNetCore.Authorization;
using Xunit;
using DomainAuthorizationService =
    GtMotive.Estimate.Microservice.Domain.Interfaces.IAuthorizationService;

namespace GtMotive.Estimate.Microservice.InfrastructureTests.Authorization;

public sealed class AuthorizationPerformanceTests
{
    [Fact]
    public async Task InProcessDecisionP95IsUnderOneHundredMilliseconds()
    {
        var handler = new ApiAuthorizationHandler(new AllowAuthorizationService());
        var requirement = new ApiAuthorizationRequirement(
            AuthorizationCatalog.Resources.Vehicles,
            [AuthorizationCatalog.Policies.VehiclesRead]);
        var durations = new List<double>();

        for (var index = 0; index < 100; index++)
        {
            var context = new AuthorizationHandlerContext(
                [requirement],
                new ClaimsPrincipal(new ClaimsIdentity([], "performance-test")),
                resource: null);
            var stopwatch = Stopwatch.StartNew();
            await handler.HandleAsync(context);
            stopwatch.Stop();
            durations.Add(stopwatch.Elapsed.TotalMilliseconds);
            Assert.True(context.HasSucceeded);
        }

        var ordered = durations.OrderBy(value => value).ToArray();
        Assert.True(ordered[94] < 100, $"Authorization p95 was {ordered[94]:F2} ms.");
    }

    private sealed class AllowAuthorizationService : DomainAuthorizationService
    {
        public Task<bool> Authorize(
            ClaimsPrincipal user,
            object resource,
            string policyName) => Task.FromResult(true);
    }
}
