using System.Security.Claims;
using System.Threading.Tasks;
using GtMotive.Estimate.Microservice.Api.Authorization;
using Xunit;

namespace GtMotive.Estimate.Microservice.UnitTests.Authorization;

public sealed class AuthorizationServiceTests
{
    [Fact]
    public async Task AuthorizeRequiresAuthenticatedPrincipalMatchingResourceAndPermission()
    {
        var service = new AuthorizationService();
        var user = Principal(
            new Claim(
                AuthorizationCatalog.PermissionClaimType,
                AuthorizationCatalog.Policies.VehiclesRead));

        Assert.True(await service.Authorize(
            user,
            AuthorizationCatalog.Resources.Vehicles,
            AuthorizationCatalog.Policies.VehiclesRead));
        Assert.False(await service.Authorize(
            user,
            AuthorizationCatalog.Resources.Rentals,
            AuthorizationCatalog.Policies.VehiclesRead));
        Assert.False(await service.Authorize(
            user,
            AuthorizationCatalog.Resources.Vehicles,
            AuthorizationCatalog.Policies.VehiclesCreate));
    }

    [Fact]
    public async Task AuthorizeRejectsUnauthenticatedPrincipal()
    {
        var service = new AuthorizationService();
        var user = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(
                AuthorizationCatalog.PermissionClaimType,
                AuthorizationCatalog.Policies.VehiclesRead)]));

        Assert.False(await service.Authorize(
            user,
            AuthorizationCatalog.Resources.Vehicles,
            AuthorizationCatalog.Policies.VehiclesRead));
    }

    private static ClaimsPrincipal Principal(params Claim[] claims) =>
        new(new ClaimsIdentity(claims, "test"));
}

