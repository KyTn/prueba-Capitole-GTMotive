using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace GtMotive.Estimate.Microservice.InfrastructureTests.Rentals;

public sealed class RentVehicleEndpointTests
{
    [Fact]
    public async Task Post_CreatesRentalAndRejectsOccupiedVehicle()
    {
        await using var factory = new RentalApiFactory();
        var firstPerson = factory.AddPerson();
        var secondPerson = factory.AddPerson();
        var vehicle = await factory.AddVehicleAsync();
        using var client = factory.CreateClient();

        var created = await client.PostAsJsonAsync(
            "/rentals",
            new { personId = firstPerson.Value, vehicleId = vehicle.Id });
        var conflict = await client.PostAsJsonAsync(
            "/rentals",
            new { personId = secondPerson.Value, vehicleId = vehicle.Id });

        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        Assert.NotNull(created.Headers.Location);
        Assert.Equal(HttpStatusCode.Conflict, conflict.StatusCode);
        var body = await created.Content.ReadFromJsonAsync<RentalResponse>();
        Assert.Equal(firstPerson.Value, body.PersonId);
        Assert.Equal(vehicle.Id, body.VehicleId);
        Assert.Single(factory.Rentals.Rentals);
    }

    private sealed record RentalResponse(
        Guid Id,
        Guid PersonId,
        Guid VehicleId,
        DateTimeOffset StartedAt,
        string Status);
}
