using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace GtMotive.Estimate.Microservice.InfrastructureTests.Rentals;

public sealed class PersonActiveRentalConflictEndpointTests
{
    [Fact]
    public async Task Post_ConcurrentVehiclesLeaveOneRentalForPerson()
    {
        await using var factory = new RentalApiFactory();
        var person = factory.AddPerson();
        var firstVehicle = await factory.AddVehicleAsync();
        var secondVehicle = await factory.AddVehicleAsync();
        using var client = factory.CreateClient();

        var responses = await Task.WhenAll(
            client.PostAsJsonAsync(
                "/rentals",
                new { personId = person.Value, vehicleId = firstVehicle.Id }),
            client.PostAsJsonAsync(
                "/rentals",
                new { personId = person.Value, vehicleId = secondVehicle.Id }));

        Assert.Single(responses, response => response.StatusCode == HttpStatusCode.Created);
        Assert.Single(responses, response => response.StatusCode == HttpStatusCode.Conflict);
        Assert.Single(factory.Rentals.Rentals);
    }
}
