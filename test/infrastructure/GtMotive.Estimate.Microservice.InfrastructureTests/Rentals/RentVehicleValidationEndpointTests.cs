using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace GtMotive.Estimate.Microservice.InfrastructureTests.Rentals;

public sealed class RentVehicleValidationEndpointTests
{
    [Fact]
    public async Task Post_EmptyPersonIdReturnsBadRequest()
    {
        await using var factory = new RentalApiFactory();
        var vehicle = await factory.AddVehicleAsync();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/rentals",
            new { personId = Guid.Empty, vehicleId = vehicle.Id });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Empty(factory.Rentals.Rentals);
    }

    [Fact]
    public async Task Post_UnknownPersonReturnsNotFound()
    {
        await using var factory = new RentalApiFactory();
        var vehicle = await factory.AddVehicleAsync();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/rentals",
            new { personId = Guid.NewGuid(), vehicleId = vehicle.Id });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Empty(factory.Rentals.Rentals);
    }

    [Fact]
    public async Task Post_UnknownVehicleReturnsNotFound()
    {
        await using var factory = new RentalApiFactory();
        var person = factory.AddPerson();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/rentals",
            new { personId = person.Value, vehicleId = Guid.NewGuid() });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Empty(factory.Rentals.Rentals);
    }
}
