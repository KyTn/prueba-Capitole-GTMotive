using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using GtMotive.Estimate.Microservice.InfrastructureTests.Rentals;
using GtMotive.Estimate.Microservice.InfrastructureTests.Vehicles;
using Xunit;

namespace GtMotive.Estimate.Microservice.InfrastructureTests.Authorization;

public sealed class AuthorizedEndpointCompatibilityTests
{
    [Fact]
    public async Task AuthorizedVehicleEndpointsReachExistingContracts()
    {
        await using var factory = new VehicleApiFactory();
        using var client = factory.CreateClient();

        using var list = await client.GetAsync("/vehicles");
        using var create = await client.PostAsJsonAsync(
            "/vehicles",
            new
            {
                registrationNumber = "1234ABC",
                brand = "Toyota",
                model = "Corolla",
                manufactureDate = new DateOnly(2024, 1, 1)
            });

        Assert.Equal(HttpStatusCode.OK, list.StatusCode);
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
    }

    [Fact]
    public async Task AuthorizedRentalEndpointsReachExistingContracts()
    {
        await using var factory = new RentalApiFactory();
        var person = factory.AddPerson();
        var vehicle = await factory.AddVehicleAsync();
        using var client = factory.CreateClient();

        using var rent = await client.PostAsJsonAsync(
            "/rentals",
            new { personId = person.Value, vehicleId = vehicle.Id });
        using var returned = await client.PostAsJsonAsync(
            "/rentals/returns",
            new { personId = person.Value, vehicleId = vehicle.Id });

        Assert.Equal(HttpStatusCode.Created, rent.StatusCode);
        Assert.Equal(HttpStatusCode.OK, returned.StatusCode);
    }
}

