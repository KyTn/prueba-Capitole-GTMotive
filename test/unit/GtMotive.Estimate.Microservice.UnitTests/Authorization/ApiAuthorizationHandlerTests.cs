using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using GtMotive.Estimate.Microservice.Api.Authorization;
using Microsoft.AspNetCore.Authorization;
using Xunit;
using DomainAuthorizationService =
    GtMotive.Estimate.Microservice.Domain.Interfaces.IAuthorizationService;

namespace GtMotive.Estimate.Microservice.UnitTests.Authorization;

public sealed class ApiAuthorizationHandlerTests
{
    [Fact]
    public async Task PoliciesAreAndCombinedAndShortCircuited()
    {
        var service = new RecordingAuthorizationService(true, false, true);
        var requirement = Requirement(
            AuthorizationCatalog.Policies.VehiclesRead,
            AuthorizationCatalog.Policies.VehiclesCreate,
            AuthorizationCatalog.Policies.VehiclesRead);
        var context = CreateContext(requirement);

        await new ApiAuthorizationHandler(service).HandleAsync(context);

        Assert.True(context.HasFailed);
        Assert.Equal(
            [AuthorizationCatalog.Policies.VehiclesRead,
             AuthorizationCatalog.Policies.VehiclesCreate],
            service.Calls);
    }

    [Fact]
    public async Task AllSuccessfulPoliciesSatisfyRequirement()
    {
        var service = new RecordingAuthorizationService(true, true);
        var requirement = Requirement(
            AuthorizationCatalog.Policies.VehiclesRead,
            AuthorizationCatalog.Policies.VehiclesCreate);
        var context = CreateContext(requirement);

        await new ApiAuthorizationHandler(service).HandleAsync(context);

        Assert.True(context.HasSucceeded);
    }

    [Fact]
    public async Task PolicyForDifferentResourceFailsClosed()
    {
        var service = new RecordingAuthorizationService(true);
        var requirement = Requirement(AuthorizationCatalog.Policies.RentalsCreate);
        var context = CreateContext(requirement);

        await new ApiAuthorizationHandler(service).HandleAsync(context);

        Assert.True(context.HasFailed);
        Assert.Empty(service.Calls);
    }

    private static ApiAuthorizationRequirement Requirement(params string[] policies) =>
        new(AuthorizationCatalog.Resources.Vehicles, policies);

    private static AuthorizationHandlerContext CreateContext(
        ApiAuthorizationRequirement requirement) =>
        new(
            [requirement],
            new ClaimsPrincipal(new ClaimsIdentity([], "test")),
            resource: null);

    private sealed class RecordingAuthorizationService(params bool[] outcomes)
        : DomainAuthorizationService
    {
        private int _index;

        public List<string> Calls { get; } = [];

        public Task<bool> Authorize(
            ClaimsPrincipal user,
            object resource,
            string policyName)
        {
            Assert.Equal(AuthorizationCatalog.Resources.Vehicles, resource);
            Calls.Add(policyName);
            return Task.FromResult(outcomes[_index++]);
        }
    }
}
