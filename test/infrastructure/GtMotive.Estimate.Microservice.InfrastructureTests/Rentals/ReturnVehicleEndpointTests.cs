using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace GtMotive.Estimate.Microservice.InfrastructureTests.Rentals;

public sealed class ReturnVehicleEndpointTests
{
    [Fact]
    public async Task Post_ClosesActiveRental()
    {
        await using var factory = new RentalApiFactory();
        var person = factory.AddPerson();
        var vehicle = await factory.AddVehicleAsync();
        var rental = await factory.AddActiveRentalAsync(person, vehicle);
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/rentals/returns",
            new { PersonId = person.Value, VehicleId = vehicle.Id });
        var body = await response.Content.ReadFromJsonAsync<ReturnResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(rental.Id, body.Id);
        Assert.Equal("closed", body.Status);
        Assert.Equal(factory.Clock.UtcNow, body.EndedAt);
        Assert.Equal("Closed", Assert.Single(factory.Rentals.Rentals).Status.ToString());
    }

    private sealed record ReturnResponse(
        Guid Id,
        Guid PersonId,
        Guid VehicleId,
        DateTimeOffset StartedAt,
        DateTimeOffset EndedAt,
        string Status);
}
