using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using GtMotive.Estimate.Microservice.Api.Authorization;
using GtMotive.Estimate.Microservice.FunctionalTests.TestDoubles;
using Microsoft.AspNetCore.Authorization;
using Xunit;

namespace GtMotive.Estimate.Microservice.FunctionalTests.Authorization;

public sealed class ConcurrentAuthorizationTests
{
    [Fact]
    public async Task ConcurrentEvaluationsKeepTheirOwnPrincipal()
    {
        var service = new RecordingAuthorizationService(
            new Dictionary<string, bool>
            {
                [AuthorizationCatalog.Policies.VehiclesRead] = true
            });
        var evaluations = Enumerable.Range(0, 32)
            .Select(index =>
            {
                var principal = new ClaimsPrincipal(
                    new ClaimsIdentity(
                        [new Claim(ClaimTypes.NameIdentifier, index.ToString())],
                        "test"));
                var requirement = new ApiAuthorizationRequirement(
                    AuthorizationCatalog.Resources.Vehicles,
                    [AuthorizationCatalog.Policies.VehiclesRead]);
                var context = new AuthorizationHandlerContext(
                    [requirement],
                    principal,
                    resource: null);
                return (
                    Handler: new ApiAuthorizationHandler(service),
                    Context: context);
            })
            .ToArray();

        await Task.WhenAll(evaluations.Select(
            item => item.Handler.HandleAsync(item.Context)));

        Assert.Equal(32, service.Calls.Count);
        Assert.Equal(
            32,
            service.Calls.Select(
                call => call.User.FindFirst(ClaimTypes.NameIdentifier)?.Value).Distinct().Count());
        Assert.All(evaluations, item => Assert.True(item.Context.HasSucceeded));
    }
}

