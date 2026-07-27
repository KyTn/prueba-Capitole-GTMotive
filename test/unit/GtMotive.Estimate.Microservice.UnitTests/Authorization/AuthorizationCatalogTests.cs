using System;
using System.Linq;
using GtMotive.Estimate.Microservice.Api.Authorization;
using Xunit;

namespace GtMotive.Estimate.Microservice.UnitTests.Authorization;

public sealed class AuthorizationCatalogTests
{
    [Fact]
    public void CatalogContainsExpectedCaseSensitiveNames()
    {
        Assert.Equal("permission", AuthorizationCatalog.PermissionClaimType);
        Assert.Equal(
            ["Rentals", "Vehicles"],
            AuthorizationCatalog.Resources.All.OrderBy(value => value, StringComparer.Ordinal));
        Assert.Equal(
            ["Rentals.Create", "Rentals.Return", "Vehicles.Create", "Vehicles.Read"],
            AuthorizationCatalog.Policies.All.OrderBy(value => value, StringComparer.Ordinal));
        Assert.False(AuthorizationCatalog.IsKnownPolicy("vehicles.create"));
        Assert.False(AuthorizationCatalog.IsKnownResource("vehicles"));
    }
}
