using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace GtMotive.Estimate.Microservice.InfrastructureTests.Vehicles;

public sealed class ListVehiclesEmptyFleetEndpointTests
{
    [Fact]
    public async Task Get_EmptyFleetReturnsOkWithJsonArrayThroughHost()
    {
        await using var factory = new VehicleApiFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/vehicles");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal("[]", await response.Content.ReadAsStringAsync());
        Assert.Equal(0, factory.Repository.Count);
    }
}
