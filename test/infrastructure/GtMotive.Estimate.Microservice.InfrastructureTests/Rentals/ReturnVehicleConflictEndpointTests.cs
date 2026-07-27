using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace GtMotive.Estimate.Microservice.InfrastructureTests.Rentals;

public sealed class ReturnVehicleConflictEndpointTests
{
    [Fact]
    public async Task Post_RejectsVehicleWithoutActiveRental()
    {
        await using var factory = new RentalApiFactory();
        var person = factory.AddPerson();
        var vehicle = await factory.AddVehicleAsync();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/rentals/returns",
            new { PersonId = person.Value, VehicleId = vehicle.Id });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Post_ConcurrentReturnsCloseExactlyOnce()
    {
        await using var factory = new RentalApiFactory();
        var person = factory.AddPerson();
        var vehicle = await factory.AddVehicleAsync();
        await factory.AddActiveRentalAsync(person, vehicle);
        using var client = factory.CreateClient();
        var request = new { PersonId = person.Value, VehicleId = vehicle.Id };

        var responses = await Task.WhenAll(
            client.PostAsJsonAsync("/rentals/returns", request),
            client.PostAsJsonAsync("/rentals/returns", request));

        Assert.Single(responses, response => response.StatusCode == HttpStatusCode.OK);
        Assert.Single(responses, response => response.StatusCode == HttpStatusCode.Conflict);
        Assert.Single(factory.Rentals.Rentals, rental => rental.EndedAt.HasValue);
    }
}
