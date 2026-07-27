using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace GtMotive.Estimate.Microservice.InfrastructureTests.Vehicles;

public sealed class CreateVehicleEndpointTests
{
    [Fact]
    public async Task Post_CreatesVehicleThroughHost()
    {
        await using var factory = new VehicleApiFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/vehicles",
            new
            {
                registrationNumber = " 1234abc ",
                brand = "Toyota",
                model = "Corolla",
                manufactureDate = new DateOnly(2024, 1, 1),
            });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        Assert.Equal(1, factory.Repository.Count);
        var body = await response.Content.ReadFromJsonAsync<VehicleResponse>();
        Assert.Equal("1234ABC", body.RegistrationNumber);
    }

    private sealed record VehicleResponse(
        Guid Id,
        string RegistrationNumber,
        string Brand,
        string Model,
        DateOnly ManufactureDate);
}
