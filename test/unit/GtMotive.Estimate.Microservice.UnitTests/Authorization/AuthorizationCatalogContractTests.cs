using System.Reflection;
using GtMotive.Estimate.Microservice.Api.Authorization;
using GtMotive.Estimate.Microservice.Api.Rentals.Rent;
using GtMotive.Estimate.Microservice.Api.Rentals.Return;
using GtMotive.Estimate.Microservice.Api.Vehicles.Create;
using GtMotive.Estimate.Microservice.Api.Vehicles.List;
using Xunit;

namespace GtMotive.Estimate.Microservice.UnitTests.Authorization;

public sealed class AuthorizationCatalogContractTests
{
    [Theory]
    [InlineData(
        typeof(VehiclesController),
        "CreateAsync",
        "Vehicles",
        "Vehicles.Create")]
    [InlineData(
        typeof(ListVehiclesController),
        "ListAsync",
        "Vehicles",
        "Vehicles.Read")]
    [InlineData(
        typeof(RentalsController),
        "RentAsync",
        "Rentals",
        "Rentals.Create")]
    [InlineData(
        typeof(RentalReturnsController),
        "ReturnAsync",
        "Rentals",
        "Rentals.Return")]
    public void EndpointAssignmentMatchesCatalog(
        System.Type controller,
        string actionName,
        string resource,
        string policy)
    {
        var action = controller.GetMethod(actionName, BindingFlags.Instance | BindingFlags.Public);
        var declaration = Assert.Single(
            action.GetCustomAttributes<ApiAuthorizationAttribute>(inherit: true));

        Assert.Equal(resource, declaration.ResourceName);
        Assert.Equal([policy], declaration.PolicyNames);
    }
}
